using NodeCanvas.Framework;
//using ODebug;
using ParadoxNotion.Design;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OutwardMods
{
    [Name("Area Switch Players")]
    [Category("Character")]
    public class AreaSwitchPlayersTime : ActionTask
    {
        public BBParameter<AreaManager.AreaEnum> Area;

        public BBParameter<int> SpawnPoint = 0;
        public BBParameter<float> IncreaseTime = 0;

        [SerializeField]
        protected string m_targetUID;

        protected override string info
        {
            get
            {
                string text = string.Empty;
                if (Area != null)
                {
                    text = "Switch Area to " + Area.value.ToString();
                }
                if (SpawnPoint != null)
                {
                    text = text + " at spawn " + SpawnPoint.ToString();
                }
                return text;
            }
        }

        protected override void OnExecute()
        {
            try
            {
                NetworkLevelLoader.Instance.RequestSwitchArea(IncreaseTime.value, AreaManager.Instance.GetArea(Area.value).SceneName, SpawnPoint.value);
            }
            catch (Exception ex)
            {
                Debug.Log($"[SoroboreanTravelAgency] AreaSwitchPlayersTime.OnExecute: {ex.Message}");
            }
            /*CampingEventManager.Instance.PrepareDungeonCampEvent
            string text3 = CampingEventManager.Instance.TryGetCampingEvent();
            OLogger.Log($"OnExecute={text3}");
            if (!string.IsNullOrEmpty(text3))
            {
                CampingEvent campingEvent = CampingEventManager.Instance.Holder.GetCampingEvent(text3);
                if ((bool)campingEvent && (bool)campingEvent.DefeatScenarioOverride)
                {
                    DefeatScenariosManager.Instance.ActivateDefeatScenario(campingEvent.DefeatScenarioOverride);
                }
            }
            else if (SceneManager.GetActiveScene().name.Contains("_Dungeon"))
            {
                text3 = "DungeonCampEvent";
                CampingEventManager.Instance.PrepareDungeonCampEvent();
            }*/
            EndAction();
        }
    }
}
