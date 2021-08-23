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
    [HarmonyPatch(typeof(MerchantFastTravel), "RefreshTravelDestinations")]
    public class MerchantFastTravel_RefreshTravelDestinations
    {
        [HarmonyPostfix]
        public static void RefreshTravelDestinationsPost(MerchantFastTravel __instance)
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

                tmpCurrentTravelData.Clear();
                if (PhotonNetwork.isNonMasterClientInRoom)
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
                else
                {
                    m_currentAreaAssociatedCity = AreaManager.AreaEnum.Tutorial;
                }
                CanTravelToHarmattan = (StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.Soroboreans) && !(bool)InHarmattanArea.Invoke(__instance, new object[] { currentArea }));
                if (!QuestEventManager.Instance.HasQuestEvent("lDHL_XMS7kKEs0uOqrLQjw") 
                    && !(bool)InCierzoArea.Invoke(__instance, new object[] { currentArea })
                    && (bool)SoroboreanTravelAgency.Instance.MyConfig.GetValue("CierzoVillageVisited"))
                {
                    m_travelToRegion.Add(100);
                }
                if (!QuestEventManager.Instance.HasQuestEvent("vW4sarzBGkalTwy_KhGI6A") 
                    && !(bool)InBergArea.Invoke(__instance, new object[] { currentArea })
                    && (bool)SoroboreanTravelAgency.Instance.MyConfig.GetValue("BergVisited"))
                {
                    m_travelToRegion.Add(500);
                }
                if ((bool)CheckIfLevantBlocked.Invoke(__instance, null) 
                    && !(bool)InLevantArea.Invoke(__instance, new object[] { currentArea })
                    && (bool)SoroboreanTravelAgency.Instance.MyConfig.GetValue("LevantVisited"))
                {
                    m_travelToRegion.Add(300);
                }
                if (!(bool)InMonsoonArea.Invoke(__instance, new object[] { currentArea })
                    && (bool)SoroboreanTravelAgency.Instance.MyConfig.GetValue("MonsoonVisited"))
                {
                    m_travelToRegion.Add(200);
                }
                if (CanTravelToHarmattan)
                {
                    m_travelToRegion.Add(400);
                }
                // lasyan3 : unlock all available regions
                tmpCurrentTravelData.Clear();
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
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }
}
