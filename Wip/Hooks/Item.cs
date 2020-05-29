using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Item;
using static ItemDetailsDisplay;
using static ItemManager;

namespace WorkInProgress.Hooks
{
    /*[HarmonyPatch(typeof(Item), "BaseInit")]
    public class Item_BaseInit
    {
        [HarmonyPrefix]
        public static bool BaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemID == 5600010) // Flint and Steel
                {
                    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    if (__instance.CurrentDurability == 0)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 1);
                    }
                    WorkInProgress.Instance.MyLogger.LogDebug("BaseInit=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
                }
                if (__instance.ItemID == 5000020) // Improvised Bedroll
                {
                    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(m_stats, 5);
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 5);
                    __instance.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.Destroy;
                }
                if (__instance.ItemID == 5000010) // Simple Tent
                {
                    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(m_stats, 10);
                    if (__instance.CurrentDurability < 0)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 2);
                    }
                    __instance.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;
                }
                // Sacs ?
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
            return true;
        }
    }//*/

    /*[HarmonyPatch(typeof(Item), "UpdateSync")]
    public class Item_UpdateSync
    {
        [HarmonyPostfix]
        public static void UpdateSync(Item __instance)
        {
            // For existing items, set a durability value to begin with (1 or max?)
            try
            {
                string[] m_receivedInfos = (string[])AccessTools.Field(typeof(Item), "m_receivedInfos").GetValue(__instance);
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel ||
                    __instance.ItemID == (int)eItemIDs.CamouflagedTent ||
                    __instance.ItemID == (int)eItemIDs.ImprovisedBedroll)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.name}]");
                    foreach (var item in m_receivedInfos)
                    {
                        WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    }
                    if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability))
                    {
                        //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.Name}]={m_currentDurability}");
                        if (m_currentDurability <= 0)
                        {
                            AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 1);
                        }
                        if (m_currentDurability > __instance.MaxDurability)
                        {
                            AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, __instance.MaxDurability);
                        }
                    }
                }
                if (__instance.ItemID == 5000020)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.name}]");
                    foreach (var item in m_receivedInfos)
                    {
                        WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    }
                    if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability))
                    {
                        //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.Name}]={m_currentDurability}");
                        //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, m_currentDurability);
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
                    || __instance.ItemID == (int)eItemIDs.CamoTentKit
                    || __instance.ItemID == (int)eItemIDs.FurTentKit
                    || __instance.ItemID == (int)eItemIDs.BedrollKit
                    || __instance.ItemID == (int)eItemIDs.LuxuryTent
                    || __instance.ItemID == (int)eItemIDs.MageTent
                    || __instance.ItemID == (int)eItemIDs.TentKit
                    )
                {
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
                if (__instance.ItemID == (int)eItemIDs.FlintAndSteel
                    || __instance.ItemID == (int)eItemIDs.CamoTentKit
                    || __instance.ItemID == (int)eItemIDs.FurTentKit
                    || __instance.ItemID == (int)eItemIDs.BedrollKit
                    || __instance.ItemID == (int)eItemIDs.LuxuryTent
                    || __instance.ItemID == (int)eItemIDs.MageTent
                    || __instance.ItemID == (int)eItemIDs.TentKit
                    )
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"Item_OnUse=" + __instance.CurrentDurability);
                    __instance.ReduceDurability(1);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("UpdateSync: " + ex.Message);
            }
        }
    }

    /*[HarmonyPatch(typeof(Item), "GetNetworkData", new Type[] { typeof(SyncedInfosOrder), typeof(SyncType) })]
    public class Item_GetNetworkData
    {
        [HarmonyPostfix]
        public static void GetNetworkData(Item __instance, SyncedInfosOrder _info, SyncType _syncType)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.ImprovisedBedroll)
                    WorkInProgress.Instance.MyLogger.LogDebug($"Item_GetNetworkData=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Item), "ToSaveData")]
    public class Item_ToSaveData
    {
        [HarmonyPostfix]
        public static void ToSaveData(Item __instance, string __result)
        {
            try
            {
                if (__instance.Name == "Sac de couchage")
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"ToSaveData[{__instance.name}]={__instance.ItemID}");
                    WorkInProgress.Instance.MyLogger.LogDebug($"  {__result}");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ItemExtension), "ToSaveData")]
    public class ItemExtension_ToSaveData
    {
        [HarmonyPostfix]
        public static void ToSaveData(ItemExtension __instance, string __result)
        {
            try
            {
                if (__instance.Item.Name == "Sac de couchage")
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"ToSaveData[{__instance.name}]={__instance.GetType().Name}");
                    WorkInProgress.Instance.MyLogger.LogDebug($"  {__result}");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

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
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }

    /*
        OnReceiveItemSync > CreateItemFromData > ItemSyncData
        ItemSyncData used by ItemManager.RequestItemLastSyncData
        ItemManager.RequestItemLastSyncData calls Item.OnReceiveNetworkSync
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
                if (_itemInfos.Contains("RSQtA4vX5UuuwBf2PooIOg"))
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveItemSync={_syncType}");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ItemManager), "CreateItemFromData")]
    public class ItemManager_CreateItemFromData
    {
        [HarmonyPrefix]
        public static bool CreateItemFromData(ItemManager __instance, string itemUID, string[] itemInfos, bool _characterItem)
        {
            try
            {
                if (itemUID == "RSQtA4vX5UuuwBf2PooIOg")
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"CreateItemFromData");
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
            return true;
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


    [HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    public class RestoreDurabilityOnLoad
    {
        [HarmonyPrefix]
        public static bool OnReceiveNetworkSync(Item __instance, string[] _infos)
        {
            try
            {
                if (Enum.IsDefined(typeof(eItemIDs), __instance.ItemID))
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync={__instance.name} ({_infos[10]})");
                    //foreach (var item in _infos)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    //}
                    if (float.TryParse(_infos[10], out float m_currentDurability))
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, m_currentDurability);
                    }
                    //return false;
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
            return true;
        }
    }

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

    /*[HarmonyPatch(typeof(Item), "BaseInit")]
    public class Item_BaseInit
    {
        [HarmonyPrefix]
        public static bool preBaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemID == 5000021 || __instance.ItemID == 5000020)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"preBaseInit={__instance.name} ({__instance.UID})");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
            return true;
        }
        [HarmonyPostfix]
        public static void BaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemID == 5000021 || __instance.ItemID == 5000020)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"BaseInit={__instance.name} ({__instance.UID})");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
        }
    }//*/

    [HarmonyPatch(typeof(Item), "Start")]
    public class Item_Start
    {
        [HarmonyPostfix]
        public static void Start(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Bedroll && __instance.GetComponent<CustomDurable>())
                {
                    /*if ((bool)__instance.GetComponent<DeployableStats>()
                        && string.IsNullOrEmpty(__instance.GetComponent<DeployableStats>().TargetName)
                        && __instance.GetComponent<DeployableStats>().CurrentDurability > -1)
                    {
                        __instance.GetComponent<DeployableStats>().TargetName = __instance.name;
                        WorkInProgress.Instance.MyLogger.LogDebug($"Start: TargetName={__instance.GetComponent<DeployableStats>().TargetName} ({__instance.GetComponent<DeployableStats>().CurrentDurability})");
                    }*/
                    if (__instance.GetComponent<CustomDurable>().CurrentDurability == -1)
                    {
                        float defaultDurability = WorkInProgress.ItemDurabilities[(eItemIDs)__instance.ItemID].MaxDurability;
                        __instance.GetComponent<CustomDurable>().CurrentDurability = defaultDurability;
                    }
                    WorkInProgress.Instance.MyLogger.LogDebug($"Start: {__instance.name}={__instance.GetComponent<CustomDurable>().CurrentDurability}");
                }
                if (__instance.ItemID == (int)eItemIDs.BedrollKit && __instance.GetComponent<CustomDurable>())
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"Start: {__instance.name}={__instance.GetComponent<CustomDurable>().CurrentDurability}");
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, __instance.GetComponent<CustomDurable>().CurrentDurability);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Start: " + ex.Message);
            }
        }
    }//*/
}
