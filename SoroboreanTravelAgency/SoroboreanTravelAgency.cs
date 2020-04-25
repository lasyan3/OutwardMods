using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static AreaManager;

namespace OutwardMods
{
    public class StrTravel
    {
        public AreaEnum TargetArea;
        public int DurationDays;
    }
    public class GameData
    {
        public int TravelDayCost;
    }
    [BepInPlugin(ID, NAME, VERSION)]
    public class SoroboreanTravelAgency : BaseUnityPlugin
    {
        const string ID = "com.lasyan3.soroboreantravelagency";
        const string NAME = "SoroboreanTravelAgency";
        const string VERSION = "1.0.3";

        public static GameData Settings;
        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        public static bool DialogIsSet = false;
        public static int TravelArea = -1;
        public static int TravelDayCost;
        public static readonly Dictionary<AreaEnum, QuestEventSignature> AreaToQuestEvent = new Dictionary<AreaEnum, QuestEventSignature>
        {
            { AreaEnum.CierzoVillage, new QuestEventSignature("ft_cierzo") { EventName = "FastTravel_Cierzo" } },
            { AreaEnum.Monsoon, new QuestEventSignature("ft_monsoon") { EventName = "FastTravel_Monsoon" } },
            { AreaEnum.Berg, new QuestEventSignature("ft_berg") { EventName = "FastTravel_Berg" } },
            { AreaEnum.Levant, new QuestEventSignature("ft_levant") { EventName = "FastTravel_Levant" } },
        };
        public static readonly Dictionary<AreaEnum, List<StrTravel>> StartAreaToTravel = new Dictionary<AreaEnum, List<StrTravel>>
        {
            {
                AreaEnum.CierzoVillage,
                new List<StrTravel>() {
                    new StrTravel {
                        TargetArea = AreaEnum.Monsoon,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Berg,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Levant,
                        DurationDays = 7,
                    },
                }
            },
            {
                AreaEnum.Monsoon,
                new List<StrTravel>() {
                    new StrTravel {
                        TargetArea = AreaEnum.CierzoVillage,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Berg,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Levant,
                        DurationDays = 7,
                    },
                }
            },
            {
                AreaEnum.Berg,
                new List<StrTravel>() {
                    new StrTravel {
                        TargetArea = AreaEnum.CierzoVillage,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Monsoon,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Levant,
                        DurationDays = 4,
                    },
                }
            },
            {
                AreaEnum.Levant,
                new List<StrTravel>() {
                    new StrTravel {
                        TargetArea = AreaEnum.Berg,
                        DurationDays = 4,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Monsoon,
                        DurationDays = 7,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.CierzoVillage,
                        DurationDays = 7,
                    },
                }
            },
        };
        public static SoroboreanTravelAgency This;

        internal void Awake()
        {
            try
            {
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                Settings = LoadSettings();
                MyLogger.LogDebug("Awaken");
                This = this;
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }


        /*private bool QuestEventManager_AddEvent_1(On.QuestEventManager.orig_AddEvent_1 orig, QuestEventManager self, string _eventUID, int _stackAmount, bool _sendEvent)
        {
            bool res = orig(self, _eventUID, _stackAmount, _sendEvent);
            if (res)
            {
                //OLogger.Log($"Added event {QuestEventManager.Instance.CurrentQuestEvents.First(e => e.EventUID == _eventUID).Name}");
            }
            return res;
        }*/

        private GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader($"BepInEx/config/{NAME}Config.json"))
                {
                    return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
            return null;
        }
    }
}
