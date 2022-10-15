using Minerals.Utilities;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Minerals.Core
{

    public class ThingDef_StaticMineral : ThingDef
    {
        // How far away it can spawn from an existing location
        // Even though it is a static mineral, the map initialization uses "reproduction" to make clusters 
        public int spawnRadius = 1;

        // The probability that this mineral type will be spawned at all on a given map
        public float perMapProbability = 0.5f;

        // For a given map, the minimum/maximum probablility a cluster will spawn for every possible location
        public float minClusterProbability;
        public float maxClusterProbability = 0.001f;

        // How  many squares each cluster will be
        public int minClusterSize = 1;
        public int maxClusterSize = 10;

        // The range of starting sizes of individuals in clusters
        public float initialSizeMin = 0.3f;
        public float initialSizeMax = 0.3f;

        // How much initial sizes of individuals randomly vary
        public float initialSizeVariation = 0.3f;

        // The biomes this can appear in
        public List<string> allowedBiomes;

        // The terrains this can appear on
        public List<string> allowedTerrains;

        // The terrains this must be near to, but not necessarily on, and how far away it can be
        public List<string> neededNearbyTerrains;
        public float neededNearbyTerrainRadius = 3f;

        // Controls how extra clusters are added near assocaited ore
        public List<string> associatedOres;
        public float nearAssociatedOreBonus = 3f;

        // If true, growth rate and initial size depends on distance from needed terrains
        public bool neededNearbyTerrainSizeEffect = true;

        // If true, only grows under roofs
        public bool mustBeUnderRoof = true;
        public bool mustBeUnderThickRoof = false;
        public bool mustBeUnroofed = false;
        public bool mustBeNotUnderThickRoof = false;
        public bool mustBeNearPassable = false;

        // The maximum number of images that will be printed per square
        public int maxMeshCount = 1;

        // The size range of images printed
        public FloatRange visualSizeRange = new FloatRange(0.3f, 1.0f);
        public float visualClustering = 0.5f;
        public float visualSpread = 1.2f;
        public float visualSizeVariation = 0.1f;

        // If graphic overlapping with nearby wall textures are rotated
        public bool growsUpWalls = false;

        // If textures overlapping walls above them should be printed on top
        public bool printOverWalls = false;

        // If largest textures are printed on top, ro if vertical order matters
        public bool largeTexturesOnTop = false;

        // Other resources it might drop
        public List<RandomResourceDrop> randomlyDropResources;

        // If it can spawn on other things
        public bool canSpawnOnThings = false;

        // Things this mineral replaces when a map is initialized
        public List<string> ThingsToReplace;

        // If it replaces everything
        public bool replaceAll = true;

        // If it must replace something in order to spawned
        public bool mustReplace = false;

        // If the primary color is based on the stone below it
        public bool coloredByTerrain = false;

        // If defined, randomly pick colors from this set
        public List<Color> randomColorsOne;
        public List<Color> randomColorsTwo;
        // If true, then the probability of each color is randomly chosen for each map, so each map has distinctive colors.
        public bool seedRandomColorByMap = false;

        // If smaller than 1, it looks smaller in water
        public float submergedSize = 1;
        public int submergedRadius = 2;

        // Tags which determine how some options behave
        public List<string> tags;

        // Has something to do with how textures on the same layer get stacked
        public float topVerticesAltitudeBias = 0.01f;

        public List<string> texturePaths;
        public List<string> snowTexturePaths;
        public bool hasSnowyTextures = false;
        // at what snow depth the snow texture is used, if it exists
        public float snowTextureThreshold = 0.5f;

        // How much to change the vertical position of the texture. Positive is up
        public float verticalOffset = 0f;

        // What stage of map generation the thing is spawned during (chunks or plants)
        public string newMapGenStep = "chunks";

        // Minimum distance from the nearest settlement the inital spawn needs to be in order to be spawned at the maximum probablity
        public float otherSettlementMiningRadius = 0f;

        // If the mean size of minerals spawned at map generation is scaled by the relative abundance in that map
        public bool sizeScaledByAbundance = false;


        // ======= Spawning clusters ======= //


        public StaticMineral TryReproduce(Map map, IntVec3 position)
        {
            IntVec3 dest;
            if (!TryFindReproductionDestination(map, position, out dest))
            {
                return null;
            }
            return TrySpawnAt(dest, map, 0.01f);
        }


        public virtual StaticMineral TrySpawnCluster(Map map, IntVec3 position, float size, int clusterCount)
        {
            StaticMineral mineral = TrySpawnAt(position, map, size);
            if (mineral != null)
            {
                GrowCluster(map, mineral, clusterCount);

            }
            return mineral;
        }

        public virtual StaticMineral SpawnCluster(Map map, IntVec3 position, float size, int clusterCount)
        {
            StaticMineral mineral = SpawnAt(map, position, size);
            if (mineral != null)
            {
                GrowCluster(map, mineral, clusterCount);

            }
            return mineral;
        }


        public virtual void GrowCluster(Map map, StaticMineral sourceMineral, int times)
        {
            if (times > 0)
            {
                StaticMineral newGrowth = sourceMineral.attributes.TryReproduce(map, sourceMineral.Position);
                if (newGrowth != null)
                {
                    newGrowth.size = Rand.Range(1f - initialSizeVariation, 1f + initialSizeVariation) * sourceMineral.size;
                    GrowCluster(map, newGrowth, times - 1);
                }

            }
        }


        public virtual Thing ThingToReplaceAtPos(Map map, IntVec3 position)
        {
            //if (defName == "BigColdstoneCrystal") Log.Message("ThingToReplaceAtPos: checking for " + defName +  " at " + position, true);
            if (ThingsToReplace == null || ThingsToReplace.Count == 0)
            {
                //if (defName == "BigColdstoneCrystal") Log.Message("ThingToReplaceAtPos: no replacement defined", true);
                return (null);
            }
            foreach (Thing thing in map.thingGrid.ThingsListAt(position))
            {
                if (thing == null || thing.def == null)
                {
                    continue;
                }
                //if (defName == "BigColdstoneCrystal") Log.Message("ThingToReplaceAtPos: found " + thing.def.defName + " at " + position, true);
                if (ThingsToReplace.Any(thing.def.defName.Equals))
                {
                    return (thing);
                }
            }
            return (null);
        }

        // ======= Spawning conditions ======= //


        public virtual bool CanSpawnAt(Map map, IntVec3 position, bool initialSpawn = false)
        {
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: checking for " + defName + " at " + position, true);

            // Check that location is in the map
            if (!position.InBounds(map))
            {
                return false;
            }
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: is in bounds" + position + " " + map, true);

            // Check that it is under a roof if it needs to be
            if (!isRoofConditionOk(map, position))
            {
                return false;
            }
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: roof is ok " + position, true);

            // Check that the terrain is ok
            if (!IsTerrainOkAt(map, position))
            {
                return false;
            }
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: terrain is ok " + position, true);

            // Look for stuff in the way
            if (PlaceIsBlocked(map, position, initialSpawn))
            {
                return false;
            }
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: not blocked " + position, true);

            // Check for things it must replace
            if (mustReplace && ThingToReplaceAtPos(map, position) == null)
            {
                return false;
            }
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: replacement is ok " + position, true);

            // Check that it is near any needed terrains
            if (!isNearNeededTerrain(map, position))
            {
                return false;
            }
            //if (defName == "BigColdstoneCrystal") Log.Message("CanSpawnAt: can spawn " + position, true);

            return true;
        }

        public virtual bool PlaceIsBlocked(Map map, IntVec3 position, bool initialSpawn)
        {
            if (ThingToReplaceAtPos(map, position) != null)
            {
                return false;
            }
            foreach (Thing thing in map.thingGrid.ThingsListAt(position))
            {
                if (thing == null || thing.def == null)
                {
                    continue;
                }

                // Blocked by pawns, items, and plants
                if (!canSpawnOnThings)
                {
                    if (thing.def.category == ThingCategory.Pawn ||
                        thing.def.category == ThingCategory.Item ||
                        thing.def.category == ThingCategory.Plant ||
                        thing.def.category == ThingCategory.Building
                    )
                    {
                        return true;
                    }
                }

                // Blocked by impassible things
                if (thing.def.passability == Traversability.Impassable)
                {
                    return true;
                }

            }
            return false;
        }

        public static bool PosHasThing(Map map, IntVec3 position, List<string> things)
        {
            if (things == null || things.Count == 0)
            {
                return false;
            }

            TerrainDef terrain = map.terrainGrid.TerrainAt(position);
            if (things.Any(terrain.defName.Equals))
            {
                return true;
            }

            foreach (Thing thing in map.thingGrid.ThingsListAt(position))
            {
                if (thing == null || thing.def == null)
                {
                    continue;
                }

                if (things.Any(thing.def.defName.Equals))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool PosIsAssociatedOre(Map map, IntVec3 position)
        {
            return PosHasThing(map, position, associatedOres);
        }


        public virtual bool CanSpawnInBiome(Map map)
        {
            if (allowedBiomes == null || allowedBiomes.Count == 0)
            {
                return true;
            }
            else
            {
                return allowedBiomes.Any(map.Biome.defName.Equals);
            }
        }

        public virtual bool IsTerrainOkAt(Map map, IntVec3 position)
        {
            if (!position.InBounds(map))
            {
                //if (defName == "BigColdstoneCrystal") Log.Message("IsTerrainOkAt: out of bounds", true);
                return false;
            }
            if (allowedTerrains == null || allowedTerrains.Count == 0)
            {
                //if (defName == "BigColdstoneCrystal") Log.Message("IsTerrainOkAt: no terrain needed", true);
                return true;
            }
            TerrainDef terrain = map.terrainGrid.TerrainAt(position);
            // if (defName == "SmallFossils") Log.Message("IsTerrainOkAt: found terrain " + terrain.defName + ". checking if it is one of: " + String.Join(", ", allowedTerrains.ToArray()), true);
            return allowedTerrains.Any(terrain.defName.Equals);
        }

        public virtual bool isNearNeededTerrain(Map map, IntVec3 position)
        {
            if (neededNearbyTerrains == null || neededNearbyTerrains.Count == 0)
            {
                return true;
            }

            for (int xOffset = -(int)Math.Ceiling(neededNearbyTerrainRadius); xOffset <= (int)Math.Ceiling(neededNearbyTerrainRadius); xOffset++)
            {
                for (int zOffset = -(int)Math.Ceiling(neededNearbyTerrainRadius); zOffset <= (int)Math.Ceiling(neededNearbyTerrainRadius); zOffset++)
                {
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        TerrainDef terrain = map.terrainGrid.TerrainAt(checkedPosition);
                        if (neededNearbyTerrains.Any(terrain.defName.Equals) && position.DistanceTo(checkedPosition) < neededNearbyTerrainRadius)
                        {
                            return true;
                        }
                        foreach (Thing thing in map.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if (neededNearbyTerrains.Any(thing.def.defName.Equals) && position.DistanceTo(checkedPosition) < neededNearbyTerrainRadius)
                            {
                                return true;
                            }
                        }

                    }
                }
            }

            return false;
        }


        // The distance a position is from a needed terrain type
        // A little slower than `isNearNeededTerrain` because all squares are checked
        public virtual float posDistFromNeededTerrain(Map map, IntVec3 position)
        {
            if (neededNearbyTerrains == null || neededNearbyTerrains.Count == 0)
            {
                return 0;
            }

            float output = -1;

            for (int xOffset = -(int)Math.Ceiling(neededNearbyTerrainRadius); xOffset <= (int)Math.Ceiling(neededNearbyTerrainRadius); xOffset++)
            {
                for (int zOffset = -(int)Math.Ceiling(neededNearbyTerrainRadius); zOffset <= (int)Math.Ceiling(neededNearbyTerrainRadius); zOffset++)
                {
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        TerrainDef terrain = map.terrainGrid.TerrainAt(checkedPosition);
                        if (neededNearbyTerrains.Any(terrain.defName.Equals))
                        {
                            float distanceToPos = position.DistanceTo(checkedPosition);
                            if (output < 0 || output > distanceToPos)
                            {
                                output = distanceToPos;
                            }
                        }
                        foreach (Thing thing in map.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if (neededNearbyTerrains.Any(thing.def.defName.Equals))
                            {
                                float distanceToPos = position.DistanceTo(checkedPosition);
                                if (output < 0 || output > distanceToPos)
                                {
                                    output = distanceToPos;
                                }
                            }
                        }

                    }
                }
            }
            return output;
        }

        // ======= Spawning individuals ======= //
        public virtual StaticMineral TrySpawnAt(IntVec3 dest, Map map, float size)
        {
            if (CanSpawnAt(map, dest))
            {
                return SpawnAt(map, dest, size);
            }
            else
            {
                return null;
            }
        }

        public virtual StaticMineral SpawnAt(Map map, IntVec3 dest, float size)
        {
            // Remove things to replace
            Thing thingToRemove = ThingToReplaceAtPos(map, dest);
            if (thingToRemove != null)
            {
                thingToRemove.Destroy(DestroyMode.Vanish);
            }

            // Hack to allow them to spawn on other minerals
            StaticMineral output = (StaticMineral)ThingMaker.MakeThing(this);
            GenSpawn.Spawn(output, dest, map, WipeMode.Vanish);
            output.size = size;
            map.mapDrawer.MapMeshDirty(dest, MapMeshFlag.Buildings);
            return output;
        }

        // ======= Reproduction ======= //
        public virtual bool TryFindReproductionDestination(Map map, IntVec3 position, out IntVec3 foundCell)
        {
            if ((!position.InBounds(map)) || position.DistanceToEdge(map) <= Mathf.CeilToInt(spawnRadius))
            {
                foundCell = position;
                return false;
            }
            Predicate<IntVec3> validator = isValidSite(map, position);
            try
            {
                return CellFinder.TryFindRandomCellNear(position, map, Mathf.CeilToInt(spawnRadius), validator, out foundCell);
            }
            catch
            {
                Log.Warning("Minerals: TryFindReproductionDestination: exception caught tying to spawn near" + position);
                foundCell = position;
                return false;
            }

            Predicate<IntVec3> isValidSite(Map myMap, IntVec3 myPosition)
            {
                return c => c.DistanceTo(myPosition) <= spawnRadius && CanSpawnAt(myMap, c);
            }

            Predicate<IntVec3> isValidSiteDebug(Map myMap, IntVec3 myPosition)
            {
                return c =>
                {
                    Log.Message("TryFindReproductionDestination: isValidSiteDebug: c: " + c);
                    Log.Message("TryFindReproductionDestination: isValidSiteDebug: c.DistanceTo(myPosition): " + c.DistanceTo(myPosition));
                    Log.Message("TryFindReproductionDestination: isValidSiteDebug: CanSpawnAt(myMap, c): " + CanSpawnAt(myMap, c));
                    return c.DistanceTo(myPosition) <= spawnRadius && CanSpawnAt(myMap, c);
                };
            }
        }


        public virtual bool isRoofConditionOk(Map map, IntVec3 position)
        {
            if (mustBeUnderThickRoof && (map.roofGrid.RoofAt(position) == null || (!map.roofGrid.RoofAt(position).isThickRoof)))
            {
                return false;
            }

            if (mustBeNotUnderThickRoof && (map.roofGrid.RoofAt(position) != null && map.roofGrid.RoofAt(position).isThickRoof))
            {
                return false;
            }

            if (mustBeUnderRoof && (!map.roofGrid.Roofed(position)))
            {
                return false;
            }

            if (mustBeUnroofed && map.roofGrid.Roofed(position))
            {
                return false;
            }

            return true;
        }

        // ======= Map initialization ======= //
        public virtual void InitNewMap(Map map, float scaling = 1)
        {
            //Log.Message("Minerals: Initializing mineral '" + this.defName + "' with scaling of " + scaling);
            ReplaceThings(map, scaling);
            InitialSpawn(map, scaling);
        }

        public virtual float abundanceSettingFactor()
        {
            float factor = 1f;
            if (tags == null || tags.Count <= 0)
            {
                return factor;
            }
            if (tags.Contains("crystal"))
            {
                factor = factor * MineralsMod.Settings.crystalAbundanceSetting;
            }
            if (tags.Contains("boulder"))
            {
                factor = factor * MineralsMod.Settings.boulderAbundanceSetting;
            }
            if (tags.Contains("small_rock"))
            {
                factor = factor * MineralsMod.Settings.rocksAbundanceSetting;
            }
            if (tags.Contains("wall") && MineralsMod.Settings.replaceWallsSetting == false)
            {
                factor = 0f;
            }
            if (tags.Contains("fictional") && MineralsMod.Settings.includeFictionalSetting == false)
            {
                factor = 0f;
            }
            return factor;
        }

        public virtual float diversitySettingFactor()
        {
            float factor = 1f;
            if (tags == null || tags.Count <= 0)
            {
                return factor;
            }
            if (tags.Contains("crystal"))
            {
                factor = factor * MineralsMod.Settings.crystalDiversitySetting;
            }
            return factor;
        }

        // The probablility of spawning at each point when a map is created
        public virtual float mapSpawnProbFactor(Map map)
        {
            return tileSpawnProbFactor(map.Tile);
        }

        // The probablility of spawning at each point when a map is created
        public virtual float tileSpawnProbFactor(int tile)
        {
            float output = 1f;

            // Base value determined by world tile location
            Rand.PushState();
            Rand.Seed = tile.GetHashCode();
            output = output * Rand.Range(minClusterProbability, maxClusterProbability);
            Rand.PopState();

            // Apply distance to settlements factor
            output *= settlementDistProbFactor(tile);

            // Apply habitability factor if it is a valuable mineral
            if (otherSettlementMiningRadius > 3f)
            {
                output *= tileHabitabilitySpawnFactor(tile);
            }

            return output;
        }


        // How spawning is effected by the habitability of the world location
        public virtual float tileHabitabilitySpawnFactor(int tile)
        {
            float output = 0.5f;

            // Value determined by mean world tile temperature
            float temp = Find.World.grid.tiles[tile].temperature;
            float diffFromIdeal = Math.Abs(temp - 15f);
            if (diffFromIdeal > 10f)
            {
                output += (diffFromIdeal - 10f) / 20f;
            }

            // Apply biome effects
            if (Find.World.grid.tiles[tile].biome.isExtremeBiome)
            {
                output += 0.5f;
            }

            // Never more than triple spawn rate
            if (output > 3)
            {
                output = 3f;
            }

            //Log.Message("Minerals: tileHabitabilitySpawnFactor: " + defName + ": " + output);
            return output;
        }

        // How much the probablility of spawning reduces based on distance to nearest settlement 
        public virtual float settlementDistProbFactor(int tile)
        {
            float output = 1f;
            if (otherSettlementMiningRadius > 0)
            {
                foreach (Settlement s in Find.WorldObjects.Settlements)
                {
                    float travelDist = Find.World.grid.TraversalDistanceBetween(tile, s.Tile, false, (int)otherSettlementMiningRadius * 2);
                    if ((!s.Faction.IsPlayer) && travelDist < otherSettlementMiningRadius)
                    {
                        //Log.Message("Minerals: settlementDistProbFactor: " + defName + ": travelDist / otherSettlementMiningRadius:" + travelDist / otherSettlementMiningRadius);
                        //Log.Message("Minerals: settlementDistProbFactor: " + defName + ": travelDist:" + travelDist);
                        output *= travelDist / otherSettlementMiningRadius;
                    }
                }

            }

            //Log.Message("Minerals: settlementDistProbFactor: " + defName + ": " + output);
            return output;
        }

        public virtual void SpawnInitialCluster(Map map, IntVec3 position, float size, int count)
        {
            SpawnCluster(map, position, size, count);
        }


        public virtual void InitialSpawn(Map map, float abundScaling = 1f, float sizeScaling = 1f)
        {

            // Check that it is a valid biome
            if (!CanSpawnInBiome(map))
            {
                //Log.Message("Minerals: " + defName + " cannot be added to this biome");
                return;
            }

            // Select probability of spawing for this map
            float spawnProbability = mapSpawnProbFactor(map) * abundScaling * abundanceSettingFactor();

            // Inferr size scaling factor based on abundance
            if (sizeScaledByAbundance)
            {
                sizeScaling *= spawnProbability / maxClusterProbability;
            }
            if (sizeScaling < 0.2f)
            {
                sizeScaling = 0.2f;
            }
            if (sizeScaling > 1.2f)
            {
                sizeScaling = 1.2f;
            }

            // Find spots to spawn it
            if (Rand.Range(0f, 1f) <= perMapProbability * diversitySettingFactor() && spawnProbability > 0)
            {
                //Log.Message("Minerals: " + defName + " will be spawned at a probability of " + spawnProbability);
                IEnumerable<IntVec3> allCells = map.AllCells.InRandomOrder(null);
                foreach (IntVec3 current in allCells)
                {
                    if (!current.InBounds(map))
                    {
                        continue;
                    }

                    // Randomly spawn some clusters
                    if (Rand.Range(0f, 1f) < spawnProbability && CanSpawnAt(map, current, true))
                    {
                        SpawnInitialCluster(map, current, Rand.Range(initialSizeMin, initialSizeMax) * sizeScaling, Rand.Range(minClusterSize, maxClusterSize));
                    }

                    // Spawn near their assocaited ore
                    if (PosIsAssociatedOre(map, current))
                    {

                        if (Rand.Range(0f, 1f) < spawnProbability * nearAssociatedOreBonus)
                        {

                            if (CanSpawnAt(map, current, true))
                            {
                                SpawnCluster(map, current, Rand.Range(initialSizeMin, initialSizeMax) * sizeScaling, Rand.Range(minClusterSize, maxClusterSize));
                            }
                            else
                            {
                                IntVec3 dest;
                                if (current.InBounds(map) && TryFindReproductionDestination(map, current, out dest))
                                {
                                    TrySpawnCluster(map, dest, Rand.Range(initialSizeMin, initialSizeMax) * sizeScaling, Rand.Range(minClusterSize, maxClusterSize));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //Log.Message("Minerals: " + this.defName + " will not be spawned in this map.");
            }

        }

        public virtual bool allowReplaceSetting()
        {
            bool output = true;
            if (replaceAll == false)
            {
                output = false;
            }
            if (tags.Contains("wall") && MineralsMod.Settings.replaceWallsSetting == false)
            {
                output = false;
            }
            if (tags.Contains("chunk_replacer") && MineralsMod.Settings.replaceChunksSetting == false)
            {
                output = false;
            }
            return output;
        }


        public virtual void ReplaceThings(Map map, float scaling = 1)
        {
            if (ThingsToReplace == null || ThingsToReplace.Count == 0 || allowReplaceSetting() == false)
            {
                return;
            }


            // Find spots to spawn it
            map.regionAndRoomUpdater.Enabled = false;
            IEnumerable<IntVec3> allCells = map.AllCells.InRandomOrder(null);
            foreach (IntVec3 current in allCells)
            {
                if (!current.InBounds(map))
                {
                    continue;
                }

                // roof filters
                if (!isRoofConditionOk(map, current))
                {
                    continue;
                }

                Thing ToReplace = ThingToReplaceAtPos(map, current);
                if (ToReplace != null)
                {

                    ToReplace.Destroy(DestroyMode.Vanish);
                    StaticMineral spawned = SpawnAt(map, current, Rand.Range(initialSizeMin, initialSizeMax));
                    map.edificeGrid.Register(spawned);
                }
            }
            map.regionAndRoomUpdater.Enabled = true;

        }

        public virtual List<string> getTexturePaths()
        {
            if (texturePaths == null)
            {
                initTexturePaths();
            }
            return texturePaths;
        }

        public virtual void initTexturePaths()
        {
            // Get paths to textures
            string textureName = System.IO.Path.GetFileName(graphicData.texPath);
            texturePaths = new List<string> { };
            snowTexturePaths = new List<string> { };
            List<string> versions = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
            foreach (string letter in versions)
            {
                string a_path = graphicData.texPath + "/" + textureName + letter;
                if (ContentFinder<Texture2D>.Get(a_path, false) != null)
                {
                    texturePaths.Add(a_path);
                    string snow_path = a_path + "_s";
                    if (ContentFinder<Texture2D>.Get(snow_path, false) != null)
                    {
                        hasSnowyTextures = true;
                        snowTexturePaths.Add(snow_path);
                    }
                }
            }

            // Check that there are enough snowy textures
            if (texturePaths.Count > 0 && snowTexturePaths.Count > 0 && texturePaths.Count != snowTexturePaths.Count)
            {
                Log.Warning("Minerals: Not an equal number of snowy and non-snowy textures for '" + graphicData.texPath + "'");
                hasSnowyTextures = false;
            }

        }

    }
}
