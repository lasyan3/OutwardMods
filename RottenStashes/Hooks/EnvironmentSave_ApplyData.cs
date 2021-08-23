using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RottenStashes.Hooks
{
    [HarmonyPatch(typeof(EnvironmentSave), "ApplyData")]
    public class EnvironmentSave_ApplyData
    {
        [HarmonyPrefix]
        public static bool ApplyData(EnvironmentSave __instance)
        {
            // LastGameTime must take the value of the last moment when we were on the map.
            // This value will then be applied to items which will use it to calculate the durability cost.
            RottenStashes.MyLogger.LogDebug($"ApplyData");
            RottenStashes.MyLogger.LogDebug($" > SavedGameTime={GameTimetoDays(__instance.GameTime)}");
            RottenStashes.MyLogger.LogDebug($" > CurrentGameTime={GameTimetoDays(EnvironmentConditions.GameTime)}");
            RottenStashes.LastGameTime = __instance.GameTime > 0 ? __instance.GameTime : EnvironmentConditions.GameTime;
            RottenStashes.MyLogger.LogDebug($" > LastGameTime={GameTimetoDays(RottenStashes.LastGameTime)}");
            if (EnvironmentConditions.GameTime > 0)
            {
                RottenStashes.MyLogger.LogDebug($" > Diff={GameTimetoDays(EnvironmentConditions.GameTime - RottenStashes.LastGameTime)}");
            }
            return true;
        }

        public static string GameTimetoDays(double p_gametime)
        {
            string str = "";
            int days = (int)p_gametime / 24;
            if (days > 0) str = $"{days}d, ";
            int hours = (int)p_gametime % 24;
            str += $"{hours}h";
            if (days == 0 && hours == 0)
            {
                int minutes = (int)p_gametime * 60;
                str = $"{minutes} min";
                if (minutes == 0)
                {
                    int seconds = (int)p_gametime * 3600;
                    str = $"{seconds} sec";
                }
            }
            return str;
        }
    }
}
