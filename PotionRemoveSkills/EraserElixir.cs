using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SideLoader;
using System;

namespace EraserElixir
{
    [BepInPlugin(ID, NAME, VERSION)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class PotionResetSkills : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.eraserelixir";
        const string NAME = "EraserElixir";
        const string VERSION = "1.0.1";

        public static PotionResetSkills Instance;
        public ManualLogSource MyLogger { get { return Logger; } }

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                //SL.BeforePacksLoaded += SL_BeforePacksLoaded;
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError("Awake: " + ex.Message);
            }
        }

        private void SL_BeforePacksLoaded()
        {
            try
            {
                //var template = new SL_Item
                //{
                //    Target_ItemID = 4300190,
                //    New_ItemID = 4300191,
                //    Name = "Eraser Elixir",
                //    Description = "Strange brew that makes you forget your most important skills (no refund!).\r\n\r\nAre you sure this is a good idea?",
                //    SLPackName = "EraserElixir",
                //    SubfolderName = "4300191",
                //};
                //CustomItems.CreateCustomItem(template);

                //var rcp191 = new SL_Recipe
                //{
                //    StationType = Recipe.CraftingType.Alchemy,
                //    Ingredients = new List<SL_Recipe.Ingredient>
                //    {
                //        /* Chersonese : blue sand
                //            Forge Golem : Crystal Powder (6600040)
                //            Mantis : Mantis granite
                //            Shell Horror : Horror Chitine
                //         Hallowed Marsh : Firefly plant
                //            Gold-Lich Mechanism (6600210)
                //            Phytosaur Horn
                //            Alpha Tuanosaur Tail (6600190)
                //         Enmerkar Forest : smoke root
                //            Obsidian Elemental : Obsidian Shard
                //         Abrassar : cactus fruit
                //            Manticore Tail
                //            Raw Jewel Meat (4000260)
                //            Shark Cartilage (6600170)

                //            Chersonese: Blue Sand () / Crystal Powder (6600040)
                //            Hallowed Marsh: Phytosaur Horn (6600160) / Alpha Tuanosaur Tail (6600190)
                //            Enmerkar Forest: Obsidian Shard (6600200)
                //            Abrassar: Giant-Heart Garnet (7400050) / Shark Cartilage (6600170)
                //        */
                //        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 6600040 }, // Crystal Powder
                //        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 6600160 }, // Phytosaur Horn
                //        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 6600200 }, // Obsidian Shard
                //        new SL_Recipe.Ingredient{Type = RecipeIngredient.ActionTypes.AddSpecificIngredient, Ingredient_ItemID = 6600170 }, // Shark Cartilage
                //    },
                //    Results = new List<SL_Recipe.ItemQty>
                //    {
                //        new SL_Recipe.ItemQty{ ItemID = 4300191, Quantity = 1 }
                //    }
                //};
                //rcp191.ApplyRecipe();
                //var ri191 = new SL_RecipeItem
                //{
                //    Target_ItemID = 4300192,
                //    RecipeUID = rcp191.UID
                //};
                ////CustomItems.CreateCustomItem(rcp191);
                //ri191.Apply();
            }
            catch (Exception ex)
            {
                MyLogger.LogError("SL_OnPacksLoaded: " + ex.Message);
            }
        }
    }
}
