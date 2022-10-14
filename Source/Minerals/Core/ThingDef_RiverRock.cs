using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.Core
{
    public class ThingDef_RiverRock : ThingDef_DynamicMineral
    {

        public override float GrowthRateAtPos(Map aMap, IntVec3 aPosition, bool includePerMapEffects = true)
        {
            TerrainDef myTerrain = aMap.terrainGrid.TerrainAt(aPosition);
            if (myTerrain.defName.Contains("Water") || myTerrain.defName.Contains("water"))
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
    }
}
