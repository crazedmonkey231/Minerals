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

    public class DistanceGrowthRateModifier : GrowthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.distFromNeededTerrain;
        }

        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return myDef.posDistFromNeededTerrain(aMap, aPosition);
        }
        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("distGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return 1f;
        }
        public override float valueAtMapMean(Map aMap)
        {
            return 1f;
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapMean(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapSeason(Map aMap)
        {
            return 1f;
        }
    }


}
