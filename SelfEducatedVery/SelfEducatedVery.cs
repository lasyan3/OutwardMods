using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace OutwardMods
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class SelfEducatedVery : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.selfeducatedvery";
        const string NAME = "SelfEducatedVery";
        const string VERSION = "1.0.2";

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        internal void Awake()
        {
            try
            {
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }
    }
}
