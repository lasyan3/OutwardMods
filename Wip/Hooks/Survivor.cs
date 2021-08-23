using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Item;
using static ItemDetailsDisplay;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(Item), "IsSaveInfoRelevant")]
    public class ForceDurabilitySaving
    {
        [HarmonyPostfix]
        public static void IsSaveInfoRelevant(Item __instance, SyncedInfosOrder _info, string _data, ref bool __result)
        {
            try
            {
                if (Enum.IsDefined(typeof(eItemIDs), __instance.ItemID))
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"IsSaveInfoRelevant[{__instance.name}]={_info} ({__result})");
                    //WorkInProgress.Instance.MyLogger.LogDebug($"  {_data}");
                    if (_info == SyncedInfosOrder.Durability)
                    {
                        __result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("IsSaveInfoRelevant:" + ex.Message);
            }
        }
    }

    /*[HarmonyPatch(typeof(ItemStats), "OnInit")]
    public class ItemStats_OnInit
    {
        [HarmonyPrefix]
        public static bool OnInit(ItemStats __instance, Item _linkedItem)
        {
            try
            {
                if (_linkedItem.ItemID == 5600010) // Flint and Steel
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug("OnInit=" + _linkedItem.CurrentDurability + "/" + _linkedItem.StartingDurability + "/" + _linkedItem.MaxDurability);
                    //AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(__instance, 3);
                    if (_linkedItem.CurrentDurability <= 0)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(_linkedItem, 1);
                    }
                    WorkInProgress.Instance.MyLogger.LogDebug("OnInit=C" + _linkedItem.CurrentDurability + "/S" + _linkedItem.StartingDurability + "/M" + _linkedItem.MaxDurability);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("OnInit: " + ex.Message);
            }
            return true;
        }
    }//*/

    /*[HarmonyPatch(typeof(CharacterInventory), "MakeLootable")]
    public class CharacterInventory_MakeLootable
    {
        [HarmonyPostfix]
        public static void MakeLootable(CharacterInventory __instance, bool _dropWeapons, bool _enablePouch, bool _forceInteractable, bool _loadedDead)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug($"MakeLootable: {__instance.Pouch && _enablePouch}");
                //var il = __instance.Pouch.gameObject.GetComponent<InteractionLoot>();
                //ItemContainer m_container = (ItemContainer)AccessTools.Field(typeof(InteractionLoot), "m_container").GetValue(il);
                //WorkInProgress.Instance.MyLogger.LogDebug($" > m_container={m_container.IsEmpty} ({m_container.ItemCount})");

                EquipmentSlot.EquipmentSlotIDs equipmentSlotIDs = EquipmentSlot.EquipmentSlotIDs.Count;
                for (int i = 0; i < __instance.Equipment.EquipmentSlots.Length; i++)
                {
                    equipmentSlotIDs = (EquipmentSlot.EquipmentSlotIDs)i;
                    if (__instance.Equipment.EquipmentSlots[i] != null && __instance.Equipment.EquipmentSlots[i].HasItemEquipped && (equipmentSlotIDs == EquipmentSlot.EquipmentSlotIDs.RightHand || equipmentSlotIDs == EquipmentSlot.EquipmentSlotIDs.LeftHand) && !_loadedDead && !PhotonNetwork.isNonMasterClientInRoom
                        && __instance.Equipment.EquipmentSlots[i].EquippedItem.MaxDurability > 1)
                    {
                        //WorkInProgress.Instance.MyLogger.LogDebug($" > Equ: {__instance.Equipment.EquipmentSlots[i].EquippedItem.Name}");
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance.Equipment.EquipmentSlots[i].EquippedItem, new System.Random().Next(1, __instance.Equipment.EquipmentSlots[i].EquippedItem.MaxDurability));
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("MakeLootable: " + ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(LootableOnDeath), "OnDeath", new Type[] { typeof(bool) })]
    public class LootableOnDeathOD
    {
        [HarmonyPostfix]
        public static void OnDeath(LootableOnDeath __instance, bool _loadedDead)
        {
            WorkInProgress.Instance.MyLogger.LogDebug($"OnDeath");
            var il = __instance.Character.Inventory.Pouch.gameObject.GetComponent<InteractionLoot>();
            ItemContainer m_container = (ItemContainer)AccessTools.Field(typeof(InteractionLoot), "m_container").GetValue(il);
            WorkInProgress.Instance.MyLogger.LogDebug($" > m_container={m_container.IsEmpty} ({m_container.ItemCount})");
        }
    }

    
    [HarmonyPatch(typeof(ItemDropper), "GenerateItem")]
    public class ItemDropper_GenerateDrop
    {
        [HarmonyPrefix]
        public static bool GenerateItem(ItemDropper __instance, ItemContainer _container, BasicItemDrop _itemDrop, int _spawnAmount)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug($"GenerateItem {_container.transform.childCount}");
                if (_itemDrop.DroppedItem == null || _container == null)
                {
                    return false;
                }
                if (!(_itemDrop.DroppedItem is Currency))
                {
                    Item item = ItemManager.Instance.GenerateItemNetwork(_itemDrop);
                    if (!(bool)item)
                    {
                        Debug.LogErrorFormat("{0} is missing from asset bundle.", _itemDrop.DroppedItem);
                        return false;
                    }
                    item.ChangeParent(_container.transform);
                    WorkInProgress.Instance.MyLogger.LogDebug($"GenerateItem {item.name}");
                    if (item.MaxDurability > 0)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(item, new System.Random().Next(1, item.MaxDurability));
                    }
                    if (_spawnAmount <= 1)
                    {
                        return false;
                    }
                    if (!item.HasMultipleUses)
                    {
                        for (int i = 0; i < _spawnAmount - 1; i++)
                        {
                            item = ItemManager.Instance.GenerateItemNetwork(_itemDrop);
                            item.ChangeParent(_container.transform);
                        }
                    }
                    else if (_spawnAmount > item.MaxStackAmount)
                    {
                        item.RemainingAmount = item.MaxStackAmount;
                        for (_spawnAmount -= item.RemainingAmount; _spawnAmount > 0; _spawnAmount -= item.RemainingAmount)
                        {
                            item = ItemManager.Instance.GenerateItemNetwork(_itemDrop);
                            item.ChangeParent(_container.transform);
                            item.RemainingAmount = Mathf.Min(_spawnAmount, item.MaxStackAmount);
                        }
                    }
                    else
                    {
                        item.RemainingAmount = _spawnAmount;
                    }
                }
                else
                {
                    _container.AddSilver(_spawnAmount);
                }
                return false;
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("GenerateItem: " + ex.Message);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemContainer), "AddItem", new Type[] { typeof(Item), typeof(bool) })]
    public class SimpleExternalItemDropper_TransferToPouch
    {
        [HarmonyPostfix]
        public static void AddItem(ItemContainer __instance, Item _item)
        {
            WorkInProgress.Instance.MyLogger.LogDebug($"AddItem[{_item.name}]: CD={_item.CurrentDurability} / MD={_item.MaxDurability}");
        }
    }

    [HarmonyPatch(typeof(InteractionLoot), "OnActivationDone")]
    public class InteractionLootoad
    {
        [HarmonyPostfix]
        public static void OnActivationDone(InteractionLoot __instance)
        {
            ItemContainer m_container = (ItemContainer)AccessTools.Field(typeof(InteractionLoot), "m_container").GetValue(__instance);
            WorkInProgress.Instance.MyLogger.LogDebug($"OnActivationDone");
            WorkInProgress.Instance.MyLogger.LogDebug($" > m_container={m_container.IsEmpty} ({m_container.ItemCount})");
            foreach (var item in m_container.GetContainedItems())
            {
                if (!item.IsPerishable && item.MaxDurability > 0 && item.CurrentDurability == item.MaxDurability)
                //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(item, new System.Random().Next(1, item.MaxDurability));
                WorkInProgress.Instance.MyLogger.LogDebug($" > {item.name}: CD={item.CurrentDurability} / MD={item.MaxDurability}");
            }
        }
    }*/
}
