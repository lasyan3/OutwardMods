﻿using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static Item;

namespace DynamicQuickslots.Hooks
{
    public class CustomQuickSlot
    {
        public int Index;
        public int ItemID;

        public override string ToString()
        {
            return $"{Index},{ItemID}";
        }
    }

    [HarmonyPatch(typeof(CharacterQuickSlotManager), "QuickSlotInput")]
    public class SelectQuickSlotSet
    {
        [HarmonyPostfix]
        public static void PostQuickSlotInput(CharacterQuickSlotManager __instance, int _index)
        {
            try
            {
                Character m_character = (Character)AccessTools.Field(typeof(CharacterQuickSlotManager), "m_character").GetValue(__instance);
                QuickSlot[] m_quickSlots = (QuickSlot[])AccessTools.Field(typeof(CharacterQuickSlotManager), "m_quickSlots").GetValue(__instance);
                var ext = m_quickSlots[_index]?.RegisteredItem?.GetComponent<QuickSlotSetExt>();
                if (m_quickSlots[_index] != null && ext != null && ext.Slots.Count > 0)
                {
                    //DynamicQuickslots.Instance.MyLogger.LogDebug($"SWITCH from {m_quickSlots[_index].ItemID}");
                    foreach (var qs in ext.Slots)
                    {
                        //DynamicQuickslots.Instance.MyLogger.LogDebug($" > {qs.Index}: {qs.ItemID}");
                        Item skill = m_character.Inventory.SkillKnowledge.GetItemFromItemID(qs.ItemID);
                        if (qs.ItemID == -1)
                        {
                            //DynamicQuickslots.Instance.MyLogger.LogDebug($" > {qs.Index}: -");
                            //__instance.ClearQuickSlot(qs.Index);
                        }
                        else if (skill != null)
                        {
                            //DynamicQuickslots.Instance.MyLogger.LogDebug($" > {qs.Index}: {skill.name}");
                            __instance.SetQuickSlot(qs.Index, skill);
                        }
                        else
                        {
                            Item it = m_character.Inventory.GetOwnedItems(qs.ItemID).FirstOrDefault();
                            if (it == null) // Item is not in inventory, perhaps equipped?
                            {
                                if (m_character.LeftHandEquipment?.ItemID == qs.ItemID)
                                {
                                    it = m_character.LeftHandEquipment;
                                }
                                else if (m_character.CurrentWeapon?.ItemID == qs.ItemID)
                                {
                                    it = m_character.CurrentWeapon;
                                }
                                else // Item is nowhere to be found, create from prefab (downsides expected!)
                                {
                                    it = ResourcesPrefabManager.Instance.GetItemPrefab(qs.ItemID);
                                }
                            }
                            //DynamicQuickslots.Instance.MyLogger.LogDebug($" > {qs.Index}: {it.name}");
                            __instance.SetQuickSlot(qs.Index, it);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DynamicQuickslots.Instance.MyLogger.LogError("QuickSlotInput: " + ex.Message);
            }
        }
    }
}
