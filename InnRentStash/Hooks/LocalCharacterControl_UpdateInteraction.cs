using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(LocalCharacterControl), "UpdateInteraction")]
    public class LocalCharacterControl_UpdateInteraction
    {
        [HarmonyPostfix]
        public static void UpdateInteraction(LocalCharacterControl __instance)
        {
            if (__instance.InputLocked)
            {
                return;
            }

            try
            {
                UID charUID = __instance.Character.UID;
                int playerID = __instance.Character.OwnerPlayerSys.PlayerID;

                if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("StashSharing"))
                {
                    InnRentStash.m_isStashSharing = !InnRentStash.m_isStashSharing;
                    if (InnRentStash.m_isStashSharing)
                    {
                        __instance.Character.CharacterUI.SmallNotificationPanel.ShowNotification("Sharing enabled", 2f);
                    }
                    else
                    {
                        __instance.Character.CharacterUI.SmallNotificationPanel.ShowNotification("Sharing disabled", 2f);
                    }
                }
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError(ex.Message);
            }
        }
    }
}
