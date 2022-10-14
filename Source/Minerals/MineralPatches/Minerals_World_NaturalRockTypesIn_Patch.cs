using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.MineralPatches
{
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("NaturalRockTypesIn")]
    public static class Minerals_World_NaturalRockTypesIn_Patch
    {

        [HarmonyPostfix]
        public static void MakeRocksAccordingToBiome(int tile, ref World __instance, ref IEnumerable<ThingDef> __result)
        {
            if (__instance.grid.tiles[tile].biome.defName == "AB_PyroclasticConflagration")
            {
                List<ThingDef> replacedList = new List<ThingDef>();
                ThingDef item = DefDatabase<ThingDef>.GetNamed("AB_Obsidianstone");
                replacedList.Add(item);
                replacedList.Add(DefDatabase<ThingDef>.GetNamed("ZF_BasaltBase"));

                __result = replacedList;
            }
            else if (__instance.grid.tiles[tile].biome.defName == "AB_OcularForest" || __instance.grid.tiles[tile].biome.defName == "AB_GallatrossGraveyard" || __instance.grid.tiles[tile].biome.defName == "AB_GelatinousSuperorganism" || __instance.grid.tiles[tile].biome.defName == "AB_MechanoidIntrusion" || __instance.grid.tiles[tile].biome.defName == "AB_RockyCrags")
            {
                return;
            }
            else
            {
                // Pick a set of random rocks
                Rand.PushState();
                Rand.Seed = tile;
                List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
                                       where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock &&
                                       !d.IsSmoothed && d.defName != "GU_RoseQuartz" && d.defName != "AB_SlimeStone" &&
                                       d.defName != "GU_AncientMetals" && d.defName != "AB_Cragstone" && d.defName != "AB_Obsidianstone" &&
                                       d.defName != "BiomesIslands_CoralRock" && d.defName != "LavaRock" && d.defName != "AB_Mudstone"
                                       select d).ToList<ThingDef>();
                int num = Rand.RangeInclusive(MineralsMod.Settings.terrainCountRangeSetting.min, MineralsMod.Settings.terrainCountRangeSetting.max);
                if (num > list.Count)
                {
                    num = list.Count;
                }
                List<ThingDef> list2 = new List<ThingDef>();
                for (int i = 0; i < num; i++)
                {
                    ThingDef item = list.RandomElement<ThingDef>();
                    list.Remove(item);
                    list2.Add(item);
                }
                Rand.PopState();
                __result = list2;
            }
        }
    }
}
