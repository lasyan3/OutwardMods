using BepInEx;
using System;
using System.IO;
using UnityEngine;
using HarmonyLib;
using BepInEx.Logging;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace MoreGatherableLoot
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class MoreGatherableLoot : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.MoreGatherableLoot";
        const string NAME = "MoreGatherableLoot";
        const string VERSION = "1.0.6";

        public static MoreGatherableLoot Instance;

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        public ConfigEntry<int> configAmountMin;
        public ConfigEntry<int> configAmountMax;

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                configAmountMin = Config.Bind("General", "AmountMin", 1, new ConfigDescription("Minimum amount of items you can collect.", new AcceptableValueRange<int>(1, 10)));
                configAmountMax = Config.Bind("General", "AmountMax", 3, new ConfigDescription("Maximum amount of items you can collect.", new AcceptableValueRange<int>(1, 10)));
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }
    }
}
