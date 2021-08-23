using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using static AreaManager;
using static ItemDetailsDisplay;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(ItemDetailsDisplay), "RefreshDisplay")]
    public class ItemDetailsDisplay_RefreshDisplay
    {
        [HarmonyPostfix]
        public static void RefreshDisplay(ItemDetailsDisplay __instance, IItemDisplay _itemDisplay)
        {
            try
            {
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                Text m_lblItemName = (Text)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lblItemName").GetValue(__instance);
                if (m_lblItemName == null || _itemDisplay == null || _itemDisplay.RefItem == null)
                {
                    return;
                }
                #region Show inventory and stash quantities in the name
                if (!(_itemDisplay.RefItem is Skill))
                {
                    int invQty = __instance.LocalCharacter.Inventory.ItemCount(_itemDisplay.RefItem.ItemID);
                    int stashQty = 0;
                    if (InnRentStash.StashAreaToStashUID.ContainsKey(areaN) && InnRentStash.Instance.ConfigStashSharing.Value)
                    {
                        TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(InnRentStash.StashAreaToStashUID[areaN]);
                        if (stash != null)
                        {
                            stashQty = stash.ItemStackCount(_itemDisplay.RefItem.ItemID);
                        }
                    }
                    if (invQty + stashQty > 0)
                    {
                        //InnRentStash.MyLogger.LogDebug($"{_itemDisplay.RefItem.DisplayName}: invQty={invQty} / stashQty={stashQty}");
                        m_lblItemName.text += $" ({invQty + stashQty})";
                    }
                }
                #endregion
                #region Add Value/Weight Ratio information
                if (_itemDisplay.RefItem.Value > 0 && _itemDisplay.RefItem.Weight > 0 && _itemDisplay.RefItem.IsSellable)
                {
                    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(__instance);
                    //ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(__instance, new object[] { m_detailRows.Count });
                    //row.SetInfo("Value rate", Math.Round(_itemDisplay.RefItem.Value / _itemDisplay.RefItem.Weight, 2).ToString());
                }
                #endregion
                #region Add Durability information
                if (_itemDisplay.RefItem.IsPerishable && _itemDisplay.RefItem.CurrentDurability > 0
                    && !_itemDisplay.RefItem.DisplayedInfos.ToList().Contains(DisplayedInfos.Durability))
                {
                    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(__instance);
                    //ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(__instance, new object[] { m_detailRows.Count });
                    //row.SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_Durability"), GameTimetoDays(_itemDisplay.RefItem.CurrentDurability / _itemDisplay.RefItem.PerishScript.DepletionRate));
                }
                #endregion
            }
            catch (Exception ex)
            {
                //InnRentStash.MyLogger.LogError("RefreshDisplay: " + ex.Message);
            }
        }

        [HarmonyPatch(typeof(ItemDetailsDisplay), "RefreshDetail")]
        public class ItemDetailsDisplay_RefreshDetail
        {
            [HarmonyPostfix]
            public static void RefreshDetail(ItemDetailsDisplay __instance, int _rowIndex, DisplayedInfos _infoType)
            {
                try
                {
                    if (_infoType != DisplayedInfos.Durability)
                    {
                        return;
                    }
                    Item m_lastItem = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(__instance);
                    if (m_lastItem.IsPerishable && m_lastItem.CurrentDurability > 0)
                    {
                        ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(__instance, new object[] { _rowIndex });
                        Text m_lblDataName = (Text)AccessTools.Field(typeof(ItemDetailRowDisplay), "m_lblDataName").GetValue(row);
                        row.SetInfo(m_lblDataName.text, GameTimetoDays(m_lastItem.CurrentDurability / m_lastItem.PerishScript.DepletionRate));
                    }
                }
                catch (Exception ex)
                {
                    InnRentStash.MyLogger.LogError("RefreshDetail: " + ex.Message);
                }
            }
        }

        private static string GameTimetoDays(double p_gametime)
        {
            string str = "";
            int days = (int)(p_gametime / 24);
            if (days > 0) str = $"{days}d, ";
            int hours = (int)(p_gametime % 24);
            str += $"{hours}h";
            if (days == 0)
            {
                hours = (int)Math.Ceiling(p_gametime % 24);
                str = $"{hours}h";
                if (hours <= 1)
                {
                    int minutes = (int)(p_gametime * 60);
                    str = $"{minutes} min";
                    if (minutes == 0)
                    {
                        int seconds = (int)(p_gametime * 3600);
                        str = $"{seconds} sec";
                    }
                }
            }
            return str;
        }
    }
}
