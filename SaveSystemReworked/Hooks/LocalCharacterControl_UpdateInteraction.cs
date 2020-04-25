﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SaveSystemReworked
{
    [HarmonyPatch(typeof(LocalCharacterControl), "UpdateInteraction")]
    public class LocalCharacterControl_UpdateInteraction
    {
        static CharacterSave QuickSave = null;

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

                if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("QuickSave"))
                {
                    SaveSystemReworked.MyLogger.LogDebug("QuickSave");
                    SaveManager.Instance.Save(true, true);
                    //QuickSave = SaveManager.Instance.ChooseCharacterSaveInstance(charUID, 0);

                    if ((bool)Global.AudioManager)
                    {
                        Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_NEWGAME_SelectSave);
                    }
                }
                if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("QuickLoad"))
                {
                    /*SaveSystemReworked.MyLogger.LogDebug("QuickLoad");
                    if (QuickSave == null)
                    {
                        QuickSave = SaveManager.Instance.ChooseCharacterSaveInstance(charUID, 0);
                    }
                    SplitScreenManager.Instance.LocalPlayers[0].SetChosenSave(QuickSave);

                    QuickSave.ApplyLoadedSaveToChar(__instance.Character);*/
                }
            }
            catch (Exception ex)
            {
                SaveSystemReworked.MyLogger.LogError(ex.Message);
            }
        }
    }
}
