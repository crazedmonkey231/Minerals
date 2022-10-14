using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.Core
{

    public class ThingDef_SaltCrystal : ThingDef_DynamicMineral
    {

        public static bool IsInWater(IntVec3 aPosition, Map aMap)
        {
            TerrainDef terrain = aMap.terrainGrid.TerrainAt(aPosition);
            return terrain.defName.Contains("Water") || terrain.defName.Contains("water");
        }

        public static float GrowthRateBonus(IntVec3 aPosition, Map aMap)
        {
            TerrainDef terrain = aMap.terrainGrid.TerrainAt(aPosition);
            if (IsInWater(aPosition, aMap)) // Grows faster on wet sand
            {
                return -3f;
            }
            else if (terrain.defName == "SandBeachWetSalt") // melts in water
            {
                return 3f;
            }
            return 1f;
        }

        public static float calcGrowthRate(float baseRate, float modifier)
        {
            if (baseRate <= 0)
            {
                if (modifier < 0)
                {
                    return baseRate + modifier;

                }
                else
                {
                    return baseRate;
                }
            }
            else // growing
            {
                if (modifier < 0)
                {
                    return modifier;
                }
                else
                {
                    return baseRate * modifier;
                }
            }
        }

        public override float GrowthRateAtPos(Map aMap, IntVec3 aPosition, bool includePerMapEffects = true)
        {
            return calcGrowthRate(base.GrowthRateAtPos(aMap, aPosition), ThingDef_SaltCrystal.GrowthRateBonus(aPosition, aMap));
        }
    }
}
