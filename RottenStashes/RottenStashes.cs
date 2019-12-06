//using Harmony;
//using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RottenStashes
{
    public class RottenStashes : PartialityMod
    {
        private readonly string _modName = "RottenStashes";
        double m_lastGameTime = 0f;

        public RottenStashes()
        {
            this.ModID = _modName;
            this.Version = "1.0.0";
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

            On.EnvironmentSave.ApplyData -= EnvironmentSave_ApplyData;
            On.Item.OnReceiveNetworkSync -= Item_OnReceiveNetworkSync;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            On.EnvironmentSave.ApplyData += EnvironmentSave_ApplyData;
            On.Item.OnReceiveNetworkSync += Item_OnReceiveNetworkSync;
        }

        private void Item_OnReceiveNetworkSync(On.Item.orig_OnReceiveNetworkSync orig, Item self, string[] _infos)
        {
            if (m_lastGameTime > 0 && _infos.Length > 13)
            {
                // Add the GameTime value from the last time we were in this area (got in EnvironmentSave_ApplyData)
                // This information will be used to update the durability of the item
                _infos[13] = "Perishable;" + m_lastGameTime.ToString("0.###"); ;
            }
            orig(self, _infos);
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
        private void EnvironmentSave_ApplyData(On.EnvironmentSave.orig_ApplyData orig, EnvironmentSave self)
        {
            m_lastGameTime = self.GameTime > 0 ? self.GameTime : EnvironmentConditions.GameTime;
            orig.Invoke(self);
        }

    }
}
