using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EraserElixir
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class DynamicQuickslots : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.dynamicquickslots";
        const string NAME = "DynamicQuickslots";
        const string VERSION = "1.0.0";

        public static DynamicQuickslots Instance;
        public ManualLogSource MyLogger { get { return Logger; } }

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                //MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError("Awake: " + ex.Message);
            }
        }
    }
}
