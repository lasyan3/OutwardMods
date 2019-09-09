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

namespace MoreGatherableLoot
{
    public class WorkInProgress : PartialityMod
    {
        private readonly string _modName = "WorkInProgress";
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
            On.SaveInstance.ApplyEnvironment += SaveInstance_ApplyEnvironment;

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

            On.Item.ChangeOwner += Item_ChangeOwner; // Disable flag showing new items

            //On.CraftingMenu.GetPlayersOwnItems += CraftingMenu_GetPlayersOwnItems; // Includes the stash for the crafting ingredients
            //On.RecipeDisplay.SetReferencedRecipe += RecipeDisplay_SetReferencedRecipe; // Show quantity of owned objects in recipes' name
            //On.ItemDisplay.SetReferencedItem += ItemDisplay_SetReferencedItem;
            //On.ItemDisplay.UpdateQuantityDisplay += ItemDisplay_UpdateQuantityDisplay;
            //On.RecipeResultDisplay.UpdateQuantityDisplay += RecipeResultDisplay_UpdateQuantityDisplay; // OK
            //On.ItemDetailsDisplay.RefreshDisplay += ItemDetailsDisplay_RefreshDisplay;
            //On.ItemDisplay.Update += ItemDisplay_Update;

            // Repair all items in inventory: CharacterEquipment.RepairEquipment
            On.CharacterEquipment.RepairEquipmentAfterRest += CharacterEquipment_RepairEquipmentAfterRest;

            //On.EnvironmentSave.ApplyData += EnvironmentSave_ApplyData;

            //CustomKeybindings.AddAction("QuickSave", KeybindingsCategory.Actions, ControlType.Both, 5);
        }

        private bool SaveInstance_ApplyEnvironment(On.SaveInstance.orig_ApplyEnvironment orig, SaveInstance self, string _areaName)
        {
            bool result = orig(self, _areaName);
            currentArea = _areaName;
            return result;
        }

        private void ItemDisplay_Update(On.ItemDisplay.orig_Update orig, ItemDisplay self)
        {
            orig(self);
            //if (self.RefItem != null) OLogger.Log($"Update={self.RefItem.Name}");
            Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self);
            if (m_lblQuantity != null)
            {
                //OLogger.Log($"Update={m_lblQuantity.text}");
                m_lblQuantity.text = "4";
                //AccessTools.Field(typeof(ItemDetailsDisplay), "m_lblItemName").SetValue(self, m_lblItemName);
            }
            else
            {
                Transform transform = self.transform.Find("lblQuantity");
                if ((bool)transform)
                {
                    OLogger.Log($"ok={self.RefItem.Name}");
                }
            }
        }

        private void ItemDetailsDisplay_RefreshDisplay(On.ItemDetailsDisplay.orig_RefreshDisplay orig, ItemDetailsDisplay self, IItemDisplay _itemDisplay)
        {
            orig(self, _itemDisplay);
            try
            {
                Text m_lblItemName = (Text)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lblItemName").GetValue(self);
                //Item it = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(self);
                if (m_lblItemName != null && _itemDisplay.RefItem != null)
                {
                    //OLogger.Log(m_lblItemName.text);
                    int invQty = self.LocalCharacter.Inventory.GetOwnedItems(_itemDisplay.RefItem.ItemID).Count;
                    int stashQty = 0;
                    if (StashAreaToStashUID.ContainsKey(currentArea))
                    {
                        TreasureChest stash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                        if (stash != null)
                        {
                            stashQty = stash.GetItemsFromID(_itemDisplay.RefItem.ItemID).Count;
                        }
                    }
                    m_lblItemName.text += $" ({invQty + stashQty})";
                }
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private void RecipeResultDisplay_UpdateQuantityDisplay(On.RecipeResultDisplay.orig_UpdateQuantityDisplay orig, RecipeResultDisplay self)
        {
            orig(self);
            if (self.RefItem != null)
            {
                Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self);
                if (m_lblQuantity != null)
                {
                    OLogger.Log($"RecipeResultDisplay={self.RefItem.Name}");
                    m_lblQuantity.text = "12";
                }
            }
        }

        private void ItemDisplay_UpdateQuantityDisplay(On.ItemDisplay.orig_UpdateQuantityDisplay orig, ItemDisplay self)
        {
            orig(self);
            if (self.RefItem != null)
            {
                OLogger.Log($"ItemDisplay={self.RefItem.Name}");
                Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self);
                if (m_lblQuantity != null)
                {
                    m_lblQuantity.text = "5";
                    AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").SetValue(self, m_lblQuantity);
                }
            }
        }

        private void ItemDisplay_SetReferencedItem(On.ItemDisplay.orig_SetReferencedItem orig, ItemDisplay self, Item _item)
        {
            try
            {
                if (_item != null) OLogger.Log($"SetReferencedItem={_item.Name}");
                /*if (self.RefItem != null)
                {
                    OLogger.Log(self.RefItem.Name);
                    self.RefItem.RemainingAmount = 5;
                }*/
                /*Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self);
                if (m_lblQuantity != null)
                {
                    //OLogger.Log("init");
                    m_lblQuantity.text = "x";
                    AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").SetValue(self, m_lblQuantity);
                }*/
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
            orig(self, _item);
        }

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
            }
            MapDisplay.Instance.Load(self.MapSaveData);
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
                OLogger.Error($"SetReferencedRecipe: {ex.Message}");
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
                OLogger.Error($"GetPlayersOwnItems: {ex.Message}");
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

    }
}
