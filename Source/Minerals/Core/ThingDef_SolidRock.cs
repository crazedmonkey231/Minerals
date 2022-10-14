using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.Core
{
    public class ThingDef_SolidRock : ThingDef_StaticMineral
    {

        public override bool isRoofConditionOk(Map map, IntVec3 position)
        {
            return base.isRoofConditionOk(map, position) || (map.roofGrid.Roofed(position) && IsNearPassable(map, position));
        }


        public bool IsNearPassable(Map map, IntVec3 position, int radius = 1)
        {
            for (int xOffset = -radius; xOffset <= radius; xOffset++)
            {
                for (int zOffset = -radius; zOffset <= radius; zOffset++)
                {
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        if (!checkedPosition.Impassable(map))
                        {
                            return true;
                        }

                    }
                }
            }
            return false;
        }
    }
}
