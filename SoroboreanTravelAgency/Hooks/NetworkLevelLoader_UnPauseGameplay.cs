using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using static AreaManager;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(NetworkLevelLoader), "UnPauseGameplay")]
    public class NetworkLevelLoader_UnPauseGameplay
    {
        [HarmonyPostfix]
        public static void UnPauseGameplay(NetworkLevelLoader __instance, string _identifier)
        {
            try
            {
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                foreach (var aqe in SoroboreanTravelAgency.AreaToQuestEvent)
                {
                    if (!QuestEventManager.Instance.HasQuestEvent(aqe.Value) && (
                        aqe.Key == areaN
                        || aqe.Key == AreaEnum.CierzoVillage
                        || aqe.Key == AreaEnum.Monsoon && QuestEventManager.Instance.CurrentQuestEvents.Any(e => e.Name == "PlayerHouse_Monsoon_HouseAvailable" || e.Name == "Faction_HolyMission")
                        || aqe.Key == AreaEnum.Berg && QuestEventManager.Instance.CurrentQuestEvents.Any(e => e.Name == "PlayerHouse_Berg_HouseAvailable" || e.Name == "Faction_BlueChamber")
                        || aqe.Key == AreaEnum.Levant && QuestEventManager.Instance.CurrentQuestEvents.Any(e => e.Name == "PlayerHouse_Levant_HouseAvailable" || e.Name == "Faction_HeroicKingdom")
                        ))
                    {
                        QuestEventManager.Instance.AddEvent(SoroboreanTravelAgency.AreaToQuestEvent[aqe.Key]);
                    }
                }

                if (SoroboreanTravelAgency.TravelArea > -1)
                {
                    Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                    if (character == null)
                    {
                        return;
                    }
                    Vector3 position = Vector3.up;
                    Quaternion rotation = Quaternion.identity;
                    switch ((AreaEnum)SoroboreanTravelAgency.TravelArea)
                    {
                        case AreaEnum.CierzoVillage:
                            position = new Vector3(1410.4f, 5.9f, 1664.0f);
                            rotation = Quaternion.Euler(0.0f, 240.9f, 0.0f);
                            break;
                        case AreaEnum.Monsoon:
                            position = new Vector3(61.8f, -4.9f, 179.2f);
                            rotation = Quaternion.Euler(0.0f, 286.7f, 0.0f);
                            break;
                        case AreaEnum.Berg:
                            position = new Vector3(1201.5f, -13.7f, 1375.6f);
                            rotation = Quaternion.Euler(0.0f, 311.5f, 0.0f);
                            break;
                        case AreaEnum.Levant:
                            position = new Vector3(-53.9f, 0.1f, 81.0f);
                            rotation = Quaternion.Euler(0.0f, 173.9f, 0.0f);
                            break;
                        default:
                            throw new Exception("unk");
                    }
                    character.transform.SetPositionAndRotation(position, rotation);
                    character.CharacterCamera.ResetCameraToPlayer();
                }
                SoroboreanTravelAgency.TravelArea = -1;
                SoroboreanTravelAgency.DialogIsSet = false;
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.MyLogger.LogError(ex.Message);
            }
        }
    }
}
