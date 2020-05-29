using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wip
{
    public class SortingScript : MonoBehaviour
    {
        private int isActive = 0;
        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                isActive = (++isActive % 6);
                On.ItemListDisplay.SortBy += ItemListDisplay_SortBy;
            }
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

        public void testEvent()
        {
            OLogger.Log("interaction called!");
        }

    }
}
