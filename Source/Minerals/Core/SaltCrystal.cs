using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using Minerals.Core;

namespace Minerals
{

    public class SaltCrystal : DynamicMineral
    {

        public override float GrowthRate
        {
            get
            {
                return ThingDef_SaltCrystal.calcGrowthRate(base.GrowthRate, ThingDef_SaltCrystal.GrowthRateBonus(Position, Map));
            }
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
            if (ThingDef_SaltCrystal.IsInWater(this.Position, this.Map)) // melts in water
            {
                stringBuilder.AppendLine("Dissolving in water.");
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
     }       
}
