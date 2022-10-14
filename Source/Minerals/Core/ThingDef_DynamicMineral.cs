using Minerals.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.Core
{

    public class ThingDef_DynamicMineral : ThingDef_StaticMineral
    {
        // The number of days it takes to grow at max growth speed
        public float growDays = 100f;


        public float minReproductionSize = 0.8f;
        public float reproduceProp = 0.001f;
        public float deathProb = 0.001f;
        public float spawnProb = 0.0001f; // chance of spawning de novo each tick
        public TemperatureGrowthRateModifier tempGrowthRateModifer;  // Temperature effects on growth rate
        public RainGrowthRateModifier rainGrowthRateModifer;  // Rain effects on growth rate
        public LightGrowthRateModifier lightGrowthRateModifer; // Light effects on growth rate
        public FertilityGrowthRateModifier fertGrowthRateModifer;  // Fertility effects on growth rate
        public DistanceGrowthRateModifier distGrowthRateModifer;  // Distance to needed terrain effects on growth rate
        public SizeGrowthRateModifier sizeGrowthRateModifer;  // Current size effects on growth rate
        public bool fastGraphicRefresh = false; // If true, the graphics are regenerated more often
        public int minSpawnClusterSize = 1; // The minimum number of crystals in clusters that are spawned during gameplay, not map creation
        public int maxSpawnClusterSize = 1; // The maximum number of crystals in clusters that are spawned during gameplay, not map creation


        public List<GrowthRateModifier> allRateModifiers
        {
            get
            {
                List<GrowthRateModifier> output = new List<GrowthRateModifier>{
                    tempGrowthRateModifer,
                    rainGrowthRateModifer,
                    lightGrowthRateModifer,
                    fertGrowthRateModifer,
                    distGrowthRateModifer,
                    sizeGrowthRateModifer
                };
                output.RemoveAll(item => item == null);
                return output;
            }
        }

        public List<GrowthRateModifier> mapRateModifiers
        {
            get
            {
                List<GrowthRateModifier> output = new List<GrowthRateModifier>{
                    tempGrowthRateModifer,
                    rainGrowthRateModifer,
                    lightGrowthRateModifer,
                    fertGrowthRateModifer,
                    distGrowthRateModifer,
                    sizeGrowthRateModifer
                };
                output.RemoveAll(item => item == null || (!item.wholeMapEffect));
                return output;
            }
        }

        public List<GrowthRateModifier> posRateModifiers
        {
            get
            {
                List<GrowthRateModifier> output = new List<GrowthRateModifier>{
                    tempGrowthRateModifer,
                    rainGrowthRateModifer,
                    lightGrowthRateModifer,
                    fertGrowthRateModifer,
                    distGrowthRateModifer,
                    sizeGrowthRateModifer
                };
                output.RemoveAll(item => item == null || item.wholeMapEffect);
                return output;
            }
        }


        public override void InitNewMap(Map map, float scaling = 1)
        {
            scaling = scaling * GrowthRateMapRecent(map);
            base.InitNewMap(map, scaling);
        }


        // ======= Growth rate factors ======= //
        public virtual float combineGrowthRateFactors(List<float> rateFactors)
        {
            List<float> positiveFactors = rateFactors.FindAll(fac => fac >= 0);
            List<float> negativeFactors = rateFactors.FindAll(fac => fac < 0);

            // if any factors are negative, add them together and ignore positive factors
            if (negativeFactors.Count > 0)
            {
                return negativeFactors.Sum();
            }

            // if all positive, multiply them
            if (positiveFactors.Count > 0)
            {
                return positiveFactors.Aggregate(1f, (acc, val) => acc * val);
            }

            // If there are no growth rate factors, grow at full speed
            return 1f;
        }

        public virtual List<float> allGrowthRateFactorsAtPos(IntVec3 aPosition, Map aMap, bool includePerMapEffects = true)
        {
            if (includePerMapEffects)
            {
                return allRateModifiers.Select(mod => mod.growthRateFactorAtPos(this, aPosition, aMap)).ToList();
            }
            else
            {
                return posRateModifiers.Select(mod => mod.growthRateFactorAtPos(this, aPosition, aMap)).ToList();
            }
        }

        public virtual List<float> allGrowthRateFactorsAtMap(Map aMap)
        {
            return mapRateModifiers.Select(mod => mod.growthRateFactorAtMap(aMap)).ToList();
        }

        public virtual List<float> allGrowthRateFactorsAtMapMean(Map aMap)
        {
            return allRateModifiers.Select(mod => mod.growthRateFactorMapMean(aMap)).ToList();
        }
        public virtual List<float> allGrowthRateFactorsMapRecent(Map aMap)
        {
            return allRateModifiers.Select(mod => mod.growthRateFactorMapRecent(this, aMap)).ToList();
        }

        //Growth rate for a given position at the current time
        public virtual float GrowthRateAtPos(Map aMap, IntVec3 aPosition, bool includePerMapEffects = true)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsAtPos(aPosition, aMap, includePerMapEffects));
        }

        //Growth rate for the map at the current  time
        public virtual float GrowthRateAtMap(Map aMap)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsAtMap(aMap));
        }

        //Growth rate for the map on average
        public virtual float GrowthRateMapMean(Map aMap)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsAtMapMean(aMap));
        }

        public virtual float GrowthRateMapRecent(Map aMap)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsMapRecent(aMap));
        }

        public override float tileHabitabilitySpawnFactor(int tile)
        {
            return 1f;
        }

        public override void SpawnInitialCluster(Map map, IntVec3 position, float size, int count)
        {
            base.SpawnInitialCluster(map, position, size, count);
        }

    }

}
