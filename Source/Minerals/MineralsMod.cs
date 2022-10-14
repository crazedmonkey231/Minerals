using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Minerals
{
    class MineralsMod : Mod
    {
        public static MineralsSettings Settings;

        public MineralsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MineralsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Minerals";
        }
    }

}
