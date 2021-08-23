using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RottenStashes
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class RottenStashes : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.rottenstashes";
        const string NAME = "RottenStashes";
        const string VERSION = "1.0.4";

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);
        public static double LastGameTime = 0f;

        internal void Awake()
        {
            try
            {
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }

        /*private void Item_ReduceDurability(On.Item.orig_ReduceDurability orig, Item self, float _durabilityLost)
        {
            // 4100550
            if (self.name.Split(new char[] { '_' })[0] == "4000050")
            {
                //OLogger.Log($"{self.name} {self.name.Split(new char[] { '_' })[0]}");
                string m_onBreakNotificationOverride = (string)AccessTools.Field(typeof(Item), "m_onBreakNotificationOverride").GetValue(self);
                OLogger.Log($"{self.CurrentDurability} -= {_durabilityLost}");
                OLogger.Log(m_onBreakNotificationOverride);
                OLogger.Log($"Owner={self.OwnerCharacter}");
                if (self.OwnerCharacter)
                {
                    OLogger.Log($"CharacterUI={self.OwnerCharacter.CharacterUI}");
                }
            }
            orig(self, _durabilityLost);
        }*/
    }
}
