using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMerchantStock
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
                if (_container.GetType() == typeof(MerchantPouch)
                               && (_itemDrop.DroppedItem.IsFood || _itemDrop.DroppedItem.IsIngredient)
                               && !_itemDrop.DroppedItem.IsDrink
                               && !_itemDrop.DroppedItem.IsEquippable && !_itemDrop.DroppedItem.IsDeployable)
                {
                    int minDrop = _itemDrop.MinDropCount * (MoreMerchantStock.Settings.AffectMinimum ? MoreMerchantStock.Settings.Amount : 1);
                    int maxDrop = _itemDrop.MaxDropCount * MoreMerchantStock.Settings.Amount;
                    _spawnAmount = UnityEngine.Random.Range(minDrop, maxDrop + 1);
                    MoreMerchantStock.MyLogger.LogDebug($"{_itemDrop.DroppedItem.DisplayName}={_spawnAmount} ({minDrop} - {maxDrop})");
                }
            }
            catch (Exception ex)
            {
                MoreMerchantStock.MyLogger.LogError(ex.Message);
            }
            return true;
        }
    }
}
