using HarmonyLib;
using System;
using static AreaManager;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(QuestEventManager), "AddEvent", new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public class QuestEventManager_AddEvent
    {
        [HarmonyPostfix]
        public static void AddEvent(string _eventUID, bool __result)
        {
            InnRentStash.MyLogger.LogDebug($"AddEvent({_eventUID})={__result}");
            if (!__result) return;
            try
            {
                AreaEnum areaN = (AreaEnum)Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                //if (string.IsNullOrEmpty(m_currentArea)) return res;
                if (!InnRentStash.StashAreaToQuestEvent.ContainsKey(areaN)) return;
                if (InnRentStash.m_currentStash == null) return;
                // If event is house buying, cancel previous rent event
                /*if (QuestEventManager.Instance.GetQuestEvent(_eventUID).Name == $"PlayerHouse_{m_currentArea}_HouseAvailable" &&
                    QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent))
                {
                    QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID);
                    m_currentStash.SetCanInteract(true);
                    //character.CharacterUI.SmallNotificationPanel.ShowNotification($"Rent canceled", 5f);
                    //DoLog("  Rent canceled (house bought)");
                }*/
                // If event is rent, activate the stash
                if (_eventUID == InnRentStash.StashAreaToQuestEvent[areaN].QuestEvent.EventUID)
                {
                    InnRentStash.m_currentStash.SetCanInteract(true);
                    InnRentStash.MyLogger.LogDebug("Activate stash");
                }
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError(ex.Message);
            }
        }
    }
}
