using Harmony;
//using ODebug;
using Partiality.Modloader;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

class GameData
{
    public int Amount;
    public bool AffectMinimum;
    public bool AlwaysRefreshStock;
}

public class MoreMerchantStock : PartialityMod
{
    private GameData data;
    private readonly string _modName = "MoreMerchantStock";

    // Constructor setting the fields of the PartialityMod class
    public MoreMerchantStock()
    {
        this.ModID = _modName;
        this.Version = "1.0.0";
        //this.loadPriority = 0;
        this.author = "lasyan3";
    }

    // Init() is called the moment the mod is loaded. Use this to set properties or load configs, and the like.
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

    // OnLoad() is called after all mods have loaded. Do most of your first-time code here. Creating objects, getting stuff from other mods, etc.
    public override void OnLoad()
    {
        base.OnLoad();
    }

    // OnDisable() is called when a mod is disabled.
    public override void OnDisable()
    {
        base.OnDisable();

        On.ItemDropper.GenerateItem -= ItemDropper_GenerateItem;
        if (data.AlwaysRefreshStock)
        {
            On.MerchantPouch.RefreshInventory -= ForceRefreshInventory;
        }
    }

    // OnEnable() is called when a mod is enabled (also when it's loaded)
    public override void OnEnable()
    {
        base.OnEnable();

        try
        {
            On.ItemDropper.GenerateItem += ItemDropper_GenerateItem;
            if (data.AlwaysRefreshStock)
            {
                On.MerchantPouch.RefreshInventory += ForceRefreshInventory;
            }

            //OLogger.Log("BiggerMerchantStock is enabled");
        }
        catch (Exception ex)
        {
            //OLogger.Error("OnEnable: " + ex.Message, _modName);
            Debug.Log($"[{_modName}] OnEnable: {ex.Message}");
        }
    }

    private void ItemDropper_GenerateItem(On.ItemDropper.orig_GenerateItem orig, ItemDropper self, ItemContainer _container, BasicItemDrop _itemDrop, int _spawnAmount)
    {
        try
        {
            _itemDrop.DroppedItem.InitCachedInfos();
            if (_container.GetType() == typeof(MerchantPouch)
                           && (_itemDrop.DroppedItem.IsFood || _itemDrop.DroppedItem.IsIngredient)
                           && !_itemDrop.DroppedItem.IsDrink
                           && !_itemDrop.DroppedItem.IsEquippable && !_itemDrop.DroppedItem.IsDeployable)
            {
                int minDrop = _itemDrop.MinDropCount * (data.AffectMinimum ? data.Amount : 1);
                int maxDrop = _itemDrop.MaxDropCount * data.Amount;
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

    private void ForceRefreshInventory(On.MerchantPouch.orig_RefreshInventory orig, MerchantPouch self, Dropable _dropable)
    {
        try
        {
            FieldInfo nextRefreshTime = AccessTools.Field(typeof(MerchantPouch), "m_nextRefreshTime");
            nextRefreshTime.SetValue(self, EnvironmentConditions.GameTime - 1);
        }
        catch (Exception ex)
        {
            //OLogger.Error("ForceRefreshInventory:" + ex.Message, _modName);
            Debug.Log($"[{_modName}] ForceRefreshInventory: {ex.Message}");
        }

        orig.Invoke(self, _dropable);
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
