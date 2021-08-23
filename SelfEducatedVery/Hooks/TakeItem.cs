using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(CharacterInventory), "TakeItem", new Type[] { typeof(Item), typeof(bool) })]
    public class Item_BaseInit
    {
        [HarmonyPostfix]
        public static void TakeItem(CharacterInventory __instance, Item takenItem, bool _tryToEquip)
        {
            try
            {
                // Gatherable, ItemContainerStatic (only not player), MerchantPouch, ItemContainer
                if (takenItem.ParentContainer is MerchantPouch
                    || (takenItem.ParentContainer is ItemContainerStatic /*&& (takenItem.ParentContainer as ItemContainerStatic).IsChildToPlayer*/)
                    || (takenItem.OwnerCharacter != null && !takenItem.OwnerCharacter.IsDead))
                {
                    //OLogger.Log($"forbiden");
                    return; // Only loot from dead enemies or when crafting
                }
                //OLogger.Log($"ParentContainer={takenItem.ParentContainer.GetType()}");
                if ((!takenItem.IsFood && !takenItem.IsIngredient)
                           || takenItem.IsDrink
                           || takenItem.IsEquippable || takenItem.IsDeployable)
                //(takenItem.IsFood || takenItem.IsIngredient) && !takenItem.IsDrink && !takenItem.IsEquippable && !takenItem.IsDeployable                
                {
                    return;
                }

                //string iName = takenItem.name.Substring(0, takenItem.name.LastIndexOf('_'));
                string iName = takenItem.name.Split(new char[] { '_' })[0];
                //OLogger.Log($"TakeItem={iName}");

                //if (!m_recipes.ContainsKey(iName))
                //{
                //    return;
                //}

                // Only ingredients not so common (like salt)
                // Alternative : add skills to unlock some recipes

                // Only items that have no RecipeItem ?
                Dictionary<string, Recipe> allRecipes = (Dictionary<string, Recipe>)AccessTools.Field(typeof(RecipeManager), "m_recipes").GetValue(RecipeManager.Instance);
                List<Recipe> lstRecipes = allRecipes.Where(r =>
                    r.Value.Ingredients.Any(i => i.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient && i.AddedIngredient.ItemID == takenItem.ItemID)
                    && !__instance.RecipeKnowledge.IsRecipeLearned(r.Key) // Ignore recipes already learned
                    ).Select(r => r.Value).ToList();
                foreach (var recipe in lstRecipes)
                {
                    // Calculate chance of learning recipes based on item's value
                    int itemValue = recipe.Results[0].RefItem.Value;
                    __instance.RecipeKnowledge.LearnRecipe(recipe);
                    //break; // only one recipe to add
                }
            }
            catch (Exception ex)
            {
                SelfEducatedVery.MyLogger.LogError("TakeItem: " + ex.Message);
            }
        }
    }
}
