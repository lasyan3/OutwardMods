using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SharedModConfig;
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
    [BepInDependency("com.sinai.SharedModConfig", BepInDependency.DependencyFlags.HardDependency)]
    public class SoroboreanTravelAgency : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.SoroboreanTravelAgency";
        const string NAME = "Soroborean Travel Agency";
        const string VERSION = "1.1.0";

        public static SoroboreanTravelAgency Instance;
        //public static GameData Settings;
        public ManualLogSource MyLogger { get { return Logger; } }
        public ModConfig MyConfig;

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
            {
                AreaEnum.CierzoOutside,
                new List<StrTravel>() {
                    new StrTravel {
                        TargetArea = AreaEnum.Berg,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Monsoon,
                        DurationDays = 3,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.Levant,
                        DurationDays = 7,
                    },
                    new StrTravel {
                        TargetArea = AreaEnum.CierzoVillage,
                        DurationDays = 0,
                    },
                }
            },
            {
                AreaEnum.HallowedMarsh,
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
                     new StrTravel {
                        TargetArea = AreaEnum.Monsoon,
                        DurationDays = 0,
                    },
               }
            },
            {
                AreaEnum.Emercar,
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
                       new StrTravel {
                        TargetArea = AreaEnum.Berg,
                        DurationDays = 0,
                    },
             }
            },
            {
                AreaEnum.Abrassar,
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
                    new StrTravel {
                        TargetArea = AreaEnum.Levant,
                        DurationDays = 0,
                    },
                }
            },
        };

        internal void Awake()
        {
            try
            {
                Instance = this;
                var harmony = new Harmony(ID);
                harmony.PatchAll();
                //Settings = LoadSettings();
                MyConfig = SetupConfig();
                //MyConfig.OnSettingsOpened += MyConfig_OnSettingsOpened;
                MyConfig.OnSettingsSaved += MyConfig_OnSettingsSaved;
                MyConfig.Register();
                Logger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private void MyConfig_OnSettingsSaved()
        {
            try
            {
                //Logger.LogDebug("MyConfig_OnSettingsSaved");
                foreach (var aqe in AreaToQuestEvent)
                {
                    string settingName = aqe.Key + "Visited";
                    bool isVisited = (bool)MyConfig.GetValue(settingName);
                    //Logger.LogDebug("  " + settingName + "=" + isVisited);
                    if (isVisited && !QuestEventManager.Instance.HasQuestEvent(aqe.Value))
                    {
                        //Logger.LogDebug("    HasQuestEvent > true");
                        QuestEventManager.Instance.AddEvent(AreaToQuestEvent[aqe.Key]);
                    }
                    if (!isVisited && QuestEventManager.Instance.HasQuestEvent(aqe.Value))
                    {
                        //Logger.LogDebug("    NotQuestEvent > false");
                        QuestEventManager.Instance.RemoveEvent(AreaToQuestEvent[aqe.Key].EventUID);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private void MyConfig_OnSettingsOpened()
        {
            try
            {
                //Logger.LogDebug("MyConfig_OnSettingsOpened");
                foreach (var aqe in AreaToQuestEvent)
                {
                    string settingName = aqe.Key + "Visited";
                    bool isVisited = (bool)MyConfig.GetValue(settingName);
                    //Logger.LogDebug("  " + settingName + "=" + isVisited);
                    if (QuestEventManager.Instance.HasQuestEvent(aqe.Value))
                    {
                        //Logger.LogDebug("    HasQuestEvent > true");
                        MyConfig.SetValue(settingName, true);
                    }
                    else
                    {
                        //Logger.LogDebug("    NotQuestEvent > false");
                        MyConfig.SetValue(settingName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
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

        private ModConfig SetupConfig()
        {
            return new ModConfig
            {
                ModName = NAME,
                Settings = new List<BBSetting>
                {
                    new FloatSetting {
                        Name = "Travel Day Cost",
                        Description = "Cost of a travel day (in silver)",
                        DefaultValue = 50f,
                        MinValue = 1f,
                        MaxValue = 100f,
                        Increment = 5f,
                        //RoundTo = 0,
                        //ShowPercent = false,
                    },
                    new BoolSetting {
                        Name = "CierzoVillageVisited",
                        Description = "Cierzo Visited",
                        DefaultValue = false,
                    },
                    new BoolSetting {
                        Name = "MonsoonVisited",
                        Description = "Monsoon Visited",
                        DefaultValue = false,
                    },
                    new BoolSetting {
                        Name = "BergVisited",
                        Description = "Berg Visited",
                        DefaultValue = false,
                    },
                    new BoolSetting {
                        Name = "LevantVisited",
                        Description = "Levant Visited",
                        DefaultValue = false,
                    },
                }
            };
        }
    }
}
