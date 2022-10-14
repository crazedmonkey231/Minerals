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

    public class SizeGrowthRateModifier : GrowthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.size;
        }

        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return 0.01f;
        }

        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("sizeGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return 0.5f;
        }
        public override float valueAtMapMean(Map aMap)
        {
            return 0.5f;
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
