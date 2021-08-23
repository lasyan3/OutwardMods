using Harmony;
using HarmonyLib;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Wip
{
    public class Legacy : PartialityMod
    {
        readonly string _modName = "WorkInProgress";

        private DictionaryExt<string, LegacyChestDataMod> m_legacyChestData = new DictionaryExt<string, LegacyChestDataMod>();

        public Legacy()
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

            // TODO : menu item selection
            // TODO : save item quantity
            // TODO : open chest get item count
            // LegacyChestData / LegacyChestSave

            On.ItemManager.LoadLegacyChestData_List1_1 += ItemManager_LoadLegacyChestData;
            On.ItemManager.RefreshLegacyChestData += ItemManager_RefreshLegacyChestData_1;

            On.LegacyChestDisplay.SetChestData += LegacyChestDisplay_SetChestData;
            On.LegacyChestPanel.Show += LegacyChestPanel_Show;
            On.LegacyChestPanel.OnItemDisplayClicked += LegacyChestPanel_OnItemDisplayClicked;
            // m_itemInChest must become ItemGroupDisplay
            //On.LegacyChestPanel.StartInit += LegacyChestPanel_StartInit;

            //On.LegacyChestSelectionPanel.Show += LegacyChestSelectionPanel_Show; // on new game
            On.ItemDisplay.UpdateQuantityDisplay += ItemDisplay_UpdateQuantityDisplay;
            //On.ItemDisplay.TryMoveTo += ItemDisplay_TryMoveTo;

            On.LegacyChestData.SetContainedData += LegacyChestData_SetContainedData;

            On.LegacyChestSave.PrepareSave_1 += LegacyChestSave_PrepareSave_1;
            
        }

        private void LegacyChestSave_PrepareSave_1(On.LegacyChestSave.orig_PrepareSave_1 orig, LegacyChestSave self)
        {
            self.LegacyChestList.Clear();
            ItemManager.Instance.RefreshLegacyChestData();
            for (int i = 0; i < m_legacyChestData.Values.Count; i++)
            {
                self.LegacyChestList.Add(new BasicSaveData(m_legacyChestData.Values[i]));
            }

        }

        private void LegacyChestPanel_StartInit(On.LegacyChestPanel.orig_StartInit orig, LegacyChestPanel self)
        {
            orig(self);
            try
            {
                ItemDisplay m_itemInChest = (ItemDisplay)AccessTools.Field(typeof(LegacyChestPanel), "m_itemInChest").GetValue(self);
                ItemDetailsDisplay m_itemDetailDisplay = (ItemDetailsDisplay)AccessTools.Field(typeof(LegacyChestPanel), "m_itemDetailDisplay").GetValue(self);
                //AccessTools.Field(typeof(LegacyChestPanel), "m_itemInChest").SetValue(self, m_itemInChest);
                ItemDetailsPanelSpawner componentInChildren = self.GetComponentInChildren<ItemDetailsPanelSpawner>();
                if ((bool)componentInChildren)
                {
                    m_itemDetailDisplay = componentInChildren.DetailPanel;
                    ItemDisplay item = (ItemDisplay)AccessTools.Field(typeof(ItemDetailsDisplay), "itemDisplay").GetValue(m_itemDetailDisplay);
                    ItemGroupDisplay group = (ItemGroupDisplay)AccessTools.Field(typeof(ItemDetailsDisplay), "groupDisplay").GetValue(m_itemDetailDisplay);
                    OLogger.Log($"item={item == null}");
                    OLogger.Log($"group={group == null}");
                    m_itemInChest = group;
                }
                ItemDisplay itemInChest = m_itemInChest;
                itemInChest.onSelectCallback = (UnityAction<ItemDisplay>)Delegate.Combine(itemInChest.onSelectCallback, new UnityAction<ItemDisplay>(self.OnItemSelected));
                Transform transform = self.transform.Find("MiddlePanel/PlayerInventory/Header/lblTitle");
                if ((bool)transform)
                {
                    //m_lblInventoryTitle = transform.GetComponent<Text>();
                }
            }
            catch (Exception ex)
            {
                OLogger.Error($"StartInit={ex.Message}");
            }
        }

        private void LegacyChestData_SetContainedData(On.LegacyChestData.orig_SetContainedData orig, LegacyChestData self, Item _containedItem)
        {
            OLogger.Log($"SetContainedData={_containedItem}");
            orig(self, _containedItem);
        }

        private void ItemDisplay_TryMoveTo(On.ItemDisplay.orig_TryMoveTo orig, ItemDisplay self, ItemContainer _targetContainer)
        {
            OLogger.Log("TryMoveTo");
            orig(self, _targetContainer);
        }

        private void ItemDisplay_UpdateQuantityDisplay(On.ItemDisplay.orig_UpdateQuantityDisplay orig, ItemDisplay self)
        {
            Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self);
            orig(self);
        }

        private void LegacyChestPanel_OnItemDisplayClicked(On.LegacyChestPanel.orig_OnItemDisplayClicked orig, LegacyChestPanel self, ItemDisplay _itemDisplay)
        {
            try
            {
                //OLogger.Log("OnItemDisplayClicked");
                ItemDisplay m_itemInChest = (ItemDisplay)AccessTools.Field(typeof(LegacyChestPanel), "m_itemInChest").GetValue(self);
                ItemContainer m_refLegacyChest = (ItemContainer)AccessTools.Field(typeof(LegacyChestPanel), "m_refLegacyChest").GetValue(self);
                CharacterUI m_characterUI = (CharacterUI)AccessTools.Field(typeof(LegacyChestPanel), "m_characterUI").GetValue(self);
                //OLogger.Log($"UQD={self.RefItem.IsStackable} {self.RefItem.MaxStackAmount}");
                if (_itemDisplay == m_itemInChest)
                {
                    self.TakeItemBack();
                }
                else //if (!(_itemDisplay is ItemGroupDisplay) || ((ItemGroupDisplay)_itemDisplay).StackCount == 1)
                {
                    /*if (m_refLegacyChest.ItemCount > 0)
                    {
                        self.TakeItemBack();
                    }*/
                    /*if (_itemDisplay is ItemGroupDisplay)
                    {
                        m_itemInChest = _itemDisplay;
                        AccessTools.Field(typeof(LegacyChestPanel), "m_itemInChest").SetValue(self, m_itemInChest);
                    }
                    OLogger.Log($"IsGroup1={_itemDisplay is ItemGroupDisplay}");
                    OLogger.Log($"IsGroup2={m_itemInChest is ItemGroupDisplay}");*/
                    //OLogger.Log($"TryMoveTo={_itemDisplay.RefItem.IsStackable} {_itemDisplay.RefItem.MaxStackAmount}");
                    _itemDisplay.TryMoveTo(m_refLegacyChest);

                    //OLogger.Log($"UQD={m_refLegacyChest.GetContainedItems()[0].IsStackable} {m_refLegacyChest.GetContainedItems()[0].MaxStackAmount}");
                    /*if (m_characterUI.IsGroupViewDisplayed)
                    {
                        m_characterUI.HideGroupView();
                    }*/
                }
                /*else
                {
                    m_characterUI.ShowGroupview((ItemGroupDisplay)_itemDisplay);
                }*/
            }
            catch (Exception ex)
            {
                OLogger.Error($"OnItemDisplayClicked={ex.Message}");
            }
        }

        private void LegacyChestSelectionPanel_Show(On.LegacyChestSelectionPanel.orig_Show orig, LegacyChestSelectionPanel self)
        {
            OLogger.Log("LegacyChestSelectionPanel_Show");
            orig(self);
        }

        private void LegacyChestPanel_Show(On.LegacyChestPanel.orig_Show orig, LegacyChestPanel self)
        {
            try
            {
                ItemDetailsDisplay m_itemDetailDisplay = (ItemDetailsDisplay)AccessTools.Field(typeof(LegacyChestPanel), "m_itemDetailDisplay").GetValue(self);
                Item m_lastItem = (Item)AccessTools.Field(typeof(ItemDetailsDisplay), "m_lastItem").GetValue(m_itemDetailDisplay);
                OLogger.Log($"Show={m_lastItem.HasMultipleUses}");
            }
            catch (Exception ex)
            {
                OLogger.Error($"LegacyChestPanel_Show={ex.Message}");
            }
            orig(self);
        }

        private void LegacyChestDisplay_SetChestData(On.LegacyChestDisplay.orig_SetChestData orig, LegacyChestDisplay self, LegacyChestData _data)
        {
            try
            {
                OLogger.Log($"SetChestData={self.ItemDisplay.GetType().Name}");
                Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self.ItemDisplay);
                m_lblQuantity.text = "5";
                //AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").SetValue(self.ItemDisplay, new UnityEngine.UI.Text();
            }
            catch (Exception ex)
            {
                OLogger.Error($"SetChestData={ex.Message}");
            }
            orig(self, _data);
        }

        private void ItemManager_RefreshLegacyChestData_1(On.ItemManager.orig_RefreshLegacyChestData_1 orig, ItemManager self, ItemContainer chest)
        {
            try
            {
                if (chest != null)
                {
                    OLogger.Log(chest.Name + ": " + chest.ItemCount);
                }
                //orig(self, chest);
                if (chest.ItemCount <= 0)
                {
                    return;
                }
                Item item = null;
                item = chest.GetContainedItems()[0];
                if ((bool)item)
                {
                    if (!m_legacyChestData.ContainsKey(chest.UID))
                    {
                        m_legacyChestData.Add(chest.UID, new LegacyChestDataMod(chest));
                    }
                    m_legacyChestData[chest.UID].SetContainedData(item, chest.GetContainedItems().Count);
                }
            }
            catch (Exception ex)
            {
                OLogger.Error($"RefreshLegacyChestData={ex.Message}");
            }
        }

        private void ItemManager_LoadLegacyChestData(On.ItemManager.orig_LoadLegacyChestData orig, ItemManager self, List<BasicSaveData> _chestData)
        {
            m_legacyChestData.Clear();
            for (int i = 0; i < _chestData.Count; i++)
            {
                m_legacyChestData.Add(_chestData[i].Identifier.ToString(), new LegacyChestDataMod(_chestData[i]));
            }

        }
    }
}
