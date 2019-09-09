using Partiality.Modloader;
using System;
using System.IO;
using UnityEngine;

namespace MoreGatherableLoot
{
    class GameData
    {
        public int Amount;
        public bool AlwaysMax;
    }

    public class MoreGatherableLoot : PartialityMod
    {
        private GameData data;
        private readonly string _modName = "MoreGatherableLoot";

        public MoreGatherableLoot()
        {
            this.ModID = _modName;
            this.Version = "1.0.0";
            //this.loadPriority = 0;
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
            try
            {
                data = LoadSettings();
            }
            catch (Exception ex)
            {
                Debug.Log($"[{_modName}] Init: {ex.Message}");
            }
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
                //_itemDrop.DroppedItem.InitCachedInfos();
                if (_container.GetType() == typeof(Gatherable) && _itemDrop.MaxDropCount < data.Amount)
                {
                    int minDrop = _itemDrop.MinDropCount * (data.AlwaysMax ? data.Amount : 1);
                    int maxDrop = _itemDrop.MaxDropCount * data.Amount;
                    if (_itemDrop.MaxDropCount > 1 && _itemDrop.MaxDropCount < data.Amount)
                    { // If already multiple quantities, increase instead of multiply
                        minDrop = data.AlwaysMax ? data.Amount : _itemDrop.MinDropCount;
                        maxDrop = data.Amount;
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
                Debug.Log($"[{_modName}] ItemDropper_GenerateItem: {ex.Message}");
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
                using (StreamReader streamReader = new StreamReader($"mods/{_modName}Config.json"))
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
                        //OLogger.Error("Format Exception", _modName);
                        Debug.Log($"[{_modName}] LoadSettings: {ex.Message}");
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                //OLogger.Error("File Not Found Exception", _modName);
                Debug.Log($"[{_modName}] LoadSettings: {ex.Message}");
            }
            catch (IOException ex)
            {
                //OLogger.Error("General IO Exception", _modName);
                Debug.Log($"[{_modName}] LoadSettings: {ex.Message}");
            }
            return null;
        }
    }
}
