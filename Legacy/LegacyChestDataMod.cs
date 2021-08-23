using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wip
{
    public class LegacyChestDataMod : ISavable
    {
        public UID ChestUID;

        public string AreaSceneName;

        public int StoredItemID;

        public int StoredItemCount;

        private string m_itemSaveData;

        private string m_cachedItemSyncData;

        public object SaveIdentifier => ChestUID.Value;

        public string ItemSaveData
        {
            get
            {
                return m_itemSaveData;
            }
            private set
            {
                m_itemSaveData = value;
                m_cachedItemSyncData = null;
            }
        }

        public string ChestName
        {
            get
            {
                Area areaFromSceneName = AreaManager.Instance.GetAreaFromSceneName(AreaSceneName);
                return (areaFromSceneName == null) ? "N/A" : areaFromSceneName.GetName();
            }
        }

        public LegacyChestDataMod()
        {
        }

        public LegacyChestDataMod(Item _item)
        {
            ChestUID = _item.UID;
            AreaSceneName = SceneManagerHelper.ActiveSceneName;
        }

        public LegacyChestDataMod(BasicSaveData _data)
        {
            string[] array = _data.SyncData.Split('~');
            ChestUID = _data.Identifier.ToString();
            AreaSceneName = array[0];
            int.TryParse(array[1], out StoredItemID);
            if (array.Length > 3)
            {
                int.TryParse(array[2], out StoredItemCount);
                ItemSaveData = array[3];
            }
            else
            {
                StoredItemCount = 1;
                ItemSaveData = array[2];
            }
        }

        public void SetContainedData(Item _containedItem, int _count)
        {
            StoredItemID = _containedItem.ItemID;
            StoredItemCount = _count;
            m_itemSaveData = _containedItem.ToSaveData();
        }

        public string ToSaveData()
        {
            return AreaSceneName + "~" + StoredItemID + "~" + StoredItemCount + "~" + ItemSaveData;
        }

        public string GetItemSyncData()
        {
            if (string.IsNullOrEmpty(m_cachedItemSyncData))
            {
                m_cachedItemSyncData = Item.GetSyncDataFromSaveData(ItemSaveData);
            }
            return m_cachedItemSyncData;
        }
    }
}
