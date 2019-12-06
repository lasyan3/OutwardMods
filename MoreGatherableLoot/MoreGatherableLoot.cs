//using ODebug;
using Partiality.Modloader;
using System;
using System.IO;
using UnityEngine;

namespace MoreGatherableLoot
{
    class GameData
    {
        public int AmountMin;
        public int AmountMax;
        public bool AlwaysMax;
    }

    public class MoreGatherableLoot : PartialityMod
    {
        private GameData data;
        private readonly string m_modName = "MoreGatherableLoot";

        public MoreGatherableLoot()
        {
            this.ModID = m_modName;
            this.Version = "1.0.2";
            //this.loadPriority = 0;
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
            data = LoadSettings();
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

            On.ItemDropper.GenerateItem -= ItemDropper_GenerateItem;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            On.ItemDropper.GenerateItem += ItemDropper_GenerateItem;
        }

        private void ItemDropper_GenerateItem(On.ItemDropper.orig_GenerateItem orig, ItemDropper self, ItemContainer _container, BasicItemDrop _itemDrop, int _spawnAmount)
        {
            try
            {
                _itemDrop.DroppedItem.InitCachedInfos();
                if (_container.GetType() == typeof(Gatherable)
                    && (_itemDrop.DroppedItem.IsFood || _itemDrop.DroppedItem.IsIngredient)
                    && !_itemDrop.DroppedItem.IsDrink
                    && !_itemDrop.DroppedItem.IsEquippable
                    && !_itemDrop.DroppedItem.IsDeployable
                    && _itemDrop.MaxDropCount < data.AmountMax)
                {
                    int minDrop = _itemDrop.MinDropCount * (data.AlwaysMax ? data.AmountMax : data.AmountMin);
                    int maxDrop = _itemDrop.MaxDropCount * data.AmountMax;
                    if (_itemDrop.MaxDropCount > 1 && _itemDrop.MaxDropCount < data.AmountMax)
                    { // If already multiple quantities, increase instead of multiply
                        minDrop = data.AlwaysMax ? data.AmountMax : data.AmountMin;
                        maxDrop = data.AmountMax;
                    }
                    // Increase count of items on specific resources (like champignons !)
                    //OLogger.Log($"{_container.GetType().Name}: {_container.name.Split(new char[] { '_' })[1]}");
                    //OLogger.Log($"{_itemDrop.DroppedItem.name.Split(new char[] { '_' })[1]} ({_itemDrop.MinDropCount} - {_itemDrop.MaxDropCount})");
                    _spawnAmount = UnityEngine.Random.Range(minDrop, maxDrop + 1);
                    //OLogger.Log($"{_itemDrop.DroppedItem.DisplayName}={_spawnAmount} ({minDrop} - {maxDrop})", "ffffffff", _modName);
                }
            }
            catch (Exception ex)
            {
                //OLogger.Error("ItemDropper_GenerateItem: " + ex.Message, _modName);
                Debug.Log($"[{m_modName}] ItemDropper_GenerateItem: {ex.Message}");
            }
            finally
            {
                orig.Invoke(self, _container, _itemDrop, _spawnAmount);
            }
        }

        private GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader($"mods/{m_modName}Config.json"))
                {
                    return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
            }
            return null;
        }
    }
}
