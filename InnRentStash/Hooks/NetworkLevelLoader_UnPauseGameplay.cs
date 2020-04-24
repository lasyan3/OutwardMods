using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(NetworkLevelLoader), "UnPauseGameplay")]
    public class NetworkLevelLoader_UnPauseGameplay
    {
        [HarmonyPostfix]
        public static void UnPauseGameplay(string _identifier)
        {
            try
            {
                InnRentStash.MyLogger.LogDebug($"UnPauseGameplay={_identifier}");
                if (_identifier == "Loading")
                {
                    InnRentStash.CheckRentStatus();
                }
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError(ex.Message);
            }
            /*orig(self, _identifier);
            try
            {
                DoOloggerLog($"UnPauseGameplay={_identifier}");
                if (_identifier == "Loading" && m_currentStash != null)
                {
                    AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                    m_currentStash.SetCanInteract(false);
                    if (!CheckStash())
                    {
                        return;
                    }
                    if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[areaN].QuestEvent))
                    {
                        Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                        if (QuestEventManager.Instance.CheckEventExpire(StashAreaToQuestEvent[areaN].QuestEvent.EventUID, StashAreaToQuestEvent[areaN].RentDuration))
                        {
                            //m_notification = $"Rent has expired!";
                            character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent has expired!", 8f);
                            QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[areaN].QuestEvent.EventUID);
                            DoOloggerLog(" > Rent expired!");
                        }
                        else
                        {
                            //m_notification = "Rent ongoing";
                            character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent ongoing", 8f);
                            m_currentStash.SetCanInteract(true);
                            DoOloggerLog(" > Rent ongoing");
                        }
                    }
                    else
                    {
                        DoOloggerLog($" > NoQuestEvent:{StashAreaToQuestEvent[areaN].QuestEvent.EventName}");
                    }
                }
            }
            catch (Exception ex)
            {
                DoOloggerError($"NetworkLevelLoader_UnPauseGameplay: {ex.Message}");
                //Debug.Log($"[{m_modName}] NetworkLevelLoader_UnPauseGameplay: {ex.Message}");
            }*/
        }
    }
}
