using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace MoreMerchantStock
{
    public class GameData
    {
        public int Amount;
        public bool AffectMinimum;
        public bool AlwaysRefreshStock;
    }

    [BepInPlugin(ID, NAME, VERSION)]
    public class MoreMerchantStock : BaseUnityPlugin
    {
        const string ID = "com.lasyan3.moremerchantstock";
        const string NAME = "MoreMerchantStock";
        const string VERSION = "1.0.0";

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
                    try
                    {
                        return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                    }
                    catch (ArgumentNullException)
                    {
                    }
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