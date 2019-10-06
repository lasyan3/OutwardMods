using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static CustomKeybindings;

namespace MoreGatherableLoot
{
    public class WorkInProgress : PartialityMod
    {
        private readonly string _modName = "WorkInProgress";
        private readonly int NewSkillArmorExpertID = 8205221;

        public WorkInProgress()
        {
            this.ModID = _modName;
            this.Version = "1.0.0";
            //this.loadPriority = 0;
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

            //On.ItemContainer.AddItem_1 += ItemContainer_AddItem_1;
            //On.SaveInstance.ApplyEnvironment += SaveInstance_ApplyEnvironment;

            /*	CharSave = new CharacterSave() --> ItemList
	            WorldSave = new WorldSave() --> QuestList, QuestEventList, KillEventSendersList
	            LegacyChestSave = new LegacyChestSave();
	            SceneSaves = new Dictionary<string, EnvironmentSave>() --> InteractionActivatorList, InteractionManagersList
            */
            //On.CharacterSave.PrepareSave += CharacterSave_PrepareSave;
            //On.CharacterSave.ApplyLoadedSaveToChar

            /*
            QuestEventManager.AddEvent(_eventUID)
                QuestEventData.CreateEventData(_eventUID)
                    QuestEventDictionary.GetQuestEvent(_uid)
                        QuestEventDictionary.RegisterEvent(QuestEventSignature _event)
                            after QuestEventDictionary.Load, call registerevent
            */
            //On.QuestEventDictionary.Load += QuestEventDictionary_Load;
            //On.QuestEventManager.AddEvent_1 += QuestEventManager_AddEvent_1;

            On.Item.ChangeOwner += Item_ChangeOwner; // Disable flag showing new items

            // Repair all items in inventory: CharacterEquipment.RepairEquipment
            On.CharacterEquipment.RepairEquipmentAfterRest += CharacterEquipment_RepairEquipmentAfterRest;

            #region Add light on wandering npcs
            On.SNPC.OnEnable += SNPC_OnEnable;

            //On.SNPC.ctor += SNPC_ctor; // Stutter
            //On.SNPC.Start += SNPC_Start; // Stutter
            //On.SNPCMoving.ctor += SNPCMoving_ctor; // Stutter
            //On.SNPCMoving.Init += SNPCMoving_Init; // Stutter
            //On.SNPCManager.Update += SNPCManager_Update;
            //On.ItemLanternVisual.Light += ItemLanternVisual_Light;

            /*On.TOD_LightAtNight.Start += TOD_LightAtNight_Start;
            On.LightControl.Start += LightControl_Start;
            On.LightClipChecker.Start += LightClipChecker_Start;
            On.LightProbesFromNavmesh.GenerateFromNavmesh += LightProbesFromNavmesh_GenerateFromNavmesh;
            On.TODLightModif.Awake += TODLightModif_Awake;
            On.TOD_LightParameters.ctor += TOD_LightParameters_ctor;*/
            #endregion

            On.ItemDetailsDisplay.RefreshDisplay += ItemDetailsDisplay_RefreshDisplay; // Add ratio information

            //On.ItemListDisplay.SortBy += ItemListDisplay_SortBy; // Sort items by value/weight ratio

            #region Skills tests
            On.CharacterEquipment.GetTotalMovementModifier += CharacterEquipment_GetTotalMovementModifier; // Update skill value
            On.ItemDetailsDisplay.RefreshDetail += ItemDetailsDisplay_RefreshDetail; // Update display with Passive skills modificators
                                                                                     //On.ResourcesPrefabManager.LoadItemPrefabs += ResourcesPrefabManager_LoadItemPrefabs;
            On.ResourcesPrefabManager.GenerateItem += ResourcesPrefabManager_GenerateItem;
            On.SkillTreeDisplay.RefreshTree += SkillTreeDisplay_RefreshTree;
            On.LocalizationManager.GetItemName += LocalizationManager_GetItemName;
            On.LocalizationManager.GetItemDesc += LocalizationManager_GetItemDesc;
            //On.CharacterInventory.TryUnlockSkill += CharacterInventory_TryUnlockSkill;*/
            #endregion

            //On.NetworkLevelLoader.UnPauseGameplay += NetworkLevelLoader_UnPauseGameplay;
            //On.ResourcesPrefabManager.LoadItemPrefabs += ResourcesPrefabManager_LoadItemPrefabs;
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
                if (self.LocalCharacter.IsLocalPlayer && self.CharacterUI.IsInventoryPanelDisplayed && self.ContainerName != "EquipmentDisplay")
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

        private Item ResourcesPrefabManager_GenerateItem(On.ResourcesPrefabManager.orig_GenerateItem orig, ResourcesPrefabManager self, string _itemIDString)
        {
            if (_itemIDString == NewSkillArmorExpertID.ToString())
            {
                Dictionary<string, Item> ITEM_PREFABS = (Dictionary<string, Item>)AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").GetValue(self);
                if (!ITEM_PREFABS.ContainsKey(_itemIDString))
                {
                    Debug.LogError("Invalid ItemID " + _itemIDString);
                    return null;
                }
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
        private void SkillTreeDisplay_RefreshTree(On.SkillTreeDisplay.orig_RefreshTree orig, SkillTreeDisplay self, SkillSchool _tree)
        {
            try
            {
                /* Replace existing skill --> OK
                  * Add existing skill --> OK
                  * Add new skill --> 
                  */
                List<SkillBranch> lstBranchs = (List<SkillBranch>)AccessTools.Field(typeof(SkillSchool), "m_branches").GetValue(_tree);
                /* Faut créer un SkillSlot mais aussi un SkillSlotDisplay ?!
                 * SkillTreeDisplay
                 *  |- SkillTreeSlotDisplay
                 *  
                 *  SkillSlotDisplay ?
                 */

                // Replace existing skill
                //Skill skReplace = (Skill)ResourcesPrefabManager.Instance.GetItemPrefab(8205190);
                //AccessTools.Field(typeof(SkillSlot), "m_skill").SetValue(_tree.SkillSlots[1], skReplace);

                // Add existing skill
                /*Skill skAdd = (Skill)ResourcesPrefabManager.Instance.GetItemPrefab(8205190);
                SkillSlot slotAdd = UnityEngine.Object.Instantiate((SkillSlot)_tree.SkillSlots[0]);
                AccessTools.Field(typeof(SkillSlot), "m_columnIndex").SetValue(slotAdd, 2);
                slotAdd.ParentBranch = lstBranchs.First(b => b.name == "Row2");
                slotAdd.RequiredSkillSlot = _tree.SkillSlots[2];
                AccessTools.Field(typeof(SkillSlot), "m_requiredAffiliatedFaction").SetValue(slotAdd, Character.StoryFactions.None);
                AccessTools.Field(typeof(SkillSlot), "m_skill").SetValue(slotAdd, skAdd);
                AccessTools.Field(typeof(SkillSlot), "m_requiredMoney").SetValue(slotAdd, 50);
                _tree.SkillSlots.Add(slotAdd);
                slotAdd.ParentBranch.SkillSlots.Add(slotAdd);//*/

                // Add new skill
                //8205221: Expertise en armures
                //8205222: Maitrise en armures
                /*Skill t = (Skill)ResourcesPrefabManager.Instance.GetItemPrefab(8205190);
                //Skill t = UnityEngine.Object.Instantiate((_tree.SkillSlots[0] as SkillSlot).Skill);
                t.ItemID = 8205220; // Ok "Formation en armures" + details
                //t.ItemID = 8205221; //
                //AccessTools.Field(typeof(Skill), "m_itemIcon").SetValue(t, ResourcesPrefabManager.Instance.GetItemIcon(8205220)); // OK
                t.SkillTreeIcon = ResourcesPrefabManager.Instance.GetItemIcon(8205220);
                //Item t = UnityEngine.Object.Instantiate((_tree.SkillSlots[0] as SkillSlot).Skill);
                //t.ItemID = 8205220; // Ok "Formation en armures" + details
                //t.ItemID = 8205221; //
                Item t = UnityEngine.Object.Instantiate((_tree.SkillSlots[0] as SkillSlot).Skill);
                t.ItemID = 8205220;*/
                //t.ItemIcon = ResourcesPrefabManager.Instance.GetItemIcon(8205220);
                Skill skNew = (Skill)ResourcesPrefabManager.Instance.GenerateItem("8205221");
                //skNew.ItemID = 8205221;
                //skNew.name = "8205221_ArmorExpert";
                SkillSlot slotNew = UnityEngine.Object.Instantiate((SkillSlot)_tree.SkillSlots[0]);
                AccessTools.Field(typeof(SkillSlot), "m_columnIndex").SetValue(slotNew, 1);
                slotNew.ParentBranch = lstBranchs.First(b => b.name == "Row2");
                slotNew.RequiredSkillSlot = _tree.SkillSlots[0];
                AccessTools.Field(typeof(SkillSlot), "m_requiredAffiliatedFaction").SetValue(slotNew, Character.StoryFactions.None);
                AccessTools.Field(typeof(SkillSlot), "m_skill").SetValue(slotNew, skNew);
                AccessTools.Field(typeof(SkillSlot), "m_requiredMoney").SetValue(slotNew, 500);
                _tree.SkillSlots.Add(slotNew);
                slotNew.ParentBranch.SkillSlots.Add(slotNew);//*/

                //foreach (var branch in lstBranchs)
                //{
                //OLogger.Log($"{branch.Index}: {branch.name} ({branch.SkillSlots.Count})");
                //foreach (var slot in branch.SkillSlots)
                /*foreach (var slot in _tree.SkillSlots)
                {
                        OLogger.Log($"{slot.GetType().Name}");
                        if (slot is SkillSlot)
                        {
                            SkillSlot ss = slot as SkillSlot;
                        if (ss.Skill != null) OLogger.Log($" |-   Skill: {ss.Skill.DisplayName}");
                            OLogger.Log($" |-   ColIdx: {ss.ColumnIndex}");
                            OLogger.Log($" |-   IsBreakthrough: {ss.IsBreakthrough}");
                            OLogger.Log($" |-   RequiresBreakthrough: {ss.RequiresBreakthrough}");
                            OLogger.Log($" |-   RequiredFaction: {ss.RequiredFaction}");
                            OLogger.Log($" |-   RequiredMoney: {ss.RequiredMoney}");
                            if (ss.ParentBranch != null) OLogger.Log($" |-   ParentBranch: {ss.ParentBranch.Index}");
                            if (ss.RequiredSkillSlot != null) OLogger.Log($" |-   Req: {ss.RequiredSkillSlot}");
                            if (ss.SiblingSlot != null) OLogger.Log($" |-   Sibling: {ss.SiblingSlot}");
                        }
                        else
                        {
                            //OLogger.Log($" |- {slot.GetType().Name}");
                        }
                }
                OLogger.Log($"SchoolSkillSlotsCount={_tree.SkillSlots.Count}");
                int m_activeSlotCount = (int)AccessTools.Field(typeof(SkillTreeDisplay), "m_activeSlotCount").GetValue(self);
                OLogger.Log($"m_activeSlotCount={m_activeSlotCount}");
                OLogger.Log("-----");
                for (int i = 0; i < _tree.SkillSlots.Count; i++)
                {
                    OLogger.Log($"{i}: {_tree.SkillSlots[i]}");
                    if (!(bool)_tree.SkillSlots[i])
                    {
                        OLogger.Log($"Skip {i}");
                        continue;
                    }
                    if (_tree.SkillSlots[i].ParentBranch.Index != -1)
                    {
                        m_activeSlotCount++;
                    }
                }
                OLogger.Log($"m_activeSlotCount={m_activeSlotCount}");//*/
            }
            catch (Exception ex)
            {
                //DoOloggerError(ex.Message);
                //Debug.Log($"[{m_modName}] SkillTreeDisplay_RefreshTree: {ex.Message}");
                OLogger.Error(ex.Message);
            }
            orig(self, _tree);
        }
        private string LocalizationManager_GetItemDesc(On.LocalizationManager.orig_GetItemDesc orig, LocalizationManager self, int _itemID)
        {
            if (_itemID == NewSkillArmorExpertID)
            {
                return "Removes the stamina and movement penalties from wearing armor.";
            }
            return orig(self, _itemID);
        }
        private string LocalizationManager_GetItemName(On.LocalizationManager.orig_GetItemName orig, LocalizationManager self, int _itemID)
        {
            if (_itemID == NewSkillArmorExpertID)
            {
                return "Armor Mastering";
            }
            return orig(self, _itemID);
        }
        private bool ItemDetailsDisplay_RefreshDetail(On.ItemDetailsDisplay.orig_RefreshDetail orig, ItemDetailsDisplay self, int _rowIndex, ItemDetailsDisplay.DisplayedInfos _infoType)
        {
            try
            {
                string locItemStat = "";
                float alteredPenalty = 0f;
                bool flag = self.LocalCharacter.Inventory.SkillKnowledge.IsItemLearned(8205221);
                Equipment cachedEquip = (Equipment)AccessTools.Field(typeof(ItemDetailsDisplay), "cachedEquipment").GetValue(self);
                if (_infoType == ItemDetailsDisplay.DisplayedInfos.MovementPenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
                {
                    if (cachedEquip.MovementPenalty > 0f && flag)
                    {
                        //OLogger.Log($"MovementPenalty={cachedEquip.MovementPenalty} --> 0");
                        alteredPenalty = 0f; // cachedEquip.MovementPenalty * 0.25f;
                        locItemStat = "ItemStat_MovementPenalty";
                    }
                }
                if (_infoType == ItemDetailsDisplay.DisplayedInfos.StamUsePenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
                {
                    if (cachedEquip.StaminaUsePenalty > 0f && flag)
                    {
                        alteredPenalty = 0f; // cachedEquip.StaminaUsePenalty * 0.25f;
                        locItemStat = "ItemStat_StaminaUsePenalty";
                    }
                }
                if (_infoType == ItemDetailsDisplay.DisplayedInfos.HeatRegenPenalty /*&& self.LocalCharacter != null && self.LocalCharacter.IsLocalPlayer*/)
                {
                    if (cachedEquip.StaminaUsePenalty > 0f && flag)
                    {
                        alteredPenalty = 0f; // cachedEquip.HeatRegenPenalty * 0.25f;
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
            bool flag = m_character.Inventory.SkillKnowledge.IsItemLearned(8205221);
            for (int i = 0; i < self.EquipmentSlots.Length; i++)
            {
                if (!self.HasItemEquipped(i) || (self.EquipmentSlots[i].SlotType == EquipmentSlot.EquipmentSlotIDs.LeftHand && self.EquipmentSlots[i].EquippedItem.TwoHanded))
                {
                    continue;
                }
                float num2 = self.EquipmentSlots[i].EquippedItem.MovementPenalty;
                //OLogger.Log($"MovementPenalty[{i}]={num2}");
                if (num2 > 0f)
                {
                    num2 *= m_character.Stats.EquipmentPenaltyModifier;
                    if (flag)
                    {
                        num2 = 0f; //*= 0.25f;
                    }
                }
                //OLogger.Log($" > {num2}");
                num += num2 * 0.01f;
            }
            //OLogger.Log($"GetTotalMovementModifier={num}");
            return num;
        }

        private void ResourcesPrefabManager_LoadItemPrefabs(On.ResourcesPrefabManager.orig_LoadItemPrefabs orig, ResourcesPrefabManager self)
        {
            orig(self);
            try
            {
                OLogger.Log("LoadItemPrefabs");
                //List<Skill> lstSkills = ResourcesPrefabManager.Instance.EDITOR_GetPlayerSkillPrefabs();
                //Skill armor = lstSkills.First(s => s.ItemID == 8205220);
                Item go = self.GetItemPrefab(8205220);
                OLogger.Log($"layer={go.gameObject.layer}"); // 
                OLogger.Log($"name={go.gameObject.name}"); // 
                OLogger.Log($"tag={go.gameObject.tag}"); // 
                OLogger.Log($"IsActive={go.gameObject.GetActive()}"); // 
                Component[] lstComp = go.gameObject.GetComponents<Component>();
                OLogger.Log($"lstComp={lstComp.Length}"); // 
                foreach (Component item in lstComp)
                {
                    OLogger.Log($"{item.name}");
                }

                /*Dictionary<string, Item> ITEM_PREFABS = (Dictionary<string, Item>)AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").GetValue(ResourcesPrefabManager.Instance);
                PassiveSkill newSkill = (PassiveSkill)ResourcesPrefabManager.Instance.GenerateItem("8205220");
                newSkill.ItemID = 8205221;
                newSkill.name = "8205221_ArmorExpert";
                newSkill.UID = "8205221_ArmorExpert_test";
                //newSkill.transform.name = "8205221_ArmorExpert";
                ITEM_PREFABS.Add(newSkill.ItemID.ToString(), newSkill);*/

                OLogger.Log($"8205220={ResourcesPrefabManager.Instance.EDITOR_GetPlayerSkillPrefabs().Count(s => s.ItemID == 8205220)}");
                OLogger.Log($"8205221={ResourcesPrefabManager.Instance.EDITOR_GetPlayerSkillPrefabs().Count(s => s.ItemID == 8205221)}");

                Item test = ResourcesPrefabManager.Instance.GenerateItem("8205221");
                //Transform transform = UnityEngine.Object.Instantiate(m_currentStash.VisualPrefab);
                //ItemVisual iv2 = transform.GetComponent<ItemVisual>();


                //armor = (PassiveSkill)lstSkills.First(s => s.ItemID == 8205220);
                // create GameObject in AllPrefabs
                // gameobject with Item component (Skill)
                // Item has: ItemId, SaveType.Savable
                /*PassiveSkill newSkill = UnityEngine.Object.Instantiate(armor);
                newSkill.ItemID = 8205221;
                //AccessTools.Field(typeof(Item), "m_localizedDescription").SetValue(newSkill, "coucou");
                Dictionary<string, Item> ITEM_PREFABS = (Dictionary<string, Item>)AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").GetValue(ResourcesPrefabManager.Instance);
                ITEM_PREFABS.Add(newSkill.ItemID.ToString(), newSkill);*/
                //AccessTools.Field(typeof(ResourcesPrefabManager), "ITEM_PREFABS").SetValue(ResourcesPrefabManager.Instance, ITEM_PREFABS);
                /*OLogger.Log($"Descr={armor.Description}"); // Réduit de 50 % la pénalité d’endurance et de mouvement des armures.
                OLogger.Log($"IsQuickSlotable={armor.IsQuickSlotable}"); // false
                OLogger.Log($"Icon={armor.ItemIcon}"); // tex_men_iconSkillPassiveArmorTraining_v_icn
                OLogger.Log($"IgnoreLearnNotification={armor.IgnoreLearnNotification}"); // false
                OLogger.Log($"HasAdditionalConditions={armor.HasAdditionalConditions}"); // false
                OLogger.Log($"SchoolIndex={armor.SchoolIndex}"); // 0
                List<Effect> lstEff = (List<Effect>)AccessTools.Field(typeof(PassiveSkill), "m_passiveEffects").GetValue(armor);
                OLogger.Log($"m_passiveEffects={lstEff.Count}");
                Skill testArmorPlus = new PassiveSkill()
                {
                    ItemID = 8205221,
                    //Description = "test",
                };
                lstSkills.Add(testArmorPlus);*/
                /*foreach (var skill in lstSkills)
                {
                    OLogger.Log($"{skill.ItemID}: {skill.Name}");
                }*/
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }
        private void NetworkLevelLoader_UnPauseGameplay(On.NetworkLevelLoader.orig_UnPauseGameplay orig, NetworkLevelLoader self, string _identifier)
        {
            orig(self, _identifier);
        }

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
                    row.SetInfo("Ratio (v/w)", Math.Round(_itemDisplay.RefItem.Value / _itemDisplay.RefItem.Weight, 2).ToString());
                    //m_lblItemName.text += $" ({_itemDisplay.RefItem.Value}/{_itemDisplay.RefItem.Weight} = {_itemDisplay.RefItem.Value/_itemDisplay.RefItem.Weight})"; 
                }
                if (m_lblItemName != null && _itemDisplay != null && _itemDisplay.RefItem != null &&
                    _itemDisplay.RefItem.Value > 0)
                {
                    List<ItemDetailRowDisplay> m_detailRows = (List<ItemDetailRowDisplay>)AccessTools.Field(typeof(ItemDetailsDisplay), "m_detailRows").GetValue(self);
                    ItemDetailRowDisplay row = (ItemDetailRowDisplay)AccessTools.Method(typeof(ItemDetailsDisplay), "GetRow").Invoke(self, new object[] { m_detailRows.Count });
                    row.SetInfo("Value", _itemDisplay.RefItem.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                //DoOloggerError(ex.Message);
            }
        }

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

    }
}
