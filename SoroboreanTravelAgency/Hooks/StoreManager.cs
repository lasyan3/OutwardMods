using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(StoreManager), "IsDlcInstalled")]
    public class StoreManager_IsDlcInstalled
    {
        [HarmonyPostfix]
        public static void IsDlcInstalled(ref bool __result)
        {
            try
            {
                __result = false;
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(NodeCanvas.Tasks.Conditions.Condition_OwnsDLC), "OnCheck")]
    public class Condition_OwnsDLC
    {
        [HarmonyPostfix]
        public static void OnCheck(ref bool __result)
        {
            try
            {
                __result = true;
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(HasDLCInteractionCondition), "CheckCondition")]
    public class HasDLCInteractionCondition_CC
    {
        [HarmonyPostfix]
        public static void CheckCondition(ref bool __result)
        {
            try
            {
                __result = true;
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }
}
