using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoreGatherableLoot
{
    class GameData
    {
        public int PercentMin;
        public int PercentMax;
        public int AlchemyValueMax;
        public int CookingValueMax;
        public int SurvivalValueMax;
    }

    public class SelfEducated : PartialityMod
    {
        private GameData m_data;
        private readonly string m_modName = "SelfEducated";

        public SelfEducated()
        {
            this.ModID = m_modName;
            this.Version = "1.0.0";
            //this.loadPriority = 0;
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
            try
            {
                m_data = LoadSettings();
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] Init: {ex.Message}");
            }
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

            On.CharacterInventory.TakeItem_3 -= CharacterInventory_TakeItem_3;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            On.CharacterInventory.TakeItem_3 += CharacterInventory_TakeItem_3;
        }

        private void CharacterInventory_TakeItem_3(On.CharacterInventory.orig_TakeItem_3 orig, CharacterInventory self, Item takenItem, bool _tryToEquip)
        {
            orig(self, takenItem, _tryToEquip);
            try
            {
                /*if (takenItem.OwnerCharacter == null || !takenItem.OwnerCharacter.IsDead)
                {
                    return; // Only loot from dead enemies
                }*/
                if ((!takenItem.IsFood && !takenItem.IsIngredient)
                           || takenItem.IsDrink
                           || takenItem.IsEquippable || takenItem.IsDeployable)
                //(takenItem.IsFood || takenItem.IsIngredient) && !takenItem.IsDrink && !takenItem.IsEquippable && !takenItem.IsDeployable                
                {
                    return;
                }

                OLogger.Log($"TakeItem={takenItem.Name}");
                // Only items that have no RecipeItem ?
                Dictionary<string, Recipe> allRecipes = (Dictionary<string, Recipe>)AccessTools.Field(typeof(RecipeManager), "m_recipes").GetValue(RecipeManager.Instance);
                List<Recipe> lstRecipes = allRecipes.Where(r =>
                    r.Value.Ingredients != null
                    // All recipes containing the item looted
                    && r.Value.Ingredients.Any(i => i.AddedIngredient != null && i.AddedIngredient.ItemID == takenItem.ItemID)
                    && r.Value.Results.Length > 0
                    // Filter by recipe's value
                    && (r.Value.CraftingStationType == Recipe.CraftingType.Survival //&& r.Value.Results[0].Item.Value <= 100
                        || r.Value.CraftingStationType == Recipe.CraftingType.Cooking //&& r.Value.Results[0].Item.Value <= 10
                        || r.Value.CraftingStationType == Recipe.CraftingType.Alchemy /*&& r.Value.Results[0].Item.Value <= 1000*/)
                    && !self.RecipeKnowledge.IsRecipeLearned(r.Key) // Ignore recipes already learned
                    ).Select(r => r.Value).ToList();
                foreach (var recipe in lstRecipes)
                {
                    // Calculate chance of learning recipes based on item's value
                    int itemValue = recipe.Results[0].Item.Value;
                    //int recipeValue = 
                    double chance = 0f;
                    switch (recipe.CraftingStationType)
                    {
                        case Recipe.CraftingType.Alchemy: // Value between 0 and 60
                            chance = m_data.PercentMax - ((float)(m_data.PercentMax - m_data.PercentMin) / m_data.AlchemyValueMax) * itemValue;
                            break;
                        case Recipe.CraftingType.Cooking: // Value between 0 and 30
                            chance = m_data.PercentMax - ((float)(m_data.PercentMax - m_data.PercentMin) / m_data.CookingValueMax) * itemValue;
                            break;
                        case Recipe.CraftingType.Survival:  // Value between 0 and 100
                            chance = m_data.PercentMax - ((float)(m_data.PercentMax - m_data.PercentMin) / m_data.SurvivalValueMax) * itemValue;
                            break;
                    }
                    //OLogger.Log($"{recipe.Name} ({Math.Round(chance, 0)} %)");
                    int r = UnityEngine.Random.Range(0, 100);
                    if (r < chance)
                    {
                        OLogger.Log($"{recipe.Name} ({r} < {Math.Round(chance, 2)} %)");
                        self.RecipeKnowledge.LearnRecipe(recipe);
                        break; // only one recipe to add
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] CharacterInventory_TakeItem_3: {ex.Message}");
            }
        }

        private GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader($"mods/{m_modName}Config.json"))
                {
                    try
                    {
                        return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                    }
                    catch (ArgumentNullException)
                    {
                    }
                    catch (FormatException ex)
                    {
                        //OLogger.Error("Format Exception", _modName);
                        Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                //OLogger.Error("File Not Found Exception", _modName);
                Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
            }
            catch (IOException ex)
            {
                //OLogger.Error("General IO Exception", _modName);
                Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
            }
            return null;
        }
    }
}
