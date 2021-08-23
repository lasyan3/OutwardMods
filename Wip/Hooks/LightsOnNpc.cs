using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorkInProgress.Hooks
{
    //private void ItemLanternVisual_Light(On.ItemLanternVisual.orig_Light orig, ItemLanternVisual self, bool _light, bool _force)
    //{
    //    orig(self, _light, _force);
    //    OLogger.Log($"colorTemperature={self.LanternLight.colorTemperature}");
    //    OLogger.Log($"type={self.LanternLight.type}");
    //    OLogger.Log($"intensity={self.LanternLight.intensity}");
    //    //OLogger.Log($"position={self.LanternLight.transform.position}");
    //}

    [HarmonyPatch(typeof(SNPC), "OnEnable")]
    public class LightsOnNpc
    {
        [HarmonyPostfix]
        public static void IngredientSelectorHasChanged(SNPC __instance)
        {
            if (__instance.gameObject != null)
            {
                Light testLi = __instance.gameObject.AddComponent<Light>();
                testLi.color = new Color(1.0f, 0.785f, 0.5f, 1.0f);
                testLi.type = LightType.Point;
                testLi.intensity = 1.1f;
                testLi.colorTemperature = 6570;
                //testLi.transform.position = self.transform.position + new Vector3(0f, 0.3f, 0f);
                //OLogger.Log($"SNPC_OnEnable={self.name}");
            }
        }
    }
}
