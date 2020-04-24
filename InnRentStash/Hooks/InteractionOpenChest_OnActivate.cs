using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(InteractionOpenChest), "OnActivate")]
    public class InteractionOpenChest_OnActivate
    {
        [HarmonyPrefix]
        public static bool OnActivate(InteractionOpenChest __instance)
        {
            // Disable generation content for house stash
            if (__instance.Item is TreasureChest && (__instance.Item as TreasureChest).SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {
                AccessTools.Field(typeof(TreasureChest), "m_hasGeneratedContent").SetValue(__instance.Item, true);
            }
            return true;
        }

    }
}
