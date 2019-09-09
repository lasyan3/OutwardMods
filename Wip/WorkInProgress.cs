using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoreGatherableLoot
{
    public class WorkInProgress : PartialityMod
    {
        private readonly string _modName = "WorkInProgress";
        private bool test = true;
        private string currentArea;
        public Dictionary<string, string> StashAreaToStashUID = new Dictionary<string, string>
        {
            {
                "Berg",
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                "CierzoNewTerrain",
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                "Levant",
                "ZbPXNsPvlUeQVJRks3zBzg"
            },
            {
                "Monsoon",
                "ImqRiGAT80aE2WtUHfdcMw"
            }
        };
        public Dictionary<string, QuestEventSignature> StashAreaToQuestEvent = new Dictionary<string, QuestEventSignature>
        {
            {
                "Berg",
                new QuestEventSignature("stash_berg")
                {
                    EventName = "Inn_Berg_StashRent",
                    IsTimedEvent = true,
                }
            },
            {
                "Levant",
                new QuestEventSignature("stash_levant")
                {
                    EventName = "Inn_Levant_StashRent",
                    IsTimedEvent = true,
                }
            },
            {
                "Monsoon",
                new QuestEventSignature("stash_monsoon")
                {
                    EventName = "Inn_Monsoon_StashRent",
                    IsTimedEvent = true,
                    //Savable = true,
                    //IsHideEvent = false,
                }
            }
        };

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
            //On.SaveInstance.ApplyEnvironment += SaveInstance_ApplyEnvironment; // Replace stashs

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

            // TODO: manage dialogs with inn keeper
            //On.DialoguePanel.AwakeInit += DialoguePanel_AwakeInit;
            //On.DialoguePanel.RefreshMultipleChoices += DialoguePanel_RefreshMultipleChoices;

            //On.InteractionOpenContainer.OnActivate += InteractionOpenContainer_OnActivate;

            //On.Item.ChangeOwner += Item_ChangeOwner; // Disable flag showing new items

            On.CraftingMenu.GetPlayersOwnItems += CraftingMenu_GetPlayersOwnItems; // Includes the stash for the crafting ingredients
            On.RecipeDisplay.SetReferencedRecipe += RecipeDisplay_SetReferencedRecipe; // Show quantity of owned objects in recipes' name

            // Repair all items in inventory: CharacterEquipment.RepairEquipment
            On.CharacterEquipment.RepairEquipmentAfterRest += CharacterEquipment_RepairEquipmentAfterRest;

            //On.EnvironmentSave.ApplyData += EnvironmentSave_ApplyData;

            On.NetworkLevelLoader.JoinSequenceDone += CheckRentStatus;
        }

        #region Rent System
        private void CheckRentStatus(On.NetworkLevelLoader.orig_JoinSequenceDone orig, NetworkLevelLoader self)
        {
            //OLogger.Log("CheckRentStatus");
            orig(self);
            try
            {
                if (StashAreaToStashUID.ContainsKey(currentArea) && CharacterManager.Instance.PlayerCharacters.Count > 0)
                {
                    Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                    Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);

                    stash.SetCanInteract(false);
                    if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[currentArea]))
                    {
                        if (QuestEventManager.Instance.CheckEventExpire(StashAreaToQuestEvent[currentArea].EventUID, 1))
                        {
                            character.CharacterUI.SmallNotificationPanel.ShowNotification($"Rent expired!", 5f);
                            QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[currentArea].EventUID);
                            OLogger.Log("Rent expired!");
                        }
                        else
                        {
                            stash.SetCanInteract(true);
                            OLogger.Log("Rent still valid");
                        }
                    }
                    /*else // TESTING : auto rent
                    {
                        if (character.Inventory.AvailableMoney >= 20)
                        {
                            character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent started", 5f);
                            character.Inventory.AvailableMoney -= 20;
                            bool addOk = QuestEventManager.Instance.AddEvent(qStashMonsoon);
                            stash.SetCanInteract(true);
                            OLogger.Log($"Rent started!");
                        }
                        else
                        {
                            character.CharacterUI.SmallNotificationPanel.ShowNotification($"Not enough money: {character.Inventory.AvailableMoney}", 5f);
                            OLogger.Log($"Not enough money: {character.Inventory.AvailableMoney}");
                        }

                    }*/
                }
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }
        private bool QuestEventManager_AddEvent_1(On.QuestEventManager.orig_AddEvent_1 orig, QuestEventManager self, string _eventUID, int _stackAmount, bool _sendEvent)
        {
            bool res = orig(self, _eventUID, _stackAmount, _sendEvent);
            //OLogger.Log($"AddEvent({_eventUID})={res}");
            try
            {
                // If event is house buying, cancel previous rent event
                if (res && StashAreaToStashUID.ContainsKey(currentArea) &&
                    QuestEventManager.Instance.GetQuestEvent(_eventUID).Name == $"PlayerHouse_{currentArea}_HouseAvailable" &&
                    QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[currentArea]))
                {
                    QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[currentArea].EventUID);
                    Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                    stash.SetCanInteract(true);
                    OLogger.Log("Rent canceled (house bought)");
                }
            }
            catch (Exception ex)
            {
                OLogger.Log($"[{_modName}] QuestEventManager_AddEvent_1: {ex.Message}");
            }
            return res;
        }
        private void QuestEventDictionary_Load(On.QuestEventDictionary.orig_Load orig)
        {
            orig();
            try
            {
                QuestEventFamily innSection = QuestEventDictionary.Sections.FirstOrDefault(s => s.Name == "Inns");
                if (innSection != null)
                {
                    foreach (QuestEventSignature qes in StashAreaToQuestEvent.Values)
                    {
                        if (!innSection.Events.Contains(qes))
                        {
                            innSection.Events.Add(qes);
                        }
                        if (QuestEventDictionary.GetQuestEvent(qes.EventUID) == null)
                        {
                            QuestEventDictionary.RegisterEvent(qes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OLogger.Log($"[{_modName}] QuestEventDictionary_Load: {ex.Message}");
            }
        }
        #endregion

        #region Repair all items in bag
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
            }
            catch (Exception ex)
            {
                Debug.Log($"[{_modName}] RepairEquipmentAfterRest: {ex.Message}");
            }
        }
        #endregion

        private void EnvironmentSave_ApplyData(On.EnvironmentSave.orig_ApplyData orig, EnvironmentSave self)
        {
            float num = (float)(self.GameTime - EnvironmentConditions.GameTime);
            if (num > 0f)
            {
                EnvironmentConditions.GameTime = self.GameTime;
            }
            if (!AreaManager.Instance.IsAreaExpired(self.AreaName, num))
            {
                ItemManager.Instance.LoadItems(self.ItemList, _clearAllItems: true);
                CharacterManager.Instance.LoadAiCharactersFromSave(self.CharList.ToArray());
                SceneInteractionManager.Instance.LoadInteractableStates(self.InteractionActivatorList);
                SceneInteractionManager.Instance.LoadDropTableStates(self.DropTablesList);
                CampingEventManager.Instance.LoadEventTableData(self.CampingEventSaveData);
                DefeatScenariosManager.Instance.LoadSaveData(self.DefeatScenarioSaveData);
                EnvironmentConditions.Instance.LoadSoulSpots(self.UsedSoulSpots);
                MapDisplay.Instance.Load(self.MapSaveData);
            }
        }


        private void RecipeDisplay_SetReferencedRecipe(On.RecipeDisplay.orig_SetReferencedRecipe orig, RecipeDisplay self, Recipe _recipe, bool _canBeCompleted, IList<Item>[] _compatibleIngredients, IList<Item> _ingredients)
        {
            orig(self, _recipe, _canBeCompleted, _compatibleIngredients, _ingredients);
            try
            {
                if (_recipe.Results.Length == 0)
                    return;
                UnityEngine.UI.Text m_lblRecipeName = (UnityEngine.UI.Text)AccessTools.Field(typeof(RecipeDisplay), "m_lblRecipeName").GetValue(self);
                int invQty = self.LocalCharacter.Inventory.GetOwnedItems(_recipe.Results[0].Item.ItemID).Count;
                int stashQty = 0;
                if (StashAreaToStashUID.ContainsKey(currentArea))
                {
                    TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                    if (stash != null)
                    {
                        stashQty = stash.GetItemsFromID(_recipe.Results[0].Item.ItemID).Count;
                    }
                }
                self.SetName(m_lblRecipeName.text += $" ({invQty + stashQty})");
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private void CraftingMenu_GetPlayersOwnItems(On.CraftingMenu.orig_GetPlayersOwnItems orig, CraftingMenu self, ref List<Item> _list, int _itemID)
        {
            //OLogger.Log("CraftingMenu_GetPlayersOwnItems");
            orig(self, ref _list, _itemID);
            try
            {
                if (!StashAreaToStashUID.ContainsKey(currentArea))
                {
                    return;
                }
                TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                if (stash == null)
                {
                    return;
                }
                _list.AddRange(stash.GetItemsFromID(_itemID));
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
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
                //OLogger.Error("OnEnable: " + ex.Message, _modName);
                Debug.Log($"[{_modName}] OnEnable: {ex.Message}");
            }
        }

        private void DialoguePanel_RefreshMultipleChoices(On.DialoguePanel.orig_RefreshMultipleChoices orig, DialoguePanel self)
        {
            try
            {
                OLogger.Log($"RefreshMultipleChoices");
                orig(self);
                List<DialogueAnswer> m_dialogueOptions = (List<DialogueAnswer>)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptions").GetValue(self);
                RectTransform m_dialogueOptionHolder = (RectTransform)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptionHolder").GetValue(self);
                int num = m_dialogueOptions.Count;
                OLogger.Log($"{num}");
                //if (num == 3)
                //{
                DialogueAnswer dialogueAnswer = UnityEngine.Object.Instantiate(m_dialogueOptions[0]);
                dialogueAnswer.transform.SetParent(m_dialogueOptionHolder);
                dialogueAnswer.transform.ResetLocal();
                dialogueAnswer.name = "itm" + num.ToString();
                m_dialogueOptions.Add(dialogueAnswer);
                num++;
                /*}
                if (num != 4) return;*/

                m_dialogueOptions[num].index = num + 1;
                m_dialogueOptions[num].dialogueChoiceIndex = 2;
                m_dialogueOptions[num].text = "coucou";
                /*ButtonClickedEvent ev = (ButtonClickedEvent)AccessTools.Field(typeof(DialogueAnswer), "onClick").GetValue(m_dialogueOptions[num]);
                ev.RemoveAllListeners();
                int value = m_dialogueOptions[num].dialogueChoiceIndex;
                ev.AddListener(delegate
                {
                    //self.OnSelectDialogueOption(value);
                    AccessTools.Method(typeof(DialogueAnswer), "OnSelectDialogueOption").Invoke(self, new object[] { value });
                });*/
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private void DialoguePanel_AwakeInit(On.DialoguePanel.orig_AwakeInit orig, DialoguePanel self)
        {
            OLogger.Log($"DialoguePanel_AwakeInit");
            orig(self);
            try
            {
                List<DialogueAnswer> m_dialogueOptions = (List<DialogueAnswer>)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptions").GetValue(self);
                RectTransform m_dialogueOptionHolder = (RectTransform)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptionHolder").GetValue(self);
                int num = m_dialogueOptions.Count;
                OLogger.Log($"{num}");
                DialogueAnswer dialogueAnswer = UnityEngine.Object.Instantiate(m_dialogueOptions[0]);
                dialogueAnswer.transform.SetParent(m_dialogueOptionHolder);
                dialogueAnswer.transform.ResetLocal();
                dialogueAnswer.name = "itm" + num.ToString();
                m_dialogueOptions.Add(dialogueAnswer);

                m_dialogueOptions[num].index = num + 1;
                m_dialogueOptions[num].dialogueChoiceIndex = 4;
                m_dialogueOptions[num].text = "coucou";
                /*m_dialogueOptions[num].onClick.RemoveAllListeners();
                int value = m_dialogueOptions[num].dialogueChoiceIndex;
                m_dialogueOptions[num].onClick.AddListener(delegate
                {
                    OnSelectDialogueOption(value);
                });*/
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private bool SaveInstance_ApplyEnvironment(On.SaveInstance.orig_ApplyEnvironment orig, SaveInstance self, string _areaName)
        {
            //OLogger.Log($"SaveInstance_ApplyEnvironment={_areaName}");
            bool result = orig(self, _areaName);
            try
            {
                currentArea = _areaName;
                if (StashAreaToStashUID.ContainsKey(currentArea))
                {
                    #region Move stash to Inn
                    //OLogger.Log("ContainsKey");
                    /* CREATE
                    ItemContainer ic2 = UnityEngine.Object.Instantiate((ItemContainer)ResourcesPrefabManager.Instance.GetItemPrefab(1000000));
                    ic2.ChangeParent(null, new Vector3(-366.3f, -1500.0f, 764.9f), new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
                    ic2.SpecialType = SpecialContainerTypes.Stash;
                    ic2.SetForceSyncPos();
                    ic2.name = "coucou";
                    SplitPlayer pl = SplitScreenManager.Instance.LocalPlayers[0];
                    ic2.OnContainerChangedOwner(pl.AssignedCharacter);//*/

                    // MOVE STASHS
                    // TODO: only if house player not bought in town! (quest event
                    /*string eventName = "";
                    switch (currentArea)
                    {
                        case "Berg": eventName = "PlayerHouse_Berg_HouseAvailable"; break;
                        case "Levant": eventName = "PlayerHouse_Levant_HouseAvailable"; break;
                        case "Monsoon": eventName = "PlayerHouse_Berg_HouseAvailable"; break;
                    }*/
                    if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"PlayerHouse_{currentArea}_HouseAvailable") == 0)
                    {
                        Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                        if (stash != null)
                        {
                            //OLogger.Log($"stash={StashAreaToStashUID[currentArea]}");
                            Vector3 newPos = new Vector3(-366.3f, -1500.0f, 764.9f);
                            Quaternion newRot = new Quaternion();
                            switch (currentArea)
                            {
                                case "Berg":
                                    newPos = new Vector3(-366.3f, -1500.0f, 764.9f);
                                    newRot = new Quaternion();
                                    break;
                                //case "CierzoNewTerrain":                            break;
                                //case "Levant":                            break;
                                case "Monsoon":
                                    newPos = new Vector3(-372.0f, -1500.0f, 560.7f);
                                    newRot = new Quaternion();
                                    break;
                                default: return result;
                            }
                            //OLogger.Log($"X={newPos.x}, Y={newPos.y}, Z={newPos.z}");
                            ItemVisual iv2 = ItemManager.GetVisuals(stash.ItemID);
                            iv2.transform.SetPositionAndRotation(newPos, newRot);
                            for (int i = 0; i < CharacterManager.Instance.PlayerCharacters.Count; i++)
                            {
                                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[i]);
                                if ((bool)character)
                                {
                                    stash.OnContainerChangedOwner(character);
                                    break;
                                }
                            }
                            stash.LinkVisuals(iv2, false);
                            stash.InteractionHolder.transform.SetPositionAndRotation(newPos, newRot);
                        }
                    }
                    else
                    {
                        OLogger.Log("House bought for this area");
                    }//*/
                    #endregion
                }

                #region TODO : Thieves in town!
                // Parcourir tous les objets appartenant au joueur dans la ville
                /*for (int i = 0; i < ItemManager.Instance.WorldItems.Count; i++)
                {
                    string uid = ItemManager.Instance.WorldItems.Keys[i];
                    Item it = ItemManager.Instance.WorldItems.Values[i];
                    if (it.OwnerCharacter != null && it.OwnerCharacter.IsLocalPlayer && !it.IsEquipped &&
                        it.ParentContainer != it.OwnerCharacter.Inventory.EquippedBag && isinworld?
                        it.ParentContainer != it.OwnerCharacter.Inventory.Pouch)
                    {
                        OLogger.Log($"Delete {it.Name}");
                        ItemManager.Instance.DestroyItem(uid);
                    }
                }*/
                #endregion

            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }

            return result;
        }
    }
}
