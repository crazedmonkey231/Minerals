using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Minerals.Core
{
    public class BigMineral : StaticMineral
    {

        public float propExtracted = 0f;

        public new ThingDef_BigMineral attributes
        {
            get
            {
                return base.attributes as ThingDef_BigMineral;
            }
        }

        public override void incPctYeild(float amount, Pawn miner)
        {
            propExtracted += (float)Mathf.Min(amount, HitPoints) / (float)MaxHitPoints * miner.GetStatValue(StatDefOf.MiningYield, true) + (miner.GetStatValue(StatDefOf.MiningYield, true) - attributes.extractionDifficulty) * 0.2f + Rand.Range(-0.2f, 0.2f);
            if (propExtracted >= 1f)
            {
                propExtracted = 0f;
                ThingDef myThingDef = DefDatabase<ThingDef>.GetNamed(attributes.defName + "Trophy", true);
                if (myThingDef != null)
                {
                    BigMineralTrophy thing = (BigMineralTrophy)ThingMaker.MakeThing(myThingDef, null);
                    Thing miniThing = thing.MakeMinified();
                    GenPlace.TryPlaceThing(miniThing, Position, Map, ThingPlaceMode.Near, null);
                    Messages.Message("Rare mineral trophy extracted!".CapitalizeFirst(), MessageTypeDefOf.NeutralEvent, true);
                    yieldPct = 0.1f;
                }
            }
            else
            {
                base.incPctYeild(amount, miner);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (DebugSettings.godMode)
            {
                stringBuilder.AppendLine("Size: " + size.ToStringPercent());
                stringBuilder.AppendLine("Extraction progress: " + propExtracted.ToStringPercent());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

    }
}
