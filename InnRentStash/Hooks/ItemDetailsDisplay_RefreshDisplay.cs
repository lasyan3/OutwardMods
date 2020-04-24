using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using static AreaManager;

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
                if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null
                    && !(_itemDisplay.RefItem is Skill))
                {
                    int invQty = __instance.LocalCharacter.Inventory.ItemCount(_itemDisplay.RefItem.ItemID);
                    int stashQty = 0;
                    if (InnRentStash.StashAreaToStashUID.ContainsKey(areaN) && InnRentStash.m_isStashSharing)
                    {
                        TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(InnRentStash.StashAreaToStashUID[areaN]);
                        if (stash != null)
                        {
                            stashQty = stash.ItemStackCount(_itemDisplay.RefItem.ItemID);
                        }
                    }
                    if (invQty + stashQty > 0)
                    {
                        InnRentStash.MyLogger.LogDebug($"{_itemDisplay.RefItem.DisplayName}: invQty={invQty} / stashQty={stashQty}");
                        m_lblItemName.text += $" ({invQty + stashQty})";
                    }
                }
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError(ex.Message);
            }
        }
    }
}
