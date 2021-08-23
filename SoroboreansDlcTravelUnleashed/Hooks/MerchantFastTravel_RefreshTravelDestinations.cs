using HarmonyLib;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MerchantFastTravel;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(MerchantFastTravel), "RefreshTravelDestinations", new Type[] { typeof(bool) })]
    public class MerchantFastTravel_RefreshTravelDestinations
    {
        [HarmonyPostfix]
        public static void RefreshTravelDestinationsPost(MerchantFastTravel __instance, bool _forceRefresh)
        {
            try
            {
                List<TravelData> tmpCurrentTravelData = (List<TravelData>)AccessTools.Field(typeof(MerchantFastTravel), "tmpCurrentTravelData").GetValue(__instance);
                List<int> m_travelToRegion = (List<int>)AccessTools.Field(typeof(MerchantFastTravel), "m_travelToRegion").GetValue(__instance);
                bool InCity = (bool)AccessTools.Property(typeof(MerchantFastTravel), "InCity").GetValue(__instance);
                AreaManager.AreaEnum m_currentAreaAssociatedCity = (AreaManager.AreaEnum)AccessTools.Field(typeof(MerchantFastTravel), "m_currentAreaAssociatedCity").GetValue(__instance);
                MethodInfo InCierzoArea = AccessTools.Method(typeof(MerchantFastTravel), "InCierzoArea");
                MethodInfo InBergArea = AccessTools.Method(typeof(MerchantFastTravel), "InBergArea");
                MethodInfo InLevantArea = AccessTools.Method(typeof(MerchantFastTravel), "InLevantArea");
                MethodInfo InMonsoonArea = AccessTools.Method(typeof(MerchantFastTravel), "InMonsoonArea");
                MethodInfo InHarmattanArea = AccessTools.Method(typeof(MerchantFastTravel), "InHarmattanArea");
                MethodInfo CheckIfLevantBlocked = AccessTools.Method(typeof(MerchantFastTravel), "CheckIfLevantBlocked");
                MethodInfo InNewSiroccoArea = AccessTools.Method(typeof(MerchantFastTravel), "InNewSiroccoArea");

                tmpCurrentTravelData.Clear();
                if (PhotonNetwork.isNonMasterClientInRoom && !_forceRefresh)
                {
                    return;
                }
                Global.RPCManager.RequestMerchantFastTravelDestinations();
                m_travelToRegion.Clear();
                Area currentArea = AreaManager.Instance.CurrentArea;
                InCity = AreaManager.Instance.GetIsAreaTownOrCity(currentArea);
                if ((bool)InCierzoArea.Invoke(__instance, new object[] { currentArea }))
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.CierzoVillage;
                }
                else if ((bool)InBergArea.Invoke(__instance, new object[] { currentArea }))
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.Berg;
                }
                else if ((bool)InLevantArea.Invoke(__instance, new object[] { currentArea }))
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.Levant;
                }
                else if ((bool)InMonsoonArea.Invoke(__instance, new object[] { currentArea }))
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.Monsoon;
                }
                else if ((bool)InHarmattanArea.Invoke(__instance, new object[] { currentArea }))
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.Harmattan;
                }
                else if ((bool)InNewSiroccoArea.Invoke(__instance, new object[] { currentArea }))
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.NewSirocco;
                }
                else
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.Tutorial;
                }
                CanTravelToHarmattan = (StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.Soroboreans) && !(bool)InHarmattanArea.Invoke(__instance, new object[] { currentArea }));
                if (!InCity || (bool)InHarmattanArea.Invoke(__instance, new object[] { currentArea }))
                {
                    if (!QuestEventManager.Instance.HasQuestEvent("lDHL_XMS7kKEs0uOqrLQjw")
                        && !(bool)InCierzoArea.Invoke(__instance, new object[] { currentArea }))
                    {
                        m_travelToRegion.Add(100);
                    }
                    if (!QuestEventManager.Instance.HasQuestEvent("vW4sarzBGkalTwy_KhGI6A")
                        && !(bool)InBergArea.Invoke(__instance, new object[] { currentArea }))
                    {
                        m_travelToRegion.Add(500);
                    }
                    if ((bool)CheckIfLevantBlocked.Invoke(__instance, null)
                        && !(bool)InLevantArea.Invoke(__instance, new object[] { currentArea }))
                    {
                        m_travelToRegion.Add(300);
                    }
                    if (!(bool)InMonsoonArea.Invoke(__instance, new object[] { currentArea }))
                    {
                        m_travelToRegion.Add(200);
                    }
                    if (!(bool)InHarmattanArea.Invoke(__instance, new object[] { currentArea }))
                    {
                        m_travelToRegion.Add(400);
                    }
                    if (StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2) && !(bool)InNewSiroccoArea.Invoke(__instance, new object[] { currentArea }) 
                        && QuestEventManager.Instance.HasQuestEvent("eYmZGb_BJ0qAtpcwrndhTg"))
                    {
                        m_travelToRegion.Add(601);
                    }
                }
                else if (CanTravelToHarmattan)
                {
                    m_travelToRegion.Add(400);
                }

                foreach (int regionId in m_travelToRegion)
                {
                    TravelData merchantTravelData = AreaManager.Instance.GetMerchantTravelData(m_currentAreaAssociatedCity, (AreaManager.AreaEnum)regionId);
                    if (merchantTravelData != null)
                    {
                        tmpCurrentTravelData.Add(merchantTravelData);
                    }
                }
                SyncTravelDestinations();
            }
            catch (Exception ex)
            {
                SoroboreansDlcTravelUnleashed.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }
}
