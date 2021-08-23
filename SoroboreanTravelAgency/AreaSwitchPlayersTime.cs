using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System;
using UnityEngine;

namespace OutwardMods
{
    [Name("Area Switch Players")]
    [Category("Character")]
    public class AreaSwitchPlayersTime : ActionTask
    {
        public BBParameter<AreaManager.AreaEnum> Area;

        public BBParameter<int> SpawnPoint = 0;
        public BBParameter<float> IncreaseTime = 0;
        public BBParameter<Character> Character;

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
                Character.value.PlayerStats.Sleep = Character.value.PlayerStats.MaxSleep + 50f;
                Character.value.PlayerStats.Food = Character.value.PlayerStats.MaxFood;
                Character.value.PlayerStats.Drink = Character.value.PlayerStats.MaxDrink;
                //Character.value.PlayerStats.RestoreAllVitals();
                Character.value.PlayerStats.Temperature = 50f;
                Character.value.PlayerStats.SetWasInTravel();
                Character.value.StatusEffectMngr.IncreaseLongStatusesAge((int)IncreaseTime.value);
                Character.value.StatusEffectMngr.RemoveShortStatuses();
                Character.value.Inventory.SkillKnowledge.ResetAllCooldowns();

                NetworkLevelLoader.Instance.RequestSwitchArea(IncreaseTime.value, AreaManager.Instance.GetArea(Area.value).SceneName, SpawnPoint.value);
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
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
