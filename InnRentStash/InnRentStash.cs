
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AreaManager;

namespace InnRentStash
{
    public class StrRent
    {
        public QuestEventSignature QuestEvent;
        public string NpcName;
        public Vector3 StashPosition;
        public Quaternion StashRotation;
        public int RentPrice;
        public int RentDuration;
        public string PlayerHouseQuestEventUID;
    }
    [BepInPlugin(ID, NAME, VERSION)]
    public class InnRentStash : BaseUnityPlugin
    {
        const string ID = "com.lasyan3.innrentstash";
        const string NAME = "InnRentStash";
        const string VERSION = "1.1.1";

        //private string m_currentArea;
        public static bool m_dialogIsSet;
        public static TreasureChest m_currentStash;
        public static bool m_isStashSharing = true;
        public static readonly Dictionary<AreaEnum, string> StashAreaToStashUID = new Dictionary<AreaEnum, string>
        {
            {
                AreaEnum.Berg,
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                AreaEnum.CierzoVillage,
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                AreaEnum.Levant,
                "ZbPXNsPvlUeQVJRks3zBzg"
            },
            {
                AreaEnum.Monsoon,
                "ImqRiGAT80aE2WtUHfdcMw"
            }
        };
        public static readonly Dictionary<AreaEnum, StrRent> StashAreaToQuestEvent = new Dictionary<AreaEnum, StrRent>
        {
            {
                AreaEnum.Berg,
                new StrRent {
                    NpcName = "name_unpc_berginnkeeper_01",
                    StashPosition = new Vector3(-366.3f, -1500.0f, 764.9f),
                    StashRotation = new Quaternion(),
                    RentPrice = 50,
                    RentDuration = 168,
                    QuestEvent = new QuestEventSignature("stash_berg")
                    {
                        EventName = "Inn_Berg_StashRent",
                        IsTimedEvent = true,
                    },
                }
            },
            {
                AreaEnum.Levant,
                 new StrRent {
                    NpcName = "name_unpc_levantinnkeeper_01",
                    StashPosition = new Vector3(-360.2f, -1509.5f, 565.4f),
                    StashRotation = Quaternion.AngleAxis(-65.0f, Vector3.up),
                    RentPrice = 50,
                    RentDuration = 168,
                    QuestEvent = new QuestEventSignature("stash_levant")
                    {
                        EventName = "Inn_Levant_StashRent",
                        IsTimedEvent = true,
                    },
                }
           },
            {
                AreaEnum.Monsoon,
                new StrRent {
                    NpcName = "name_unpc_monsooninnkeeper_01",
                    StashPosition = new Vector3(-372.0f, -1500.0f, 557.7f),
                    StashRotation = new Quaternion(),
                    RentPrice = 50,
                    RentDuration = 168,
                    QuestEvent = new QuestEventSignature("stash_monsoon")
                    {
                        EventName = "Inn_Monsoon_StashRent",
                        IsTimedEvent = true,
                        //Savable = true,
                        //IsHideEvent = false,
                    },
                }
           }
        };

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        internal void Awake()
        {
            try
            {
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }

        public static void CheckRentStatus()
        {
            try
            {
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                MyLogger.LogDebug($"CheckRentStatus={areaN}");
                if (!CheckStash())
                {
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                m_currentStash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[areaN]);
                if (!StashAreaToStashUID.Values.Contains(m_currentStash.UID))
                {
                    m_currentStash = null;
                    MyLogger.LogDebug($" > Unknown stash");
                    return;
                }
                if (m_currentStash == null)
                {
                    MyLogger.LogDebug($" > NullStash");
                    return;
                }

                //OLogger.Log($"Silver={m_currentStash.ContainedSilver}");
                // If we are in Cierzo, check quest failure
                if (areaN == AreaEnum.CierzoVillage)
                {
                    if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"General_NotLighthouseOwner") > 0)
                    {
                        m_currentStash = null;
                    }
                    return;
                }

                if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"PlayerHouse_{areaN}_HouseAvailable") > 0)
                {
                    // House has been bought here, cancel rent event if present
                    MyLogger.LogDebug(" > House bought here");
                    if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[areaN].QuestEvent))
                    {
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[areaN].QuestEvent.EventUID);
                    }
                    return;
                }

                #region Move stash to inn
                Vector3 newPos = StashAreaToQuestEvent[areaN].StashPosition;
                Quaternion newRot = StashAreaToQuestEvent[areaN].StashRotation;
                m_currentStash.transform.SetPositionAndRotation(newPos, newRot);
                //ItemVisual iv2 = ItemManager.GetVisuals(m_currentStash.ItemID);
                //ItemVisual iv2 = ItemManager.GetVisuals(m_currentStash);
                Transform transform = UnityEngine.Object.Instantiate(m_currentStash.VisualPrefab);
                ItemVisual iv2 = transform.GetComponent<ItemVisual>();
                //ItemVisual iv2 = UnityEngine.Object.Instantiate(m_currentStash.LoadedVisual);
                iv2.ItemID = m_currentStash.ItemID;
                //ItemVisual iv2 = m_currentStash.LoadedVisual;
                iv2.transform.SetPositionAndRotation(newPos, newRot);
                #endregion

                m_currentStash.SetCanInteract(false);
                if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[areaN].QuestEvent))
                {
                    if (QuestEventManager.Instance.CheckEventExpire(StashAreaToQuestEvent[areaN].QuestEvent.EventUID, StashAreaToQuestEvent[areaN].RentDuration))
                    {
                        //m_notification = $"Rent has expired!";
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent has expired!", 8f);
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[areaN].QuestEvent.EventUID);
                        MyLogger.LogDebug(" > Rent expired!");
                    }
                    else
                    {
                        //m_notification = "Rent ongoing";
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent ongoing", 8f);
                        m_currentStash.SetCanInteract(true);
                        MyLogger.LogDebug(" > Rent still valid");
                    }
                }
                else
                {
                    MyLogger.LogDebug($" > NoQuestEvent");
                }

            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }

        public static bool CheckStash()
        {
            AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            if (!StashAreaToStashUID.Keys.Contains(areaN))
            {
                MyLogger.LogDebug($" > Unknown area");
                return false;
            }

            if (CharacterManager.Instance.PlayerCharacters.Count == 0)
            {
                MyLogger.LogDebug($" > PlayerCharacters=0");
                return false;
            }
            if (CharacterManager.Instance.PlayerCharacters.Values.Count == 0)
            {
                MyLogger.LogDebug($" > PlayerCharacters.Values=0");
                return false;
            }
            return true;
        }
    }
}
