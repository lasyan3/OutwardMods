using HarmonyLib;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static AreaManager;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(NPCInteraction), "OnActivate")]
    public class NPCInteraction_OnActivate
    {
        [HarmonyPostfix]
        public static void OnActivate(NPCInteraction __instance)
        {
            try
            {
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(__instance.ActorLocKey))
                {
                    //OLogger.Log($"{self.ActorLocKey}");
                }
                // TODO: travel with red merchant
                if (SoroboreanTravelAgency.DialogIsSet)
                {
                    return;
                }
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                if (!SoroboreanTravelAgency.StartAreaToTravel.ContainsKey(areaN))
                {
                    return;
                }
                SoroboreanTravelAgency.TravelDayCost = SoroboreanTravelAgency.Settings.TravelDayCost; // 80 - 100 - 120
                if (areaN == AreaEnum.Levant) // TODO: check quest "Blood under the Sun"
                {
                    SoroboreanTravelAgency.TravelDayCost = (int)(SoroboreanTravelAgency.TravelDayCost * 1.75); // 140 - 175 - 210
                }
                // TODO: HallowPeaceGuardBlock
                if (__instance.ActorLocKey == "name_unpc_caravantrader_01")
                {
                    var graphOwner = __instance.NPCDialogue.DialogueController;
                    var graph = (Graph)graphOwner.GetType().BaseType.GetField("_graph", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graphOwner as GraphOwner<DialogueTreeExt>);
                    var nodes = typeof(Graph).GetField("_nodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graph as Graph) as List<Node>;
                    var firstNode = nodes.First(n => n.GetType().Name == "MultipleChoiceNodeExt");

                    int cnt = (firstNode as MultipleChoiceNodeExt).availableChoices.Count - 2;
                    (firstNode as MultipleChoiceNodeExt).availableChoices.Insert(cnt, new MultipleChoiceNodeExt.Choice(new Statement("I want to travel, please.")));
                    StatementNodeExt nStart = graph.AddNode<StatementNodeExt>();
                    nStart.statement = new Statement($"Where do you want to go?");
                    nStart.SetActorName(__instance.ActorLocKey);

                    StatementNodeExt nResultMoneyBad = graph.AddNode<StatementNodeExt>();
                    nResultMoneyBad.statement = new Statement("Sorry, you don't have enough silver. Come back when you can afford it!");
                    nResultMoneyBad.SetActorName(__instance.ActorLocKey);
                    StatementNodeExt nResultRationsBad = graph.AddNode<StatementNodeExt>();
                    nResultRationsBad.statement = new Statement("Sorry, you don't have enough rations to travel this far.");
                    nResultRationsBad.SetActorName(__instance.ActorLocKey);
                    StatementNodeExt nCancel = graph.AddNode<StatementNodeExt>();
                    nCancel.statement = new Statement("See you soon!");
                    nCancel.SetActorName(__instance.ActorLocKey);
                    FinishNode nFinish = graph.AddNode<FinishNode>();
                    StatementNodeExt nNoDestination = graph.AddNode<StatementNodeExt>();
                    nNoDestination.statement = new Statement("Sorry, you have not discovered any other town... You can only travel to places you visited at least once!");
                    nNoDestination.SetActorName(__instance.ActorLocKey);

                    bool hasEntry = false;
                    MultipleChoiceNodeExt nChoose = graph.AddNode<MultipleChoiceNodeExt>();
                    foreach (StrTravel travel in SoroboreanTravelAgency.StartAreaToTravel[areaN])
                    {
                        if (!QuestEventManager.Instance.HasQuestEvent(SoroboreanTravelAgency.AreaToQuestEvent[travel.TargetArea]))
                        {
                            continue; // This town has not been visited yet
                        }

                        hasEntry = true;
                        string areaLabel = travel.TargetArea.ToString();
                        if (travel.TargetArea == AreaEnum.CierzoVillage)
                        {
                            areaLabel = "Cierzo";
                        }
                        nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement($"{areaLabel} ({travel.DurationDays * SoroboreanTravelAgency.TravelDayCost} silver and {travel.DurationDays} rations).")));

                        ConditionNode nCheckMoney = graph.AddNode<ConditionNode>();
                        nCheckMoney.condition = new Condition_OwnsItem()
                        {
                            character = character,
                            item = new ItemReference { ItemID = 9000010 },
                            minAmount = travel.DurationDays * SoroboreanTravelAgency.TravelDayCost
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
                            Script = SoroboreanTravelAgency.This,
                            TargetArea = travel.TargetArea
                        });
                        actions.AddAction(new AreaSwitchPlayersTime()
                        {
                            Character = new BBParameter<Character>(character),
                            Area = travel.TargetArea,
                            IncreaseTime = travel.DurationDays * 24
                        });
                        actions.AddAction(new NodeCanvas.Tasks.Actions.RemoveItem()
                        {
                            fromCharacter = new BBParameter<Character>(character),
                            Items = new List<BBParameter<ItemReference>>() { new ItemReference { ItemID = 9000010 }, new ItemReference { ItemID = 4100550 } },
                            Amount = new List<BBParameter<int>>() { new BBParameter<int>(travel.DurationDays * SoroboreanTravelAgency.TravelDayCost), new BBParameter<int>(travel.DurationDays) },
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

                    SoroboreanTravelAgency.DialogIsSet = true;
                }
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.MyLogger.LogError(ex.Message);
            }
        }
    }
}
