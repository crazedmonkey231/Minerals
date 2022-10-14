using Minerals.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Minerals.MineralComponents
{

    public class DynamicMineralWatcher : MapComponent
    {

        public static int ticksPerLook = 1000; // 100 is about once a second on 1x speed
        public int tick_counter = 1;

        public DynamicMineralWatcher(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            // Run each class' watcher
            tick_counter += 1;
            if (tick_counter > ticksPerLook)
            {
                tick_counter = 1;
                Look();
            }
        }

        // The main function controlling what is done each time the map is looked at
        public void Look()
        {
            SpawnDynamicMinerals();
        }


        public void SpawnDynamicMinerals()
        {
            foreach (ThingDef_DynamicMineral mineralType in DefDatabase<ThingDef_DynamicMineral>.AllDefs)
            {
                // Check that the map type is ok
                if (!mineralType.CanSpawnInBiome(map))
                {
                    continue;
                }
                //Log.Message("   Biome OK");

                // Get number of positions to check
                float perMapGrowthFactor = mineralType.GrowthRateAtMap(map);
                float numToCheck = map.Area * mineralType.spawnProb * perMapGrowthFactor * MineralsMod.Settings.mineralSpawningSetting;
                if (numToCheck <= 0)
                {
                    continue;
                }

                // If less than one cell should be checked, randomly decide to check one or none
                if (numToCheck < 1)
                {
                    if (Rand.Range(0f, 1f) < numToCheck)
                    {
                        numToCheck = 1;
                    }
                    else
                    {
                        continue;
                    }
                }

                // Never check more than 1/10 of the map (performance failsafe)
                if (numToCheck > map.Area / 10)
                {
                    numToCheck = map.Area / 10;
                }

                // Round to integer
                numToCheck = (float)Math.Round(numToCheck);

                // Try to spawn in a subset of positions
                for (int i = 0; i < numToCheck; i++)
                {
                    // Pick a random location
                    IntVec3 aPos = CellIndicesUtility.IndexToCell(Rand.RangeInclusive(0, map.Area - 1), map.Size.x);

                    // Dont always spawn if growth rate is not good
                    if (Rand.Range(0f, 1f) > mineralType.GrowthRateAtPos(map, aPos, false))
                    {
                        continue;
                    }
                    mineralType.TrySpawnCluster(map, aPos, Rand.Range(0.01f, 0.05f), Rand.Range(mineralType.minSpawnClusterSize, mineralType.maxSpawnClusterSize));

                }
            }
        }
    }
}
