﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace Minerals
{
	/// <summary>
	/// SolidRock class
	/// </summary>
	/// <author>zachary-foster</author>
	/// <permission>No restrictions</permission>
	public class SolidRock : StaticMineral
	{


	}       


	/// <summary>
	/// ThingDef_StaticMineral class.
	/// </summary>
	/// <author>zachary-foster</author>
	/// <permission>No restrictions</permission>
	public class ThingDef_SolidRock : ThingDef_StaticMineral
	{
		public List<string> ThingsToReplace; 

		public override void InitNewMap(Map map, float scaling = 1)
		{
			// Print to log
			Log.Message("Minerals: " + defName + " will replace roofed " + ThingsToReplace + ".");

			// Find spots to spawn it
			IEnumerable<IntVec3> allCells = map.AllCells.InRandomOrder(null);
			foreach (IntVec3 current in allCells)
			{
				if (!current.InBounds(map))
				{
					continue;
				}

				if (! current.Roofed(map))
				{
					continue;
				}

				// Replace unroofed rock
				foreach (Thing thing in map.thingGrid.ThingsListAt(current))
				{
					if (thing == null || thing.def == null)
					{
						continue;
					}

					if (ThingsToReplace.Any(thing.def.defName.Equals))
					{
						thing.Destroy();
						SpawnAt(map, current);
					}
				}
			}

			// Call parent function for standard spawning
			base.InitNewMap(map, scaling);
		}

	}

}