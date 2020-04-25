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
            RottenStashes.LastGameTime = __instance.GameTime > 0 ? __instance.GameTime : EnvironmentConditions.GameTime;
            return true;
        }

    }
}
