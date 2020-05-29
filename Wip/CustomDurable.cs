using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkInProgress
{
    public class CustomDurable : ItemExtension
    {
        public float CurrentDurability = -1f;
        public float MaxDurability = -1f;
        public new bool Savable = true;

        public CustomDurable()
        {
            //Savable = true;
        }

        public override string ToNetworkInfo()
        {
            return CurrentDurability.ToString() + ";" + MaxDurability.ToString();
        }

        public override void OnReceiveNetworkSync(string[] _networkInfo)
        {
            if (float.TryParse(_networkInfo[1], out float f))
            {
                CurrentDurability = f;
            }
            if (float.TryParse(_networkInfo[0], out float f2))
            {
                MaxDurability = f2;
            }
        }
    }
}
