using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ItemDetailsDisplay;

namespace WorkInProgress.Hooks
{
    public enum eItemIDs
    {
        TentKit = 5000010,
        BedrollKit = 5000020,
        Bedroll = 5000021,
        CamoTentKit = 5000030,
        FurTentKit = 5000040,
        LuxuryTent = 5000050,
        MageTent = 5000060,
        FlintAndSteel = 5600010,

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

    /*[HarmonyPatch(typeof(InteractionLightUp), "Activate")]
    public class InteractionLightUp_Activate
    {
        [HarmonyPrefix]
        public static bool Activate(InteractionLightUp __instance, Character _character)
        {
            // Activate > OnActivate > ActivationDone
            try
            {
                IList<Item> ownedItems = _character.Inventory.GetOwnedItems(5600010);
                if (ownedItems.Count > 0)
                {
                    Item m_cachedFlint = ownedItems[0];
                    if (m_cachedFlint.CurrentDurability
                }
                return true;
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

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
                WorkInProgress.Instance.MyLogger.LogDebug($"RestrictUsageOfTents[{(__instance as InteractionSleep).Item.name}]={ds.CurrentDurability}");
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
                    _character.CharacterUI.ShowInfoNotification("Remove bagpack before sleep!");
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
            WorkInProgress.Instance.MyLogger.LogDebug("RestoreDurabilityOnTakeItem=" + takenItem.name);
            AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(takenItem, WorkInProgress.Instance.LastDeployableDurability);
        }
    }*/
}
