using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SaveSystemReworked
{
    [HarmonyPatch(typeof(Character), "Die")]
    public class Character_Die
    {
        [HarmonyPrefix]
        public static bool Patch_Character_Die(Character __instance, ref Vector3 _hitVec, bool _loadedDead = false)
        {
            // For all non-Player characters, run the regular Die-functionality
            if (__instance.IsLocalPlayer == false)
            {
                return true;
            }

            // If Player character dies, restore values (Stats, Inventory etc.) to state from last Save
            NetworkLevelLoader.Instance.LocalCharStarted(__instance);

            // Go back to Main Menu
            MenuManager.Instance.BackToMainMenu();
            
            // Do not run original code
            return false;
        }
    }
}
