using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Character;
using SideLoader;
using WorkInProgress.Hooks;

namespace WorkInProgress
{
    public class StrDurability
    {
        public int MaxDurability;
    }


    [BepInPlugin(ID, NAME, VERSION)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class WorkInProgress : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.wip";
        const string NAME = "WorkInProgress";
        const string VERSION = "1.0.1";

        public static WorkInProgress Instance;
        public ManualLogSource MyLogger { get { return Logger; } }

        public static readonly Dictionary<eItemIDs, StrDurability> ItemDurabilities = new Dictionary<eItemIDs, StrDurability>
        {
            {
                eItemIDs.FlintAndSteel,
                new StrDurability { MaxDurability = 5 }
            },
            {
                eItemIDs.BedrollKit,
                new StrDurability {
                    MaxDurability = 5
                }
            },
            {
                eItemIDs.Bedroll,
                new StrDurability {
                    MaxDurability = 5
                }
            },
        };

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                SL.OnPacksLoaded += SL_OnPacksLoaded;
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }

        private void SL_OnPacksLoaded()
        {
            try
            {
                var prefab = CustomItems.CreateCustomItem((int)eItemIDs.FlintAndSteel, (int)eItemIDs.FlintAndSteel, eItemIDs.FlintAndSteel.ToString());
                MultipleUsage m_stackable = (MultipleUsage)AccessTools.Field(typeof(Item), "m_stackable").GetValue(prefab);
                m_stackable.AutoStack = false;
                var stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, ItemDurabilities[eItemIDs.FlintAndSteel].MaxDurability);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.Destroy;

                prefab = CustomItems.CreateCustomItem((int)eItemIDs.BedrollKit, (int)eItemIDs.BedrollKit, eItemIDs.BedrollKit.ToString());
                stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, ItemDurabilities[eItemIDs.BedrollKit].MaxDurability);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;
                prefab.gameObject.AddComponent<CustomDurable>();
                prefab = CustomItems.CreateCustomItem((int)eItemIDs.Bedroll, (int)eItemIDs.Bedroll, "whatever");
                prefab.gameObject.AddComponent<CustomDurable>();

                prefab = CustomItems.CreateCustomItem((int)eItemIDs.TentKit, (int)eItemIDs.TentKit, eItemIDs.TentKit.ToString());
                stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, 10);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;

                prefab = CustomItems.CreateCustomItem((int)eItemIDs.CamoTentKit, (int)eItemIDs.CamoTentKit, eItemIDs.CamoTentKit.ToString());
                stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, 15);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;

            }
            catch (Exception ex)
            {
                MyLogger.LogError("SL_OnPacksLoaded: " + ex.Message);
            }
        }



        //obj = new GameObject(ID);
        //GameObject.DontDestroyOnLoad(obj);
        //script = obj.AddComponent<TestScript>();

        /*obj = new GameObject(ID);
        GameObject.DontDestroyOnLoad(obj);
        script = obj.AddComponent<SortingScript>();*/

        // Disable flag showing new items
        //On.Item.ChangeOwner += Item_ChangeOwner;

        // Repair all items in inventory: CharacterEquipment.RepairEquipment
        //On.CharacterEquipment.RepairEquipmentAfterRest += CharacterEquipment_RepairEquipmentAfterRest;

        // Add light on wandering npcs
        //On.SNPC.OnEnable += SNPC_OnEnable;

        #region Skills tests
        //On.CharacterEquipment.GetTotalMovementModifier += CharacterEquipment_GetTotalMovementModifier; // Update skill value
        //On.ItemDetailsDisplay.RefreshDetail += ItemDetailsDisplay_RefreshDetail; // Update display with Passive skills modificators
        //On.ResourcesPrefabManager.GenerateItem += ResourcesPrefabManager_GenerateItem;
        //On.LocalizationManager.GetItemName += LocalizationManager_GetItemName;
        //On.LocalizationManager.GetItemDesc += LocalizationManager_GetItemDesc;
        //On.Trainer.GetSkillTree += Trainer_GetSkillTree;
        ////On.CharacterSave.PrepareSave += CharacterSave_PrepareSave;
        ///*On.ItemManager.OnReceiveItemSync += ItemManager_OnReceiveItemSync;
        ////On.ItemManager.LoadItems += ItemManager_LoadItems;
        ////On.ItemManager.LoadItemsForCharacter += ItemManager_LoadItemsForCharacter;
        ////On.EnvironmentSave.ApplyData += EnvironmentSave_ApplyData;
        //On.EnvironmentSave.PrepareSave += EnvironmentSave_PrepareSave;
        //On.ItemManager.CreateItemFromData += ItemManager_CreateItemFromData;
        //On.ItemManager.AddStaticItemUID += ItemManager_AddStaticItemUID;*/
        #endregion

        #region Spawn Adventurers
        //On.SNPC.StartIdleAnim += SNPC_StartIdleAnim;
        #endregion

        #region Sorting items
        /*On.LocalCharacterControl.UpdateInteraction += LocalCharacterControl_UpdateInteraction;
        CustomKeybindings.AddAction("ChangeSorting", KeybindingsCategory.Actions, ControlType.Both, 5);
        On.ItemListDisplay.SortBy += ItemListDisplay_SortBy;*/
        #endregion

        //On.DT_QuestEventCheats.RefreshCurrentQuestEvent += DT_QuestEventCheats_RefreshCurrentQuestEvent;

        //On.ItemDetailsDisplay.GetTemporaryDesc += ItemDetailsDisplay_GetTemporaryDesc;

        //On.PlayerCharacterStats.UpdateWeight += PlayerCharacterStats_UpdateWeight; // Burden malus

        //On.Character.DodgeAllowed += Character_DodgeAllowed;
        //On.Character.DodgeInput_1 += Character_DodgeInput_1;

        //On.EnvironmentSave.PrepareSave += EnvironmentSave_PrepareSave;

        /*private void Character_DodgeInput_1(On.Character.orig_DodgeInput_1 orig, Character self, Vector3 _direction)
        {
            //orig(self, _direction);
            OLogger.Log("DodgeInput");
            bool m_preparingToSleep = (bool)AccessTools.Field(typeof(Character), "m_preparingToSleep").GetValue(self);
            bool m_currentlyChargingAttack = (bool)AccessTools.Field(typeof(Character), "m_currentlyChargingAttack").GetValue(self);
            bool m_inLocomotion = (bool)AccessTools.Field(typeof(Character), "m_inLocomotion").GetValue(self);
            bool m_nextIsLocomotion = (bool)AccessTools.Field(typeof(Character), "m_nextIsLocomotion").GetValue(self);
            int m_dodgeAllowedInAction = (int)AccessTools.Field(typeof(Character), "m_dodgeAllowedInAction").GetValue(self);
            if (!self.IsPhotonPlayerLocal 
                || !(self.Stats.MovementSpeed > 0f) 
                || m_preparingToSleep 
                //|| !self.HasEnoughStamina(6f)
                || (self.LocomotionAction && !m_currentlyChargingAttack) 
                //|| ((!m_inLocomotion || !m_nextIsLocomotion) && !m_nextIsLocomotion && m_dodgeAllowedInAction <= 0)
                )
            {
                return;
            }
            //AccessTools.Method(typeof(Character), "CancelActions").Invoke(self, null);
            m_dodgeAllowedInAction = 0;
            if ((bool)self.CharacterCamera && self.CharacterCamera.InZoomMode)
            {
                self.SetZoomMode(_zoomed: false);
            }
            SpellCastType m_currentSpellCastType = (SpellCastType)AccessTools.Field(typeof(Character), "m_currentSpellCastType").GetValue(self);
            if (m_currentSpellCastType != SpellCastType.NONE)
            {
                if (m_currentSpellCastType == SpellCastType.PickupBagGround || m_currentSpellCastType == SpellCastType.DropBagGround)
                {
                    OLogger.Log("\tcancel");
                    self.ForceCancel(_backToLocomotion: false);
                }
                self.ResetCastType();
            }
            //self.ForceCancel(_backToLocomotion: false);
            self.photonView.RPC("SendDodgeTriggerTrivial", PhotonTargets.All, _direction);
            //self.ActionPerformed();
            self.Invoke("ResetDodgeTrigger", 0.5f);
        }//*/

        /*private void Character_DodgeAllowed(On.Character.orig_DodgeAllowed orig, Character self, int _allowed)
        {
            orig(self, _allowed);
            OLogger.Log("DodgeAllowed");
            //Animator m_animator = (Animator)AccessTools.Field(typeof(Character), "m_animator").GetValue(self);
            //AnimatorStateInfo nextAnimatorStateInfo = m_animator.GetNextAnimatorStateInfo(0);
            //if (nextAnimatorStateInfo.length == 0f || nextAnimatorStateInfo.IsTag("Locomotion"))
            //{
            //    AccessTools.Field(typeof(Character), "m_dodgeAllowedInAction").SetValue(self, _allowed);
            //    //m_dodgeAllowedInAction = _allowed;
            //}
        }//*/

        //private void ItemManager_AddStaticItemUID(On.ItemManager.orig_AddStaticItemUID orig, ItemManager self, Item _newItem, string _UID)
        //{
        //    if (_newItem.ItemIDString.StartsWith("820522")) OLogger.Log($"AddStaticItemUID={_newItem.Name}");
        //    orig(self, _newItem, _UID);
        //}

        //private bool ItemManager_CreateItemFromData(On.ItemManager.orig_CreateItemFromData orig, ItemManager self, string itemUID, string[] itemInfos, bool _characterItem)
        //{
        //    if (itemInfos[1].StartsWith("820522")) OLogger.Log($"CreateItemFromData={string.Join(",", itemInfos)}");
        //    bool res = orig(self, itemUID, itemInfos, _characterItem);
        //    return res;
        //}

        //private void EnvironmentSave_PrepareSave(On.EnvironmentSave.orig_PrepareSave orig, EnvironmentSave self)
        //{
        //    OLogger.Log("EnvironmentSave_PrepareSave");
        //    orig(self);
        //    OLogger.Log(self.ItemList.Count);
        //    DictionaryExt<string, Item> worldItems = ItemManager.Instance.WorldItems; // uniquement les skills appris !
        //    Item _outValue = null;
        //    foreach (string key in worldItems.Keys)
        //    {
        //        if (!worldItems.TryGetValue(key, out _outValue) || !_outValue.ItemIDString.StartsWith("820522")) continue;
        //        OLogger.Log($"{_outValue.Name}={_outValue.ItemIDString}");
        //        /*if (worldItems.TryGetValue(key, out _outValue) && !_outValue.IsChildToPlayer && !_outValue.NonSavable && !_outValue.IsPendingDestroy && !_outValue.IsBeingTaken && (_outValue.OwnerCharacter == null || !_outValue.OwnerCharacter.IsItemCharacter) && (!(bool)_outValue.OwnerCharacter || !_outValue.OwnerCharacter.NonSavable) && !(_outValue is Quest))
        //        {
        //            OLogger.Log($"{_outValue.Name}={_outValue.ItemIDString}");
        //        }*/
        //        OLogger.Log($"{_outValue.ItemIDString}={_outValue.IsChildToPlayer} {_outValue.OwnerCharacter == null}");
        //        if (!_outValue.IsChildToPlayer)
        //        {
        //            var sa = self.ItemList.FirstOrDefault(i => i.Identifier == _outValue.SaveIdentifier);
        //            if (sa != null)
        //            {
        //                self.ItemList.Remove(sa);
        //            }
        //        }
        //    }
        //    OLogger.Log(self.ItemList.Count);
        //}

        //private void EnvironmentSave_ApplyData(On.EnvironmentSave.orig_ApplyData orig, EnvironmentSave self)
        //{
        //    OLogger.Log("EnvironmentSave_ApplyData");
        //    orig(self);
        //}

        //private void ItemManager_LoadItemsForCharacter(On.ItemManager.orig_LoadItemsForCharacter orig, ItemManager self, string _charUID, BasicSaveData[] _itemSaves)
        //{
        //    OLogger.Log("LoadItemsForCharacter");
        //    orig(self, _charUID, _itemSaves);
        //}

        //private void ItemManager_LoadItems(On.ItemManager.orig_LoadItems orig, ItemManager self, List<BasicSaveData> _itemSaves, bool _clearAllItems)
        //{
        //    OLogger.Log("LoadItems");
        //    orig(self, _itemSaves, _clearAllItems);
        //}

        //private void ItemManager_OnReceiveItemSync(On.ItemManager.orig_OnReceiveItemSync orig, ItemManager self, string _itemInfos, ItemManager.ItemSyncType _syncType)
        //{
        //    OLogger.Log("OnReceiveItemSync");
        //    orig(self, _itemInfos, _syncType);
        //    string[] array = _itemInfos.Split('~');
        //    foreach (var item in array)
        //    {
        //        if (item.Contains("820522"))
        //            OLogger.Log(item);
        //    }
        //}

        //private void CharacterSave_PrepareSave(On.CharacterSave.orig_PrepareSave orig, CharacterSave self)
        //{
        //    orig(self);
        //    OLogger.Log("CharacterSave_PrepareSave");
        //    Character character = CharacterManager.Instance.GetCharacter(self.PSave.UID);
        //    Item item = null;
        //    foreach (string key in ItemManager.Instance.WorldItems.Keys)
        //    {
        //        item = ItemManager.Instance.WorldItems[key];
        //        if (item.ItemIDString.Contains("820522"))
        //        {
        //            OLogger.Log($"{item.Name}");
        //        }
        //    }
        //    /*foreach (BasicSaveData sv in self.ItemList)
        //    {
        //        OLogger.Log($"{sv.SyncData}");
        //    }*/
        //}

        //private void PlayerCharacterStats_UpdateWeight(On.PlayerCharacterStats.orig_UpdateWeight orig, PlayerCharacterStats self)
        //{
        //    orig(self);
        //    try
        //    {
        //        Character m_character = (Character)AccessTools.Field(typeof(PlayerCharacterStats), "m_character").GetValue(self);
        //        bool m_generalBurdenPenaltyActive = (bool)AccessTools.Field(typeof(PlayerCharacterStats), "m_generalBurdenPenaltyActive").GetValue(self);
        //        float m_generalBurdenRatio = (float)AccessTools.Field(typeof(PlayerCharacterStats), "m_generalBurdenRatio").GetValue(self);
        //        Stat m_movementSpeed = (Stat)AccessTools.Field(typeof(PlayerCharacterStats), "m_movementSpeed").GetValue(self);
        //        Stat m_staminaRegen = (Stat)AccessTools.Field(typeof(PlayerCharacterStats), "m_staminaRegen").GetValue(self);
        //        Stat m_staminaUseModifiers = (Stat)AccessTools.Field(typeof(PlayerCharacterStats), "m_staminaUseModifiers").GetValue(self);

        //        if (m_generalBurdenPenaltyActive)
        //        {
        //            m_generalBurdenRatio = 1f;
        //            m_generalBurdenPenaltyActive = false;
        //            m_movementSpeed.RemoveMultiplierStack("Burden");
        //            m_staminaRegen.RemoveMultiplierStack("Burden");
        //            m_staminaUseModifiers.RemoveMultiplierStack("Burden_Dodge");
        //            m_staminaUseModifiers.RemoveMultiplierStack("Burden_Sprint");
        //        }

        //        float totalWeight = m_character.Inventory.TotalWeight;
        //        bool HasLearnedArmorTraining = m_character.Inventory.SkillKnowledge.IsItemLearned(8205220);
        //        bool HasLearnedArmorMaster = m_character.Inventory.SkillKnowledge.IsItemLearned(NewSkillArmorExpertID);
        //        if (HasLearnedArmorMaster)
        //        {
        //            return;
        //        }
        //        if (totalWeight > 30f)
        //        {
        //            m_generalBurdenPenaltyActive = true;
        //            float num = totalWeight / 30f;
        //            if (HasLearnedArmorTraining)
        //            {
        //                num /= 2f;
        //            }
        //            if (num != m_generalBurdenRatio)
        //            {
        //                m_generalBurdenRatio = num;
        //                // 100 --> 3.0f: 6% / 15%
        //                // 50  --> 1.6f: 3% /  8%

        //                m_movementSpeed.AddMultiplierStack("Burden", num * -0.02f);
        //                m_staminaRegen.AddMultiplierStack("Burden", num * -0.05f);
        //                m_staminaUseModifiers.AddMultiplierStack("Burden_Dodge", num * 0.05f, TagSourceManager.Dodge);
        //                m_staminaUseModifiers.AddMultiplierStack("Burden_Sprint", num * 0.05f, TagSourceManager.Sprint);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error($"UpdateWeight={ex.Message}");
        //    }
        //}

        //private string ItemDetailsDisplay_GetTemporaryDesc(On.ItemDetailsDisplay.orig_GetTemporaryDesc orig, ItemDetailsDisplay self, Item _item)
        //{
        //    try
        //    {
        //        EffectSynchronizer2 test = new EffectSynchronizer2(_item);
        //        List<string> lstEffects = new List<string>();
        //        foreach (var item in test.LstEffects)
        //        {
        //            // AddStatusEffect
        //            // AffectStamina
        //            // AffectBurntHealth
        //            // AffectBurntStamina
        //            // 

        //            try
        //            {
        //                if (item is AddStatusEffect)
        //                    lstEffects.Add((item as AddStatusEffect).Status.StatusName);
        //                //else if (item is AffectStamina)
        //                //lstEffects.Add((item as AffectStamina).ToString());
        //                //else if (item is AffectBurntHealth)
        //                //lstEffects.Add((item as AffectBurntHealth).ToString());
        //                //else if (item is AffectBurntStamina)
        //                //lstEffects.Add((item as AffectBurntStamina).ToString());
        //                //else
        //                //OLogger.Log(item.GetType().Name);
        //            }
        //            catch
        //            {
        //                //OLogger.Error("GetTemporaryDesc");
        //            }
        //        }
        //        string res = orig(self, _item);
        //        if (lstEffects.Count > 0)
        //        {
        //            res += "\r\n\r\n" + string.Join("\r\n", lstEffects.ToArray());
        //        }
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error("GetTemporaryDesc:" + ex.Message);
        //        return orig(self, _item);
        //    }
        //}

        //private void DT_QuestEventCheats_RefreshCurrentQuestEvent(On.DT_QuestEventCheats.orig_RefreshCurrentQuestEvent orig, DT_QuestEventCheats self)
        //{
        //    try
        //    {
        //        foreach (var item in QuestEventDictionary.Sections)
        //        {
        //            item.RelinkEvents();
        //        }
        //        /*Dictionary<string, QuestEventFamily> dctQu = new Dictionary<string, QuestEventFamily>();
        //        for (int i = 0; i < QuestEventDictionary.Sections.Count; i++)
        //        {
        //            //OLogger.Log($"Section={QuestEventDictionary.Sections[i].Name}");
        //            for (int j = 0; j < QuestEventDictionary.Sections[i].Events.Count; j++)
        //            {
        //                //OLogger.Log($"  Event={QuestEventDictionary.Sections[i].Events[j].EventName}");
        //                dctQu.Add(QuestEventDictionary.Sections[i].Events[j].EventUID, QuestEventDictionary.Sections[i]);
        //            }
        //        }
        //        //OLogger.Log("try2");
        //        foreach(var currentQuest in QuestEventManager.Instance.CurrentQuestEvents)
        //        {
        //            QuestEventSignature m_signature = (QuestEventSignature)AccessTools.Field(typeof(QuestEventData), "m_signature").GetValue(currentQuest);
        //            if (dctQu.ContainsKey(currentQuest.EventUID))
        //            {
        //                m_signature.ParentSection = dctQu[currentQuest.EventUID];
        //            }
        //            //OLogger.Log($"{currentQuest.Name}={currentQuest.GetParentFamilly().Name}");
        //        }*/
        //        //OLogger.Log("done");
        //        orig(self); return;

        //        int[] eventPerSections = (int[])AccessTools.Field(typeof(DT_QuestEventCheats), "eventPerSections").GetValue(self);
        //        GameObject m_sectionDisplay = (GameObject)AccessTools.Field(typeof(DT_QuestEventCheats), "m_sectionDisplay").GetValue(self);
        //        GameObject m_questEventDiplay = (GameObject)AccessTools.Field(typeof(DT_QuestEventCheats), "m_questEventDiplay").GetValue(self);
        //        List<DT_QuestEventDisplay> m_currentQuestEvents = (List<DT_QuestEventDisplay>)AccessTools.Field(typeof(DT_QuestEventCheats), "m_currentQuestEvents").GetValue(self);
        //        ScrollRect m_scrollRectCurrent = (ScrollRect)AccessTools.Field(typeof(DT_QuestEventCheats), "m_scrollRectCurrent").GetValue(self);
        //        if (eventPerSections == null)
        //        {
        //            eventPerSections = new int[QuestEventDictionary.Sections.Count];
        //        }
        //        DT_QuestEventDisplay eventDisplay = null;
        //        m_sectionDisplay.SetActive(value: true);
        //        m_questEventDiplay.SetActive(value: true);
        //        for (int i = 0; i < eventPerSections.Length; i++)
        //        {
        //            eventPerSections[i] = 0;
        //        }
        //        IList<QuestEventData> currentQuestEvents = QuestEventManager.Instance.CurrentQuestEvents;
        //        int num = 0;
        //        for (int j = 0; j < currentQuestEvents.Count; j++)
        //        {
        //            int num2 = QuestEventDictionary.Sections.IndexOf(currentQuestEvents[j].GetParentFamilly());
        //            OLogger.Log($"{currentQuestEvents[j].Name} = {currentQuestEvents[j].GetParentFamilly().Name} ({num2})");
        //            eventPerSections[num2]++;
        //            if (j >= m_currentQuestEvents.Count)
        //            {
        //                GameObject gameObject = UnityEngine.Object.Instantiate(m_questEventDiplay, m_scrollRectCurrent.content);
        //                eventDisplay = gameObject.GetComponent<DT_QuestEventDisplay>();
        //                m_currentQuestEvents.Add(eventDisplay);
        //                string eventUID = currentQuestEvents[j].EventUID;
        //                eventDisplay.GetComponentInChildren<Button>().onClick.AddListener(delegate
        //                {
        //                    self.OnRemoveQuestEvent(eventUID, eventDisplay.gameObject);
        //                });
        //            }
        //            else
        //            {
        //                eventDisplay = m_currentQuestEvents[j];
        //            }
        //            num++;
        //            eventDisplay.transform.SetSiblingIndex(num);
        //            eventDisplay.GetComponent<DT_QuestEventDisplay>().SetEvent(currentQuestEvents[j]);
        //        }
        //        for (int k = num; k < m_currentQuestEvents.Count; k++)
        //        {
        //            m_currentQuestEvents[k].gameObject.SetActive(value: false);
        //        }
        //        m_sectionDisplay.SetActive(value: false);
        //        m_questEventDiplay.SetActive(value: false);
        //        AccessTools.Field(typeof(DT_QuestEventCheats), "eventPerSections").SetValue(self, eventPerSections);
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error("RefreshCurrentQuestEvent:" + ex.Message);
        //    }
        //}

        //private void NetworkLevelLoader_UnPauseGameplay(On.NetworkLevelLoader.orig_UnPauseGameplay orig, NetworkLevelLoader self, string _identifier)
        //{
        //    orig(self, _identifier);
        //}

        //private void NPCInteraction_OnActivate(On.NPCInteraction.orig_OnActivate orig, NPCInteraction self)
        //{
        //    orig(self);
        //    try
        //    {
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error("NPCInteraction_OnActivate:" + ex.Message);
        //    }
        //}

        //private void SNPC_StartIdleAnim(On.SNPC.orig_StartIdleAnim orig, SNPC self, float _blend)
        //{
        //    orig(self, _blend);
        //    OLogger.Log($"StartIdle={self.IdleAnimations[0]}");
        //}

        //private int ItemListDisplay_SortByRatio(ItemDisplay _item1, ItemDisplay _item2)
        //{
        //    if (_item1.isActiveAndEnabled && _item2.isActiveAndEnabled)
        //    {
        //        float r1 = _item1.RefItem.Value / _item1.RefItem.Weight;
        //        float r2 = _item2.RefItem.Value / _item2.RefItem.Weight;
        //        return r2.CompareTo(r1);
        //        /*f (num != 0)
        //        {
        //            return num;
        //        }
        //        return orig(_item1, _item2);*/
        //    }
        //    return _item1.isActiveAndEnabled.CompareTo(_item2.isActiveAndEnabled);
        //}
        //private void ItemListDisplay_SortBy(On.ItemListDisplay.orig_SortBy orig, ItemListDisplay self, ItemListDisplay.SortingType _type)
        //{
        //    try
        //    {
        //        if (self.LocalCharacter.IsLocalPlayer && self.CharacterUI.IsInventoryPanelDisplayed && self.ContainerName != "EquipmentDisplay" && m_isSortByRatio)
        //        {
        //            //OLogger.Log($"sort={self.ContainerName}");

        //            List<ItemDisplay> m_assignedDisplays = (List<ItemDisplay>)AccessTools.Field(typeof(ItemListDisplay), "m_assignedDisplays").GetValue(self);
        //            m_assignedDisplays.Sort(ItemListDisplay_SortByRatio);

        //            for (int i = 0; i < m_assignedDisplays.Count; i++)
        //            {
        //                m_assignedDisplays[i].transform.SetSiblingIndex(i);
        //            }
        //            AccessTools.Method(typeof(ItemListDisplay), "ForceRefreshDisplay").Invoke(self, null);
        //        }
        //        else
        //        {
        //            orig(self, _type);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        orig(self, _type);
        //        //DoOloggerError(ex.Message);
        //        //Debug.Log($"[{m_modName}] OnEnable: {ex.Message}");
        //    }
        //}

        //#region Skills
        //private Item ResourcesPrefabManager_GenerateItem(On.ResourcesPrefabManager.orig_GenerateItem orig, ResourcesPrefabManager self, string _itemIDString)
        //{
        //    try
        //    {
        //        if (_itemIDString == NewSkillArmorExpertID.ToString() && !m_isSkillLoaded)
        //        {
        //            Dictionary<string, Item> ITEM_PREFABS = (Dictionary<string, Item>)AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").GetValue(self);
        //            /*if (!ITEM_PREFABS.ContainsKey(_itemIDString))
        //            {
        //                Debug.LogError("Invalid ItemID " + _itemIDString);
        //                return null;
        //            }*/
        //            //m_isSkillLoaded = true;
        //            Item item = ITEM_PREFABS["8205220"];
        //            try
        //            {
        //                item = UnityEngine.Object.Instantiate(item);
        //                if (!(bool)item)
        //                {
        //                    return item;
        //                }
        //                item.ItemID = NewSkillArmorExpertID;
        //                item.name = NewSkillArmorExpertID.ToString() + "_ArmorExpert";
        //                item.gameObject.SetActive(value: true);
        //                return item;
        //            }
        //            catch (Exception message)
        //            {
        //                Debug.LogError(message);
        //                return item;
        //            }
        //        }
        //        else
        //        {
        //            return orig(self, _itemIDString);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //DoOloggerError(ex.Message);
        //        //Debug.Log($"[{m_modName}] SkillTreeDisplay_RefreshTree: {ex.Message}");
        //        OLogger.Error(ex.Message);
        //    }
        //    return orig(self, _itemIDString);
        //}
        //private SkillSchool Trainer_GetSkillTree(On.Trainer.orig_GetSkillTree orig, Trainer self)
        //{
        //    SkillSchool _tree = orig(self);
        //    if (_tree == null) return null;

        //    //OLogger.Log($"SkillSchool={_tree.name}");
        //    List<SkillBranch> lstBranchs = (List<SkillBranch>)AccessTools.Field(typeof(SkillSchool), "m_branches").GetValue(_tree);

        //    /*for (int i = 0; i < _tree.SkillSlots.Count; i++)
        //    {
        //        BaseSkillSlot slot = _tree.SkillSlots[i];
        //        OLogger.Log($"{i}: {slot.GetType().Name}");
        //        if (slot is SkillSlot)
        //        {
        //            SkillSlot ss = slot as SkillSlot;
        //            if (ss.Skill != null) OLogger.Log($" |-   Skill: {ss.Skill.DisplayName}");
        //            OLogger.Log($" |-   ColIdx: {ss.ColumnIndex}");
        //            if (ss.ParentBranch != null) OLogger.Log($" |-   ParentBranch: {ss.ParentBranch.Index}");
        //            //if (ss.RequiredSkillSlot != null) OLogger.Log($" |-   Req: {ss.RequiredSkillSlot}");
        //            //if (ss.SiblingSlot != null) OLogger.Log($" |-   Sibling: {ss.SiblingSlot}");
        //        }
        //        else
        //        {
        //            //OLogger.Log($" |- {slot.GetType().Name}");
        //        }
        //    }//*/

        //    // Add new skill
        //    if (_tree.name == "AbrassarMercenary")
        //    {
        //        Skill skNew = (Skill)ResourcesPrefabManager.Instance.GenerateItem(NewSkillArmorExpertID.ToString());
        //        SkillSlot slotNew = UnityEngine.Object.Instantiate((SkillSlot)_tree.SkillSlots[3]);
        //        AccessTools.Field(typeof(SkillSlot), "m_columnIndex").SetValue(slotNew, 3);
        //        slotNew.ParentBranch = lstBranchs.First(b => b.name == "Row4");
        //        slotNew.RequiredSkillSlot = _tree.SkillSlots[3];
        //        slotNew.RequiresBreakthrough = true;
        //        AccessTools.Field(typeof(SkillSlot), "m_requiredAffiliatedFaction").SetValue(slotNew, Character.StoryFactions.None);
        //        AccessTools.Field(typeof(SkillSlot), "m_skill").SetValue(slotNew, skNew);
        //        AccessTools.Field(typeof(SkillSlot), "m_requiredMoney").SetValue(slotNew, 1500);
        //        _tree.SkillSlots.Add(slotNew);
        //        slotNew.ParentBranch.SkillSlots.Add(slotNew);
        //    }

        //    return _tree;
        //}
        //private string LocalizationManager_GetItemDesc(On.LocalizationManager.orig_GetItemDesc orig, LocalizationManager self, int _itemID)
        //{
        //    switch (_itemID)
        //    {
        //        case NewSkillArmorExpertID:
        //            return "Removes the stamina and movement penalties from wearing armor.";
        //        default:
        //            return orig(self, _itemID);
        //    }
        //}
        //private string LocalizationManager_GetItemName(On.LocalizationManager.orig_GetItemName orig, LocalizationManager self, int _itemID)
        //{
        //    switch (_itemID)
        //    {
        //        case NewSkillArmorExpertID:
        //            return "Armor Mastering";
        //        default:
        //            return orig(self, _itemID);
        //    }
        //}
        //private bool ItemDetailsDisplay_RefreshDetail(On.ItemDetailsDisplay.orig_RefreshDetail orig, ItemDetailsDisplay self, int _rowIndex, ItemDetailsDisplay.DisplayedInfos _infoType)
        //{
        //    try
        //    {
        //        string locItemStat = "";
        //        float alteredPenalty = 0f;
        //        bool flag = self.LocalCharacter.Inventory.SkillKnowledge.IsItemLearned(8205220);
        //        bool flag2 = self.LocalCharacter.Inventory.SkillKnowledge.IsItemLearned(NewSkillArmorExpertID);
        //        Equipment cachedEquip = (Equipment)AccessTools.Field(typeof(ItemDetailsDisplay), "cachedEquipment").GetValue(self);
        //        //OLogger.Log($"{cachedEquip.DisplayName} ({cachedEquip.ItemIDString})");
        //        if (_infoType == ItemDetailsDisplay.DisplayedInfos.MovementPenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
        //        {
        //            if (cachedEquip.MovementPenalty > 0f)
        //            {
        //                if (flag)
        //                {
        //                    alteredPenalty = cachedEquip.MovementPenalty * 0.5f;
        //                }
        //                if (flag2)
        //                {
        //                    alteredPenalty = 0f;
        //                }
        //                locItemStat = "ItemStat_MovementPenalty";
        //            }
        //        }
        //        if (_infoType == ItemDetailsDisplay.DisplayedInfos.StamUsePenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
        //        {
        //            if (cachedEquip.StaminaUsePenalty > 0f && flag)
        //            {
        //                if (flag)
        //                {
        //                    alteredPenalty = cachedEquip.StaminaUsePenalty * 0.5f;
        //                }
        //                if (flag2)
        //                {
        //                    alteredPenalty = 0f;
        //                }
        //                locItemStat = "ItemStat_StaminaUsePenalty";
        //            }
        //        }
        //        if (_infoType == ItemDetailsDisplay.DisplayedInfos.HeatRegenPenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
        //        {
        //            if (cachedEquip.StaminaUsePenalty > 0f && flag)
        //            {
        //                if (flag)
        //                {
        //                    alteredPenalty = cachedEquip.HeatRegenPenalty * 0.5f;
        //                }
        //                if (flag2)
        //                {
        //                    alteredPenalty = 0f;
        //                }
        //                locItemStat = "ItemStat_HeatRegenPenalty";
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(locItemStat) && alteredPenalty >= 0f)
        //        {
        //            ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { _rowIndex });
        //            string penDisp = (string)AccessTools.Method(typeof(ItemDetailsDisplay), "GetPenaltyDisplay").Invoke(self, new object[] {
        //                0f - alteredPenalty, false, true });
        //            if (alteredPenalty > 0f)
        //            {
        //                row.SetInfo(LocalizationManager.Instance.GetLoc(locItemStat), penDisp);
        //            }
        //            else
        //            {
        //                row.Hide();
        //            }
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error(ex.Message);
        //    }
        //    return orig(self, _rowIndex, _infoType);
        //}
        //private float CharacterEquipment_GetTotalMovementModifier(On.CharacterEquipment.orig_GetTotalMovementModifier orig, CharacterEquipment self)
        //{
        //    float num = 0f;
        //    Character m_character = (Character)AccessTools.Field(typeof(CharacterEquipment), "m_character").GetValue(self);
        //    bool flag = m_character.Inventory.SkillKnowledge.IsItemLearned(8205220);
        //    bool flag2 = m_character.Inventory.SkillKnowledge.IsItemLearned(NewSkillArmorExpertID);
        //    for (int i = 0; i < self.EquipmentSlots.Length; i++)
        //    {
        //        if (!self.HasItemEquipped(i) || (self.EquipmentSlots[i].SlotType == EquipmentSlot.EquipmentSlotIDs.LeftHand && self.EquipmentSlots[i].EquippedItem.TwoHanded))
        //        {
        //            continue;
        //        }
        //        float num2 = self.EquipmentSlots[i].EquippedItem.MovementPenalty;
        //        if (num2 > 0f)
        //        {
        //            num2 *= m_character.Stats.EquipmentPenaltyModifier;
        //            if (flag)
        //            {
        //                num2 *= 0.5f;
        //            }
        //            if (flag2)
        //            {
        //                num2 = 0f; //*= 0.25f;
        //            }
        //        }
        //        num += num2 * 0.01f;
        //    }
        //    //OLogger.Log($"GetTotalMovementModifier={num}");
        //    return num;
        //}
        //#endregion

        //#region Light on NPC
        //private void ItemLanternVisual_Light(On.ItemLanternVisual.orig_Light orig, ItemLanternVisual self, bool _light, bool _force)
        //{
        //    orig(self, _light, _force);
        //    OLogger.Log($"colorTemperature={self.LanternLight.colorTemperature}");
        //    OLogger.Log($"type={self.LanternLight.type}");
        //    OLogger.Log($"intensity={self.LanternLight.intensity}");
        //    //OLogger.Log($"position={self.LanternLight.transform.position}");
        //}
        //private void SNPC_OnEnable(On.SNPC.orig_OnEnable orig, SNPC self)
        //{
        //    orig(self);
        //    try
        //    {
        //        if (self.gameObject != null)
        //        {
        //            Light testLi = self.gameObject.AddComponent<Light>();
        //            testLi.color = new Color(1.0f, 0.785f, 0.5f, 1.0f);
        //            testLi.type = LightType.Point;
        //            testLi.intensity = 1.1f;
        //            testLi.colorTemperature = 6570;
        //            //testLi.transform.position = self.transform.position + new Vector3(0f, 0.3f, 0f);
        //            //OLogger.Log($"SNPC_OnEnable={self.name}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //OLogger.Error(ex.Message);
        //    }
        //}
        //#endregion

        //private void CharacterEquipment_RepairEquipmentAfterRest(On.CharacterEquipment.orig_RepairEquipmentAfterRest orig, CharacterEquipment self)
        //{
        //    orig(self);
        //    try
        //    {
        //        Character m_character = (Character)AccessTools.Field(typeof(CharacterEquipment), "m_character").GetValue(self);
        //        int repairLength = m_character.CharacterResting.GetRepairLength();
        //        float num = m_character.PlayerStats.RestRepairEfficiency;
        //        foreach (Item it in self.LastOwnedBag.Container.GetContainedItems())
        //        {
        //            if (it.RepairedInRest)
        //            {
        //                float num2 = num * (float)repairLength * 0.01f;
        //                float num3 = it.DurabilityRatio + num2;
        //                if (num3 > 1f)
        //                {
        //                    num3 = 1f;
        //                }
        //                it.RepairRatio(num3);
        //            }
        //        }
        //        foreach (Item it in m_character.Inventory.Pouch.GetContainedItems())
        //        {
        //            if (it.RepairedInRest && it.IsEquippable)
        //            {
        //                float num2 = num * (float)repairLength * 0.01f;
        //                float num3 = it.DurabilityRatio + num2;
        //                if (num3 > 1f)
        //                {
        //                    num3 = 1f;
        //                }
        //                it.RepairRatio(num3);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Log($"[{_modName}] RepairEquipmentAfterRest: {ex.Message}");
        //    }
        //}

        //private void Item_ChangeOwner(On.Item.orig_ChangeOwner orig, Item self, Character _newOwner)
        //{
        //    orig(self, _newOwner);
        //    try
        //    {
        //        AccessTools.Field(typeof(Item), "m_isNewInInventory").SetValue(self, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error("OnEnable: " + ex.Message);
        //        Debug.Log($"[{_modName}] OnEnable: {ex.Message}");
        //    }
        //}

        //#region Item Sorting
        //private void LocalCharacterControl_UpdateInteraction(On.LocalCharacterControl.orig_UpdateInteraction orig, LocalCharacterControl self)
        //{
        //    orig(self);
        //    if (self.InputLocked)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        UID charUID = self.Character.UID;
        //        int playerID = self.Character.OwnerPlayerSys.PlayerID;

        //        if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("ChangeSorting"))
        //        {
        //            m_isSortByRatio = !m_isSortByRatio;
        //            self.Character.CharacterUI.SmallNotificationPanel.ShowNotification($"Ratio sort {m_isSortByRatio}", 2f);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OLogger.Error(ex.Message);
        //    }
        //}

        //#endregion
    }
}
