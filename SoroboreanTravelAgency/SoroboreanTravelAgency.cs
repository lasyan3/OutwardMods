using Harmony;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Conditions;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static AreaManager;

namespace Wip
{
    public class StrTravel
    {
        public AreaEnum TargetArea;
        //public int TravelPrice;
        //public int TravelDuration;
        //public int TravelRations;
        //public int TravelSpawnPoint = 0;
        public int DurationDays;
    }
    public class SoroboreanTravelAgency : PartialityMod
    {
        private readonly string m_modName = "WorkInProgress";
        bool m_dialogIsSet = false;
        public int m_travelArea = -1;
        private const int TravelDayCost = 30;
        private readonly Dictionary<AreaEnum, List<StrTravel>> StartAreaToTravel = new Dictionary<AreaEnum, List<StrTravel>>
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
                    }
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
                    }
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
                    }
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
                    }
                }
            },
        };

        public SoroboreanTravelAgency()
        {
            this.ModID = m_modName;
            this.Version = "1.0.0";
            this.author = "lasyan3";
        }

        public override void Init()
        {
            base.Init();
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
        }

        private void NetworkLevelLoader_UnPauseGameplay(On.NetworkLevelLoader.orig_UnPauseGameplay orig, NetworkLevelLoader self, string _identifier)
        {
            orig(self, _identifier);
            try
            {
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
            //OLogger.Log($"{self.ActorLocKey}");
            orig(self);
            try
            {
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
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

                    MultipleChoiceNodeExt nChoose = graph.AddNode<MultipleChoiceNodeExt>();
                    graph.ConnectNodes(firstNode, nStart, cnt);
                    graph.ConnectNodes(nStart, nChoose);

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


                    foreach (StrTravel travel in StartAreaToTravel[areaN])
                    {
                        nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement($"{travel.TargetArea} ({travel.DurationDays * TravelDayCost} silver and {travel.DurationDays} rations).")));

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
                    nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("Changed my mind.")));

                    graph.ConnectNodes(nChoose, nCancel);

                    graph.ConnectNodes(nResultMoneyBad, nFinish);
                    graph.ConnectNodes(nResultRationsBad, nFinish);
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
    }
}
