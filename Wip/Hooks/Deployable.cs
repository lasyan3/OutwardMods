using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorkInProgress.Hooks
{
    /*[HarmonyPatch(typeof(Deployable), "OnItemUse")]
    public class Deployable_OnItemUse
    {
        [HarmonyPostfix]
        public static void OnItemUse(Deployable __instance)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug($"Deployable_OnItemUse[{__instance.name}]");
                WorkInProgress.Instance.MyLogger.LogDebug($"Deployable_OnItemUse.Item[{__instance.Item.name}]");
                //WorkInProgress.Instance.MyLogger.LogDebug($"Deployable_OnItemUse[{__instance.Item.Name}]=C" + __instance.Item.CurrentDurability + "/S" + __instance.Item.StartingDurability + "/M" + s.MaxDurability);
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }*/

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
    public class SetCustomDurable
    {
        [HarmonyPrefix]
        public static bool DeployableCast(Deployable __instance)
        {
            try
            {
                if (__instance.Item.ItemID == (int)eItemIDs.BedrollKit && __instance.DeployedStateItemPrefab)
                {
                    WorkInProgress.Instance.MyLogger.LogDebug($"preDeployableCast_Deployed[{__instance.name}]={__instance.Item.CurrentDurability}");
                    __instance.DeployedStateItemPrefab.GetComponent<CustomDurable>().CurrentDurability = __instance.Item.CurrentDurability;
                }
                else if (__instance.Item.ItemID == (int)eItemIDs.Bedroll && __instance.PackedStateItemPrefab)
                {
                    var curDs = __instance.Item.GetComponent<CustomDurable>();
                    WorkInProgress.Instance.MyLogger.LogDebug($"preDeployableCast_Packed[{__instance.name}]={curDs.CurrentDurability}");
                    __instance.PackedStateItemPrefab.GetComponent<CustomDurable>().CurrentDurability = curDs.CurrentDurability;
                }

                /*//WorkInProgress.Instance.MyLogger.LogDebug($"DeployableCast[{__instance.name}]=" + __instance.Item.CurrentDurability);
                if (__instance.IsToolkit)
                {
                    if (AccessTools.Field(typeof(Deployable), "m_itemDeployedOn").GetValue(__instance) == null)
                    {
                        if ((bool)__instance.DeployedStateItemPrefab && !PhotonNetwork.isNonMasterClientInRoom)
                        {
                            WorkInProgress.Instance.MyLogger.LogDebug($"DeployedStateItemPrefab={__instance.DeployedStateItemPrefab.ItemIDString}");
                            Item item = UnityEngine.Object.Instantiate(__instance.DeployedStateItemPrefab);
                            Vector3 m_deployPosition = (Vector3)AccessTools.Field(typeof(Deployable), "m_deployPosition").GetValue(__instance);
                            Quaternion m_deployedRotation = (Quaternion)AccessTools.Field(typeof(Deployable), "m_deployedRotation").GetValue(__instance);
                            item.ChangeParent(null, m_deployPosition, m_deployedRotation);
                            item.SetForceSyncPos();
                            item.GetComponent<Deployable>().StartDeployAnimation();
                            Dropable component = item.GetComponent<Dropable>();
                            if ((bool)component)
                            {
                                component.GenerateContents();
                            }
                            AccessTools.Field(typeof(Deployable), "m_changingState").SetValue(__instance, 0);
                            if (!(__instance.Item is Skill))
                            {
                                __instance.Item.RemoveQuantity(1);
                            }
                            //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(item, __instance.Item.CurrentDurability);
                            //DeployableStats ds = UnityEngine.Object.Instantiate
                            //item.AddItemExtension(new DeployableStats(42f));
                            WorkInProgress.Instance.MyLogger.LogDebug($"DeployableCast={item.gameObject.name}");
                            //WorkInProgress.Instance.LastGameObjectDeployed = item.gameObject;
                            //WorkInProgress.Instance.LastDeployableDurability = __instance.Item.CurrentDurability;
                            //item.GetComponent<DeployableStats>().TargetName = __instance.Item.name;
                            item.GetComponent<DeployableStats>().CurrentDurability = __instance.Item.CurrentDurability;
                            return false;
                        }
                    }
                }
                else
                {
                    if ((bool)__instance.PackedStateItemPrefab && !PhotonNetwork.isNonMasterClientInRoom && !(__instance.PackedStateItemPrefab is Skill))
                    {
                        Item item3 = UnityEngine.Object.Instantiate(__instance.PackedStateItemPrefab);
                        Character m_character = (Character)AccessTools.Field(typeof(Deployable), "m_character").GetValue(__instance);
                        if (__instance.AutoTake && m_character != null)
                        {
                            if ((bool)m_character.CharacterUI)
                            {
                                m_character.Inventory.NotifyItemTake(item3, 1);
                            }
                            m_character.Inventory.TakeItem(item3, _tryToEquip: false);
                        }
                        else
                        {
                            item3.transform.position = __instance.Item.transform.position + __instance.DisassembleOffset;
                        }
                        //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(item3, WorkInProgress.Instance.LastDeployableDurability);
                        WorkInProgress.Instance.MyLogger.LogDebug("RestoreDurabilityOnTakeItem=" + item3.CurrentDurability);
                        var stats = item3.GetComponent<ItemStats>();
                        AccessTools.Field(typeof(ItemStats), "m_currentDurability").SetValue(stats, 1);
                        return false;
                    }
                }//*/
                /*Item m_itemDeployedOn = (Item)AccessTools.Field(typeof(Deployable), "m_itemDeployedOn").GetValue(__instance);
                Item m_item = (Item)AccessTools.Field(typeof(Deployable), "m_item").GetValue(__instance);
                Character m_character = (Character)AccessTools.Field(typeof(Deployable), "m_character").GetValue(__instance);
                int m_changingState = (int)AccessTools.Field(typeof(Deployable), "m_changingState").GetValue(__instance);
                Item AddOnPrefab = (Item)AccessTools.Field(typeof(Deployable), "AddOnPrefab").GetValue(__instance);

                if (__instance.IsToolkit)
                {
                    if (m_itemDeployedOn == null)
                    {
                        if ((bool)__instance.DeployedStateItemPrefab && !PhotonNetwork.isNonMasterClientInRoom)
                        {
                            Item item = UnityEngine.Object.Instantiate(__instance.DeployedStateItemPrefab);
                            Vector3 m_deployPosition = (Vector3)AccessTools.Field(typeof(Deployable), "m_deployPosition").GetValue(__instance);
                            Quaternion m_deployedRotation = (Quaternion)AccessTools.Field(typeof(Deployable), "m_deployedRotation").GetValue(__instance);
                            item.ChangeParent(null, m_deployPosition, m_deployedRotation);
                            item.SetForceSyncPos();
                            item.GetComponent<Deployable>().StartDeployAnimation();
                            Dropable component = item.GetComponent<Dropable>();
                            if ((bool)component)
                            {
                                component.GenerateContents();
                            }
                        }
                    }
                    else if ((bool)AddOnPrefab)
                    {
                        Transform transform;
                        if (m_itemDeployedOn.ChildAddOnMngr == null)
                        {
                            GameObject gameObject = new GameObject("AddOnManager");
                            m_itemDeployedOn.SetChildAddOnManager(gameObject.AddComponent<ItemAddOnManager>());
                            gameObject.transform.SetParent(m_itemDeployedOn.transform);
                            gameObject.transform.ResetLocal();
                            transform = gameObject.transform;
                        }
                        else
                        {
                            transform = m_itemDeployedOn.ChildAddOnMngr.transform;
                        }
                        if (!PhotonNetwork.isNonMasterClientInRoom)
                        {
                            Item item2 = UnityEngine.Object.Instantiate(AddOnPrefab);
                            item2.ChangeParent(transform);
                            item2.transform.ResetLocal();
                            item2.SetForceSyncPos();
                            item2.GetComponent<Deployable>().StartDeployAnimation();
                            Dropable component2 = item2.GetComponent<Dropable>();
                            if ((bool)component2)
                            {
                                component2.GenerateContents();
                            }
                        }
                    }
                    m_changingState = 0;
                    if (!(m_item is Skill))
                    {
                        m_item.RemoveQuantity(1);
                    }
                }
                else
                {
                    if (!(bool)__instance.PackedStateItemPrefab)
                    {
                        return false;
                    }
                    m_changingState = 0;
                    if (!PhotonNetwork.isNonMasterClientInRoom)
                    {
                        if (!(__instance.PackedStateItemPrefab is Skill))
                        {
                            Item item3 = UnityEngine.Object.Instantiate(__instance.PackedStateItemPrefab);
                            if (__instance.AutoTake && m_character != null)
                            {
                                if ((bool)m_character.CharacterUI)
                                {
                                    m_character.Inventory.NotifyItemTake(item3, 1);
                                }
                                m_character.Inventory.TakeItem(item3, _tryToEquip: false);
                            }
                            else
                            {
                                item3.transform.position = m_item.transform.position + __instance.DisassembleOffset;
                            }
                        }
                        if (!(m_item is Skill))
                        {
                            ItemManager.Instance.DestroyItem(m_item.UID);
                        }
                    }
                    else if (!(__instance.PackedStateItemPrefab is Skill))
                    {
                        Item item4 = UnityEngine.Object.Instantiate(__instance.PackedStateItemPrefab);
                        if (__instance.AutoTake && m_character != null && (bool)m_character.CharacterUI)
                        {
                            m_character.Inventory.NotifyItemTake(item4, 1);
                        }
                    }
                }
                AccessTools.Field(typeof(Deployable), "m_changingState").SetValue(__instance, m_changingState);
                return false;*/
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("DeployableCast: " + ex.Message);
            }
            return true;
        }

        /*[HarmonyPostfix]
        public static void DeployableCastPost(Deployable __instance)
        {
            //WorkInProgress.Instance.MyLogger.LogDebug($"DeployableCastPost[{__instance.name}]={__instance.Item.CurrentDurability}");
            if (__instance.IsToolkit)
            {
                // Search for recently created GameObject with DeployableStats not yet set
                var list = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.StartsWith("5000021_Bedroll")
                    && g.GetComponent<DeployableStats>() != null
                    && string.IsNullOrEmpty(g.GetComponent<DeployableStats>().TargetName)
                    && g.GetComponent<DeployableStats>().CurrentDurability == -1
                    ).ToList();
                foreach (var go in list)
                {
                    go.GetComponent<DeployableStats>().CurrentDurability = __instance.Item.CurrentDurability;
                    WorkInProgress.Instance.MyLogger.LogDebug($" > {go.name}: CurrentDurability={go.GetComponent<DeployableStats>().CurrentDurability}");
                    //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(go.GetComponent<Item>(), __instance.Item.CurrentDurability);
                }
            }
            else
            {
                var list = Resources.FindObjectsOfTypeAll<Item>().Where(g => g.name.StartsWith("5000020_Bedroll")
                    && g.CurrentDurability == 100
                    ).ToList();
                foreach (var go in list)
                {
                    //go.GetComponent<DeployableStats>().CurrentDurability = __instance.Item.CurrentDurability;
                    WorkInProgress.Instance.MyLogger.LogDebug($" > {go.name}: CurrentDurability={go.CurrentDurability}");
                    //AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(go.GetComponent<Item>(), __instance.Item.CurrentDurability);
                }
            }
        }*/
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
