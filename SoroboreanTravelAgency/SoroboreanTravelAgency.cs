using Harmony;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Conditions;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static AreaManager;

namespace OutwardMods
{
    public class StrTravel
    {
        public AreaEnum TargetArea;
        public int DurationDays;
    }
    class GameData
    {
        public int TravelDayCost;
    }
    public class SoroboreanTravelAgency : PartialityMod
    {
        private GameData data;
        readonly string m_modName = "SoroboreanTravelAgency";
        bool m_dialogIsSet = false;
        public int m_travelArea = -1;
        int TravelDayCost;
        readonly Dictionary<AreaEnum, QuestEventSignature> AreaToQuestEvent = new Dictionary<AreaEnum, QuestEventSignature>
        {
            { AreaEnum.CierzoVillage, new QuestEventSignature("ft_cierzo") { EventName = "FastTravel_Cierzo" } },
            { AreaEnum.Monsoon, new QuestEventSignature("ft_monsoon") { EventName = "FastTravel_Monsoon" } },
            { AreaEnum.Berg, new QuestEventSignature("ft_berg") { EventName = "FastTravel_Berg" } },
            { AreaEnum.Levant, new QuestEventSignature("ft_levant") { EventName = "FastTravel_Levant" } },
        };
        readonly Dictionary<AreaEnum, List<StrTravel>> StartAreaToTravel = new Dictionary<AreaEnum, List<StrTravel>>
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

        public SoroboreanTravelAgency()
        {
            this.ModID = m_modName;
            this.Version = "1.0.1";
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
            data = LoadSettings();
        }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            On.NPCInteraction.OnActivate += NPCInteraction_OnActivate;
            On.NetworkLevelLoader.UnPauseGameplay += NetworkLevelLoader_UnPauseGameplay;
            //On.QuestEventManager.AddEvent_1 += QuestEventManager_AddEvent_1;
            On.QuestEventDictionary.Load += QuestEventDictionary_Load;
        }

        private void QuestEventDictionary_Load(On.QuestEventDictionary.orig_Load orig)
        {
            orig();
            try
            {
                // Add Fast Travel events
                /*foreach (var item in QuestEventDictionary.Sections)
                {
                    OLogger.Log(item.Name);
                }*/
                QuestEventFamily innSection = QuestEventDictionary.Sections.FirstOrDefault(s => s.Name == "Neutral_General");
                if (innSection != null)
                {
                    foreach (var item in AreaToQuestEvent)
                    {
                        if (!innSection.Events.Contains(item.Value))
                        {
                            innSection.Events.Add(item.Value);
                        }
                        if (QuestEventDictionary.GetQuestEvent(item.Value.EventUID) == null)
                        {
                            QuestEventDictionary.RegisterEvent(item.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] QuestEventDictionary_Load: {ex.Message}");
            }
        }

        private bool QuestEventManager_AddEvent_1(On.QuestEventManager.orig_AddEvent_1 orig, QuestEventManager self, string _eventUID, int _stackAmount, bool _sendEvent)
        {
            bool res = orig(self, _eventUID, _stackAmount, _sendEvent);
            if (res)
            {
                OLogger.Log($"Added event {QuestEventManager.Instance.CurrentQuestEvents.First(e => e.EventUID == _eventUID).Name}");
            }
            return res;
        }

        private void NetworkLevelLoader_UnPauseGameplay(On.NetworkLevelLoader.orig_UnPauseGameplay orig, NetworkLevelLoader self, string _identifier)
        {
            orig(self, _identifier);
            try
            {
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                foreach (var aqe in AreaToQuestEvent)
                {
                    if (!QuestEventManager.Instance.HasQuestEvent(aqe.Value) && (
                        aqe.Key == areaN
                        || aqe.Key == AreaEnum.CierzoVillage
                        || aqe.Key == AreaEnum.Monsoon && QuestEventManager.Instance.CurrentQuestEvents.Any(e => e.Name == "PlayerHouse_Monsoon_HouseAvailable" || e.Name == "Faction_HolyMission")
                        || aqe.Key == AreaEnum.Berg && QuestEventManager.Instance.CurrentQuestEvents.Any(e => e.Name == "PlayerHouse_Berg_HouseAvailable" || e.Name == "Faction_BlueChamber")
                        || aqe.Key == AreaEnum.Levant && QuestEventManager.Instance.CurrentQuestEvents.Any(e => e.Name == "PlayerHouse_Levant_HouseAvailable" || e.Name == "Faction_HeroicKingdom")
                        ))
                    {
                        QuestEventManager.Instance.AddEvent(AreaToQuestEvent[aqe.Key]);
                    }
                }

                if (m_travelArea > -1)
                {
                    Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                    if (character == null)
                    {
                        return;
                    }
                    Vector3 position = Vector3.up;
                    Quaternion rotation = Quaternion.identity;
                    switch ((AreaEnum)m_travelArea)
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
                m_travelArea = -1;
                m_dialogIsSet = false;
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] NetworkLevelLoader_UnPauseGameplay: {ex.Message}");
            }
        }

        private void NPCInteraction_OnActivate(On.NPCInteraction.orig_OnActivate orig, NPCInteraction self)
        {
            orig(self);
            try
            {
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(self.ActorLocKey))
                {
                    //OLogger.Log($"{self.ActorLocKey}");
                }
                // TODO: travel with red merchant
                if (m_dialogIsSet)
                {
                    return;
                }
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                if (!StartAreaToTravel.ContainsKey(areaN))
                {
                    return;
                }
                TravelDayCost = data.TravelDayCost; // 80 - 100 - 120
                if (areaN == AreaEnum.Levant) // TODO: check quest "Blood under the Sun"
                {
                    TravelDayCost = (int)(TravelDayCost * 1.75); // 140 - 175 - 210
                }
                if (self.ActorLocKey == "name_unpc_caravantrader_01")
                {
                    var graphOwner = self.NPCDialogue.DialogueController;
                    var graph = (Graph)graphOwner.GetType().BaseType.GetField("_graph", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graphOwner as GraphOwner<DialogueTreeExt>);
                    var nodes = typeof(Graph).GetField("_nodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graph as Graph) as List<Node>;
                    var firstNode = nodes.First(n => n.GetType().Name == "MultipleChoiceNodeExt");

                    int cnt = (firstNode as MultipleChoiceNodeExt).availableChoices.Count - 2;
                    (firstNode as MultipleChoiceNodeExt).availableChoices.Insert(cnt, new MultipleChoiceNodeExt.Choice(new Statement("I want to travel, please.")));
                    StatementNodeExt nStart = graph.AddNode<StatementNodeExt>();
                    nStart.statement = new Statement($"Where do you want to go?");
                    nStart.SetActorName(self.ActorLocKey);

                    StatementNodeExt nResultMoneyBad = graph.AddNode<StatementNodeExt>();
                    nResultMoneyBad.statement = new Statement("Sorry, you don't have enough silver. Come back when you can afford it!");
                    nResultMoneyBad.SetActorName(self.ActorLocKey);
                    StatementNodeExt nResultRationsBad = graph.AddNode<StatementNodeExt>();
                    nResultRationsBad.statement = new Statement("Sorry, you don't have enough rations to travel this far.");
                    nResultRationsBad.SetActorName(self.ActorLocKey);
                    StatementNodeExt nCancel = graph.AddNode<StatementNodeExt>();
                    nCancel.statement = new Statement("See you soon!");
                    nCancel.SetActorName(self.ActorLocKey);
                    FinishNode nFinish = graph.AddNode<FinishNode>();
                    StatementNodeExt nNoDestination = graph.AddNode<StatementNodeExt>();
                    nNoDestination.statement = new Statement("Sorry, you don't have discovered any other town... You can only travel to places you visited at least once!");
                    nNoDestination.SetActorName(self.ActorLocKey);

                    bool hasEntry = false;
                    MultipleChoiceNodeExt nChoose = graph.AddNode<MultipleChoiceNodeExt>();
                    foreach (StrTravel travel in StartAreaToTravel[areaN])
                    {
                        if (!QuestEventManager.Instance.HasQuestEvent(AreaToQuestEvent[travel.TargetArea]))
                        {
                            continue; // This town has not been visited yet
                        }

                        hasEntry = true;
                        string areaLabel = travel.TargetArea.ToString();
                        if (travel.TargetArea == AreaEnum.CierzoVillage)
                        {
                            areaLabel = "Cierzo";
                        }
                        nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement($"{areaLabel} ({travel.DurationDays * TravelDayCost} silver and {travel.DurationDays} rations).")));

                        ConditionNode nCheckMoney = graph.AddNode<ConditionNode>();
                        nCheckMoney.condition = new Condition_OwnsItem()
                        {
                            character = character,
                            item = new ItemReference { ItemID = 9000010 },
                            minAmount = travel.DurationDays * TravelDayCost
                        };
                        ConditionNode nCheckRations = graph.AddNode<ConditionNode>();
                        nCheckRations.condition = new Condition_OwnsItem()
                        {
                            character = character,
                            item = new ItemReference { ItemID = 4100550 }, // Travel Ration
                            minAmount = travel.DurationDays
                        };
                        graph.ConnectNodes(nChoose, nCheckMoney);

                        ActionNode nWishRent = graph.AddNode<ActionNode>();
                        ActionList actions = new ActionList();
                        actions.AddAction(new NodeCanvas.Tasks.Actions.PlaySound()
                        {
                            Sound = GlobalAudioManager.Sounds.UI_MERCHANT_CompleteTransaction
                        });
                        actions.AddAction(new NodeCanvas.Tasks.Actions.FadeOut()
                        {
                            fadeTime = 1.0f
                        });
                        actions.AddAction(new SetTravelArea()
                        {
                            Script = this,
                            TargetArea = travel.TargetArea
                        });
                        actions.AddAction(new AreaSwitchPlayersTime()
                        {
                            Area = travel.TargetArea,
                            IncreaseTime = travel.DurationDays * 24
                        });
                        actions.AddAction(new NodeCanvas.Tasks.Actions.RemoveItem()
                        {
                            fromCharacter = new BBParameter<Character>(character),
                            Items = new List<BBParameter<ItemReference>>() { new ItemReference { ItemID = 9000010 }, new ItemReference { ItemID = 4100550 } },
                            Amount = new List<BBParameter<int>>() { new BBParameter<int>(travel.DurationDays * TravelDayCost), new BBParameter<int>(travel.DurationDays) },
                        });
                        nWishRent.action = actions;
                        graph.ConnectNodes(nCheckMoney, nCheckRations);
                        graph.ConnectNodes(nCheckMoney, nResultMoneyBad);
                        graph.ConnectNodes(nCheckRations, nWishRent);
                        graph.ConnectNodes(nCheckRations, nResultRationsBad);
                        graph.ConnectNodes(nWishRent, nFinish);
                    }
                    if (hasEntry)
                    {
                        graph.ConnectNodes(firstNode, nStart, cnt);
                        nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("Changed my mind.")));
                        graph.ConnectNodes(nStart, nChoose);
                        graph.ConnectNodes(nChoose, nCancel);
                    }
                    else
                    {
                        graph.ConnectNodes(firstNode, nNoDestination, cnt);
                    }

                    graph.ConnectNodes(nResultMoneyBad, nFinish);
                    graph.ConnectNodes(nResultRationsBad, nFinish);
                    graph.ConnectNodes(nNoDestination, nFinish);
                    graph.ConnectNodes(nCancel, nFinish);//*/

                    m_dialogIsSet = true;
                }
            }
            catch (Exception ex)
            {
                //OLogger.Error("NPCInteraction_OnActivate:" + ex.Message);
                Debug.Log($"[{m_modName}] NPCInteraction_OnActivate: {ex.Message}");
            }
        }
        private GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader($"mods/{m_modName}Config.json"))
                {
                    return JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                //OLogger.Error("General IO Exception", _modName);
                Debug.Log($"[{m_modName}] LoadSettings: {ex.Message}");
            }
            return null;
        }
    }
}
