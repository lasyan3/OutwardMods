using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Wip
{
    class GameData
    {
        public int PercentMin;
        public int PercentMax;
        public int AlchemyValueMax;
        public int CookingValueMax;
        public int SurvivalValueMax;
    }

    public class SelfEducated : PartialityMod
    {
        //private GameData m_data;
        private readonly string m_modName = "SelfEducated";
        private readonly Dictionary<string, List<string>> m_recipes = new Dictionary<string, List<string>>() {
            //{ ""/*_"*/, new List<string>(){ "" } },
            { "2100061"/*_GalvanicGolem1HRapierBroken"*/, new List<string>(){ "138_GolemRapier_Recipe" } },

            { "3000033"/*_BlackScavengerHelmBottom"*/, new List<string>(){ "153_ScavengerMask_Recipe" } },
            { "3000032"/*_ScavengerHelmTop"*/, new List<string>(){ "153_ScavengerMask_Recipe" } },

            { "4000010"/*_Gaberries"*/, new List<string>(){ "17_GaberryJam_Recipe", "51_BoiledGaberry_Recipe" } },
            { "4000030"/*_Turmmip"*/, new List<string>(){ "21_TurmmipPotage_Recipe", "52_GrilledTurmmip_Recipe", "108_ManaPotion_Recipe" } },
            { "4000050"/*_RawMeat"*/, new List<string>(){ "3_Cookedmeat_Recipe" } },
            { "4000060"/*_RawAlphaMeat"*/, new List<string>(){ "19_AlphaJerky_Recipe", "4_CookedAlphaMeat_Recipe", "49_AlphaSandwich_Recipe", "59_SavagePorridge_Recipe", "68_BreadOfTheWild_Recipe" } },
            { "4000070"/*_CrustaceanEgg"*/, new List<string>(){ "100_LightRag_Recipe", "13_OceanFricassee_Recipe", "8_BoiledCrustaceanEgg_Recipe" } },
            { "4000090"/*_AzurShrimp"*/, new List<string>(){ "72_BoiledAzurShrimp_Recipe", "47_LuxeLichette_Recipe" } },
            { "4000110"/*_RainbowTrout"*/, new List<string>(){ "46_CierzoCeviche_Recipe", "47_LuxeLichette_Recipe", "6_GrilledRainbowTrout_Recipe" } },
            { "4000130"/*_Salmon"*/, new List<string>(){ "5_GrilledSalmon_Recipe" } },
            { "4000150"/*_BleedingMushroom"*/, new List<string>(){ "110_HealthPotion_Recipe", "175_TrapCharge-NerveGas_Recipe" } },
            { "4000180"/*_StarMushroom"*/, new List<string>(){ "108_ManaPotion_Recipe" } },
            { "4000190"/*_Marshmelon"*/, new List<string>(){ "56_GrilledMarshmelon_Recipe", "58_RagoutDuMarais_Recipe", "59_SavagePorridge_Recipe", "60_MarshmelonJelly_Recipe" } },
            { "4000200"/*_CactusFruit"*/, new List<string>(){ "57_SoftenedCactusFruit_Recipe", "74_NeedleTea_Recipe", "66_SpinyMeringue_Recipe", "67_CactusPie_Recipe" } },
            { "4000210"/*_OchreSpiceBeetle"*/, new List<string>(){ "11_BitterSpicyTea_Recipe", "16_PungentPaste_Recipe", "69_MyconicCleanser_Recipe", "113_DisciplinePotion_Recipe", "164_HallowedMarshAntidote_Recipe", "129_SurvivorElixir_Recipe" } },
            { "4000211"/*_GravelBeetle"*/, new List<string>(){ "75_MineralTea_Recipe", "59_SavagePorridge_Recipe", "112_RagePotion_Recipe", "127_StonefleshElixir_Recipe", "110_HealthPotion_Recipe", "115_CoolPotion_Recipe" } },
            { "4000220"/*_CommonMushroom"*/, new List<string>(){ "20_DryMushroomBar_Recipe", "53_GrilledMushroom_Recipe", "111_Antidote_Recipe" } },
            { "4000230"/*_BirdEgg"*/, new List<string>(){ "7_BoiledBirdEgg_Recipe" } },
            { "4000240"/*_Woolshroom"*/, new List<string>(){ "63_GrilledWoolshroom_Recipe", "68_BreadOfTheWild_Recipe", "69_MyconicCleanser_Recipe", "70_StrignySalad_Recipe", "119_StealthPotion_Recipe" } },
            { "4000250"/*_Miasmapod"*/, new List<string>(){ "55_BoiledMiasmapod_Recipe", "58_RagoutDuMarais_Recipe", "103_PoisonVarnish_Recipe" } },
            { "4000260"/*_JewelBirdMeat"*/, new List<string>(){ "54_CookedJewelMeat_Recipe", "65_DiadèmeDeGibier_Recipe" } },
            { "4000270"/*_SmokeRoot"*/, new List<string>(){ "62_SearedRoot_Recipe", "68_BreadOfTheWild_Recipe", "112_RagePotion_Recipe" } },
            { "4000280"/*_KrimpNut"*/, new List<string>(){ "109_StaminaPotion_Recipe" } },

            { "4100030"/*_GaberryJam"*/, new List<string>(){ "50_GaberryTartine_Recipe" } },
            { "4100170"/*_Bread"*/, new List<string>(){ "64_Toast_Recipe" } },
            { "4100420"/*_MarshMelonJelly"*/, new List<string>(){ "61_MarshmelonTartine_Recipe" } },
            { "4100570"/*_CrabeyesGrilled"*/, new List<string>(){ "99_PoisonRag_Recipe", "176_GrilledCrabeyeSeed_Recipe", "103_PoisonVarnish_Recipe", "174_TrapCharge-Toxic_Recipe" } },

            { "6000010"/*_FireflyPowder"*/, new List<string>(){ "104_LightVarnish_Recipe", "117_BlessedPotion_Recipe", "164_HallowedMarshAntidote_Recipe" } },
            { "6000030"/*_Livweedi"*/, new List<string>(){ "113_DisciplinePotion_Recipe", "175_TrapCharge-NerveGas_Recipe" } },
            { "6000040"/*_GhostEye"*/, new List<string>(){ "107_SpiritualVarnish_Recipe", "116_MistPotion_Recipe", "119_StealthPotion_Recipe" } },
            { "6000060"/*_Seaweed"*/, new List<string>(){ "102_IceRag_Recipe", "13_OceanFricassee_Recipe", "73_SoothingTea_Recipe" } },
            { "6200050"/*_Ammolite"*/, new List<string>(){ "182_AmmoliteArmor_Recipe", "183_AmmoliteHelmet_Recipe", "184_AmmoliteBoots_Recipe" } },
            { "6400070"/*_PalladiumScrap"*/, new List<string>(){ "172_PalladiumSpikes_Recipe" } },
            { "6400110"/*_BlueSand"*/, new List<string>(){ "178_Coldstone_Recipe" } },
            { "6400130"/*_ManaStone"*/, new List<string>(){ "45_CrystalPowder_Recipe" } },
            { "6400140"/*_IronScrap"*/, new List<string>(){ "139_Bullet_Recipe", "162_ShivDagger_Recipe", "171_IronSpikes_Recipe" } },
            { "6500010"/*_FireStone"*/, new List<string>(){ "105_FireVarnish_Recipe" } },
            { "6500020"/*_ColdStone"*/, new List<string>(){ "137_ColdTorch_Recipe", "106_IceVarnish_Recipe" } },
            { "6600020"/*_Hide"*/, new List<string>(){ "93_MakeshiftLeatherHat_Recipe", "94_MakeshiftLeatherAttire_Recipe", "95_MakeshiftLeatherBoots_Recipe" } },
            { "6600030"/*_ScaledHide"*/, new List<string>(){ "96_ScaledLeatherHat_Recipe", "97_ScaledLeatherAttire_Recipe", "98_ScaledLeatherBoots_Recipe", "180_ScaledSatchel_Recipe" } },
            { "6600050"/*_PredatorBones"*/, new List<string>(){ "22_FangAxe_Recipe", "23_FangSword_Recipe", "24_FangClub_Recipe", "25_FangTrident_Recipe", "26_FangGreataxe_Recipe", "27_FangGreatsword_Recipe", "28_FangGreatclub_Recipe", "29_FangHalberd_Recipe", "30_FangShield_Recipe", "152_BouillonduPredateur_Recipe" } },
            { "6600060"/*_CoralHornAntlers"*/, new List<string>(){ "120_CoralhornBow_Recipe" } },
            { "6600070"/*_ThickOil"*/, new List<string>(){ "101_FireRag_Recipe", "114_WarmPotion_Recipe", "76_Firestone_Recipe", "173_TrapCharge-Incendiary_Recipe" } },
            { "6600080"/*_InsectHusk"*/, new List<string>(){ "189_ChitinDesertTunic_Recipe" } },
            { "6600090"/*_MantisGranite"*/, new List<string>(){ "83_MantisGreatpick_Recipe" } },
            { "6600110"/*_OccultRemains"*/, new List<string>(){ "163_BonePistol_Recipe", "118_PossessedPotion_Recipe" } },
            { "6600120"/*_HorrorChitin"*/, new List<string>(){ "77_HorrorBow_Recipe", "78_HorrorAxe_Recipe", "79_HorrorShield_Recipe" } },
            { "6600130"/*_BeastGolemScraps"*/, new List<string>(){ "88_BeastGolemAxe_Recipe", "89_BeastGolemHalberd_Recipe" } },
            { "6600140"/*_ThornfishCartilage"*/, new List<string>(){ "86_ThornfishSpear_Recipe", "87_ThornfishClaymore_Recipe" } },
            { "6600150"/*_ManticoreTail"*/, new List<string>(){ "161_ManticoreDagger_Recipe", "80_ManticoreGreatmace_Recipe" } },
            { "6600160"/*_PhytosaurHornCrafting"*/, new List<string>(){ "92_PhytosaurSpear_Recipe" } },
            { "6600170"/*_SharkCartilage"*/, new List<string>(){ "84_CrescentGreataxe_Recipe", "85_CrescentScythe_Recipe" } },
            { "6600180"/*_AssassinTongue"*/, new List<string>(){ "81_AssassinSword_Recipe", "82_AssassinClaymore_Recipe" } },
            { "6600190"/*_AlphaTuanosaurTail"*/, new List<string>(){ "90_TuanosaurAxe_Recipe", "91_TuanosaurGreataxe_Recipe" } },
            { "6600200"/*_ObsidianShard"*/, new List<string>(){ "157_ObsidianMace_Recipe", "158_ObsidianGreatmace_Recipe", "159_ObsidianPistol_Recipe" } },
            { "6600210"/*_GoldLichMechanismCrafting"*/, new List<string>(){ "166_GoldLichClaymore_Recipe", "167_GoldLichSpear_Recipe", "168_GoldLichSword_Recipe", "169_GoldLichMace_Recipe", "170_GoldLichShield_Recipe" } },

        };


        public SelfEducated()
        {
            this.ModID = m_modName;
            this.Version = "1.0.0";
            //this.loadPriority = 0;
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
            try
            {
                //m_data = LoadSettings();
            }
            catch (Exception ex)
            {
                OLogger.Error($"[{m_modName}] Init: {ex.Message}");
                Debug.Log($"[{m_modName}] Init: {ex.Message}");
            }
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

            On.CharacterInventory.TakeItem_3 -= CharacterInventory_TakeItem_3;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            On.CharacterInventory.TakeItem_3 += CharacterInventory_TakeItem_3;
            //On.CharacterInventory.TakeItem += CharacterInventory_TakeItem;
            //On.CharacterInventory.TakeItem_1 += CharacterInventory_TakeItem_1;
            //On.CharacterInventory.TakeItem_2 += CharacterInventory_TakeItem_2;
            //On.SaveInstance.ApplyEnvironment += SaveInstance_ApplyEnvironment;
        }

        private void CharacterInventory_TakeItem_2(On.CharacterInventory.orig_TakeItem_2 orig, CharacterInventory self, string[] _itemUIDs, bool _tryToEquip)
        {
            OLogger.Log($"TakeItem_2");
            orig(self, _itemUIDs, _tryToEquip);
        }

        private void CharacterInventory_TakeItem_1(On.CharacterInventory.orig_TakeItem_1 orig, CharacterInventory self, string _itemUID, bool _tryToEquip)
        {
            OLogger.Log($"TakeItem_1");
            orig(self, _itemUID, _tryToEquip);
        }

        private void CharacterInventory_TakeItem(On.CharacterInventory.orig_TakeItem orig, CharacterInventory self, string _itemUID)
        {
            OLogger.Log($"TakeItem");
            orig(self, _itemUID);
        }

        private bool SaveInstance_ApplyEnvironment(On.SaveInstance.orig_ApplyEnvironment orig, SaveInstance self, string _areaName)
        {
            try
            {
                Dictionary<string, Recipe> allRecipes = (Dictionary<string, Recipe>)AccessTools.Field(typeof(RecipeManager), "m_recipes").GetValue(RecipeManager.Instance);
                List<Recipe> lstRecipes = allRecipes.Where(r =>
                    r.Value.Ingredients != null
                    && r.Value.Results.Length > 0
                    ).Select(r => r.Value).ToList();
                List<string> lines = new List<string>();
                lines.Add("Type;Recette;Ing 1;;Value 1;Ing 2;;Value 2;Ing 3;;Value 3;Ing 4;;Value 4");
                string line;
                foreach (var recipe in lstRecipes)
                {
                    line = $"{recipe.CraftingStationType};{recipe.hideFlags};{recipe.name}";
                    //recipe.RecipeSteps[0].
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        //line += $";{ingredient.ActionType}";
                        if (ingredient.AddedIngredient)
                        {
                            line += $";{ingredient.AddedIngredient.name};{ingredient.AddedIngredient.Name};{ingredient.AddedIngredient.Value}";
                        }
                        else
                        {
                            line += ";;;";
                        }
                    }
                    lines.Add(line);
                }
                File.WriteAllLines(@"D:\SteamLibrary\steamapps\common\Outward\recipes.csv", lines.ToArray());
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
            return orig(self, _areaName);
        }

        private void CharacterInventory_TakeItem_3(On.CharacterInventory.orig_TakeItem_3 orig, CharacterInventory self, Item takenItem, bool _tryToEquip)
        {
            //OLogger.Log($"forbiden:{(takenItem.ParentContainer.IsManagedByLocalPlayer)}");
            orig(self, takenItem, _tryToEquip);
            try
            {
                // Gatherable, ItemContainerStatic (only not player), MerchantPouch, ItemContainer
                if (takenItem.ParentContainer is MerchantPouch
                    || (takenItem.ParentContainer is ItemContainerStatic /*&& (takenItem.ParentContainer as ItemContainerStatic).IsChildToPlayer*/)
                    || (takenItem.OwnerCharacter != null && !takenItem.OwnerCharacter.IsDead))
                {
                    //OLogger.Log($"forbiden");
                    return; // Only loot from dead enemies or when crafting
                }
                //OLogger.Log($"ParentContainer={takenItem.ParentContainer.GetType()}");
                if ((!takenItem.IsFood && !takenItem.IsIngredient)
                           || takenItem.IsDrink
                           || takenItem.IsEquippable || takenItem.IsDeployable)
                //(takenItem.IsFood || takenItem.IsIngredient) && !takenItem.IsDrink && !takenItem.IsEquippable && !takenItem.IsDeployable                
                {
                    return;
                }

                //string iName = takenItem.name.Substring(0, takenItem.name.LastIndexOf('_'));
                string iName = takenItem.name.Split(new char[] { '_' })[0];
                //OLogger.Log($"TakeItem={iName}");

                if (!m_recipes.ContainsKey(iName))
                {
                    return;
                }

                // Only ingredients not so common (like salt)
                // Alternative : add skills to unlock some recipes

                // Only items that have no RecipeItem ?
                Dictionary<string, Recipe> allRecipes = (Dictionary<string, Recipe>)AccessTools.Field(typeof(RecipeManager), "m_recipes").GetValue(RecipeManager.Instance);
                List<Recipe> lstRecipes = allRecipes.Where(r =>
                    m_recipes[iName].Contains(r.Value.name) && !self.RecipeKnowledge.IsRecipeLearned(r.Key) // Ignore recipes already learned
                    ).Select(r => r.Value).ToList();
                foreach (var recipe in lstRecipes)
                {
                    // Calculate chance of learning recipes based on item's value
                    int itemValue = recipe.Results[0].Item.Value;
                    //int recipeValue = 
                    double chance = 100f;
                    /*switch (recipe.CraftingStationType)
                    {
                        case Recipe.CraftingType.Alchemy: // Value between 0 and 60
                            chance = m_data.PercentMax - ((float)(m_data.PercentMax - m_data.PercentMin) / m_data.AlchemyValueMax) * itemValue;
                            break;
                        case Recipe.CraftingType.Cooking: // Value between 0 and 30
                            chance = m_data.PercentMax - ((float)(m_data.PercentMax - m_data.PercentMin) / m_data.CookingValueMax) * itemValue;
                            break;
                        case Recipe.CraftingType.Survival:  // Value between 0 and 100
                            chance = m_data.PercentMax - ((float)(m_data.PercentMax - m_data.PercentMin) / m_data.SurvivalValueMax) * itemValue;
                            break;
                    }*/
                    //OLogger.Log($"{recipe.Name} ({Math.Round(chance, 0)} %)");
                    int r = UnityEngine.Random.Range(0, 100);
                    if (r < chance)
                    {
                        //OLogger.Log($"{recipe.Name} ({r} < {Math.Round(chance, 2)} %)");
                        self.RecipeKnowledge.LearnRecipe(recipe);
                        break; // only one recipe to add
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] CharacterInventory_TakeItem_3: {ex.Message}");
            }
        }

        private GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader($"mods/{m_modName}Config.json"))
                {
                    try
                    {
                        return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                    }
                    catch (ArgumentNullException)
                    {
                    }
                    catch (FormatException ex)
                    {
                        OLogger.Error("Format Exception", m_modName);
                        Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                OLogger.Error("File Not Found Exception", m_modName);
                Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
            }
            catch (IOException ex)
            {
                OLogger.Error("General IO Exception", m_modName);
                Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
            }
            return null;
        }
    }
}
