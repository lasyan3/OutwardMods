﻿using Harmony;
using Partiality.Modloader;
using System;
using UnityEngine;

namespace OutwardMods
{
    public class EnhancedTsarSet : PartialityMod
    {
        private readonly string m_modName = "EnhancedTsarSet";

        public EnhancedTsarSet()
        {
            this.ModID = m_modName;
            this.Version = "1.0.0";
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

            On.Item.BaseInit -= Item_BaseInit;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            // Alter item's stats
            On.Item.BaseInit += Item_BaseInit;
        }

        private void Item_BaseInit(On.Item.orig_BaseInit orig, Item self)
        {
            orig(self);
            try
            {
                if (self.ItemIDString == "3100140" || self.ItemIDString == "3100141" || self.ItemIDString == "3100142")
                {
                    //Armor armor = self as Armor;
                    EquipmentStats m_stats = (EquipmentStats)AccessTools.Field(typeof(Item), "m_stats").GetValue(self);
                    if (m_stats == null)
                    {
                        Debug.Log($"[{m_modName}] Item {self.ItemIDString} has no stats!");
                        return;
                    }
                    if (self.ItemIDString == "3100140") // Armor
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
                    else if (self.ItemIDString == "3100141") // Helm
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
                    else if (self.ItemIDString == "3100142") // Boots
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
                Debug.Log($"[{m_modName}] Item_BaseInit: {ex.Message}");
            }
        }
    }
}