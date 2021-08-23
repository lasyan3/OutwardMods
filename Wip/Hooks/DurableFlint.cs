using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Item;
using static ItemDetailsDisplay;
using static ItemManager;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(ItemSyncData), "CheckIsDefaultAddOn")]
    public class InitializeDurabilityForFlint
    {
        // CheckIsDefaultAddOn called by ItemSyncData called by ItemManager.OnReceiveItemSync
        [HarmonyPostfix]
        public static void CheckIsDefaultAddOn(ItemSyncData __instance)
        {
            try
            {
                //if (__instance.SyncData[1] == ((int)eItemIDs.FlintAndSteel).ToString())
                //    WorkInProgress.Instance.MyLogger.LogDebug($"CheckIsDefaultAddOn[{__instance.SyncData[0]}]={__instance.SyncData[10]}");
                if (__instance.SyncData[1] == ((int)eItemIDs.FlintAndSteel).ToString())
                {
                    if (string.IsNullOrEmpty(__instance.SyncData[10]) || __instance.SyncData[10] == "-1")
                    {
                        __instance.SyncData[10] = WorkInProgress.ItemDurabilities[eItemIDs.BedrollKit].MaxDurability.ToString();
                        WorkInProgress.Instance.MyLogger.LogDebug($"{Enum.GetName(typeof(eItemIDs), int.Parse(__instance.SyncData[1]))}: No durability saved, default set to {__instance.SyncData[10]}");
                    }
                    else
                    {
                        WorkInProgress.Instance.MyLogger.LogDebug($"{Enum.GetName(typeof(eItemIDs), int.Parse(__instance.SyncData[1]))}: Load durability with {__instance.SyncData[10]}");
                    }
                    //int i = 0;
                    //foreach (var item in __instance.SyncData)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {i}={item}");
                    //    i++;
                    //}
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("CheckIsDefaultAddOn: " + ex.Message);
            }
        }
    }//*/

    [HarmonyPatch(typeof(Item), "BaseInit")]
    public class Item_BaseInit
    {
        [HarmonyPostfix]
        public static void BaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel)
                {
                    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    if (__instance.CurrentDurability == -1)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, WorkInProgress.ItemDurabilities[eItemIDs.FlintAndSteel].MaxDurability);
                        WorkInProgress.Instance.MyLogger.LogDebug($"BaseInit[{__instance.name}: NewDurability={__instance.CurrentDurability}");
                    }
                    if (m_stats && m_stats.MaxDurability == -1)
                    {
                        AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(m_stats, WorkInProgress.ItemDurabilities[eItemIDs.FlintAndSteel].MaxDurability);
                    }
                }
                //if (__instance.ItemID == (int)eItemIDs.BedrollKit)
                //{
                //    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                //    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, WorkInProgress.ItemDurabilities[eItemIDs.BedrollKit].MaxDurability);
                //}
                //if (__instance.ItemID == 5000010) // Simple Tent
                //{
                //    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                //    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(m_stats, 10);
                //    if (__instance.CurrentDurability < 0)
                //    {
                //        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 2);
                //    }
                //    __instance.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;
                //}
                // Sacs ?
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
            //return true;
        }
    }//*/

    /*[HarmonyPatch(typeof(Item), "UpdateSync")]
    public class Item_UpdateSync
    {
        [HarmonyPostfix]
        public static void UpdateSync(Item __instance)
        {
            // For existing items, set the durability value to max
            try
            {
                string[] m_receivedInfos = (string[])AccessTools.Field(typeof(Item), "m_receivedInfos").GetValue(__instance);
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel)
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.name}]");
                    //foreach (var item in m_receivedInfos)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    //}
                    //if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability))
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.Name}]={m_currentDurability}");
                    //    if (m_currentDurability <= 0)
                    //    {
                    //        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 1);
                    //    }
                    //    if (m_currentDurability > __instance.MaxDurability)
                    //    {
                    //        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, __instance.MaxDurability);
                    //    }
                    //}
                    //else 
                    //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.Name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
                    if (__instance.CurrentDurability <= 0 && __instance.MaxDurability > 0)
                    {
                        WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.Name}]: m_currentDurability {__instance.CurrentDurability} > {__instance.MaxDurability}");
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, __instance.MaxDurability);
                    }
                }
                if (__instance.ItemID == (int)eItemIDs.BedrollKit)
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.name}]");
                    //foreach (var item in m_receivedInfos)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    //}
                    if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability) && m_currentDurability > 0)
                    {
                        WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.Name}]: m_currentDurability {__instance.CurrentDurability} > {__instance.MaxDurability}");
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, m_currentDurability);
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("UpdateSync: " + ex.Message);
            }
        }
    }//*/

    [HarmonyPatch(typeof(Item), "InitDisplayedStats")]
    public class ShowDurabilityOnDisplay
    {
        [HarmonyPostfix]
        public static void InitDisplayedStats(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel
                    )
                //if (Enum.IsDefined(typeof(eItemIDs), __instance.ItemID))
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"InitDisplayedStats[{__instance.Name}]: add Durability");
                    DisplayedInfos[] m_displayedInfos = (DisplayedInfos[])AccessTools.Field(typeof(Item), "m_displayedInfos").GetValue(__instance);
                    m_displayedInfos = new DisplayedInfos[1] { DisplayedInfos.Durability };
                    AccessTools.Field(typeof(Item), "m_displayedInfos").SetValue(__instance, m_displayedInfos);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("InitDisplayedStats: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(Item), "OnUse")]
    public class ReduceDurabilityOnUse
    {
        [HarmonyPostfix]
        public static void OnUse(Item __instance, Character _targetChar)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug($"Item_OnUse={__instance.name}");
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel)
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"Item_OnUse=" + __instance.CurrentDurability);
                    __instance.ReduceDurability(1);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Item_OnUse: " + ex.Message);
            }
        }
    }

    /*
        OnReceiveItemSync > CreateItemFromData > ItemSyncData > m_pendingSync.Add(itemUIDFromSyncInfo, itemSyncData);
        ItemSyncData used by ItemManager.RequestItemLastSyncData
        ItemManager.RequestItemLastSyncData calls Item.OnReceiveNetworkSync with infos from m_pendingSync
        Item.OnReceiveNetworkSync stores infos in m_receivedInfos (m_synced = false)
        ...
        Item.UpdateSync uses m_receivedInfos (m_synced = true)
     */

    /*[HarmonyPatch(typeof(ItemManager), "OnReceiveItemSync")]
    public class ItemManager_OnReceiveItemSync
    {
        [HarmonyPostfix]
        public static void OnReceiveItemSync(ItemManager __instance, string _itemInfos, ItemSyncType _syncType)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveItemSync={__instance.name} ({_syncType})");
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }//*/

    /*[HarmonyPatch(typeof(ItemManager), "CreateItemFromData")]
    public class ItemManager_CreateItemFromData
    {
        [HarmonyPostfix]
        public static void CreateItemFromData(ItemManager __instance, string itemUID, string[] itemInfos, bool _characterItem)
        {
            try
            {
                if (itemInfos[1] == "4200040")
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"CreateItemFromData[{itemUID}]: {itemInfos[10]}");
                    //foreach (var item in itemInfos)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    //}
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
            //return true;
        }
    }*/

    /*[HarmonyPatch(typeof(ItemSyncData), "RefreshSyncData")]
    public class ItemSyncData_RefreshSyncData
    {
        [HarmonyPrefix]
        public static bool RefreshSyncData(ItemSyncData __instance, string[] _data)
        {
            try
            {
                if (_data[0] == "RSQtA4vX5UuuwBf2PooIOg")
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"RefreshSyncData");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
            return true;
        }
    }*/

    /*[HarmonyPatch(typeof(ItemManager), "UpdateItemInitialization")]
    public class ItemManager_UpdateItemInitialization
    {
        [HarmonyPostfix]
        public static void UpdateItemInitialization(ItemManager __instance)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"UpdateItemInitialization");
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ItemManager), "UpdateItemLongLoadingCleanup")]
    public class ItemManager_UpdateItemLongLoadingCleanup
    {
        [HarmonyPostfix]
        public static void UpdateItemLongLoadingCleanup(ItemManager __instance)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"UpdateItemLongLoadingCleanup");
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ItemManager), "RequestItemLastSyncData")]
    public class ItemManager_RequestItemLastSyncData
    {
        [HarmonyPrefix]
        public static bool RequestItemLastSyncData(ItemManager __instance, Item _item)
        {
            try
            {
                if (_item.ItemID == 5000021)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"RequestItemLastSyncData={_item.name} ({_item.IsIndestructible}) ({_item.IsPickable})");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
            return true;
        }
    }*/


    /*[HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    public class RestoreDurabilityOnLoad
    {
        [HarmonyPrefix]
        public static bool OnReceiveNetworkSync(Item __instance, string[] _infos)
        {
            try
            {
                //if (Enum.IsDefined(typeof(eItemIDs), __instance.ItemID))
                //{
                //    if (float.TryParse(_infos[10], out float m_currentDurability))
                //    {
                //        if (m_currentDurability > 0) WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync[{__instance.name}]: m_currentDurability={m_currentDurability}");
                //        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, m_currentDurability);
                //    }
                //}
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("OnReceiveNetworkSync: " + ex.Message);
            }
            return true;
        }
    }*/

    /*[HarmonyPatch(typeof(Item), "RemoveQuantity", new Type[] { typeof(int), typeof(bool) })]
    public class Item_RemoveQuantity
    {
        [HarmonyPostfix]
        public static void RemoveQuantity(Item __instance, int _quantity, bool _destroyOnEmpty)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"RemoveQuantity={__instance.name}");
                var list = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.StartsWith("5000021_Bedroll_")
                    //&& g.GetComponent<Item>() != null
                    //&& g.GetComponent<Item>().CurrentDurability == 100
                    ).ToList();
                foreach (var go in list)
                {
                    //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(go.GetComponent<Item>(), __instance.Item.CurrentDurability);
                    WorkInProgress.Instance.MyLogger.LogDebug($" > {go.name}");// ({go.GetComponent<Item>().CurrentDurability})");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ItemManager), "ItemHasBeenAdded")]
    public class ItemManager_ItemHasBeenAdded
    {
        [HarmonyPrefix]
        public static bool prefix(ItemManager __instance, ref Item _newItem)
        {
            //WorkInProgress.Instance.MyLogger.LogDebug($"preItemHasBeenAdded=" + _newItem.name + $"({__instance.WorldItems.Count})");
            return true;
        }

        [HarmonyPostfix]
        public static void ItemHasBeenAdded(ItemManager __instance, ref Item _newItem)
        {
            try
            {
                if (_newItem.ItemID == 5000020 || _newItem.ItemID == 5000021)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"ItemHasBeenAdded=" + _newItem.name + $"({__instance.WorldItems.Count}) ({_newItem.UID})");
                }
                //Item item = __instance.WorldItems[_newItem.UID];
                //WorkInProgress.Instance.MyLogger.LogDebug($"ItemHasBeenAdded={item.name}");
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Item), "RegisterUID")]
    public class Item_RegisterUID
    {
        [HarmonyPrefix]
        public static bool RegisterUID(Item __instance)
        {
            try
            {
                if (__instance.ItemID == 5000021 || __instance.ItemID == 5000020)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"RegisterUID={__instance.name} ({__instance.UID})");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
            return true;
        }
    }*/

    /*[HarmonyPatch(typeof(Item), "Start")]
    public class InitializeDurability
    {
        [HarmonyPostfix]
        public static void Start(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug("Start=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
                    //ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    //if (__instance.CurrentDurability == 0)
                    //{
                    //    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 2);
                    //    WorkInProgress.Instance.MyLogger.LogDebug("Start=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
                    //}
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Start: " + ex.Message);
            }
        }
    }//*/

    /*[HarmonyPatch(typeof(Item), "ReduceDurability")]
    public class ReduceDurabilitys
    {
        [HarmonyPostfix]
        public static void ReduceDurability(Item __instance, float _durabilityLost)
        {
            WorkInProgress.Instance.MyLogger.LogDebug($"ReduceDurability[{__instance.name}]: {_durabilityLost}");
        }
    }*/

    [HarmonyPatch(typeof(Item), "ChangeOwner")]
    public class HideNewFlag
    {
        [HarmonyPostfix]
        public static void ChangeOwner(Item __instance)
        {
            try
            {
                AccessTools.Field(typeof(Item), "m_isNewInInventory").SetValue(__instance, false);
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("ChangeOwner: " + ex.Message);
            }
        }
    }

}
