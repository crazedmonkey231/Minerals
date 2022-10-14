using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Minerals.Core;

namespace Minerals.Utilities
{

    public class LightGrowthRateModifier : GrowthRateModifier
    {
        public float lightByBiome(BiomeDef biome)
        {
            if (biome.defName == "AB_RockyCrags")
            {
                return 0f;
            }
            else
            {
                return 1f;
            }

        }
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Map.glowGrid.GameGlowAt(aMineral.Position);
        }
        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aMap.glowGrid.GameGlowAt(aPosition);
        }
        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("lightGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return lightByBiome(world.grid.tiles[worldTile].biome);
        }
        public override float valueAtMapMean(Map aMap)
        {
            return lightByBiome(aMap.Biome);
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return lightByBiome(aMap.Biome);
        }
    }
}
