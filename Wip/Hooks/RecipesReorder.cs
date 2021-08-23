using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Item;

namespace WorkInProgress.Hooks
{
    [HarmonyPatch(typeof(CraftingMenu), "OnRecipeSelected")]
    public class RecipesReorder
    {
        public static Dictionary<string, List<int>> dctIngr2 = new Dictionary<string, List<int>>();

        [HarmonyPrefix]
        public static bool PreOnRecipeSelected(CraftingMenu __instance, int _index) // Called on select recipe or crafted
        {
            try
            {
                if (_index != -1)
                {
                    List<KeyValuePair<int, Recipe>> m_complexeRecipes = (List<KeyValuePair<int, Recipe>>)AccessTools.Field(typeof(CraftingMenu), "m_complexeRecipes").GetValue(__instance);
                    //WorkInProgress.Instance.MyLogger.LogDebug($"PreOnRecipeSelected: {m_complexeRecipes[_index].Value.name}");
                    List<RecipeDisplay> m_recipeDisplays = (List<RecipeDisplay>)AccessTools.Field(typeof(CraftingMenu), "m_recipeDisplays").GetValue(__instance);
                    IList<int> m_bestIngredients = (IList<int>)AccessTools.Field(typeof(RecipeDisplay), "m_bestIngredients").GetValue(m_recipeDisplays[_index]);
                    if (dctIngr2.ContainsKey(m_complexeRecipes[_index].Value.name))
                    {
                        //WorkInProgress.Instance.MyLogger.LogDebug($" > Load ingredients");
                        AccessTools.Field(typeof(RecipeDisplay), "m_bestIngredients").SetValue(m_recipeDisplays[_index], dctIngr2[m_complexeRecipes[_index].Value.name]);
                    }
                    //foreach (var item in m_bestIngredients)
                    //{
                    //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                    //}
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("PreOnRecipeSelected: " + ex.Message);
            }
            return true;
        }//*/
    }

    [HarmonyPatch(typeof(CraftingMenu), "IngredientSelectorHasChanged")]
    public class CraftingMenu_SaveSelectedIngredient
    {
        [HarmonyPostfix]
        public static void IngredientSelectorHasChanged(CraftingMenu __instance, int _selectorIndex, int _itemID)
        {
            try
            {
                int m_lastRecipeIndex = (int)AccessTools.Field(typeof(CraftingMenu), "m_lastRecipeIndex").GetValue(__instance);
                //WorkInProgress.Instance.MyLogger.LogDebug($"IngredientSelectorHasChanged: {_selectorIndex}={_itemID} ({m_lastRecipeIndex})");
                if (m_lastRecipeIndex != -1)
                {
                    List<KeyValuePair<int, Recipe>> m_complexeRecipes = (List<KeyValuePair<int, Recipe>>)AccessTools.Field(typeof(CraftingMenu), "m_complexeRecipes").GetValue(__instance);
                    Recipe rec = m_complexeRecipes[m_lastRecipeIndex].Value;
                    //WorkInProgress.Instance.MyLogger.LogDebug($" > {rec.name}");

                    List<RecipeDisplay> m_recipeDisplays = (List<RecipeDisplay>)AccessTools.Field(typeof(CraftingMenu), "m_recipeDisplays").GetValue(__instance);
                    IList<int> m_bestIngredients = (IList<int>)AccessTools.Field(typeof(RecipeDisplay), "m_bestIngredients").GetValue(m_recipeDisplays[m_lastRecipeIndex]);
                    if (!RecipesReorder.dctIngr2.ContainsKey(rec.name))
                    {
                        RecipesReorder.dctIngr2.Add(rec.name, m_bestIngredients.ToList());
                    }
                    RecipesReorder.dctIngr2[rec.name][_selectorIndex] = _itemID;
                    //WorkInProgress.Instance.MyLogger.LogDebug($" > Ingredient saved!");
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("IngredientSelectorHasChanged: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(CraftingMenu), "ProcessIngredientMatches")]
    public class ProcessIngredientMatches2
    {
        [HarmonyPostfix]
        public static void ProcessIngredientMatches(CraftingMenu __instance, Recipe _recipe, IList<List<int>> _compatibleItems, IList<int> _bestIngredientsMatch)
        {
            try
            {
                int m_lastRecipeIndex = (int)AccessTools.Field(typeof(CraftingMenu), "m_lastRecipeIndex").GetValue(__instance);
                //WorkInProgress.Instance.MyLogger.LogDebug($"ProcessIngredientMatches: {_recipe.name}");
                if (RecipesReorder.dctIngr2.ContainsKey(_recipe.name))
                {
                    for (int i = 0; i < RecipesReorder.dctIngr2[_recipe.name].Count; i++)
                    {
                        if (RecipesReorder.dctIngr2[_recipe.name][i] > -1
                            && !_compatibleItems.Any(l => l.Contains(RecipesReorder.dctIngr2[_recipe.name][i])))
                        {
                            //WorkInProgress.Instance.MyLogger.LogDebug($" > Delete save");
                            RecipesReorder.dctIngr2.Remove(_recipe.name);
                            break;
                        }
                    }
                }
                //if (RecipesReorder.dctIngr2.ContainsKey(_recipe.name))
                //{
                //    _bestIngredientsMatch = RecipesReorder.dctIngr2[_recipe.name];
                //}
                //foreach (var item in _bestIngredientsMatch)
                //{
                //    WorkInProgress.Instance.MyLogger.LogDebug($" > {item}");
                //}
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("ProcessIngredientMatches: " + ex.Message);
            }
        }
    }
}
