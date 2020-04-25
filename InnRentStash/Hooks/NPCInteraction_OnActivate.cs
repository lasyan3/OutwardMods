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

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(NPCInteraction), "OnActivate")]
    public class NPCInteraction_OnActivate
    {
        [HarmonyPostfix]
        public static void OnActivate(NPCInteraction __instance)
        {
            try
            {
                AreaEnum areaN = (AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
                if (!InnRentStash.StashAreaToQuestEvent.ContainsKey(areaN) || CharacterManager.Instance.PlayerCharacters.Count == 0)
                {
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
                }
                if (__instance.ActorLocKey != InnRentStash.StashAreaToQuestEvent[areaN].NpcName || InnRentStash.m_dialogIsSet)
                {
                    return;
                }
                var graphOwner = __instance.NPCDialogue.DialogueController;
                var graph = (Graph)graphOwner.GetType().BaseType.GetField("_graph", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graphOwner as GraphOwner<DialogueTreeExt>);
                var nodes = typeof(Graph).GetField("_nodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graph as Graph) as List<Node>;
                var firstNode = nodes.First(n => n.GetType().Name == "MultipleChoiceNodeExt");

                //TreeNode<Node>.DebugDialogue(nodes[0], 0);
                /* Un dialogue c'est: afficher un texte + action(optionnel) + choix(optionnel)
                 * 
                 * Outil de conversion Graph --> ma structure
                 */
                (firstNode as MultipleChoiceNodeExt).availableChoices.Insert(1, new MultipleChoiceNodeExt.Choice(new Statement("I want to rent a stash, please.")));
                StatementNodeExt nStart = graph.AddNode<StatementNodeExt>();
                nStart.statement = new Statement($"Of course! Renting a stash costs only {InnRentStash.StashAreaToQuestEvent[areaN].RentPrice} silver for one week.");
                nStart.SetActorName(__instance.ActorLocKey);
                MultipleChoiceNodeExt nChoose = graph.AddNode<MultipleChoiceNodeExt>();
                nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("Sh*t up and take my money!")));
                nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("I will be back.")));
                ConditionNode nCheckMoney = graph.AddNode<ConditionNode>();
                nCheckMoney.condition = new Condition_OwnsItem()
                {
                    character = character,
                    item = new ItemReference { ItemID = 9000010 },
                    minAmount = InnRentStash.StashAreaToQuestEvent[areaN].RentPrice
                };
                ConditionNode nCheckHouse = graph.AddNode<ConditionNode>();

                nCheckHouse.condition = new Condition_QuestEventOccured()
                {
                    QuestEventRef = new QuestEventReference { EventUID = InnRentStash.StashAreaToQuestEvent[areaN].PlayerHouseQuestEventUID }
                };
                StatementNodeExt nCheckHouseBad = graph.AddNode<StatementNodeExt>();
                nCheckHouseBad.statement = new Statement("You have a house, no need to rent a stash anymore.");
                nCheckHouseBad.SetActorName(__instance.ActorLocKey);
                ConditionNode nCheckAlready = graph.AddNode<ConditionNode>();
                nCheckAlready.condition = new Condition_QuestEventOccured()
                {
                    QuestEventRef = new QuestEventReference { EventUID = InnRentStash.StashAreaToQuestEvent[areaN].QuestEvent.EventUID }
                };
                StatementNodeExt nCheckAlreadyBad = graph.AddNode<StatementNodeExt>();
                nCheckAlreadyBad.statement = new Statement("You've already rented a stash for the week! Just use it.");
                nCheckAlreadyBad.SetActorName(__instance.ActorLocKey);

                ActionNode nWishRent = graph.AddNode<ActionNode>();
                ActionList action = new ActionList();
                action.AddAction(new NodeCanvas.Tasks.Actions.RemoveItem()
                {
                    fromCharacter = new BBParameter<Character>(character),
                    Items = new List<BBParameter<ItemReference>>() { new ItemReference { ItemID = 9000010 } },
                    Amount = new List<BBParameter<int>>() { new BBParameter<int>(InnRentStash.StashAreaToQuestEvent[areaN].RentPrice) },
                });
                action.AddAction(new NodeCanvas.Tasks.Actions.SendQuestEvent()
                {
                    QuestEventRef = new QuestEventReference { EventUID = InnRentStash.StashAreaToQuestEvent[areaN].QuestEvent.EventUID }
                });
                action.AddAction(new NodeCanvas.Tasks.Actions.PlaySound()
                {
                    Sound = GlobalAudioManager.Sounds.UI_MERCHANT_CompleteTransaction
                });
                nWishRent.action = action;

                StatementNodeExt nResultOk = graph.AddNode<StatementNodeExt>();
                nResultOk.statement = new Statement("Thanks, I've unlocked the stash for you.");
                nResultOk.SetActorName(__instance.ActorLocKey);
                StatementNodeExt nResultBad = graph.AddNode<StatementNodeExt>();
                nResultBad.statement = new Statement("Sorry, you don't have enough silver. Come back when you can afford it!");
                nResultBad.SetActorName(__instance.ActorLocKey);
                StatementNodeExt nCancel = graph.AddNode<StatementNodeExt>();
                nCancel.statement = new Statement("See you soon!");
                nCancel.SetActorName(__instance.ActorLocKey);
                FinishNode nFinish = graph.AddNode<FinishNode>();

                graph.ConnectNodes(firstNode, nCheckHouse, 1); // Check if the player owns the house of the town
                graph.ConnectNodes(nCheckHouse, nCheckHouseBad); // The player owns it --> exit
                graph.ConnectNodes(nCheckHouse, nCheckAlready); // Check if the player has already a rent ongoing in the town
                graph.ConnectNodes(nCheckAlready, nCheckAlreadyBad); // The player already has the rent --> exit
                graph.ConnectNodes(nCheckAlready, nStart); // All checks successfull, we can show the pricefor the rent
                graph.ConnectNodes(nStart, nChoose); // Show the choices for the player (to rent or not)
                graph.ConnectNodes(nChoose, nCheckMoney); // Check if the player has enough money
                graph.ConnectNodes(nChoose, nCancel); // The player doesn't want to rent --> exit
                graph.ConnectNodes(nCheckMoney, nWishRent); // The player has enough money, go activate the rent
                graph.ConnectNodes(nCheckMoney, nResultBad); // The player doesn't have enough money --> exit
                graph.ConnectNodes(nWishRent, nResultOk); // Activate the rent!

                graph.ConnectNodes(nCheckHouseBad, nFinish);
                graph.ConnectNodes(nCheckAlreadyBad, nFinish);
                graph.ConnectNodes(nResultBad, nFinish);
                graph.ConnectNodes(nResultOk, nFinish);
                graph.ConnectNodes(nCancel, nFinish);//*/

                InnRentStash.m_dialogIsSet = true;
            }
            catch (Exception ex)
            {
                InnRentStash.MyLogger.LogError(ex.Message);
            }
        }
    }
}
