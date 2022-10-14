using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using Minerals.Core;

namespace Minerals.Core
{

	public class SolidRock : StaticMineral
	{
        public new ThingDef_SolidRock attributes
        {
            get
            {
                return base.attributes as ThingDef_SolidRock;
            }
        }
	}      
}
