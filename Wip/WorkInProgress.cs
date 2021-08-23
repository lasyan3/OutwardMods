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
    public enum eItemIDs
    {
        TentKit = 5000010,
        Tent = 5000011,
        BedrollKit = 5000020,
        Bedroll = 5000021,
        CamoTentKit = 5000030,
        CamoTent = 5000031,
        FurTentKit = 5000040,
        FurTent = 5000041,
        LuxuryTentKit = 5000050,
        LuxuryTent = 5000051,
        MageTentKit = 5000060,
        MageTent = 5000061,
        CleanWater = 5600000,
        RiverWater = 5600001,
        RancidWater = 5600003,
        FlintAndSteel = 5600010,
        Waterskin = 4200040,
    }

    public class StrDurability
    {
        public int MaxDurability;
        public float DepletionRate = 1.0f;
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
            { eItemIDs.FlintAndSteel, new StrDurability { MaxDurability = 5 } },
            { eItemIDs.BedrollKit, new StrDurability { MaxDurability = 5 } },
            { eItemIDs.Bedroll, new StrDurability { MaxDurability = 5 } },
            { eItemIDs.Tent, new StrDurability { MaxDurability = 10 } },
            { eItemIDs.TentKit, new StrDurability { MaxDurability = 10 } },
            { eItemIDs.CamoTentKit, new StrDurability { MaxDurability = 10 } },
            { eItemIDs.CamoTent, new StrDurability { MaxDurability = 10 } },
            { eItemIDs.FurTentKit, new StrDurability { MaxDurability = 10 } },
            { eItemIDs.FurTent, new StrDurability { MaxDurability = 10 } },
            { eItemIDs.LuxuryTentKit, new StrDurability { MaxDurability = 20 } },
            { eItemIDs.LuxuryTent, new StrDurability { MaxDurability = 20 } },
            { eItemIDs.MageTentKit, new StrDurability { MaxDurability = 20 } },
            { eItemIDs.MageTent, new StrDurability { MaxDurability = 20 } },
            { eItemIDs.CleanWater, new StrDurability { MaxDurability = 5*24 } },
            { eItemIDs.RiverWater, new StrDurability { MaxDurability = 2*24 } },
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
                MyLogger.LogError("Awake: " + ex.Message);
            }
        }

        private void SL_OnPacksLoaded()
        {
            try
            {
                /*var prefab = CustomItems.CreateCustomItem((int)eItemIDs.FlintAndSteel, (int)eItemIDs.FlintAndSteel, eItemIDs.FlintAndSteel.ToString());
                MultipleUsage m_stackable = (MultipleUsage)AccessTools.Field(typeof(Item), "m_stackable").GetValue(prefab);
                m_stackable.AutoStack = false;
                var stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, ItemDurabilities[eItemIDs.FlintAndSteel].MaxDurability);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.Destroy;//*/

                #region Tentes
                /*prefab = CustomItems.CreateCustomItem((int)eItemIDs.BedrollKit, (int)eItemIDs.BedrollKit, eItemIDs.BedrollKit.ToString());
                stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, ItemDurabilities[eItemIDs.BedrollKit].MaxDurability);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;
                prefab.gameObject.AddComponent<CustomDurable>();
                prefab = CustomItems.CreateCustomItem((int)eItemIDs.Bedroll, (int)eItemIDs.Bedroll, "whatever");
                prefab.gameObject.AddComponent<CustomDurable>();

                prefab = CustomItems.CreateCustomItem((int)eItemIDs.TentKit, (int)eItemIDs.TentKit, eItemIDs.TentKit.ToString());
                stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, ItemDurabilities[eItemIDs.TentKit].MaxDurability);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;
                prefab.gameObject.AddComponent<CustomDurable>();
                prefab = CustomItems.CreateCustomItem((int)eItemIDs.Tent, (int)eItemIDs.Tent, "whatever");
                prefab.gameObject.AddComponent<CustomDurable>();

                //prefab = CustomItems.CreateCustomItem((int)eItemIDs.CamoTentKit, (int)eItemIDs.CamoTentKit, eItemIDs.CamoTentKit.ToString());
                //stats = prefab.GetComponent<ItemStats>();
                //AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, 15);
                //prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;

                var repareTent = new SL_Recipe
                {
                    StationType = Recipe.CraftingType.Survival,
                    Ingredients = new List<SL_Recipe.Ingredient>
                    {
                        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = (int)eItemIDs.TentKit },
                        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 6500090 }, // Linen Cloth
                        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 6500090 }, // Linen Cloth
                    },
                    Results = new List<SL_Recipe.ItemQty>
                    {
                        new SL_Recipe.ItemQty{ ItemID = (int)eItemIDs.TentKit, Quantity = 1 }
                    }
                };
                repareTent.ApplyRecipe();
                //*/
                #endregion

                #region Waterskin
                /*prefab = CustomItems.CreateCustomItem((int)eItemIDs.Waterskin, (int)eItemIDs.Waterskin, eItemIDs.Waterskin.ToString());
                stats = prefab.GetComponent<ItemStats>();
                AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(stats, ItemDurabilities[eItemIDs.CleanWater].MaxDurability);
                prefab.BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.DoNothing;
                var p = prefab.gameObject.AddComponent<Perishable>();
                AccessTools.Field(typeof(Perishable), "m_baseDepletionRate").SetValue(p, ItemDurabilities[eItemIDs.CleanWater].DepletionRate);

                // Add RecipeItems
                var r = new SL_Recipe
                {
                    StationType = Recipe.CraftingType.Cooking,
                    Ingredients = new List<SL_Recipe.Ingredient>
                    {
                        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 5600003 },
                        // 4000210 = Ochre Spice Beetle
                        // 4000211 = Gravel Beetle
                        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 4000211 },
                    },
                    Results = new List<SL_Recipe.ItemQty>
                    {
                        new SL_Recipe.ItemQty{ ItemID = 5600000, Quantity = 1 }
                    }
                };
                r.ApplyRecipe();//*/
                #endregion

            }
            catch (Exception ex)
            {
                MyLogger.LogError("SL_OnPacksLoaded: " + ex.Message);
            }
        }

        // Repair all items in inventory: CharacterEquipment.RepairEquipment
        //On.CharacterEquipment.RepairEquipmentAfterRest += CharacterEquipment_RepairEquipmentAfterRest;

        #region Sorting items
        /*On.LocalCharacterControl.UpdateInteraction += LocalCharacterControl_UpdateInteraction;
        CustomKeybindings.AddAction("ChangeSorting", KeybindingsCategory.Actions, ControlType.Both, 5);
        On.ItemListDisplay.SortBy += ItemListDisplay_SortBy;*/
        #endregion

        //On.DT_QuestEventCheats.RefreshCurrentQuestEvent += DT_QuestEventCheats_RefreshCurrentQuestEvent;

        //On.ItemDetailsDisplay.GetTemporaryDesc += ItemDetailsDisplay_GetTemporaryDesc;

        //On.PlayerCharacterStats.UpdateWeight += PlayerCharacterStats_UpdateWeight; // Burden malus

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
