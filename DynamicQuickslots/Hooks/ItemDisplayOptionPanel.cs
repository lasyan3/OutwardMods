using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DynamicQuickslots.Hooks
{
    enum EnCustomInventoryActions
    {
        AssignQuickslotSet = 20,
        DeleteQuickslotSet = 21,
    }

    /// <summary>
    /// Adds new actions for managing the QuickSlotSets
    /// </summary>
    [HarmonyPatch(typeof(ItemDisplayOptionPanel), "GetActiveActions")]
    public class AddNewActions
    {
        [HarmonyPostfix]
        public static void PostGetActiveActions(ItemDisplayOptionPanel __instance, GameObject pointerPress, ref List<int> __result)
        {
            try
            {
                __result.Add((int)EnCustomInventoryActions.AssignQuickslotSet);
                ItemDisplay m_activatedItemDisplay = (ItemDisplay)AccessTools.Field(typeof(ItemDisplayOptionPanel), "m_activatedItemDisplay").GetValue(__instance);
                var ext = m_activatedItemDisplay?.RefItem?.gameObject?.GetComponent<QuickSlotSetExt>();
                if (ext != null && ext.Slots.Count > 0)
                {
                    __result.Add((int)EnCustomInventoryActions.DeleteQuickslotSet);
                }
            }
            catch (Exception ex)
            {
                DynamicQuickslots.Instance.MyLogger.LogError("PostGetActiveActions: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Returns the text for the new actions (no localization)
    /// </summary>
    [HarmonyPatch(typeof(ItemDisplayOptionPanel), "GetActionText")]
    public class GetNewActionText
    {
        [HarmonyPrefix]
        public static bool PreGetActionText(ItemDisplayOptionPanel __instance, int _actionID, ref string __result)
        {
            try
            {
                if (_actionID == (int)EnCustomInventoryActions.AssignQuickslotSet)
                {
                    __result = "New QuickSlotSet";
                    ItemDisplay m_activatedItemDisplay = (ItemDisplay)AccessTools.Field(typeof(ItemDisplayOptionPanel), "m_activatedItemDisplay").GetValue(__instance);
                    var ext = m_activatedItemDisplay?.RefItem?.gameObject?.GetComponent<QuickSlotSetExt>();
                    if (ext != null && ext.Slots.Count > 0)
                    {
                        __result = "Replace QuickSlotSet";
                    }
                    return false;
                }
                if (_actionID == (int)EnCustomInventoryActions.DeleteQuickslotSet)
                {
                    __result = "Delete QuickSlotSet";
                    return false;
                }
            }
            catch (Exception ex)
            {
                DynamicQuickslots.Instance.MyLogger.LogError("PreGetActionText: " + ex.Message);
            }
            return true;
        }
    }

    /// <summary>
    /// Do the selected action (related to QuickSlotSets)
    /// </summary>
    [HarmonyPatch(typeof(ItemDisplayOptionPanel), "ActionHasBeenPressed")]
    public class DoAction
    {
        [HarmonyPrefix]
        public static bool PreActionHasBeenPressed(ItemDisplayOptionPanel __instance, int _actionID)
        {
            try
            {
                if (_actionID == (int)EnCustomInventoryActions.AssignQuickslotSet)
                {
                    ItemDisplay m_activatedItemDisplay = (ItemDisplay)AccessTools.Field(typeof(ItemDisplayOptionPanel), "m_activatedItemDisplay").GetValue(__instance);
                    int itemID = m_activatedItemDisplay.LastRefItemID;
                    //DynamicQuickslots.Instance.MyLogger.LogDebug($" > Add quickslot set for {itemID}");
                    QuickSlot[] m_quickSlots = (QuickSlot[])AccessTools.Field(typeof(CharacterQuickSlotManager), "m_quickSlots").GetValue(__instance.LocalCharacter.QuickSlotMngr);
                    var ext = m_activatedItemDisplay?.RefItem?.gameObject?.GetOrAddComponent<QuickSlotSetExt>();
                    if (ext)
                    {
                        ext.Slots.Clear();
                        foreach (var qs in m_quickSlots.Where(qs => qs.ItemID > -1))
                        {
                            ext.Slots.Add(new CustomQuickSlot
                            {
                                Index = qs.Index,
                                ItemID = qs.ItemID
                            });
                        }
                    }
                } else if (_actionID == (int)EnCustomInventoryActions.DeleteQuickslotSet)
                {
                    ItemDisplay m_activatedItemDisplay = (ItemDisplay)AccessTools.Field(typeof(ItemDisplayOptionPanel), "m_activatedItemDisplay").GetValue(__instance);
                    int itemID = m_activatedItemDisplay.LastRefItemID;
                    var ext = m_activatedItemDisplay?.RefItem?.gameObject?.GetOrAddComponent<QuickSlotSetExt>();
                    //DynamicQuickslots.Instance.MyLogger.LogDebug($" > Delete quickslot et for {itemID}");
                    if (ext)
                    {
                        ext.Slots.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                DynamicQuickslots.Instance.MyLogger.LogError("PreActionHasBeenPressed: " + ex.Message);
            }
            return true;
        }
    }
}
