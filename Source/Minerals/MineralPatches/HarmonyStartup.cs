using HarmonyLib;
using Minerals.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.MineralPatches
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("crazedmonkey231.minerals");

            // Spawn rocks on map generation
            MethodInfo targetmethod = AccessTools.Method(typeof(GenStep_RockChunks), "Generate");
            HarmonyMethod postfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("initNewMapRocks"));
            harmony.Patch(targetmethod, null, postfixmethod);

            // Spawn ice after plants
            MethodInfo icetargetmethod = AccessTools.Method(typeof(GenStep_Plants), "Generate");
            HarmonyMethod icepostfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("initNewMapIce"));
            harmony.Patch(icetargetmethod, null, icepostfixmethod);

            harmony.PatchAll();
        }

        public static void initNewMapRocks(GenStep_RockChunks __instance, Map map)
        {
            mapBuilder.initRocks(map);
        }

        public static void initNewMapIce(GenStep_RockChunks __instance, Map map)
        {
            mapBuilder.initIce(map);
        }

        [HarmonyPatch(typeof(SkyfallerMaker))]
        [HarmonyPatch("SpawnSkyfaller")]
        [HarmonyPatch(new Type[] { typeof(ThingDef), typeof(IEnumerable<Thing>), typeof(IntVec3), typeof(Map) })]
        static class ImpactPatch
        {
            [HarmonyPrefix]
            public static void Prefix(ref IEnumerable<Thing> things)
            {
                List<Thing> replacementList = new List<Thing>();
                foreach (Thing item in things)
                {
                    Thing toReturn = item;
                    if (item.def.mineable && (!StaticMineral.isMineral(item)))
                    {
                        // check if any of the minerals replace this one 
                        foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
                        {
                            if (mineralType.ThingsToReplace == null || mineralType.ThingsToReplace.Count == 0)
                            {
                                continue;
                            }

                            if (mineralType.ThingsToReplace.Any(item.def.defName.Equals))
                            {
                                toReturn = (StaticMineral)ThingMaker.MakeThing(mineralType);
                                break;
                            }
                        }

                    }
                    replacementList.Add(toReturn);
                }
                things = replacementList;
            }
        }

    }
}
