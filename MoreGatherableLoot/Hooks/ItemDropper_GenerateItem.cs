using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;


namespace MoreGatherableLoot
{
    [HarmonyPatch(typeof(ItemDropper), "GenerateItem")]
    public class ItemDropper_GenerateItem
    {
        [HarmonyPrefix]
        public static bool GenerateItem(ItemContainer _container, BasicItemDrop _itemDrop, ref int _spawnAmount)
        {
            try
            {
                _itemDrop.DroppedItem.InitCachedInfos();
                //MoreGatherableLoot.MyLogger.LogDebug($"{_itemDrop.DroppedItem.DisplayName}");
                if (_container.GetType() == typeof(Gatherable)
                    && (_itemDrop.DroppedItem.IsFood || _itemDrop.DroppedItem.IsIngredient)
                    && !_itemDrop.DroppedItem.IsDrink
                    && !_itemDrop.DroppedItem.IsEquippable
                    && !_itemDrop.DroppedItem.IsDeployable
                    && _itemDrop.MaxDropCount < MoreGatherableLoot.Settings.AmountMax)
                {
                    int minDrop = _itemDrop.MinDropCount * (MoreGatherableLoot.Settings.AlwaysMax ? MoreGatherableLoot.Settings.AmountMax : MoreGatherableLoot.Settings.AmountMin);
                    int maxDrop = _itemDrop.MaxDropCount * MoreGatherableLoot.Settings.AmountMax;
                    if (_itemDrop.MaxDropCount > 1 && _itemDrop.MaxDropCount < MoreGatherableLoot.Settings.AmountMax)
                    { // If already multiple quantities, increase instead of multiply
                        minDrop = MoreGatherableLoot.Settings.AlwaysMax ? MoreGatherableLoot.Settings.AmountMax : MoreGatherableLoot.Settings.AmountMin;
                        maxDrop = MoreGatherableLoot.Settings.AmountMax;
                    }
                    // Increase count of items on specific resources (like champignons !)
                    //MoreGatherableLoot.MyLogger.LogDebug($"{_container.GetType().Name}: {_container.name.Split(new char[] { '_' })[1]}");
                    //MoreGatherableLoot.MyLogger.LogDebug($"{_itemDrop.DroppedItem.name.Split(new char[] { '_' })[1]} ({_itemDrop.MinDropCount} - {_itemDrop.MaxDropCount})");
                    _spawnAmount = UnityEngine.Random.Range(minDrop, maxDrop + 1);
                    //MoreGatherableLoot.MyLogger.LogDebug($"{_itemDrop.DroppedItem.DisplayName}={_spawnAmount} ({minDrop} - {maxDrop})");
                    MoreGatherableLoot.MyLogger.LogDebug($"\t|- New amount={_spawnAmount} ({minDrop} - {maxDrop})");
                }
            }
            catch (Exception ex)
            {
                MoreGatherableLoot.MyLogger.LogError(ex.Message);
            }
            return true;
        }
    }
}
