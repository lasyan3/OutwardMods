using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(ItemDetailsDisplay), "RefreshDisplay")]
    public class ItemDetailsDisplay_RefreshDisplay
    {
        [HarmonyPostfix]
        public static void RefreshDisplay(ItemDetailsDisplay __instance, IItemDisplay _itemDisplay)
        {
            try
            {
                //Item m_lastItem = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(self);
                //OLogger.Log($"RefreshDisplay={m_lastItem.DisplayName}");

                //Item it = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(self);
                if (_itemDisplay != null && _itemDisplay.RefItem != null &&
                    _itemDisplay.RefItem.Value > 0 && _itemDisplay.RefItem.Weight > 0 && _itemDisplay.RefItem.IsSellable)
                {
                    //OLogger.Log(m_lblItemName.text);
                    //int invQty = self.LocalCharacter.Inventory.GetOwnedItems(_itemDisplay.RefItem.ItemID).Count;
                    //int stashQty = 0;
                    //if (StashAreaToStashUID.ContainsKey(m_currentArea))
                    //{
                    //    TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[m_currentArea]);
                    //    if (stash != null)
                    //    {
                    //        stashQty = stash.GetItemsFromID(_itemDisplay.RefItem.ItemID).Count;
                    //    }
                    //}
                    //m_lblItemName.text += $" ({invQty + stashQty})";
                    //List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(__instance);
                    //ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(__instance, new object[] { m_detailRows.Count });
                    //row.SetInfo("Ratio", Math.Round(_itemDisplay.RefItem.Value / _itemDisplay.RefItem.Weight, 2).ToString());
                    //m_lblItemName.text += $" ({_itemDisplay.RefItem.Value}/{_itemDisplay.RefItem.Weight} = {_itemDisplay.RefItem.Value/_itemDisplay.RefItem.Weight})"; 
                }
                //if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null &&
                //    _itemDisplay.RefItem.Value > 0)
                //{
                //    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(self);
                //    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { m_detailRows.Count });
                //    row.SetInfo("Value", _itemDisplay.RefItem.Value.ToString());
                //}
                //if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null &&
                //    _itemDisplay.RefItem.IsPerishable)
                //{
                //    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(__instance);
                //    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(__instance, new object[] { m_detailRows.Count });
                //    row.SetInfo("Durability", GameTimetoDays(_itemDisplay.RefItem.CurrentDurability / _itemDisplay.RefItem.PerishScript.DepletionRate));
                //}
                //if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null)
                //{
                //    try
                //    {
                //        EffectSynchronizer2 test = new EffectSynchronizer2(_itemDisplay.RefItem);
                //        List<string> lstEffects = new List<string>();
                //        foreach (var item in test.LstEffects)
                //        {
                //            // AddStatusEffect
                //            // AffectStamina
                //            // AffectBurntHealth
                //            // AffectBurntStamina
                //            // 
                //            //OLogger.Log(item.GetType().Name);
                //            if (item is AddStatusEffect)
                //            {
                //                //OLogger.Log((item as AddStatusEffect).Status);
                //                //OLogger.Log((item as AddStatusEffect).Status.StatusName);
                //                lstEffects.Add((item as AddStatusEffect).Status.StatusName);
                //            }
                //        }
                //        if (lstEffects.Count > 0)
                //        {
                //            //row.SetInfo("Effects", string.Join("\r\n", lstEffects.ToArray()));
                //            Text m_lblItemDesc = (Text)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lblItemDesc").GetValue(self);
                //            if (m_lblItemDesc != null)
                //            {
                //                m_lblItemDesc.text += "\r\n\r\n" + string.Join("\r\n", lstEffects.ToArray());
                //            }
                //        }
                //        //OLogger.Log("DONE");
                //    }
                //    catch (Exception ex)
                //    {
                //        OLogger.Error("Item_OnUse:" + ex.Message);
                //    }
                //}
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("RefreshDisplay: " + ex.Message);
            }
        }//*/

        //[HarmonyPrefix]
        //public static bool RefreshDisplay(ItemDetailsDisplay __instance, IItemDisplay _itemDisplay) { return false; }

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
