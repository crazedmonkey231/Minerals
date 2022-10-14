﻿using UnityEngine; 
using RimWorld;      
using Verse;         
using Minerals.Core;

namespace Minerals.Events
{

    public class IncidentWorker_BlueSnow : IncidentWorker_MakeGameCondition
    {


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            if (MineralsMod.Settings.includeFictionalSetting == false)
            {
                return false;
            }

            Map map = (Map)parms.target;

            if (map.mapTemperature.OutdoorTemp > 0f || map.mapTemperature.OutdoorTemp < -20 || map.weatherManager.SnowRate < 0.1f)
            {
                return false;
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Find.LetterStack.ReceiveLetter(this.def.letterLabel, def.letterText, LetterDefOf.NeutralEvent);
            //parms.customLetterDef = LetterDefOf.NeutralEvent;
            //parms.customLetterLabel = this.def.letterLabel;
            //parms.customLetterText = def.letterText;
            map.gameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(DefDatabase<GameConditionDef>.GetNamed("BlueSnowCondition")));
            //return base.TryExecuteWorker(parms);
            return true;
        }
    }


    public class GameCondition_BlueSnow : GameCondition
    {

        public ThingDef_DynamicMineral coldstoneDef = DefDatabase<ThingDef_DynamicMineral>.GetNamed("ColdstoneCrystal");
        public int ticksPerSpawn = 100;
        public int currentTick = 1;

        public override float SkyGazeChanceFactor(Map map)
        {
            return base.SkyGazeChanceFactor(map) * 2;
        }

        public override float SkyGazeJoyGainFactor(Map map)
        {
            return base.SkyGazeJoyGainFactor(map) * 2;
        }
            
        public override void GameConditionTick()
        {
            currentTick += 1;
            foreach (Map aMap in this.AffectedMaps)
            {
                if (aMap.weatherManager.curWeather.defName != "BlueSnow")
                {
                    int previousWeatherAge = aMap.weatherManager.curWeatherAge;
                    aMap.weatherManager.TransitionTo(DefDatabase<WeatherDef>.GetNamed("BlueSnow"));
                    if (previousWeatherAge < 4000)
                    {
                        aMap.weatherManager.curWeatherAge = 4000 - previousWeatherAge;
                    }
                }
                if (aMap.mapTemperature.OutdoorTemp > 5f || aMap.mapTemperature.OutdoorTemp < -40)
                {
                    this.End();
                }
                if (currentTick > ticksPerSpawn)
                {
                    IntVec3 spawnPos = CellFinder.RandomCell(aMap);
                    coldstoneDef.TrySpawnAt(spawnPos, aMap, 0.2f);
                    currentTick = 1;
                }
            }
        }
            
    }

    [StaticConstructorOnStartup]
    public class WeatherOverlay_BlueSnow : WeatherOverlay_SnowHard
    {

        static Material SnowOverlayWorld;

        public WeatherOverlay_BlueSnow()
        {
            this.worldOverlayMat = WeatherOverlay_BlueSnow.SnowOverlayWorld;
            this.worldOverlayPanSpeed1 = 0.005f;
            this.worldPanDir1 = new Vector2(-0.3f, -1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.006f;
            this.worldPanDir2 = new Vector2(-0.29f, -1f);
            this.worldPanDir2.Normalize();
            this.OverlayColor = new Color(0.3f,0.3f,1f);
        }
        
        static WeatherOverlay_BlueSnow()
        {
            WeatherOverlay_BlueSnow.SnowOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);
        }
    }


}
