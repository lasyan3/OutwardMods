using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RottenStashes.Hooks
{
    [HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    public class Item_OnReceiveNetworkSync
    {
        [HarmonyPrefix]
        public static bool OnReceiveNetworkSync(ref string[] _infos)
        {
            if (RottenStashes.LastGameTime > 0 && _infos.Length > 13)
            {
                // Add the GameTime value from the last time we were in this area (got in EnvironmentSave_ApplyData)
                // This information will be used to update the durability of the item
                _infos[13] = "Perishable;" + RottenStashes.LastGameTime.ToString("0.###");
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
    }
}
