using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(Item), "BaseInit")]
    public class Item_BaseInit
    {
        [HarmonyPostfix]
        public static void BaseInit(Item __instance)
        {
            try
            {
                if (__instance.ItemIDString == "3100140" || __instance.ItemIDString == "3100141" || __instance.ItemIDString == "3100142")
                {
                    //Armor armor = self as Armor;
                    EquipmentStats m_stats = (EquipmentStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(__instance);
                    if (m_stats == null)
                    {
                        EnhancedTsarSet.MyLogger.LogDebug($"Item {__instance.ItemIDString} has no stats!");
                        return;
                    }
                    if (__instance.ItemIDString == "3100140") // Armor
                    {
                        float damageResistance = 40.0f;

                        // Elemental resistances
                        float[] m_damageResistance = (float[])AccessTools.Field(typeof(EquipmentStats), "m_damageResistance").GetValue(m_stats);
                        m_damageResistance[(int)DamageType.Types.Decay] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Electric] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Ethereal] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Fire] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Frost] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Physical] = damageResistance;
                        AccessTools.Field(typeof(EquipmentStats), "m_damageResistance").SetValue(m_stats, m_damageResistance);

                        AccessTools.Field(typeof(EquipmentStats), "m_impactResistance").SetValue(m_stats, 30.0f);
                        //AccessTools.Field(typeof(EquipmentStats), "m_damageProtection").SetValue(m_stats, 4.0f);

                        // Temperature resistances
                        AccessTools.Field(typeof(EquipmentStats), "m_coldProtection").SetValue(m_stats, 10.0f);
                        AccessTools.Field(typeof(EquipmentStats), "m_heatProtection").SetValue(m_stats, 10.0f);
                        //AccessTools.Field(typeof(Item), "m_stats").SetValue(self, m_stats);
                    }
                    else if (__instance.ItemIDString == "3100141") // Helm
                    {
                        float damageResistance = 20.0f;

                        // Elemental resistances
                        float[] m_damageResistance = (float[])AccessTools.Field(typeof(EquipmentStats), "m_damageResistance").GetValue(m_stats);
                        m_damageResistance[(int)DamageType.Types.Decay] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Electric] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Ethereal] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Fire] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Frost] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Physical] = damageResistance;
                        AccessTools.Field(typeof(EquipmentStats), "m_damageResistance").SetValue(m_stats, m_damageResistance);

                        AccessTools.Field(typeof(EquipmentStats), "m_impactResistance").SetValue(m_stats, 15.0f);

                        // Temperature resistances
                        AccessTools.Field(typeof(EquipmentStats), "m_coldProtection").SetValue(m_stats, 5.0f);
                        AccessTools.Field(typeof(EquipmentStats), "m_heatProtection").SetValue(m_stats, 5.0f);
                    }
                    else if (__instance.ItemIDString == "3100142") // Boots
                    {
                        float damageResistance = 20.0f;

                        // Elemental resistances
                        float[] m_damageResistance = (float[])AccessTools.Field(typeof(EquipmentStats), "m_damageResistance").GetValue(m_stats);
                        m_damageResistance[(int)DamageType.Types.Decay] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Electric] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Ethereal] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Fire] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Frost] = damageResistance;
                        m_damageResistance[(int)DamageType.Types.Physical] = damageResistance;
                        AccessTools.Field(typeof(EquipmentStats), "m_damageResistance").SetValue(m_stats, m_damageResistance);

                        AccessTools.Field(typeof(EquipmentStats), "m_impactResistance").SetValue(m_stats, 15.0f);

                        // Temperature resistances
                        AccessTools.Field(typeof(EquipmentStats), "m_coldProtection").SetValue(m_stats, 5.0f);
                        AccessTools.Field(typeof(EquipmentStats), "m_heatProtection").SetValue(m_stats, 5.0f);
                    }
                }
            }
            catch (Exception ex)
            {
                EnhancedTsarSet.MyLogger.LogError(ex.Message);
            }
        }
    }
}
