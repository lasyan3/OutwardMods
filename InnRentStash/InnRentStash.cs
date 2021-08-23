
using BepInEx;
using BepInEx.Configuration;
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
        public string PlayerHouseQuestEventUID;
    }
    [BepInPlugin(ID, NAME, VERSION)]
    public class InnRentStash : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.InnRentStash";
        const string NAME = "Inn Rent Stash";
        const string VERSION = "1.1.4";
        const bool DEBUG = false;

        //private string m_currentArea;
        //public static bool DialogIsSet;
        public static TreasureChest CurrentStash;
        //public static bool IsStashSharing;
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
            },
            {
                AreaEnum.Harmattan,
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
                    QuestEvent = new QuestEventSignature("stash_monsoon")
                    {
                        EventName = "Inn_Monsoon_StashRent",
                        IsTimedEvent = true,
                        //Savable = true,
                        //IsHideEvent = false,
                    },
                }
           },
            {
                AreaEnum.Harmattan,
                new StrRent {
                    NpcName = "name_merchant_soroborpoorinn_01",
                    StashPosition = new Vector3(-170.0f, -1522.4f, 596.4f),
                    StashRotation = Quaternion.AngleAxis(-90.0f, Vector3.up),
                    QuestEvent = new QuestEventSignature("stash_harmattan")
                    {
                        EventName = "Inn_Harmattan_StashRent",
                        IsTimedEvent = true,
                        //Savable = true,
                        //IsHideEvent = false,
                    },
                }
           }
        };

        public static InnRentStash Instance;

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        public ConfigEntry<int> ConfigRentDuration;
        public ConfigEntry<int> ConfigRentPrice;
        public ConfigEntry<bool> ConfigStashSharing;

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                ConfigRentDuration = Config.Bind("General", "RentDuration", 7, new ConfigDescription("Duration of the rent (in days)", new AcceptableValueRange<int>(1, 100), new ConfigurationManagerAttributes { ShowRangeAsPercent = false }));
                ConfigRentPrice = Config.Bind("General", "RentPrice", 50, new ConfigDescription("Price of the rent", new AcceptableValueRange<int>(10, 500)));
                ConfigStashSharing = Config.Bind("General", "StashSharing", true, new ConfigDescription("Items in the stash are available for crafting"));
                if (DEBUG) MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError("Awake:" + ex.Message);
            }
        }

        public static void CheckRentStatus()
        {
            try
            {
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                if (DEBUG) MyLogger.LogDebug($"CheckRentStatus={areaN}");
                if (!CheckStash())
                {
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                CurrentStash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[areaN]);
                if (!StashAreaToStashUID.Values.Contains(CurrentStash.UID))
                {
                    CurrentStash = null;
                    if (DEBUG) MyLogger.LogDebug($" > Unknown stash");
                    return;
                }
                if (CurrentStash == null)
                {
                    if (DEBUG) MyLogger.LogDebug($" > NullStash");
                    return;
                }

                //OLogger.Log($"Silver={m_currentStash.ContainedSilver}");
                // If we are in Cierzo, check quest failure
                if (areaN == AreaEnum.CierzoVillage)
                {
                    if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"General_NotLighthouseOwner") > 0)
                    {
                        CurrentStash = null;
                    }
                    return;
                }

                if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"PlayerHouse_{areaN}_HouseAvailable") > 0)
                {
                    // House has been bought here, cancel rent event if present
                    if (DEBUG) MyLogger.LogDebug(" > House bought here");
                    if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[areaN].QuestEvent))
                    {
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[areaN].QuestEvent.EventUID);
                    }
                    return;
                }

                #region Move stash to inn
                Vector3 newPos = StashAreaToQuestEvent[areaN].StashPosition;
                Quaternion newRot = StashAreaToQuestEvent[areaN].StashRotation;
                CurrentStash.transform.SetPositionAndRotation(newPos, newRot);
                //ItemVisual iv2 = ItemManager.GetVisuals(m_currentStash.ItemID);
                //ItemVisual iv2 = ItemManager.GetVisuals(m_currentStash);
                Transform transform = UnityEngine.Object.Instantiate(CurrentStash.GetItemVisual());
                ItemVisual iv2 = transform.GetComponent<ItemVisual>();
                //ItemVisual iv2 = UnityEngine.Object.Instantiate(m_currentStash.LoadedVisual);
                iv2.ItemID = CurrentStash.ItemID;
                //ItemVisual iv2 = m_currentStash.LoadedVisual;
                iv2.transform.SetPositionAndRotation(newPos, newRot);
                #endregion

                CurrentStash.SetCanInteract(false);
                if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[areaN].QuestEvent))
                {
                    if (QuestEventManager.Instance.CheckEventExpire(StashAreaToQuestEvent[areaN].QuestEvent.EventUID, Instance.ConfigRentDuration.Value * 24))
                    {
                        //m_notification = $"Rent has expired!";
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent has expired!", 8f);
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[areaN].QuestEvent.EventUID);
                        if (DEBUG) MyLogger.LogDebug(" > Rent expired!");
                    }
                    else
                    {
                        //m_notification = "Rent ongoing";
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent ongoing", 8f);
                        CurrentStash.SetCanInteract(true);
                        if (DEBUG) MyLogger.LogDebug(" > Rent still valid");
                    }
                }
                else
                {
                    if (DEBUG) MyLogger.LogDebug($" > NoQuestEvent");
                }

            }
            catch (Exception ex)
            {
                MyLogger.LogError("CheckRentStatus: " + ex.Message);
            }
        }

        public static bool CheckStash()
        {
            AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            if (!StashAreaToStashUID.Keys.Contains(areaN))
            {
                if (DEBUG) MyLogger.LogDebug($" > Unknown area");
                return false;
            }

            if (CharacterManager.Instance.PlayerCharacters.Count == 0)
            {
                if (DEBUG) MyLogger.LogDebug($" > PlayerCharacters=0");
                return false;
            }
            if (CharacterManager.Instance.PlayerCharacters.Values.Count == 0)
            {
                if (DEBUG) MyLogger.LogDebug($" > PlayerCharacters.Values=0");
                return false;
            }
            return true;
        }
    }
}
