
using Minerals.Utilities;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Minerals.Core
{

    public class DynamicMineral : StaticMineral
    {
        // Controls how often occasional checks are done, like distance to nearby things
        private int tickCounter = Rand.Range(0, 1000);

        public new ThingDef_DynamicMineral attributes
        {
            get
            {
                return base.attributes as ThingDef_DynamicMineral;
            }
        }


        public override float distFromNeededTerrain
        {
            get
            {
                int ticksPerUpdate = 20;
                if (myDistFromNeededTerrain == null || tickCounter % ticksPerUpdate == 0) // not yet set
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



        public virtual float GrowthRate
        {
            get
            {
                float output = 1f; // If there are no growth rate factors, grow at full speed

                // Get growth rate factors
                List<float> rateFactors = allGrowthRateFactors;
                List<float> positiveFactors = rateFactors.FindAll(fac => fac >= 0);
                List<float> negativeFactors = rateFactors.FindAll(fac => fac < 0);

                // if any factors are negative, add them together and ignore positive factors
                if (negativeFactors.Count > 0)
                {
                    output = negativeFactors.Sum();
                }
                else if (positiveFactors.Count > 0) // if all positive, multiply them
                {
                    output = positiveFactors.Aggregate(1f, (acc, val) => acc * val);
                }


                return output * MineralsMod.Settings.mineralGrowthSetting;
            }
        }



        public float GrowthPerTick
        {
            get
            {
                float growthPerTick = (1f / (GenDate.TicksPerDay * attributes.growDays));
                return growthPerTick * GrowthRate;
            }
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Size: " + size.ToStringPercent());
            stringBuilder.AppendLine("Growth rate: " + GrowthRate.ToStringPercent());
            float propSubmerged = 1 - submersibleFactor();
            if (propSubmerged > 0)
            {
                stringBuilder.AppendLine("Submerged: " + propSubmerged.ToStringPercent());
            }
            if (DebugSettings.godMode)
            {
                foreach (GrowthRateModifier mod in attributes.allRateModifiers)
                {
                    stringBuilder.AppendLine(mod.GetType().Name + ": " + mod.growthRateFactorAtPos(this));
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }


        public override void TickLong()
        {
            // Half the time, dont do anything
            if (Rand.Bool)
            {
                return;
            }

            // Try to grow
            float GrowthThisTick = GrowthPerTick;
            size += GrowthThisTick * 4000; // 1 long tick = 2000

            // Try to reproduce
            if (GrowthThisTick > 0 && size > attributes.minReproductionSize && Rand.Range(0f, 1f) < attributes.reproduceProp * GrowthRate * MineralsMod.Settings.mineralReproductionSetting)
            {
                attributes.TryReproduce(Map, Position);
            }

            // Refresh appearance if apparent size has changed
            float apparentSize = printSize();
            if (attributes.fastGraphicRefresh && Math.Abs(sizeWhenLastPrinted - apparentSize) > 0.2f)
            {
                sizeWhenLastPrinted = apparentSize;
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
            }

            // Count ticks for occasional updates, like dist to nearby terrain 
            tickCounter += 1;

            // Try to die
            if (size <= 0 && Rand.Range(0f, 1f) < attributes.deathProb)
            {
                Destroy(DestroyMode.Vanish);
            }

        }
            
        public List<float> allGrowthRateFactors 
        {
            get
            {
                return attributes.allRateModifiers.Select(mod => mod.growthRateFactorAtPos(this)).ToList();
            }
        }


    }
}



