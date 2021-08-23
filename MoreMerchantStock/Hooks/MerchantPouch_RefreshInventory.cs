using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoreMerchantStock
{
    [HarmonyPatch(typeof(MerchantPouch), "RefreshInventory")]
    public class MerchantPouch_RefreshInventory
    {
        [HarmonyPrefix]
        public static bool RefreshInventory(MerchantPouch __instance)
        {
            try
            {
                if (MoreMerchantStock.Settings.AlwaysRefreshStock)
                {
                    FieldInfo nextRefreshTime = AccessTools.Field(typeof(MerchantPouch), "m_nextRefreshTime");
                    nextRefreshTime.SetValue(__instance, EnvironmentConditions.GameTime - 1);
                    //MoreMerchantStock.MyLogger.LogDebug($"nextRefreshTime={nextRefreshTime}");
                }
            }
            catch (Exception ex)
            {
                MoreMerchantStock.MyLogger.LogError(ex.Message);
            }

            return true;
        }
    }
}
