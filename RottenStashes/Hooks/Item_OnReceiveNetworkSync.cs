using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AreaManager;

namespace RottenStashes.Hooks
{
    [HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    public class Item_OnReceiveNetworkSync
    {
        public static readonly List<string> StashUID = new List<string>
        {
            "ImqRiGAT80aE2WtUHfdcMw",
            "ZbPXNsPvlUeQVJRks3zBzg",
        };

        [HarmonyPrefix]
        public static bool OnReceiveNetworkSync(Item __instance, ref string[] _infos)
        {
            try
            {
                //if (_infos[1] != "4000020") return true;
                //if (_infos[4][0] != '1') return true; // Only items inside a container

                //string containerUID = _infos[4].Substring(1).Split(';')[0];
                if (RottenStashes.LastGameTime > 0
                    && _infos.Length > 13
                    && (_infos[4].Length == 0 || _infos[4][0] == '1' && StashUID.Contains(_infos[4].Substring(1).Split(';')[0]))
                    && _infos[13].Contains("Perishable;")
                    )
                {
                    // Add the GameTime value from the last time we were in this area (got in EnvironmentSave_ApplyData)
                    // This information will be used to update the durability of the item
                    Item it = ResourcesPrefabManager.Instance.GetItemPrefab(_infos[1]);
                    var extensions = _infos[13].Split(':');
                    for (int i = 0; i < extensions.Length; i++)
                    {
                        if (extensions[i].StartsWith("Perishable;"))
                        {
                            extensions[i] = "Perishable;" + RottenStashes.LastGameTime.ToString("0.###");
                            //RottenStashes.MyLogger.LogDebug($"OnReceiveNetworkSync: {it.Name}={extensions[i]}");
                            break;
                        }
                    }
                    _infos[13] = string.Join(":", extensions);
                    //RottenStashes.MyLogger.LogDebug($"OnReceiveNetworkSync: {it.Name}={_infos[13]}");
                }
                return true;
                /*
                    UID: wqp0aWMHkki-CnfzBV2aVw,
                    ItemId: 4000050,
                    ,
                    ,
                    _hierarchyInfos: 0Null,
                    m_forcePos: ,
                    m_targetPos: (75.3, -5.1, 192.1),
                    m_targetRot: (359.6, 204.2, 359.4),
                    m_stuck: False,
                    ,
                    m_currentDurability: 74.85,
                    -1,
                    ,
                    Perishable;,
                    RebuyLimitTime: ,
                    ResellLimitTime: ,
                    PreviousOwnerUID: acFasKZz60S-XXvejdEWGQ,
                    m_aquireTime: ,
                    m_isNewInInventory: 0,
                    m_litStatus:,
                */
            }
            catch (Exception ex)
            {
                RottenStashes.MyLogger.LogError("OnReceiveNetworkSync: " + ex.Message);
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(Perishable), "OnReceiveNetworkSync")]
    //public class Perishable_OnReceiveNetworkSync
    //{
    //    [HarmonyPrefix]
    //    public static bool OnReceiveNetworkSync(Perishable __instance, ref string[] _networkInfo)
    //    {
    //        try
    //        {
    //            RottenStashes.MyLogger.LogDebug($"Perishable={__instance.Item.Name}");
    //            for (int i = 0; i < _networkInfo.Length; i++)
    //            {
    //                RottenStashes.MyLogger.LogDebug($" > {i}:{_networkInfo[i]}");
    //            }
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            RottenStashes.MyLogger.LogError("OnReceiveNetworkSync: " + ex.Message);
    //        }
    //        return true;
    //    }
    //}
}
