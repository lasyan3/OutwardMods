using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(SaveInstance), "ApplyEnvironment")]
    public class SaveInstance_ApplyEnvironment
    {
        [HarmonyPostfix]
        public static void ApplyEnvironment()
        {
            try
            {
                //m_currentArea = _areaName;
                //InnRentStash.MyLogger.LogDebug($"ApplyEnvironment");

                #region TODO : Thieves in town!
                // Parcourir tous les objets appartenant au joueur dans la ville
                /*for (int i = 0; i < ItemManager.Instance.WorldItems.Count; i++)
                {
                    string uid = ItemManager.Instance.WorldItems.Keys[i];
                    Item it = ItemManager.Instance.WorldItems.Values[i];
                    if (it.OwnerCharacter != null && it.OwnerCharacter.IsLocalPlayer && !it.IsEquipped &&
                        /*it.ParentContainer != it.OwnerCharacter.Inventory.EquippedBag &&* it.IsInWorld
                        /*it.ParentContainer != it.OwnerCharacter.Inventory.Pouch*)
                    {
                        DoLog($"Delete {it.Name}");
                        ItemManager.Instance.DestroyItem(uid);
                    }
                }*/
                #endregion
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError("ApplyEnvironment: " + ex.Message);
            }
        }
    }
}
