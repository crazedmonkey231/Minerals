using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.Core
{

    public class ThingDef_BigMineral : ThingDef_StaticMineral
    {
        // The radius that will be searched to replace things
        public int replaceRadius = 1;
        // The minmum propotion of things in radius to replace for a replacement to happen 
        public float repalceThreshold = 0.3f;
        // How likly an extraction is to be successful 
        public float extractionDifficulty = 0.9f;

        public override Thing ThingToReplaceAtPos(Map map, IntVec3 position)
        {
            Thing toReplace = base.ThingToReplaceAtPos(map, position);
            if (toReplace == null)
            {
                return (null);
            }

            int spotsChecked = 0;
            float replaceCount = 0;
            for (int xOffset = -replaceRadius; xOffset <= replaceRadius; xOffset++)
            {
                for (int zOffset = -replaceRadius; zOffset <= replaceRadius; zOffset++)
                {
                    spotsChecked = spotsChecked + 1;
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        foreach (Thing thing in map.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if (thing == null || thing.def == null)
                            {
                                continue;
                            }

                            if (ThingsToReplace.Any(thing.def.defName.Equals))
                            {
                                if (StaticMineral.isMineral(thing))
                                {
                                    replaceCount += ((StaticMineral)thing).size;
                                }
                                else
                                {
                                    replaceCount += 1;
                                }
                            }
                        }
                    }
                }
            }
            if (((float)replaceCount) / ((float)spotsChecked) > repalceThreshold)
            {
                return (toReplace);
            }
            else
            {
                return (null);
            }
        }
    }
}
