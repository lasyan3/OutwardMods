using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(Deployable), "OnItemUse")]
    public class Deployable_OnItemUse
    {
        [HarmonyPostfix]
        public static void OnItemUse(Deployable __instance)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"{__instance.name}'s Durability--");
                __instance.Item.ReduceDurability(1);
                //WorkInProgress.Instance.MyLogger.LogDebug($"Deployable_OnItemUse[{__instance.Item.Name}]=C" + __instance.Item.CurrentDurability + "/S" + __instance.Item.StartingDurability + "/M" + s.MaxDurability);
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }

    /*[HarmonyPatch(typeof(Deployable), "Disassemble")]
    public class SaveDurabilityFromDeployableBeforeDisassemble
    {
        [HarmonyPostfix]
        public static void Disassemble(Deployable __instance, Character _character)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug($"Deployable_Disassemble[{__instance.name}]");
                WorkInProgress.Instance.MyLogger.LogDebug($"Deployable_Disassemble[{__instance.Item.name}]=" + __instance.Item.CurrentDurability);
                //WorkInProgress.Instance.LastDeployableDurability = __instance.Item.CurrentDurability;
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("Disassemble: " + ex.Message);
            }
        }
    }*/

    [HarmonyPatch(typeof(Deployable), "DeployableCast")]
    public class PassDurabilityWithCustomDurable
    {
        [HarmonyPrefix]
        public static bool DeployableCast(Deployable __instance)
        {
            try
            {
                //WorkInProgress.Instance.MyLogger.LogDebug($"preDeployableCast[{__instance.name}]");
                if ((__instance.Item.ItemID == (int)eItemIDs.BedrollKit 
                    || __instance.Item.ItemID == (int)eItemIDs.TentKit)
                    && __instance.DeployedStateItemPrefab)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"Set deployed {__instance.name} CustomDurable to {__instance.Item.CurrentDurability}");
                    __instance.DeployedStateItemPrefab.gameObject.GetOrAddComponent<CustomDurable>().CurrentDurability = __instance.Item.CurrentDurability;
                }
                else if ((__instance.Item.ItemID == (int)eItemIDs.Bedroll
                    || __instance.Item.ItemID == (int)eItemIDs.Tent)
                    && __instance.PackedStateItemPrefab)
                {
                    var curDs = __instance.Item.GetComponent<CustomDurable>();
                    WorkInProgress.Instance.MyLogger.LogDebug($"Set Packed {__instance.name} CustomDurable to {curDs.CurrentDurability}");
                    __instance.PackedStateItemPrefab.gameObject.GetOrAddComponent<CustomDurable>().CurrentDurability = curDs.CurrentDurability;
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("DeployableCast: " + ex.Message);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DeployablePlacer), "CancelDeploy")]
    public class CancelDeployDurability
    {
        [HarmonyPostfix]
        public static void CancelDeploy(DeployablePlacer __instance)
        {
            try
            {
                Deployable m_deployedItem = (Deployable)AccessTools.Field(typeof(DeployablePlacer), "m_deployedItem").GetValue(__instance);
                if (Enum.IsDefined(typeof(eItemIDs), m_deployedItem.Item.ItemID))
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"Cancel deployment of {m_deployedItem.Item.name} > Durability++");
                    m_deployedItem.Item.ReduceDurability(-1);
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("CancelDeploy: " + ex.Message);
            }
        }
    }

}
/*
 * Click sur l'objet dans l'inventaire:
[Debug  :WorkInProgress] StartPlacement[5000020_Bedroll kit_W1SN3IKXE0]=C5/S-1/M5
[Debug  :WorkInProgress] Deployable_OnItemUse[5000020_Bedroll kit_W1SN3IKXE0]
[Debug  :WorkInProgress] Deployable_OnItemUse.Item[5000020_Bedroll kit_W1SN3IKXE0]
    Valider le placement:
[Debug  :WorkInProgress] OnReceiveDeploy[5000020_Bedroll kit_W1SN3IKXE0]
[Debug  :WorkInProgress] DeployableCast[5000020_Bedroll kit_W1SN3IKXE0]=C5/S-1/M5
 */
