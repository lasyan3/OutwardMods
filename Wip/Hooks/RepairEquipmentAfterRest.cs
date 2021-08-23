using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorkInProgress.Hooks
{
    [HarmonyPatch(typeof(CharacterEquipment), "RepairEquipmentAfterRest")]
    public class RepairEquipmentAfterRest
    {
        [HarmonyPostfix]
        public static void PostRepairEquipmentAfterRest(CharacterEquipment __instance)
        {
            try
            {
                Character m_character = (Character)AccessTools.Field(typeof(CharacterEquipment), "m_character").GetValue(__instance);
                int repairLength = m_character.CharacterResting.GetRepairLength();
                float num = m_character.PlayerStats.RestRepairEfficiency;
                foreach (Item it in __instance.LastOwnedBag.Container.GetContainedItems())
                {
                    if (it.RepairedInRest)
                    {
                        float num2 = num * (float)repairLength * 0.01f;
                        float num3 = it.DurabilityRatio + num2;
                        if (num3 > 1f)
                        {
                            num3 = 1f;
                        }
                        it.SetDurabilityRatio(num3);
                    }
                }
                foreach (Item it in m_character.Inventory.Pouch.GetContainedItems())
                {
                    if (it.RepairedInRest && it.IsEquippable)
                    {
                        float num2 = num * (float)repairLength * 0.01f;
                        float num3 = it.DurabilityRatio + num2;
                        if (num3 > 1f)
                        {
                            num3 = 1f;
                        }
                        it.SetDurabilityRatio(num3);
                    }
                }
            }
            catch (Exception ex)
            {
                WorkInProgress.Instance.MyLogger.LogError("RepairEquipment: " + ex.Message);
            }
        }
    }
}
