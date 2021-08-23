using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Item;
using static ItemDetailsDisplay;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(ItemSyncData), "CheckIsDefaultAddOn")]
    public class InitializeDurabilityForKits
    {
        // CheckIsDefaultAddOn called by ItemSyncData called by ItemManager.OnReceiveItemSync
        [HarmonyPostfix]
        public static void CheckIsDefaultAddOn(ItemSyncData __instance)
        {
            try
            {
                //if ((__instance.SyncData[1] == ((int)eItemIDs.BedrollKit).ToString()
                //    || __instance.SyncData[1] == ((int)eItemIDs.TentKit).ToString()))
                //{
                //    WorkInProgress.Instance.MyLogger.LogDebug($"CheckIsDefaultAddOn[{__instance.SyncData[0]}]={__instance.SyncData[10]}");
                //}
                if (__instance.SyncData[1] == ((int)eItemIDs.BedrollKit).ToString()
                    || __instance.SyncData[1] == ((int)eItemIDs.TentKit).ToString())
                    {
                    if (string.IsNullOrEmpty(__instance.SyncData[10]) || __instance.SyncData[10] == "-1")
                    {
                        __instance.SyncData[10] = WorkInProgress.ItemDurabilities[eItemIDs.BedrollKit].MaxDurability.ToString();
                        WorkInProgress.Instance.MyLogger.LogDebug($"{Enum.GetName(typeof(eItemIDs), int.Parse(__instance.SyncData[1]))}: No durability saved, default to {__instance.SyncData[10]}");
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
    public class SetDurabilityUsingCustomDurable
    {
        /* Bedroll is the item deployed
         * BedrollKit is the packed item in the inventory
         * CustomDurable is a customized ItemExtension added to share the durability between the two
         * 
         */
        [HarmonyPostfix]
        public static void BaseInit(Item __instance)
        {
            try
            {
                if ((__instance.ItemID == (int)eItemIDs.Bedroll
                    || __instance.ItemID == (int)eItemIDs.Tent)
                    //&& __instance.GetComponent<CustomDurable>()
                    )
                {
                    //var ext = __instance.gameObject.GetOrAddComponent<CustomDurable>();
                    var ext = __instance.GetComponent<CustomDurable>();
                    if (ext && ext.CurrentDurability == -1)
                    {
                        float defaultDurability = WorkInProgress.ItemDurabilities[(eItemIDs)__instance.ItemID].MaxDurability;
                        ext.CurrentDurability = defaultDurability;
                        WorkInProgress.Instance.MyLogger.LogDebug($"BaseInit[{__instance.name}]: NewDurability={defaultDurability}");
                    }
                }
                if ((__instance.ItemID == (int)eItemIDs.BedrollKit
                    || __instance.ItemID == (int)eItemIDs.TentKit)

                    )
                {
                    // Goal : restore durability from CustomDurable (from deployed item)
                    var ext = __instance.GetComponent<CustomDurable>();

                    ItemStats m_stats = (ItemStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    //WorkInProgress.Instance.MyLogger.LogDebug("BaseInit=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
                    if (__instance.CurrentDurability == -1)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, WorkInProgress.ItemDurabilities[(eItemIDs)__instance.ItemID].MaxDurability);
                        AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(m_stats, WorkInProgress.ItemDurabilities[(eItemIDs)__instance.ItemID].MaxDurability);
                        WorkInProgress.Instance.MyLogger.LogDebug($"BaseInit[{__instance.name}]: NewDurability={__instance.CurrentDurability}");
                    }
                    if (ext && ext.CurrentDurability > 0 /*&& !__instance.IsFirstSyncDone*/)
                    {
                        AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, ext.CurrentDurability);
                        WorkInProgress.Instance.MyLogger.LogDebug($"Restore [{__instance.name}] durability using CustomDurable: {ext.CurrentDurability}");
                        //ext.CurrentDurability = -1; // reset durability (no more needed until deployed again)
                        //UnityEngine.Object.Destroy(ext);
                        var dep = __instance.GetComponent<Deployable>();
                        if (dep)
                        {
                            if (dep.PackedStateItemPrefab)
                            {
                                WorkInProgress.Instance.MyLogger.LogDebug($" > PackedStateItemPrefab.CurrentDurability={dep.PackedStateItemPrefab.gameObject.GetComponent<CustomDurable>().CurrentDurability}");
                            }
                            if (dep.DeployedStateItemPrefab)
                            {
                                WorkInProgress.Instance.MyLogger.LogDebug($" > DeployedStateItemPrefab.CurrentDurability={dep.DeployedStateItemPrefab.gameObject.GetComponent<CustomDurable>().CurrentDurability}");
                                var deped = dep.DeployedStateItemPrefab.gameObject.GetComponent<Deployable>();
                                if (deped && deped.PackedStateItemPrefab)
                                {
                                    WorkInProgress.Instance.MyLogger.LogDebug($" > PackedStateItemPrefab2.CurrentDurability={deped.PackedStateItemPrefab.gameObject.GetComponent<CustomDurable>().CurrentDurability}");
                                    deped.PackedStateItemPrefab.gameObject.GetComponent<CustomDurable>().CurrentDurability = -1;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
        }
    }//*/

    [HarmonyPatch(typeof(Item), "InitDisplayedStats")]
    public class ShowDurabilityOnDisplayTents
    {
        [HarmonyPostfix]
        public static void InitDisplayedStats(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.BedrollKit
                    || __instance.ItemID == (int)eItemIDs.CamoTentKit
                    || __instance.ItemID == (int)eItemIDs.FurTentKit
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

    [HarmonyPatch(typeof(InteractionBase), "Activate", new Type[] { typeof(Character) })]
    public class RestrictUsageOfTents
    {
        [HarmonyPrefix]
        public static bool Activate(InteractionBase __instance, Character _character)
        {
            // Activate > OnActivate > ActivationDone
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug("Activate=" + __instance.GetType().Name);
                if (__instance.GetType() != typeof(InteractionSleep))
                {
                    return true;
                }
                var ds = (__instance as InteractionSleep).Item.GetComponent<CustomDurable>();
                if (ds == null)
                {
                    return true;
                }
                //WorkInProgress.Instance.MyLogger.LogDebug($"RestrictUsageOfTents[{(__instance as InteractionSleep).Item.name}]={ds.CurrentDurability}");
                //var list = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.StartsWith("5000021_Bedroll_")
                //    && g.GetComponent<Item>() != null
                //    && g.GetComponent<Item>().CurrentDurability == 100
                //    ).ToList();
                //foreach (var go in list)
                //{
                //    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(go.GetComponent<Item>(), 42);
                //    WorkInProgress.Instance.MyLogger.LogDebug($" > {go.name} ({go.GetComponent<Item>().CurrentDurability})");
                //}
                if (_character.Inventory.HasABag)
                {
                    _character.CharacterUI.ShowInfoNotification("You must remove your bagpack before sleep!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Activate: " + ex.Message);
            }
            return true;
        }
    }

    /*[HarmonyPatch(typeof(Item), "GetNetworkData", new Type[] { typeof(SyncedInfosOrder), typeof(SyncType) })]
    public class Item_GetNetworkData
    {
        [HarmonyPostfix]
        public static void GetNetworkData(Item __instance, SyncedInfosOrder _info, SyncType _syncType, ref string __result)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Tent && _info == SyncedInfosOrder.ItemExtensions && _syncType == SyncType.Save)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"GetNetworkData[{__instance.name}]: {_info} / {_syncType}");
                    DictionaryExt<string, ItemExtension> m_extensions = (DictionaryExt<string, ItemExtension>)AccessTools.Field(typeof(Item), "m_extensions").GetValue(__instance);
                    foreach (string key in m_extensions.Keys)
                    {
                        WorkInProgress.Instance.MyLogger.LogDebug($" > {m_extensions[key].GetType().Name} Savable={m_extensions[key].Savable}");
                        //if (_syncType == SyncType.Save && m_extensions[key].GetType().Name.Contains("CustomDurable"))
                        //{
                        //    if (__result.Length != 0)
                        //    {
                        //        __result += ":";
                        //    }
                        //    __result += $"{key};{m_extensions[key].ToSaveData()}";
                        //}
                    }
                    //WorkInProgress.Instance.MyLogger.LogDebug($" >> Result={__result}");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }//*/

    /*[HarmonyPatch(typeof(InteractionSleep), "StartInit")]
    public class SetDurabilityOfWorldItem
    {
        [HarmonyPostfix]
        public static void StartInit(InteractionSleep __instance)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug("InteractionSleep_StartInit");
                if (WorkInProgress.Instance.LastGameObjectDeployed != null)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug("SetDurabilityOfWorldItem=" + WorkInProgress.Instance.LastGameObjectDeployed.name);
                    WorkInProgress.Instance.MyLogger.LogDebug($"IsAllItemSynced={ItemManager.Instance.IsAllItemSynced}");
                    WorkInProgress.Instance.MyLogger.LogDebug($"PendingSyncCount={ItemManager.Instance.PendingSyncCount}");
                    WorkInProgress.Instance.MyLogger.LogDebug($"WorldItems={ItemManager.Instance.WorldItems.Count}");
                    var item = WorkInProgress.Instance.LastGameObjectDeployed.GetComponent<Item>();
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(item, WorkInProgress.Instance.LastDeployableDurability);
                    WorkInProgress.Instance.MyLogger.LogDebug(" > " + item.CurrentDurability);
                    WorkInProgress.Instance.LastGameObjectDeployed = null;
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("StartInit: " + ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(CharacterInventory), "TakeItem", new Type[] { typeof(Item), typeof(bool) })]
    public class RestoreDurabilityOnTakeItem
    {
        [HarmonyPostfix]
        public static void TakeItem(CharacterInventory __instance, Item takenItem, bool _tryToEquip)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"TakeItem[{takenItem.name}]");
                if (WorkInProgress.Instance.IsCraftingDone)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug("RestoreDurabilityOnTakeItem=" + takenItem.name);
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(takenItem, takenItem.MaxDurability);
                    WorkInProgress.Instance.IsCraftingDone = false;
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("TakeItem: " + ex.Message);
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
    }

    [HarmonyPatch(typeof(ItemExtension), "ToSaveData")]
    public class ItemExtension_ToSaveData
    {
        [HarmonyPostfix]
        public static void ToSaveData(ItemExtension __instance, string __result)
        {
            try
            {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"ToSaveData[{__instance.name}]={__instance.GetType().Name}");
                    //WorkInProgress.Instance.MyLogger.LogDebug($"  {__result}");
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

}
