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

    public class RainGrowthRateModifier : GrowthRateModifier
    {
        private float rainfallToRain(float rainfall)
        {
            float rainProxy = rainfall / 1000f;
            if (rainProxy > 3f)
            {
                rainProxy = 3f;
            }
            return rainProxy;
        }

        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Map.weatherManager.curWeather.rainRate;
        }
        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aMap.weatherManager.curWeather.rainRate;
        }

        public override float valueAtMap(Map aMap)
        {
            return aMap.weatherManager.curWeather.rainRate;
        }

        public override float valueAtTile(World world, int worldTile)
        {
            return rainfallToRain(world.grid.tiles[worldTile].rainfall);
        }

        public override float valueAtMapMean(Map aMap)
        {
            return rainfallToRain(aMap.TileInfo.rainfall);
        }

        public override float valueAtMapSeasonal(Map aMap)
        {
            return (growthRateFactor(valueAtMapMean(aMap) * 0.5f) + growthRateFactor(valueAtMapMean(aMap) * 1.5f)) / 2f;
        }

        public override float growthRateFactorMapMean(Map aMap)
        {
            return (growthRateFactor(valueAtMapMean(aMap) * 0.5f) + growthRateFactor(valueAtMapMean(aMap) * 1.5f) + growthRateFactor(valueAtMapMean(aMap))) / 3f;
        }

    }

}
