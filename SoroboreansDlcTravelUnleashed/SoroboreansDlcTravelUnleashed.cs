using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static AreaManager;

namespace OutwardMods
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class SoroboreansDlcTravelUnleashed : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.SoroboreansDlcTravelUnleashed";
        const string NAME = "Soroborean DLC Travel Unleashed";
        const string VERSION = "1.0.1";

        public static SoroboreansDlcTravelUnleashed Instance;
        public ManualLogSource MyLogger { get { return Logger; } }

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                Logger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }
    }
}
