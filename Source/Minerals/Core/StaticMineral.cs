
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using RimWorld.Planet;
using Minerals.Utilities;

namespace Minerals.Core
{

    public class StaticMineral : Mineable
    {

        // ======= Private Variables ======= //
        protected float yieldPct = 0;
        protected float sizeWhenLastPrinted = 0f;
        protected int currentTextureIndex = 0;

        // The current size of the mineral
        protected float mySize = 1f;

        // Cache for mineral texture locations
        protected Vector3[] textureLocations;

        // Cache for mineral texture sizes
        protected float[] textureSizes;

        // Cache for mineral texture indexes
        protected int[] textureIndexes;





        public float size
        {
            get
            {
                return mySize;
            }

            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 1)
                {
                    value = 1;
                }
                mySize = value;
            }
        }


        protected float? myDistFromNeededTerrain = null;
        public virtual float distFromNeededTerrain
        {
            get
            {
                if (myDistFromNeededTerrain == null) // not yet set
                {
                    myDistFromNeededTerrain = attributes.posDistFromNeededTerrain(Map, Position);
                }

                return (float)myDistFromNeededTerrain;
            }

            set
            {
                myDistFromNeededTerrain = value;
            }
        }


        public virtual ThingDef_StaticMineral attributes
        {
            get
            {
                return def as ThingDef_StaticMineral;
            }
        }

        // ======= Yeilding resources ======= //

        public virtual void incPctYeild(float amount, Pawn miner)
        {
            // Increase yeild for when it is destroyed
            yieldPct += (float)Mathf.Min(amount, HitPoints) / (float)MaxHitPoints * miner.GetStatValue(StatDefOf.MiningYield, true);

            // Drop resources
            foreach (RandomResourceDrop toDrop in attributes.randomlyDropResources)
            {
                float dropChance = size * toDrop.DropProbability * ((float) Math.Min(amount, HitPoints) / (float) MaxHitPoints) * miner.GetStatValue(StatDefOf.MiningYield, true) * MineralsMod.Settings.resourceDropFreqSetting;
                if (Rand.Range(0f, 1f) < dropChance)
                {
                    ThingDef myThingDef = DefDatabase<ThingDef>.GetNamed(toDrop.ResourceDefName, false);
                    if (myThingDef != null)
                    {
                        int dropNum = (int) Math.Round(toDrop.CountPerDrop * MineralsMod.Settings.resourceDropAmountSetting);
                        if (dropNum >= 1)
                        {
                            Thing thing = ThingMaker.MakeThing(myThingDef, null);
                            thing.stackCount = dropNum;
                            GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near, null);
                        }
                   }

                }

            }

        }


        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {

            if (def.building.mineableThing != null && def.building.mineableYieldWasteable && dinfo.Def == DamageDefOf.Mining && dinfo.Instigator != null && dinfo.Instigator is Pawn)
            {
                incPctYeild(dinfo.Amount, (Pawn)dinfo.Instigator);
            }

            base.PreApplyDamage(ref dinfo, out absorbed);

        }

        public float GetSizeBasedOnNearest(Vector3 subcenter, float baseSize)
        {
            float distToTrueCenter = Vector3.Distance(this.TrueCenter(), subcenter);
            float sizeOfNearest = 0;
            float distToNearest = 1;
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = -1; zOffset <= 1; zOffset++)
                {
                    if (xOffset == 0 & zOffset == 0)
                    {
                        continue;
                    }
                    IntVec3 checkedPosition = Position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(Map))
                    {
                        List<Thing> list = Map.thingGrid.ThingsListAt(checkedPosition);
                        foreach (Thing item in list)
                        {
                            if (item.def.defName == attributes.defName)
                            {
                                float distanceToPos = Vector3.Distance(item.TrueCenter(), subcenter);

                                if (distToNearest > distanceToPos & distanceToPos <= 1) 
                                {
                                    distToNearest = distanceToPos;
                                    sizeOfNearest = ((StaticMineral) item).size;
                                }
                            }
                        }
                    }
                }
            }

            float correctedSize = (0.75f - distToTrueCenter) * baseSize + (1 - distToNearest) * sizeOfNearest;
            return attributes.visualSizeRange.LerpThroughRange(correctedSize);
        }

        public static float randPos(float clustering, float spread)
        {
            // Weighted average of normal and uniform distribution
            return (Rand.Gaussian(0, 0.2f) * clustering + Rand.Range(-0.5f, 0.5f) * (1 - clustering)) * spread;
        }

        public virtual float submersibleFactor()
        {
            // Check that underwater minerals are enabled
            if (!MineralsMod.Settings.underwaterMineralsSetting)
            {
                return 1f;
            }

            // Check that it is submersible
            if (attributes.submergedSize >= 1)
            {
                return 1f;
            }

            // Check if is on dry land
            TerrainDef myTerrain = Map.terrainGrid.TerrainAt(Position);
            if (myTerrain == null)
            {
                return 1f;
            }
            if (!(myTerrain.defName.Contains("Water") || myTerrain.defName.Contains("IceShallow") || myTerrain.defName.Contains("MuddyIce")))
            {
                return 1f;
            }

            // count number of dry cells aroud it
            float dryCount = 0;
            float spotsChecked = 0;
            for (int xOffset = -attributes.submergedRadius; xOffset <= attributes.submergedRadius; xOffset++)
            {
                for (int zOffset = -attributes.submergedRadius; zOffset <= attributes.submergedRadius; zOffset++)
                {
                    spotsChecked = spotsChecked + 1;
                    IntVec3 checkedPosition = Position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(Map))
                    {
                        TerrainDef terrain = Map.terrainGrid.TerrainAt(checkedPosition);
                        if (terrain != null && !(terrain.defName.Contains("Water") || myTerrain.defName.Contains("IceShallow") || myTerrain.defName.Contains("MuddyIce")))
                        {
                            dryCount = dryCount + 1;
                        }
                    }
                }
            }

            // calculate
            float propDry = 0f;
            if (spotsChecked > 0)
            {
                propDry = dryCount / spotsChecked;
            }
            return attributes.submergedSize + (1 - attributes.submergedSize) * propDry;
        }

        public virtual float printSizeFactor()
        {
            float effectiveSize = 1f;
            effectiveSize = effectiveSize * submersibleFactor();
            return effectiveSize;
        }

        public virtual float printSize()
        {
            return printSizeFactor() * size;
        }

        public virtual void initializeTextureLocations()
        {

            Rand.PushState();
            Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode();

            // initalize the array if it has not already been initalized
            if (textureLocations == null)
            {
                textureLocations = new Vector3[attributes.maxMeshCount];
            }

            // Calculate the location of each texture
            Vector3 trueCenter = this.TrueCenter();
            for (int i = 0; i < textureLocations.Length; i++)
            {
                Vector3 pos = trueCenter;
                pos.x += randPos(attributes.visualClustering, attributes.visualSpread * MineralsMod.Settings.visualSpreadFactor);
                pos.z += randPos(attributes.visualClustering, attributes.visualSpread * MineralsMod.Settings.visualSpreadFactor);
                pos.z += attributes.verticalOffset;
                pos.y = attributes.Altitude;
                textureLocations[i] = pos;
            }

            // The size effects the altitude, which is a location attribute, so:
            initializeTextureSizes();

            Rand.PopState();
        }

        public virtual Vector3 getTextureLocation(int index)
        {
            // initalize the array if it has not already been initalized
            if (textureLocations == null)
            {
                initializeTextureLocations();
            }

            // Return per-calculated location
            return(textureLocations[index]);
        }

        public virtual float customAltitude(int i) {
            return attributes.Altitude;// + zProportionOfTextureBottom * 0.01f + xPropDistToEven * 0.001f / Map.Size.z;
        } 

        public virtual void initializeTextureSizes() {
        
            Rand.PushState();
            Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode();

            // initalize the array if it has not already been initalized
            if (textureSizes == null)
            {
                textureSizes = new float[attributes.maxMeshCount];
            }

            // Calculate the size of each texture
            for (int i = 0; i < textureLocations.Length; i++)
            {
                // Get location of texture
                Vector3 pos = getTextureLocation(i);

                // Adjust size for distance from center to other crystals
                float thisSize = GetSizeBasedOnNearest(pos, size);

                // Add random variation
                thisSize = thisSize + (thisSize * Rand.Range(- attributes.visualSizeVariation, attributes.visualSizeVariation));

                // Make large textures appear on top
                if (attributes.largeTexturesOnTop)
                {
                    textureLocations[i].y = customAltitude(i) + 0.01f * thisSize;
                }
                else
                {
                    textureLocations[i].y = customAltitude(i);
                }

                textureSizes[i] = thisSize;

            }

            Rand.PopState();

        }

        public virtual float getTextureSize(int index)
        {
            // initalize the array if it has not already been initalized
            if (textureSizes == null)
            {
                initializeTextureSizes();
            }

            // Return per-calculated location
            return(textureSizes[index]);
        }

        public virtual void initializeTextures() {

            Rand.PushState();
            Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode();

            // initalize the array if it has not already been initalized
            if (textureIndexes == null)
            {
                textureIndexes = new int[attributes.maxMeshCount];
            }
                
            List<int> possibilities = Enumerable.Range(0, attributes.getTexturePaths().Count).OrderBy(order=>Rand.Range(0, 100)).ToList();
            for (int i = 0; i < attributes.maxMeshCount; i++)
            {
                // get a new random set of textures if run out of options
                if (possibilities.Count == 0)
                {
                    possibilities = Enumerable.Range(0, attributes.getTexturePaths().Count).OrderBy(order=>Rand.Range(0, 100)).ToList();
                }
                textureIndexes[i] = possibilities[0];
                possibilities.RemoveAt(0);
            }

            Rand.PopState();

        }

        public virtual string getTexturePath()
        {
            // initalize the array if it has not already been initalized
            if (textureIndexes == null)
            {
                initializeTextures();
            }
                
            return(attributes.getTexturePaths()[textureIndexes[currentTextureIndex]]);
        }

        // https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object/2742288
        public static bool isSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                || potentialDescendant == potentialBase;
        }

        public static bool isMineral(Thing thing)
        {
            return isSameOrSubclass(typeof(StaticMineral), thing.GetType());
        }

        public static Thing isMineralWall(Map map, IntVec3 pos)
        {
            if (pos.InBounds(map))
            {
                List<Thing> list = pos.GetThingList(map);
                foreach (Thing item in list)
                {

                    if (isMineral(item) && item.def.passability == Traversability.Impassable)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public virtual float interactWithWalls(int i, ref Vector3 center, float size)
        {
            if (MineralsMod.Settings.mineralsGrowUpWallsSetting && attributes.growsUpWalls)
            {
                Vector3 squareCenter = this.TrueCenter();
                float leftOverlap = (squareCenter.x - 0.5f) - (center.x - size / 2);
                if (leftOverlap > 0) // left
                {
                    IntVec3 leftSide = Position - new IntVec3(1, 0, 0);
                    Thing leftWall = isMineralWall(Map, leftSide);
                    if (leftWall != null)
                    {
                        // Put half of the textures on the front of the wall
                        if (Rand.Bool)
                        {
                            center.y = leftWall.def.Altitude + 0.1f;
                        }
                        // make textures higher up the wall show on top
                        center.y = center.y + Math.Min(leftOverlap / size, 1f) * 0.1f;

                        // rotate based on proportion of texture overlapping
                        return Math.Min(90f * (leftOverlap / size), 90f);
                    }
                }
                float rightOverlap = (center.x + size / 2) - (squareCenter.x + 0.5f);
                if (rightOverlap > 0)
                {
                    IntVec3 rightSide = Position + new IntVec3(1, 0, 0);
                    Thing rightWall = isMineralWall(Map, rightSide);
                    if (rightWall != null)
                    {
                        // Put half of the textures on the front of the wall
                        if (Rand.Bool)
                        {
                            center.y = rightWall.def.Altitude + 0.1f;
                        }
                        // make textures higher up the wall show on top
                        center.y = center.y + Math.Min(rightOverlap / size, 1f) * 0.1f;

                        // rotate based on proportion of texture overlapping
                        return -Math.Min(90f * (rightOverlap / size), 90f);
                    }
                }
                float topOverlap = (center.z + size / 2) - (squareCenter.z + 0.5f);
                if (topOverlap > 0)
                {
                    IntVec3 topSide = Position + new IntVec3(0, 0, 1);
                    Thing topWall = isMineralWall(Map, topSide);
                    if (topWall != null)
                    {
                        center.y = topWall.def.Altitude + 0.1f;
                        return 180;
                    }
                }
            }
            if (attributes.printOverWalls)
            {
                Vector3 squareCenter = this.TrueCenter();
                float topOverlap = (center.z + size / 2) - (squareCenter.z + 0.4f);
                if (topOverlap > 0)
                {
                    IntVec3 topSide = Position + new IntVec3(0, 0, 1);
                    Thing topWall = isMineralWall(Map, topSide);
                    if (topWall != null)
                    {
                        center.y = topWall.def.Altitude + 0.001f;
                        return 0f;
                    }
                }
            }

            return 0f;
        }

        public virtual bool hiddenInSnow(int i)
        {
            return snowLevel() > attributes.snowTextureThreshold + (attributes.hideAtSnowDepth - attributes.snowTextureThreshold) * getTextureSize(i) / attributes.visualSizeRange.max;
        }

        public virtual void printSubTexture(SectionLayer layer, int i, float sizeFactor = 1f)
        {
            Rand.PushState();
            Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode() + i.GetHashCode();

            // Get location
            Vector3 center = getTextureLocation(i);

            // Get size
            float thisSize = getTextureSize(i) * sizeFactor;
            if (thisSize <= 0)
            {
                Rand.PopState();
                return;
            }

            // Check if snow is covering it
            if (hiddenInSnow(i))
            {
                Rand.PopState();
                return;
            }

            // Get rotation
            float thisRotation = interactWithWalls(i, ref center, thisSize);

            // Print image
            Material matSingle = Graphic.MatSingle;
            Vector2 sizeVec = new Vector2(thisSize, thisSize);
            Printer_Plane.PrintPlane(layer, center, sizeVec, matSingle, thisRotation, Rand.Bool, null, null, attributes.topVerticesAltitudeBias * thisSize, 0f);

            Rand.PopState();
        }


        public override void Print(SectionLayer layer)
        {

            // get print size
            float sizeFactor = printSizeFactor();

            if (sizeFactor <= 0)
            {
                return;
            }
 
            if (this.attributes.graphicData.graphicClass.Name != "Graphic_Random" || this.attributes.graphicData.linkType == LinkDrawerType.CornerFiller) {
                Rand.PushState();
                Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode();
                currentTextureIndex = 0;
				base.Print(layer);
                Rand.PopState();
			} else {
                int numToPrint = Mathf.CeilToInt(printSize() * (float)attributes.maxMeshCount);
				if (numToPrint < 1)
				{
					numToPrint = 1;
				}
				for (int i = 0; i < numToPrint; i++)
				{
                    printSubTexture(layer, i, sizeFactor);
                    currentTextureIndex = i;
				}
			}

        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Size: " + size.ToStringPercent());
            float propSubmerged = 1 - submersibleFactor();
            if (propSubmerged > 0)
            {
                stringBuilder.AppendLine("Submerged: " + propSubmerged.ToStringPercent());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref mySize, "mySize", 1);
        }

        public virtual float snowLevel()
        {
            if (Map == null)
            {
                return 0f;
            }
            if (attributes.passability == Traversability.Impassable)
            {
                if (Position.Roofed(Map))
                {
                    return 0f;
                }

                float total = 0f;
                int numChecked = 0;
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int zOffset = -1; zOffset <= 1; zOffset++)
                    {
                        IntVec3 checkedPosition = Position + new IntVec3(xOffset, 0, zOffset);
                        if (checkedPosition.InBounds(Map) && (! checkedPosition.Impassable(Map)))
                        {
                            total += checkedPosition.GetSnowDepth(Map);
                            numChecked += 1;
                        }
                    }
                }
                if (numChecked == 0)
                {
                    return 0f;
                }
                else
                {
                    return total / numChecked;
                }
            }
            else
            {
                return Position.GetSnowDepth(Map);
            }
        }


        public override Graphic Graphic
        {
            get
            {
      
                // Pick a random path 
                string printedTexturePath = getTexturePath();

                // Check if it should be snowy
                if (attributes.hasSnowyTextures && snowLevel() > attributes.snowTextureThreshold)
                {
                    printedTexturePath = printedTexturePath + "_s";
                }
                Graphic printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexturePath, attributes.graphicData.shaderType.Shader);

                // convert to corner filler if needed
                printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexture.path, printedTexture.Shader, printedTexture.drawSize, DrawColor, DrawColorTwo, printedTexture.data);
                if (attributes.graphicData.linkType == LinkDrawerType.CornerFiller)
                {
                     return new Graphic_LinkedCornerFiller(printedTexture);
                }
                else
                {
                    return  printedTexture;

                }

             }
        }

        public virtual float RandomColorProb(Color colorUsed) {
            Rand.PushState();
            Rand.Seed = Map.GetHashCode() + colorUsed.GetHashCode();
            float output = Rand.Range(0.1f, 1f);
            Rand.PopState();
            return output * output * output;
        }

        public override Color DrawColor {
            get
            {
                if (this.attributes.coloredByTerrain)
                {
                    TerrainDef terrain = this.Position.GetTerrain(this.Map);
                    if (terrain.graphic.Color == Color.white)
                    {
                        return base.DrawColor;
                    }
                    else
                    {
                        return terrain.graphic.Color;
                    }
                }

                if (this.attributes.randomColorsOne != null && this.attributes.randomColorsOne.Count > 0)
                {
                    if (attributes.seedRandomColorByMap)
                    {
                        return this.attributes.randomColorsOne.RandomElementByWeight(RandomColorProb);
                    }
                    else
                    {
                        return this.attributes.randomColorsOne.RandomElement();
                    }
          
                }

                return base.DrawColor;
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                if (this.attributes.randomColorsTwo != null && this.attributes.randomColorsTwo.Count > 0)
                {
                    if (attributes.seedRandomColorByMap)
                    {
                        return this.attributes.randomColorsTwo.RandomElementByWeight(RandomColorProb);
                    }
                    else
                    {
                        return this.attributes.randomColorsTwo.RandomElement();
                    }

                }

                return base.DrawColorTwo;
            }
        }
    }       
}