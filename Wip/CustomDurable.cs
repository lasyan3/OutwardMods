using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace WorkInProgress
//{
public class CustomDurable : ItemExtension
{
    public float CurrentDurability = -1f;
    public float MaxDurability = -1f;

    public override string ToNetworkInfo()
    {
        //WorkInProgress.Instance.MyLogger.LogDebug($"ToNetworkInfo.C={CurrentDurability.ToString() + ";" + MaxDurability.ToString()}");
        return CurrentDurability.ToString() + ";" + MaxDurability.ToString();
    }

    internal void Awake()
    {
        Savable = true;
        AwakeInit();
    }

    public override void OnReceiveNetworkSync(string[] _networkInfo)
    {
        //WorkInProgress.Instance.MyLogger.LogDebug($"OnReceiveNetworkSync");
        if (float.TryParse(_networkInfo[0], out float f))
        {
            //WorkInProgress.Instance.MyLogger.LogDebug($"Load CustomDurable.C={f}");
            CurrentDurability = f;
        }
        if (float.TryParse(_networkInfo[1], out float f2))
        {
            MaxDurability = f2;
        }
    }
}
//}
