using Harmony;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using NodeCanvas.Tasks.Conditions;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Wip;
using static AreaManager;
using static CustomKeybindings;

namespace Wip
{
    public class WorkInProgress : PartialityMod
    {
        readonly string _modName = "WorkInProgress";
        const int NewSkillArmorExpertID = 8205221;
        bool m_isSortByRatio = false;

        public GameObject obj;
        public string ID = "OTW_WIP";
        public static SortingScript script;

        public WorkInProgress()
        {
            this.ModID = _modName;
            this.Version = "1.0.0";
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

        }

        public override void OnEnable()
        {
            base.OnEnable();

            //obj = new GameObject(ID);
            //GameObject.DontDestroyOnLoad(obj);
            //script = obj.AddComponent<TestScript>();

            /*obj = new GameObject(ID);
            GameObject.DontDestroyOnLoad(obj);
            script = obj.AddComponent<SortingScript>();*/

            // Disable flag showing new items
            On.Item.ChangeOwner += Item_ChangeOwner;

            // Repair all items in inventory: CharacterEquipment.RepairEquipment
            On.CharacterEquipment.RepairEquipmentAfterRest += CharacterEquipment_RepairEquipmentAfterRest;

            // Add light on wandering npcs
            On.SNPC.OnEnable += SNPC_OnEnable;

            // Add ratio information
            On.ItemDetailsDisplay.RefreshDisplay += ItemDetailsDisplay_RefreshDisplay;

            #region Skills tests
            On.CharacterEquipment.GetTotalMovementModifier += CharacterEquipment_GetTotalMovementModifier; // Update skill value
            On.ItemDetailsDisplay.RefreshDetail += ItemDetailsDisplay_RefreshDetail; // Update display with Passive skills modificators
            On.ResourcesPrefabManager.GenerateItem += ResourcesPrefabManager_GenerateItem;
            On.LocalizationManager.GetItemName += LocalizationManager_GetItemName;
            On.LocalizationManager.GetItemDesc += LocalizationManager_GetItemDesc;
            On.Trainer.GetSkillTree += Trainer_GetSkillTree;
            #endregion

            #region Spawn Adventurers
            //On.SNPC.StartIdleAnim += SNPC_StartIdleAnim;
            #endregion

            #region Sorting items
            /*On.LocalCharacterControl.UpdateInteraction += LocalCharacterControl_UpdateInteraction;
            CustomKeybindings.AddAction("ChangeSorting", KeybindingsCategory.Actions, ControlType.Both, 5);
            On.ItemListDisplay.SortBy += ItemListDisplay_SortBy;*/
            #endregion

            On.DT_QuestEventCheats.RefreshCurrentQuestEvent += DT_QuestEventCheats_RefreshCurrentQuestEvent;

            //On.EnvironmentSave.PrepareSave += EnvironmentSave_PrepareSave;
        }

        private void DT_QuestEventCheats_RefreshCurrentQuestEvent(On.DT_QuestEventCheats.orig_RefreshCurrentQuestEvent orig, DT_QuestEventCheats self)
        {
            try
            {
                foreach (var item in QuestEventDictionary.Sections)
                {
                    item.RelinkEvents();
                }
                //orig(self); return;

                int[] eventPerSections = (int[])AccessTools.Field(typeof(DT_QuestEventCheats), "eventPerSections").GetValue(self);
                GameObject m_sectionDisplay = (GameObject)AccessTools.Field(typeof(DT_QuestEventCheats), "m_sectionDisplay").GetValue(self);
                GameObject m_questEventDiplay = (GameObject)AccessTools.Field(typeof(DT_QuestEventCheats), "m_questEventDiplay").GetValue(self);
                List<DT_QuestEventDisplay> m_currentQuestEvents = (List<DT_QuestEventDisplay>)AccessTools.Field(typeof(DT_QuestEventCheats), "m_currentQuestEvents").GetValue(self);
                ScrollRect m_scrollRectCurrent = (ScrollRect)AccessTools.Field(typeof(DT_QuestEventCheats), "m_scrollRectCurrent").GetValue(self);
                if (eventPerSections == null)
                {
                    eventPerSections = new int[QuestEventDictionary.Sections.Count];
                }
                DT_QuestEventDisplay eventDisplay = null;
                m_sectionDisplay.SetActive(value: true);
                m_questEventDiplay.SetActive(value: true);
                for (int i = 0; i < eventPerSections.Length; i++)
                {
                    eventPerSections[i] = 0;
                }
                IList<QuestEventData> currentQuestEvents = QuestEventManager.Instance.CurrentQuestEvents;
                int num = 0;
                for (int j = 0; j < currentQuestEvents.Count; j++)
                {
                    int num2 = QuestEventDictionary.Sections.IndexOf(currentQuestEvents[j].GetParentFamilly());
                    //OLogger.Log($"{currentQuestEvents[j].Name} = {currentQuestEvents[j].GetParentFamilly().Name} ({num2})");
                    eventPerSections[num2]++;
                    if (j >= m_currentQuestEvents.Count)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate(m_questEventDiplay, m_scrollRectCurrent.content);
                        eventDisplay = gameObject.GetComponent<DT_QuestEventDisplay>();
                        m_currentQuestEvents.Add(eventDisplay);
                        string eventUID = currentQuestEvents[j].EventUID;
                        eventDisplay.GetComponentInChildren<Button>().onClick.AddListener(delegate
                        {
                            self.OnRemoveQuestEvent(eventUID, eventDisplay.gameObject);
                        });
                    }
                    else
                    {
                        eventDisplay = m_currentQuestEvents[j];
                    }
                    num++;
                    eventDisplay.transform.SetSiblingIndex(num);
                    eventDisplay.GetComponent<DT_QuestEventDisplay>().SetEvent(currentQuestEvents[j]);
                }
                for (int k = num; k < m_currentQuestEvents.Count; k++)
                {
                    m_currentQuestEvents[k].gameObject.SetActive(value: false);
                }
                m_sectionDisplay.SetActive(value: false);
                m_questEventDiplay.SetActive(value: false);
                AccessTools.Field(typeof(DT_QuestEventCheats), "eventPerSections").SetValue(self, eventPerSections);
            }
            catch (Exception ex)
            {
                OLogger.Error("RefreshCurrentQuestEvent:" + ex.Message);
            }
        }

        private void NetworkLevelLoader_UnPauseGameplay(On.NetworkLevelLoader.orig_UnPauseGameplay orig, NetworkLevelLoader self, string _identifier)
        {
            orig(self, _identifier);
        }

        private void NPCInteraction_OnActivate(On.NPCInteraction.orig_OnActivate orig, NPCInteraction self)
        {
            orig(self);
            try
            {
            }
            catch (Exception ex)
            {
                OLogger.Error("NPCInteraction_OnActivate:" + ex.Message);
            }
        }

        private void SNPC_StartIdleAnim(On.SNPC.orig_StartIdleAnim orig, SNPC self, float _blend)
        {
            orig(self, _blend);
            OLogger.Log($"StartIdle={self.IdleAnimations[0]}");
        }

        private int ItemListDisplay_SortByRatio(ItemDisplay _item1, ItemDisplay _item2)
        {
            if (_item1.isActiveAndEnabled && _item2.isActiveAndEnabled)
            {
                float r1 = _item1.RefItem.Value / _item1.RefItem.Weight;
                float r2 = _item2.RefItem.Value / _item2.RefItem.Weight;
                return r2.CompareTo(r1);
                /*f (num != 0)
                {
                    return num;
                }
                return orig(_item1, _item2);*/
            }
            return _item1.isActiveAndEnabled.CompareTo(_item2.isActiveAndEnabled);
        }
        private void ItemListDisplay_SortBy(On.ItemListDisplay.orig_SortBy orig, ItemListDisplay self, ItemListDisplay.SortingType _type)
        {
            try
            {
                if (self.LocalCharacter.IsLocalPlayer && self.CharacterUI.IsInventoryPanelDisplayed && self.ContainerName != "EquipmentDisplay" && m_isSortByRatio)
                {
                    //OLogger.Log($"sort={self.ContainerName}");

                    List<ItemDisplay> m_assignedDisplays = (List<ItemDisplay>)AccessTools.Field(typeof(ItemListDisplay), "m_assignedDisplays").GetValue(self);
                    m_assignedDisplays.Sort(ItemListDisplay_SortByRatio);

                    for (int i = 0; i < m_assignedDisplays.Count; i++)
                    {
                        m_assignedDisplays[i].transform.SetSiblingIndex(i);
                    }
                    AccessTools.Method(typeof(ItemListDisplay), "ForceRefreshDisplay").Invoke(self, null);
                }
                else
                {
                    orig(self, _type);
                }
            }
            catch (Exception ex)
            {
                orig(self, _type);
                //DoOloggerError(ex.Message);
                //Debug.Log($"[{m_modName}] OnEnable: {ex.Message}");
            }
        }

        #region Skills
        private Item ResourcesPrefabManager_GenerateItem(On.ResourcesPrefabManager.orig_GenerateItem orig, ResourcesPrefabManager self, string _itemIDString)
        {
            try
            {
                if (_itemIDString == NewSkillArmorExpertID.ToString())
                {
                    Dictionary<string, Item> ITEM_PREFABS = (Dictionary<string, Item>)AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").GetValue(self);
                    /*if (!ITEM_PREFABS.ContainsKey(_itemIDString))
                    {
                        Debug.LogError("Invalid ItemID " + _itemIDString);
                        return null;
                    }*/
                    Item item = ITEM_PREFABS["8205220"];
                    try
                    {
                        item = UnityEngine.Object.Instantiate(item);
                        if (!(bool)item)
                        {
                            return item;
                        }
                        item.ItemID = NewSkillArmorExpertID;
                        item.name = NewSkillArmorExpertID.ToString() + "_ArmorExpert";
                        item.gameObject.SetActive(value: true);
                        return item;
                    }
                    catch (Exception message)
                    {
                        Debug.LogError(message);
                        return item;
                    }
                }
                else
                {
                    return orig(self, _itemIDString);
                }
            }
            catch (Exception ex)
            {
                //DoOloggerError(ex.Message);
                //Debug.Log($"[{m_modName}] SkillTreeDisplay_RefreshTree: {ex.Message}");
                OLogger.Error(ex.Message);
            }
            return orig(self, _itemIDString);
        }
        private SkillSchool Trainer_GetSkillTree(On.Trainer.orig_GetSkillTree orig, Trainer self)
        {
            SkillSchool _tree = orig(self);
            if (_tree == null) return null;

            //OLogger.Log($"SkillSchool={_tree.name}");
            List<SkillBranch> lstBranchs = (List<SkillBranch>)AccessTools.Field(typeof(SkillSchool), "m_branches").GetValue(_tree);

            /*for (int i = 0; i < _tree.SkillSlots.Count; i++)
            {
                BaseSkillSlot slot = _tree.SkillSlots[i];
                OLogger.Log($"{i}: {slot.GetType().Name}");
                if (slot is SkillSlot)
                {
                    SkillSlot ss = slot as SkillSlot;
                    if (ss.Skill != null) OLogger.Log($" |-   Skill: {ss.Skill.DisplayName}");
                    OLogger.Log($" |-   ColIdx: {ss.ColumnIndex}");
                    if (ss.ParentBranch != null) OLogger.Log($" |-   ParentBranch: {ss.ParentBranch.Index}");
                    //if (ss.RequiredSkillSlot != null) OLogger.Log($" |-   Req: {ss.RequiredSkillSlot}");
                    //if (ss.SiblingSlot != null) OLogger.Log($" |-   Sibling: {ss.SiblingSlot}");
                }
                else
                {
                    //OLogger.Log($" |- {slot.GetType().Name}");
                }
            }//*/

            // Add new skill
            if (_tree.name == "AbrassarMercenary")
            {
                Skill skNew = (Skill)ResourcesPrefabManager.Instance.GenerateItem(NewSkillArmorExpertID.ToString());
                SkillSlot slotNew = UnityEngine.Object.Instantiate((SkillSlot)_tree.SkillSlots[3]);
                AccessTools.Field(typeof(SkillSlot), "m_columnIndex").SetValue(slotNew, 3);
                slotNew.ParentBranch = lstBranchs.First(b => b.name == "Row3");
                slotNew.RequiredSkillSlot = _tree.SkillSlots[3];
                AccessTools.Field(typeof(SkillSlot), "m_requiredAffiliatedFaction").SetValue(slotNew, Character.StoryFactions.None);
                AccessTools.Field(typeof(SkillSlot), "m_skill").SetValue(slotNew, skNew);
                AccessTools.Field(typeof(SkillSlot), "m_requiredMoney").SetValue(slotNew, 1500);
                _tree.SkillSlots.Add(slotNew);
                slotNew.ParentBranch.SkillSlots.Add(slotNew);
            }

            return _tree;
        }
        private string LocalizationManager_GetItemDesc(On.LocalizationManager.orig_GetItemDesc orig, LocalizationManager self, int _itemID)
        {
            switch (_itemID)
            {
                case NewSkillArmorExpertID:
                    return "Removes the stamina and movement penalties from wearing armor.";
                default:
                    return orig(self, _itemID);
            }
        }
        private string LocalizationManager_GetItemName(On.LocalizationManager.orig_GetItemName orig, LocalizationManager self, int _itemID)
        {
            switch (_itemID)
            {
                case NewSkillArmorExpertID:
                    return "Armor Mastering";
                default:
                    return orig(self, _itemID);
            }
        }
        private bool ItemDetailsDisplay_RefreshDetail(On.ItemDetailsDisplay.orig_RefreshDetail orig, ItemDetailsDisplay self, int _rowIndex, ItemDetailsDisplay.DisplayedInfos _infoType)
        {
            try
            {
                string locItemStat = "";
                float alteredPenalty = 0f;
                bool flag = self.LocalCharacter.Inventory.SkillKnowledge.IsItemLearned(8205220);
                bool flag2 = self.LocalCharacter.Inventory.SkillKnowledge.IsItemLearned(NewSkillArmorExpertID);
                Equipment cachedEquip = (Equipment)AccessTools.Field(typeof(ItemDetailsDisplay), "cachedEquipment").GetValue(self);
                //OLogger.Log($"{cachedEquip.DisplayName} ({cachedEquip.ItemIDString})");
                if (_infoType == ItemDetailsDisplay.DisplayedInfos.MovementPenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
                {
                    if (cachedEquip.MovementPenalty > 0f)
                    {
                        if (flag)
                        {
                            alteredPenalty = cachedEquip.MovementPenalty * 0.5f;
                        }
                        if (flag2)
                        {
                            alteredPenalty = 0f;
                        }
                        locItemStat = "ItemStat_MovementPenalty";
                    }
                }
                if (_infoType == ItemDetailsDisplay.DisplayedInfos.StamUsePenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
                {
                    if (cachedEquip.StaminaUsePenalty > 0f && flag)
                    {
                        if (flag)
                        {
                            alteredPenalty = cachedEquip.StaminaUsePenalty * 0.5f;
                        }
                        if (flag2)
                        {
                            alteredPenalty = 0f;
                        }
                        locItemStat = "ItemStat_StaminaUsePenalty";
                    }
                }
                if (_infoType == ItemDetailsDisplay.DisplayedInfos.HeatRegenPenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
                {
                    if (cachedEquip.StaminaUsePenalty > 0f && flag)
                    {
                        if (flag)
                        {
                            alteredPenalty = cachedEquip.HeatRegenPenalty * 0.5f;
                        }
                        if (flag2)
                        {
                            alteredPenalty = 0f;
                        }
                        locItemStat = "ItemStat_HeatRegenPenalty";
                    }
                }
                if (!string.IsNullOrEmpty(locItemStat) && alteredPenalty >= 0f)
                {
                    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { _rowIndex });
                    string penDisp = (string)AccessTools.Method(typeof(ItemDetailsDisplay), "GetPenaltyDisplay").Invoke(self, new object[] {
                        0f - alteredPenalty, false, true });
                    if (alteredPenalty > 0f)
                    {
                        row.SetInfo(LocalizationManager.Instance.GetLoc(locItemStat), penDisp);
                    }
                    else
                    {
                        row.Hide();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
            return orig(self, _rowIndex, _infoType);
        }
        private float CharacterEquipment_GetTotalMovementModifier(On.CharacterEquipment.orig_GetTotalMovementModifier orig, CharacterEquipment self)
        {
            float num = 0f;
            Character m_character = (Character)AccessTools.Field(typeof(CharacterEquipment), "m_character").GetValue(self);
            bool flag = m_character.Inventory.SkillKnowledge.IsItemLearned(8205220);
            bool flag2 = m_character.Inventory.SkillKnowledge.IsItemLearned(NewSkillArmorExpertID);
            for (int i = 0; i < self.EquipmentSlots.Length; i++)
            {
                if (!self.HasItemEquipped(i) || (self.EquipmentSlots[i].SlotType == EquipmentSlot.EquipmentSlotIDs.LeftHand && self.EquipmentSlots[i].EquippedItem.TwoHanded))
                {
                    continue;
                }
                float num2 = self.EquipmentSlots[i].EquippedItem.MovementPenalty;
                if (num2 > 0f)
                {
                    num2 *= m_character.Stats.EquipmentPenaltyModifier;
                    if (flag)
                    {
                        num2 *= 0.5f;
                    }
                    if (flag2)
                    {
                        num2 = 0f; //*= 0.25f;
                    }
                }
                num += num2 * 0.01f;
            }
            //OLogger.Log($"GetTotalMovementModifier={num}");
            return num;
        }
        #endregion

        private void ItemDetailsDisplay_RefreshDisplay(On.ItemDetailsDisplay.orig_RefreshDisplay orig, ItemDetailsDisplay self, IItemDisplay _itemDisplay)
        {
            orig(self, _itemDisplay);
            try
            {
                //Item m_lastItem = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(self);
                //OLogger.Log($"RefreshDisplay={m_lastItem.DisplayName}");

                Text m_lblItemName = (Text)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lblItemName").GetValue(self);
                //Item it = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(self);
                if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null &&
                    _itemDisplay.RefItem.Value > 0 && _itemDisplay.RefItem.Weight > 0)
                {
                    //OLogger.Log(m_lblItemName.text);
                    /*int invQty = self.LocalCharacter.Inventory.GetOwnedItems(_itemDisplay.RefItem.ItemID).Count;
                    int stashQty = 0;
                    if (StashAreaToStashUID.ContainsKey(m_currentArea))
                    {
                        TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[m_currentArea]);
                        if (stash != null)
                        {
                            stashQty = stash.GetItemsFromID(_itemDisplay.RefItem.ItemID).Count;
                        }
                    }
                    m_lblItemName.text += $" ({invQty + stashQty})";*/
                    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(self);
                    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { m_detailRows.Count });
                    row.SetInfo("Ratio", Math.Round(_itemDisplay.RefItem.Value / _itemDisplay.RefItem.Weight, 2).ToString());
                    //m_lblItemName.text += $" ({_itemDisplay.RefItem.Value}/{_itemDisplay.RefItem.Weight} = {_itemDisplay.RefItem.Value/_itemDisplay.RefItem.Weight})"; 
                }
                /*if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null &&
                    _itemDisplay.RefItem.Value > 0)
                {
                    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(self);
                    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { m_detailRows.Count });
                    row.SetInfo("Value", _itemDisplay.RefItem.Value.ToString());
                }*/
                if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null &&
                    _itemDisplay.RefItem.IsPerishable)
                {
                    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(self);
                    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { m_detailRows.Count });
                    row.SetInfo("Durability", GameTimetoDays(_itemDisplay.RefItem.CurrentDurability / _itemDisplay.RefItem.PerishScript.DepletionRate));
                }
            }
            catch (Exception ex)
            {
                //DoOloggerError(ex.Message);
            }
        }

        #region Light on NPC
        private void ItemLanternVisual_Light(On.ItemLanternVisual.orig_Light orig, ItemLanternVisual self, bool _light, bool _force)
        {
            orig(self, _light, _force);
            OLogger.Log($"colorTemperature={self.LanternLight.colorTemperature}");
            OLogger.Log($"type={self.LanternLight.type}");
            OLogger.Log($"intensity={self.LanternLight.intensity}");
            //OLogger.Log($"position={self.LanternLight.transform.position}");
        }
        private void SNPC_OnEnable(On.SNPC.orig_OnEnable orig, SNPC self)
        {
            orig(self);
            try
            {
                if (self.gameObject != null)
                {
                    Light testLi = self.gameObject.AddComponent<Light>();
                    testLi.color = new Color(1.0f, 0.785f, 0.5f, 1.0f);
                    testLi.type = LightType.Point;
                    testLi.intensity = 1.1f;
                    testLi.colorTemperature = 6570;
                    //testLi.transform.position = self.transform.position + new Vector3(0f, 0.3f, 0f);
                    //OLogger.Log($"SNPC_OnEnable={self.name}");
                }
            }
            catch (Exception ex)
            {
                //OLogger.Error(ex.Message);
            }
        }
        #endregion

        private void CharacterEquipment_RepairEquipmentAfterRest(On.CharacterEquipment.orig_RepairEquipmentAfterRest orig, CharacterEquipment self)
        {
            orig(self);
            try
            {
                Character m_character = (Character)AccessTools.Field(typeof(CharacterEquipment), "m_character").GetValue(self);
                int repairLength = m_character.CharacterResting.GetRepairLength();
                float num = m_character.PlayerStats.RestRepairEfficiency;
                foreach (Item it in self.LastOwnedBag.Container.GetContainedItems())
                {
                    if (it.RepairedInRest)
                    {
                        float num2 = num * (float)repairLength * 0.01f;
                        float num3 = it.DurabilityRatio + num2;
                        if (num3 > 1f)
                        {
                            num3 = 1f;
                        }
                        it.RepairRatio(num3);
                    }
                }
                foreach (Item it in m_character.Inventory.Pouch.GetContainedItems())
                {
                    if (it.RepairedInRest && it.IsEquippable)
                    {
                        float num2 = num * (float)repairLength * 0.01f;
                        float num3 = it.DurabilityRatio + num2;
                        if (num3 > 1f)
                        {
                            num3 = 1f;
                        }
                        it.RepairRatio(num3);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[{_modName}] RepairEquipmentAfterRest: {ex.Message}");
            }
        }

        private void Item_ChangeOwner(On.Item.orig_ChangeOwner orig, Item self, Character _newOwner)
        {
            orig(self, _newOwner);
            try
            {
                AccessTools.Field(typeof(Item), "m_isNewInInventory").SetValue(self, false);
            }
            catch (Exception ex)
            {
                OLogger.Error("OnEnable: " + ex.Message);
                Debug.Log($"[{_modName}] OnEnable: {ex.Message}");
            }
        }

        #region Item Sorting
        private void LocalCharacterControl_UpdateInteraction(On.LocalCharacterControl.orig_UpdateInteraction orig, LocalCharacterControl self)
        {
            orig(self);
            if (self.InputLocked)
            {
                return;
            }

            try
            {
                UID charUID = self.Character.UID;
                int playerID = self.Character.OwnerPlayerSys.PlayerID;

                if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("ChangeSorting"))
                {
                    m_isSortByRatio = !m_isSortByRatio;
                    self.Character.CharacterUI.SmallNotificationPanel.ShowNotification($"Ratio sort {m_isSortByRatio}", 2f);
                }
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        #endregion

        private string GameTimetoDays(double p_gametime)
        {
            string str = "";
            int days = (int)p_gametime / 24;
            if (days > 0) str = $"{days}d, ";
            int hours = (int)p_gametime % 24;
            str += $"{hours}h";
            if (days == 0 && hours == 0)
            {
                int minutes = (int)p_gametime * 60;
                str = $"{minutes} min";
                if (minutes == 0)
                {
                    int seconds = (int)p_gametime * 3600;
                    str = $"{seconds} sec";
                }
            }
            return str;
        }
    }
}
