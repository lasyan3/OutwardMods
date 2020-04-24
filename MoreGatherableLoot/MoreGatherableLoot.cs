using BepInEx;
using System;
using System.IO;
using UnityEngine;
using HarmonyLib;
using BepInEx.Logging;

namespace MoreGatherableLoot
{
    public class GameData
    {
        public int AmountMin;
        public int AmountMax;
        public bool AlwaysMax;
    }

    [BepInPlugin(ID, NAME, VERSION)]
    public class MoreGatherableLoot : BaseUnityPlugin
    {
        const string ID = "com.lasyan3.moregatherableloot";
        const string NAME = "MoreGatherableLoot";
        const string VERSION = "1.0.3";

        public static GameData Settings;
        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        internal void Awake()
        {
            try
            {
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                Settings = LoadSettings();
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }

        private GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader($"BepInEx/config/{NAME}Config.json"))
                {
                    return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
            return null;
        }
    }
}
