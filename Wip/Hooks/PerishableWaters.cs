using HarmonyLib;
using System;
using static Item;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(WaterContainer), "Fill")]
    public class WaterContainer_Fill
    {
        [HarmonyPostfix]
        public static void Fill(WaterContainer __instance, WaterType _waterType)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug("Fill");
                if (_waterType == WaterType.Clean || _waterType == WaterType.Fresh)
                {
                    if (_waterType == WaterType.Clean)
                    {
                        AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(__instance.GetComponent<ItemStats>(), WorkInProgress.ItemDurabilities[eItemIDs.CleanWater].MaxDurability);
                    }
                    else
                    {
                        AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(__instance.GetComponent<ItemStats>(), WorkInProgress.ItemDurabilities[eItemIDs.RiverWater].MaxDurability);
                    }
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, __instance.MaxDurability);
                    AccessTools.Field(typeof(Perishable), "m_baseDepletionRate").SetValue(__instance.GetComponent<Perishable>(), WorkInProgress.ItemDurabilities[eItemIDs.RiverWater].DepletionRate);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Fill: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(WaterContainer), "OnUse")]
    public class WaterResetDurabilityOnLastUse
    {
        [HarmonyPostfix]
        public static void OnUse(WaterContainer __instance, Character _targetChar)
        {
            try
            {
                if (__instance.IsEmpty)
                {
                    AccessTools.Field(typeof(Perishable), "m_baseDepletionRate").SetValue(__instance.GetComponent<Perishable>(), 0f);
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, -1);
                    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(__instance.GetComponent<ItemStats>(), -1);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Item_OnUse: " + ex.Message);
            }
        }
    }

    /*[HarmonyPatch(typeof(Item), "InitDisplayedStats")]
    public class PerishableWaters
    {
        [HarmonyPostfix]
        public static void InitDisplayedStats(Item __instance)
        {
            try
            {
                ItemDetailsDisplay.DisplayedInfos[] m_displayedInfos = (ItemDetailsDisplay.DisplayedInfos[])AccessTools.Field(typeof(Item), "m_displayedInfos").GetValue(__instance);
                m_displayedInfos = new ItemDetailsDisplay.DisplayedInfos[3]
                {
                    ItemDetailsDisplay.DisplayedInfos.QuickSlot,
                    ItemDetailsDisplay.DisplayedInfos.Capacity,
                    ItemDetailsDisplay.DisplayedInfos.Content
                };
                //AccessTools.Field(typeof(Item), "m_displayedInfos").SetValue(__instance, m_displayedInfos);
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("InitDisplayedStats: " + ex.Message);
            }
        }
    }*/

    [HarmonyPatch(typeof(ItemDetailsDisplay), "RefreshDisplay")]
    public class HideWaterDurability
    {
        [HarmonyPostfix]
        public static void RefreshDisplay(ItemDetailsDisplay __instance, IItemDisplay _itemDisplay)
        {
            try
            {
                if (_itemDisplay != null && _itemDisplay.RefItem != null && _itemDisplay.RefItem.GetType().Name == "WaterContainer")
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"InitDisplayedStats");
                    //Perishable m_perishScript = (Perishable)AccessTools.Field(typeof(Item), "m_perishScript").GetValue(_itemDisplay.RefItem);
                    //WorkInProgress.Instance.MyLogger.LogDebug($" > IsPerishable={_itemDisplay.RefItem.IsPerishable}");
                    //WorkInProgress.Instance.MyLogger.LogDebug($" > m_perishScript={m_perishScript != null}");
                    //if (m_perishScript != null)
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > DepletionRate={m_perishScript.DepletionRate}");
                    //WorkInProgress.Instance.MyLogger.LogDebug($" > CurrentDurability={_itemDisplay.RefItem.CurrentDurability}");
                    ItemDetailsDisplay.DisplayedInfos[] m_displayedInfos = (ItemDetailsDisplay.DisplayedInfos[])AccessTools.Field(typeof(Item), "m_displayedInfos").GetValue(_itemDisplay.RefItem);
                    m_displayedInfos = new ItemDetailsDisplay.DisplayedInfos[3]
                    {
                        ItemDetailsDisplay.DisplayedInfos.QuickSlot,
                        ItemDetailsDisplay.DisplayedInfos.Capacity,
                        ItemDetailsDisplay.DisplayedInfos.Content
                    };
                    AccessTools.Field(typeof(Item), "m_displayedInfos").SetValue(_itemDisplay.RefItem, m_displayedInfos);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("HideWaterDurability: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(Perishable), "Update")]
    public class PerishableUpdate
    {
        [HarmonyPostfix]
        public static void Update(Perishable __instance)
        {
            try
            {
                if (__instance.Item.ItemID == (int)eItemIDs.Waterskin && __instance.Item.CurrentDurability == 0f && __instance.Item.IsFirstSyncDone)
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"Perishable_Update[{__instance.name}]: IsFirstSyncDone={__instance.Item.IsFirstSyncDone} / CurrentDurability={__instance.Item.CurrentDurability}");
                    // When waterskin expires, replace the water with Rancid Water and remove the durability
                    //WorkInProgress.Instance.MyLogger.LogDebug($"Perishable_Update[{__instance.name}]: Fill Rancid");
                    (__instance.Item as global::WaterContainer).Fill(WaterType.Rancid, (__instance.Item as global::WaterContainer).RemainingUse);
                    AccessTools.Field(typeof(Perishable), "m_baseDepletionRate").SetValue(__instance.Item.GetComponent<Perishable>(), 0f);
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance.Item, -1);
                    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(__instance.Item.GetComponent<ItemStats>(), -1);
                    if (__instance.Item.OwnerCharacter)
                    {
                        __instance.Item.OwnerCharacter.CharacterUI.ShowInfoNotification("Water has turned rancid!", __instance.Item);
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Update: " + ex.Message);
            }
        }
    }

    /*[HarmonyPatch(typeof(Item), "Start")]
    public class InitializeDurabilityWater
    {
        [HarmonyPostfix]
        public static void Start(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Waterskin)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"Start[{__instance.name}]: Current={__instance.CurrentDurability} / Max={__instance.MaxDurability}");
                    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, 5);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Start: " + ex.Message);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    public class WaterRestoreDurabilityOnLoad
    {
        [HarmonyPrefix]
        public static bool OnReceiveNetworkSync(Item __instance, string[] _infos)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Waterskin)
                {
                    //foreach (var item in _infos)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    //}
                    //WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync(pre)[{__instance.name}]: {__instance.CurrentDurability}");
                    //if (float.TryParse(_infos[10], out float m_currentDurability))
                    //{
                    //    if (m_currentDurability > 0) WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync[{__instance.name}]: m_currentDurability={m_currentDurability}");
                    //    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, m_currentDurability);
                    //}
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

    [HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    public class WaterRestoreDurabilityOnLoadPost
    {
        [HarmonyPostfix]
        public static void OnReceiveNetworkSync(Item __instance, string[] _infos)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Waterskin)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync(post)[{__instance.name}]: {__instance.CurrentDurability}");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }//*/

    /*[HarmonyPatch(typeof(Item), "BaseInit")]
    public class Water_BaseInit_Pre
    {
        [HarmonyPrefix]
        public static bool BaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Waterskin)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"BaseInit(pre)[{__instance.name}]=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Item), "BaseInit")]
    public class Water_BaseInit_Post
    {
        [HarmonyPostfix]
        public static void BaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemID == (int)eItemIDs.Waterskin)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"BaseInit(post)[{__instance.name}]=C" + __instance.CurrentDurability + "/S" + __instance.StartingDurability + "/M" + __instance.MaxDurability);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("BaseInit: " + ex.Message);
            }
        }
    }//*/

    /*[HarmonyPatch(typeof(Item), "UpdateSync")]
    public class WaterContainer_UpdateSync_Pre
    {
        [HarmonyPrefix]
        public static bool UpdateSync(Item __instance)
        {
            try
            {
                string[] m_receivedInfos = (string[])AccessTools.Field(typeof(Item), "m_receivedInfos").GetValue(__instance);
                if (__instance.ItemID == (int)eItemIDs.Waterskin && (__instance as global::WaterContainer).ContainedWater != WaterType.Rancid)
                {
                    //if (m_receivedInfos != null && !float.TryParse(m_receivedInfos[10], out float test))
                    //{
                    //    m_receivedInfos[10] = "2";
                    //}
                    //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync(pre)[{__instance.name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
                    //if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability))
                    //    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync(pre)[{__instance.name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("UpdateSync: " + ex.Message);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Item), "UpdateSync")]
    public class WaterContainer_UpdateSync
    {
        [HarmonyPostfix]
        public static void UpdateSync(Item __instance)
        {
            // For existing items, set the durability value to max
            try
            {
                string[] m_receivedInfos = (string[])AccessTools.Field(typeof(Item), "m_receivedInfos").GetValue(__instance);
                if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability))
                    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync(post)[{__instance.name}]: {m_currentDurability}");
                if (__instance.ItemID == (int)eItemIDs.Waterskin && (__instance as global::WaterContainer).ContainedWater != WaterType.Rancid)
                {
                    //WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync(post)[{__instance.name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
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
                    //if (m_receivedInfos != null && float.TryParse(m_receivedInfos[10], out float m_currentDurability))
                    //    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync(post)[{__instance.name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
                    //if (__instance.CurrentDurability <= 0 && __instance.MaxDurability > 0)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($"UpdateSync[{__instance.name}]: m_currentDurability {__instance.CurrentDurability} > {__instance.MaxDurability}");
                    //    AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(__instance, __instance.MaxDurability);
                    //}
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("UpdateSync: " + ex.Message);
            }
        }
    }//*/

    /*[HarmonyPatch(typeof(Item), "GetNetworkData", new Type[] { typeof(SyncedInfosOrder), typeof(SyncType) })]
    public class GetNetworkData_Pre
    {
        [HarmonyPrefix]
        public static bool GetNetworkData(Item __instance, SyncedInfosOrder _info, SyncType _syncType)
        {
            if (__instance.ItemID == (int)eItemIDs.Waterskin)
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"GetNetworkData(pre)[{__instance.name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Item), "GetNetworkData", new Type[] { typeof(SyncedInfosOrder), typeof(SyncType) })]
    public class GetNetworkData_Post
    {
        [HarmonyPostfix]
        public static void GetNetworkData(Item __instance, SyncedInfosOrder _info, SyncType _syncType)
        {
            if (__instance.ItemID == (int)eItemIDs.Waterskin)
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"GetNetworkData(post)[{__instance.name}]: CD={__instance.CurrentDurability} MD={__instance.MaxDurability}");
            }
        }
    }
    [HarmonyPatch(typeof(Item), "ReduceDurability")]
    public class ReduceDurabilitys
    {
        [HarmonyPostfix]
        public static void ReduceDurability(Item __instance, float _durabilityLost)
        {
            if (__instance.ItemID == (int)eItemIDs.Waterskin)
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"ReduceDurability[{__instance.name}]: {_durabilityLost}");
            }
        }
    }
    [HarmonyPatch(typeof(Item), "SetDurabilityRatio")]
    public class SetDurabilityRatios
    {
        [HarmonyPostfix]
        public static void SetDurabilityRatio(Item __instance)
        {
            if (__instance.ItemID == (int)eItemIDs.Waterskin)
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"SetDurabilityRatio");
            }
        }
    }
    [HarmonyPatch(typeof(Item), "RepairAmount")]
    public class RepairAmounts
    {
        [HarmonyPostfix]
        public static void RepairAmount(Item __instance)
        {
            if (__instance.ItemID == (int)eItemIDs.Waterskin)
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"RepairAmount");
            }
        }
    }//*/

    [HarmonyPatch(typeof(ItemSyncData), "CheckIsDefaultAddOn")]
    public class InitializeDurabilityForExistingItems
    {
        // CheckIsDefaultAddOn called by ItemSyncData called by ItemManager.OnReceiveItemSync
        [HarmonyPostfix]
        public static void CheckIsDefaultAddOn(ItemSyncData __instance)
        {
            try
            {
                if (__instance.SyncData[1] == "4200040"
                    && (__instance.SyncData[12] == "WaterContainerWaterType/Clean;" || __instance.SyncData[12] == "WaterContainerWaterType/Fresh;")
                    && string.IsNullOrEmpty(__instance.SyncData[10]))
                {
                    __instance.SyncData[10] = WorkInProgress.ItemDurabilities[eItemIDs.RiverWater].MaxDurability.ToString();
                    if (__instance.SyncData[12] == "WaterContainerWaterType/Clean;")
                    {
                        __instance.SyncData[10] = WorkInProgress.ItemDurabilities[eItemIDs.CleanWater].MaxDurability.ToString();
                    }
                    //WorkInProgress.Instance.MyLogger.LogDebug($"InitializeDurabilityW[{__instance.SyncData[0]}]: {__instance.SyncData[12]}");
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
    }

}
