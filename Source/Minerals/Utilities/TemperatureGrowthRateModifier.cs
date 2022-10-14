using Minerals.Core;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.Utilities
{
    public class TemperatureGrowthRateModifier : GrowthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Position.GetTemperature(aMineral.Map);
        }

        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aPosition.GetTemperature(aMap);
        }

        public override float valueAtMap(Map aMap)
        {
            return aMap.mapTemperature.OutdoorTemp;
        }

        public override float valueAtTile(World world, int worldTile)
        {
            return world.tileTemperatures.GetOutdoorTemp(worldTile);
        }

        public override float valueAtMapMean(Map aMap)
        {
            return aMap.TileInfo.temperature;
        }

        public override float valueAtMapSeasonal(Map aMap)
        {
            return aMap.mapTemperature.SeasonalTemp;
        }

        public override float growthRateFactorMapMean(Map aMap)
        {
            return (growthRateFactor(valueAtMapMean(aMap) + 15f) + growthRateFactor(valueAtMapMean(aMap)) + growthRateFactor(valueAtMapMean(aMap) - 15f)) / 3f;
        }
    }
}
