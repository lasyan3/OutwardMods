using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorkInProgress.Hooks
{
    [HarmonyPatch(typeof(Item), "ChangeOwner")]
    public class DisableNewFlag
    {
        [HarmonyPostfix]
        public static void ChangeOwner(Item __instance)
        {
            AccessTools.Field(typeof(Item), "m_isNewInInventory").SetValue(__instance, false);
        }
    }
}
