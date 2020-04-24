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
        [HarmonyPostfix]
        public static void SetReferencedRecipe(RecipeDisplay __instance, Recipe _recipe)

        {
            try
            {
                if (_recipe.Results.Length == 0)
                {
                    return;
                }
                Text m_lblRecipeName = (Text)AccessTools.Field(typeof(RecipeDisplay), "m_lblRecipeName").GetValue(__instance);
                int invQty = __instance.LocalCharacter.Inventory.ItemCount(_recipe.Results[0].Item.ItemID);
                int stashQty = 0;
                if (InnRentStash.m_currentStash != null && InnRentStash.m_currentStash.IsInteractable && InnRentStash.m_isStashSharing)
                {
                    stashQty = InnRentStash.m_currentStash.ItemStackCount(_recipe.Results[0].Item.ItemID);
                }
                if (invQty + stashQty > 0)
                {
                    __instance.SetName(m_lblRecipeName.text += $" ({invQty + stashQty})");
                }
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError(ex.Message);
            }
        }
    }
}
