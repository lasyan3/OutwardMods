using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Item;

namespace DynamicQuickslots.Hooks
{
    [HarmonyPatch(typeof(GatherWaterInteraction), "OnActivate")]
    public class GatherWaterInteraction_OnActivate
    {
        [HarmonyPrefix]
        public static bool OnActivate(GatherWaterInteraction __instance)
        {
            try
            {
                WorkInProgress.Instance.MyLogger.LogDebug("GatherWaterInteraction");
                if (__instance.WaterType != WaterType.Clean && __instance.WaterType != WaterType.Fresh)
                {
                    return true;
                }
                WorkInProgress.Instance.MyLogger.LogDebug(" > " + __instance.WaterType);
                WaterContainer waterSkinToFill = __instance.LastCharacter.Inventory.GetWaterSkinToFill(__instance.WaterType);
                if (waterSkinToFill == null)
                {
                    return false;
                }
                WorkInProgress.Instance.MyLogger.LogDebug(" > " + waterSkinToFill);
                if (__instance.WaterType == WaterType.Clean)
                {
                    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(waterSkinToFill.GetComponent<ItemStats>(), WorkInProgress.ItemDurabilities[eItemIDs.CleanWater].MaxDurability);
                }
                else
                {
                    AccessTools.Field(typeof(ItemStats), "m_baseMaxDurability").SetValue(waterSkinToFill.GetComponent<ItemStats>(), WorkInProgress.ItemDurabilities[eItemIDs.RiverWater].MaxDurability);
                }
                AccessTools.Field(typeof(Item), "m_currentDurability").SetValue(waterSkinToFill, waterSkinToFill.MaxDurability);
                waterSkinToFill.gameObject.AddComponent<Perishable>();

            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("GatherWaterInteraction_OnActivate: " + ex.Message);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RecipeManager), "LoadCraftingRecipe")]
    public class RecipeManager_LoadCraftingRecipe
    {
        [HarmonyPostfix]
        public static void LoadCraftingRecipe(RecipeManager __instance)
        {
            try
            {
                Dictionary<string, Recipe> m_recipes = (Dictionary<string, Recipe>)AccessTools.Field(typeof(RecipeManager), "m_recipes").GetValue(__instance);
                Dictionary<Recipe.CraftingType, List<UID>> m_recipeUIDsPerUstensils = (Dictionary<Recipe.CraftingType, List<UID>>)AccessTools.Field(typeof(RecipeManager), "m_recipeUIDsPerUstensils").GetValue(__instance);

                // Remove recipe from Rancid Water to Clean Water
                var re = m_recipes.First(r => r.Value.name == "12_CleanWater_Recipe2");
                m_recipes.Remove(re.Key);
                m_recipeUIDsPerUstensils[Recipe.CraftingType.Cooking].Remove(re.Value.UID);
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("LoadCraftingRecipe: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(CraftingMenu), "OnIngredientSelectorClicked")]
    public class CleaningWaterNeedsCookingPot
    {
        [HarmonyPrefix]
        public static bool OnIngredientSelectorClicked(CraftingMenu __instance, int _selectorIndex)
        {
            try
            {
                IngredientSelector[] m_ingredientSelectors = (IngredientSelector[])AccessTools.Field(typeof(CraftingMenu), "m_ingredientSelectors").GetValue(__instance);
                List<int> ingredientSelectorCachedItems = (List<int>)AccessTools.Field(typeof(CraftingMenu), "ingredientSelectorCachedItems").GetValue(__instance);
                int m_lastRecipeIndex = (int)AccessTools.Field(typeof(CraftingMenu), "m_lastRecipeIndex").GetValue(__instance);
                List<KeyValuePair<int, Recipe>> m_complexeRecipes = (List<KeyValuePair<int, Recipe>>)AccessTools.Field(typeof(CraftingMenu), "m_complexeRecipes").GetValue(__instance);
                List<RecipeDisplay> m_recipeDisplays = (List<RecipeDisplay>)AccessTools.Field(typeof(CraftingMenu), "m_recipeDisplays").GetValue(__instance);
                DictionaryExt<int, CompatibleIngredient> m_availableIngredients = (DictionaryExt<int, CompatibleIngredient>)AccessTools.Field(typeof(CraftingMenu), "m_availableIngredients").GetValue(__instance);
                CharacterUI m_characterUI = (CharacterUI)AccessTools.Field(typeof(CraftingMenu), "m_characterUI").GetValue(__instance);
                ItemListSelector m_tempSelectorWindow = (ItemListSelector)AccessTools.Field(typeof(CraftingMenu), "m_tempSelectorWindow").GetValue(__instance);
                Transform m_selectorWindowPos = (Transform)AccessTools.Field(typeof(CraftingMenu), "m_selectorWindowPos").GetValue(__instance);
                bool m_simpleMode = (bool)AccessTools.Field(typeof(CraftingMenu), "m_simpleMode").GetValue(__instance);

                IngredientSelector ingredientSelector = m_ingredientSelectors[_selectorIndex];
                ingredientSelectorCachedItems.Clear();
                if (m_lastRecipeIndex != -1)
                {
                    if (m_complexeRecipes[m_lastRecipeIndex].Value.Ingredients[_selectorIndex].ActionType == RecipeIngredient.ActionTypes.AddGenericIngredient)
                    {
                        IList<int> stepCompatibleIngredients = m_recipeDisplays[m_lastRecipeIndex].GetStepCompatibleIngredients(_selectorIndex);
                        for (int i = 0; i < stepCompatibleIngredients.Count; i++)
                        {
                            if (m_availableIngredients.TryGetValue(stepCompatibleIngredients[i], out CompatibleIngredient _outValue) && _outValue.AvailableQty > 0)
                            {
                                ingredientSelectorCachedItems.Add(_outValue.ItemID);
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < m_availableIngredients.Count; j++)
                    {
                        if (m_availableIngredients.Values[j].AvailableQty > 0 || ingredientSelector.AssignedIngredient == m_availableIngredients.Values[j])
                        {
                            // lasyan3 : Disallow cleaning water without cooking pot
                            if (m_availableIngredients.Values[j].IsWaterItem && m_simpleMode) continue;
                            // lasyan3 : Disallow use of rancid water
                            //if (m_availableIngredients.Values[j].IsWaterItem && m_availableIngredients.Values[j].ItemPrefab.ItemID == (int)eItemIDs.RancidWater) continue;
                            ingredientSelectorCachedItems.Add(m_availableIngredients.Values[j].ItemID);
                        }
                    }
                }
                if (ingredientSelectorCachedItems.Count > 0)
                {
                    m_tempSelectorWindow = m_characterUI.GetListSelector();
                    if ((bool)m_tempSelectorWindow)
                    {
                        m_tempSelectorWindow.onItemHovered = __instance.RefreshItemDetailDisplay;
                        m_tempSelectorWindow.onItemClicked = delegate (IItemDisplay _display)
                        {
                            __instance.IngredientSelectorHasChanged(_selectorIndex, _display?.RefItem.ItemID ?? (-1));
                            m_tempSelectorWindow.Hide();
                        };
                        m_tempSelectorWindow.Position = (((bool)m_selectorWindowPos) ? m_selectorWindowPos.position : __instance.transform.position);
                        m_tempSelectorWindow.Title = ingredientSelector.Text;
                        m_tempSelectorWindow.ShowSelector(ingredientSelectorCachedItems, ingredientSelector.gameObject, m_lastRecipeIndex == -1);
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("OnIngredientSelectorClicked: " + ex.Message);
            }
            return false;
        }

    }

    [HarmonyPatch(typeof(CraftingMenu), "TryCraft")]
    public class CTryCraft
    {
        [HarmonyPrefix]
        public static bool TryCraft(CraftingMenu __instance)
        {
            try
            {
                int m_lastRecipeIndex = (int)AccessTools.Field(typeof(CraftingMenu), "m_lastRecipeIndex").GetValue(__instance);
                bool m_canCraft = (bool)AccessTools.Field(typeof(CraftingMenu), "m_canCraft").GetValue(__instance);
                if (m_lastRecipeIndex == -1 && m_canCraft)
                {
                    int num = 0;
                    CompatibleIngredient compatibleIngredient = null;
                    IngredientSelector[] m_ingredientSelectors = (IngredientSelector[])AccessTools.Field(typeof(CraftingMenu), "m_ingredientSelectors").GetValue(__instance);
                    for (int i = 0; i < m_ingredientSelectors.Length; i++)
                    {
                        if (m_ingredientSelectors[i].AssignedIngredient != null && !m_ingredientSelectors[i].IsMissingIngredient)
                        {
                            compatibleIngredient = m_ingredientSelectors[i].AssignedIngredient;
                            num++;
                        }
                    }
                    if (num == 1 && compatibleIngredient != null)
                    {
                        // lasyan3 : rancid water needs other ingredients to be cleaned
                        if (compatibleIngredient.IsWaterItem && compatibleIngredient.WaterType == WaterType.Rancid)
                        {
                            CharacterUI m_characterUI = (CharacterUI)AccessTools.Field(typeof(CraftingMenu), "m_characterUI").GetValue(__instance);
                            m_characterUI.ShowInfoNotificationLoc("Cleaning such water needs one more ingredient...");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("TryCraft: " + ex.Message);
            }
            return true;
        }

    }
}
