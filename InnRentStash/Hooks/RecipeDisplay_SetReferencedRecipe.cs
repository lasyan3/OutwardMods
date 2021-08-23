using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(RecipeDisplay), "SetReferencedRecipe", new Type[] { typeof(Recipe), typeof(bool), typeof(IList<int>[]), typeof(IList<int>) })]
    public class RecipeDisplay_SetReferencedRecipe
    {
        //static IList<int> lstBkp;
        //[HarmonyPrefix]
        //public static bool PreSetReferencedRecipe(RecipeDisplay __instance, Recipe _recipe, bool _canBeCompleted, IList<int>[] _compatibleIngredients, IList<int> _ingredients)
        //{
        //    if (__instance.BestIngredientsPerStep.Count > 0)
        //    {
        //        lstBkp = new List<int>(__instance.BestIngredientsPerStep);
        //    }
        //    return true;
        //}

        [HarmonyPostfix]
        public static void SetReferencedRecipe(RecipeDisplay __instance, Recipe _recipe, bool _canBeCompleted, IList<int>[] _compatibleIngredients, IList<int> _ingredients)

        {
            try
            {
                if (_recipe.Results.Length == 0)
                {
                    return;
                }

                Text m_lblRecipeName = (Text)AccessTools.Field(typeof(RecipeDisplay), "m_lblRecipeName").GetValue(__instance);
                int invQty = __instance.LocalCharacter.Inventory.ItemCount(_recipe.Results[0].ItemID);
                int stashQty = 0;
                if (InnRentStash.CurrentStash != null && InnRentStash.CurrentStash.IsInteractable && InnRentStash.Instance.ConfigStashSharing.Value)
                {
                    stashQty = InnRentStash.CurrentStash.ItemStackCount(_recipe.Results[0].ItemID);
                }
                if (invQty + stashQty > 0)
                {
                    __instance.SetName(m_lblRecipeName.text += $" ({invQty + stashQty})");
                }
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError("SetReferencedRecipe: " + ex.Message);
            }
        }

    }
}
