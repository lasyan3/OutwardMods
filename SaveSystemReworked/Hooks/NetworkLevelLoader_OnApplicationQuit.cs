using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaveSystemReworked
{
    [HarmonyPatch(typeof(NetworkLevelLoader), "OnApplicationQuit")]
    public class NetworkLevelLoader_OnApplicationQuit
    {
        [HarmonyPrefix]
        public static bool OnApplicationQuit(NetworkLevelLoader __instance)
        {
            SaveSystemReworked.MyLogger.LogDebug("OnApplicationQuit");
            return false;
        }
    }
}
