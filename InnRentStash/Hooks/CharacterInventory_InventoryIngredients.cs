using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(CharacterInventory), "InventoryIngredients",
    new Type[] { typeof(Tag), typeof(DictionaryExt<int, CompatibleIngredient>) },
    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    public class CharacterInventory_InventoryIngredients
    {
        [HarmonyPostfix]
        public static void InventoryIngredients(CharacterInventory __instance, Tag _craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> _sortedIngredient)
        {
            try
            {
                if (InnRentStash.CurrentStash == null || !InnRentStash.CurrentStash.IsInteractable || !InnRentStash.Instance.ConfigStashSharing.Value)
                {
                    return;
                }
                //InnRentStash.MyLogger.LogDebug($"InventoryIngredients for {_craftingStationTag}");
                /*foreach (var m in AccessTools.GetDeclaredMethods(typeof(CharacterInventory)))
                {
                    InnRentStash.MyLogger.LogDebug($"{m.Name}");
                    if (m.Name == "InventoryIngredients")
                    {
                        foreach (var p in m.GetParameters())
                        {
                            InnRentStash.MyLogger.LogDebug($"  |- {p.Name} {p.IsOut} {p}");
                        }
                    }
                }*/
                MethodInfo mi = AccessTools.GetDeclaredMethods(typeof(CharacterInventory)).FirstOrDefault(m => m.Name == "InventoryIngredients"
                    && m.GetParameters().Any(p => p.Name == "_items"));
                //InnRentStash.MyLogger.LogDebug($"{mi}");
                //InnRentStash.MyLogger.LogDebug($"Before={_sortedIngredient.Count}");
                mi.Invoke(__instance, new object[] {
                        _craftingStationTag, _sortedIngredient, InnRentStash.CurrentStash.GetContainedItems()
                });
                //InnRentStash.MyLogger.LogDebug($"After={_sortedIngredient.Count}");
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError("InventoryIngredients: " + ex.Message);
            }
        }
    }
}
