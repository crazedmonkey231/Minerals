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

    public abstract class GrowthRateModifier
    {
        public float aboveMaxDecayRate;  // How quickly it decays when above maxStableFert
        public float maxStable; // Will decay above this level
        public float maxGrow; // Will not grow above this level
        public float maxIdeal; // Grows fastest at this level
        public float minIdeal; // Grows fastest at this level
        public float minGrow; // Will not grow below this level
        public float minStable; // Will decay below this fertility level
        public float belowMinDecayRate;  // How quickly it decays when below minStableFert
        public bool wholeMapEffect = false; // If a whole-map attribute can be used instead of a per-position attribute (faster)

        public abstract float valueAtPos(DynamicMineral aMineral);
        public abstract float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap);
        public abstract float valueAtMap(Map aMap);
        public abstract float valueAtMapSeasonal(Map aMap);
        public abstract float valueAtMapMean(Map aMap);
        public abstract float valueAtTile(World world, int worldTile);

        public virtual float growthRateFactor(float myValue)
        {
            // decays if too high or low
            float stableRangeSize = maxStable - minStable;
            if (myValue > maxStable)
            {
                return -aboveMaxDecayRate * (myValue - maxStable) / stableRangeSize;
            }
            if (myValue < minStable)
            {
                return -belowMinDecayRate * (minStable - myValue) / stableRangeSize;
            }

            // does not grow if too high or low
            if (myValue < minGrow || myValue > maxGrow)
            {
                return 0f;
            }

            // slowed growth if too high or low
            if (myValue < minIdeal)
            {
                return 1f - ((minIdeal - myValue) / (minIdeal - minGrow));
            }
            if (myValue > maxIdeal)
            {
                return 1f - ((myValue - maxIdeal) / (maxGrow - maxIdeal));
            }

            return 1f;
        }

        public virtual float growthRateFactorAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return growthRateFactor(valueAtPos(myDef, aPosition, aMap));
        }

        public virtual float growthRateFactorAtPos(DynamicMineral aMineral)
        {
            return growthRateFactor(valueAtPos(aMineral));
        }

        public virtual float growthRateFactorAtMap(Map aMap)
        {
            return growthRateFactor(valueAtMap(aMap));
        }

        public virtual float growthRateFactorMapMean(Map aMap)
        {
            return growthRateFactor(valueAtMapMean(aMap));
        }

        public virtual float growthRateFactorMapSeason(Map aMap)
        {
            return growthRateFactor(valueAtMapSeasonal(aMap));
        }

        public virtual float growthRateFactorMapRecent(ThingDef_DynamicMineral myDef, Map aMap)
        {
            float mapMean = growthRateFactorMapMean(aMap);
            float mapSeason = growthRateFactorMapSeason(aMap);
            float meanWeight = (myDef.growDays * 2f) / 60f;
            if (meanWeight > 1f)
            {
                meanWeight = 1f;
            }
            if (meanWeight < 0f)
            {
                meanWeight = 0f;
            }
            return mapMean * meanWeight + mapSeason * (1 - meanWeight);
        }

    }

}
