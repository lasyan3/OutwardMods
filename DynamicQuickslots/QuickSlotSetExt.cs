using EraserElixir.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EraserElixir
{
    /// <summary>
    /// This extension will store the list of favorite quickslots for an item
    /// </summary>
    public class QuickSlotSetExt : ItemExtension
    {
        public List<CustomQuickSlot> Slots;

        public override string ToNetworkInfo()
        {
            string dataOut = string.Join("/", Slots);
            //DynamicQuickslots.Instance.MyLogger.LogDebug($"ToNetworkInfo:{dataOut}");
            return dataOut;
        }

        internal void Awake()
        {
            Savable = true;
            Slots = new List<CustomQuickSlot>();
            AwakeInit();
        }

        public override void OnReceiveNetworkSync(string[] _networkInfo)
        {
            if (_networkInfo.Length > 0)
            {
                //DynamicQuickslots.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync={_networkInfo[0]}");
                Slots = new List<CustomQuickSlot>();
                string[] slots = _networkInfo[0].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var slot in slots)
                {
                    string[] slotData = slot.Split(new char[] { ',' });
                    Slots.Add(new CustomQuickSlot
                    {
                        Index = int.Parse(slotData[0]),
                        ItemID = int.Parse(slotData[1])
                    });
                }
            }
        }
    }
}
