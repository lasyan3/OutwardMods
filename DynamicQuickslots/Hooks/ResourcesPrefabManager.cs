using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
    class Load
    {
        /// <summary>
        /// Add the component QuickSlotSetExt to all items that are "QuickSlotable"
        /// </summary>
        [HarmonyPostfix]
        public static void PostLoad(ResourcesPrefabManager __instance)
        {
            try
            {
                Dictionary<string, Item> ITEM_PREFABS = (Dictionary<string, Item>)AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").GetValue(__instance);
                ITEM_PREFABS.Where(i => i.Value.IsQuickSlotable).Do(i => i.Value.gameObject.AddComponent<QuickSlotSetExt>());
            }
            catch (Exception ex)
            {
                DynamicQuickslots.Instance.MyLogger.LogError("PostLoad: " + ex.Message);
            }
        }
    }
}
