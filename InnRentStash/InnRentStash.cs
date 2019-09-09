using Harmony;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.UI.Button;

namespace MoreGatherableLoot
{
    public class InnRentStash : PartialityMod
    {
        private readonly string _modName = "InnRentStash";
        private string currentArea;
        private bool dialogSet;
        public Dictionary<string, string> StashAreaToStashUID = new Dictionary<string, string>
        {
            {
                "Berg",
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                "CierzoNewTerrain",
                "ImqRiGAT80aE2WtUHfdcMw"
            },
            {
                "Levant",
                "ZbPXNsPvlUeQVJRks3zBzg"
            },
            {
                "Monsoon",
                "ImqRiGAT80aE2WtUHfdcMw"
            }
        };
        public Dictionary<string, QuestEventSignature> StashAreaToQuestEvent = new Dictionary<string, QuestEventSignature>
        {
            {
                "Berg",
                new QuestEventSignature("stash_berg")
                {
                    EventName = "Inn_Berg_StashRent",
                    IsTimedEvent = true,
                }
            },
            {
                "Levant",
                new QuestEventSignature("stash_levant")
                {
                    EventName = "Inn_Levant_StashRent",
                    IsTimedEvent = true,
                }
            },
            {
                "Monsoon",
                new QuestEventSignature("stash_monsoon")
                {
                    EventName = "Inn_Monsoon_StashRent",
                    IsTimedEvent = true,
                    //Savable = true,
                    //IsHideEvent = false,
                }
            }
        };

        public InnRentStash()
        {
            this.ModID = _modName;
            this.Version = "1.0.0";
            //this.loadPriority = 0;
            this.author = "lasyan3";
        }

        public override void Init() { base.Init(); }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

        }

        public override void OnEnable()
        {
            base.OnEnable();

            //On.ItemContainer.AddItem_1 += ItemContainer_AddItem_1;
            On.SaveInstance.ApplyEnvironment += SaveInstance_ApplyEnvironment; // Replace stashs

            /*	CharSave = new CharacterSave() --> ItemList
	            WorldSave = new WorldSave() --> QuestList, QuestEventList, KillEventSendersList
	            LegacyChestSave = new LegacyChestSave();
	            SceneSaves = new Dictionary<string, EnvironmentSave>() --> InteractionActivatorList, InteractionManagersList
            */
            //On.CharacterSave.PrepareSave += CharacterSave_PrepareSave;
            //On.CharacterSave.ApplyLoadedSaveToChar

            /*
            QuestEventManager.AddEvent(_eventUID)
                QuestEventData.CreateEventData(_eventUID)
                    QuestEventDictionary.GetQuestEvent(_uid)
                        QuestEventDictionary.RegisterEvent(QuestEventSignature _event)
                            after QuestEventDictionary.Load, call registerevent
            */
            On.QuestEventDictionary.Load += QuestEventDictionary_Load;
            On.QuestEventManager.AddEvent_1 += CancelRentOnBuyHouse;

            // TODO: manage dialogs with inn keeper
            /* DialoguePanel_RefreshMultipleChoices contains List<DialogueAnswer> (only used in DialoguePanel)
             * How to create those???
             *  Set by DialoguePanel.RefreshMultipleChoices --> MultipleChoiceRequestInfo.options
             *  Set by DialoguePanel.OnMultipleChoiceRequest
             * ...
             * NPCInteraction.m_dialogueTree = ExtTree?
             * NPCInteraction.NPCDialogue?
             * 
             * NPCInteraction.OnReceiveStartDialogueResult
             *  > _character.CharacterUI.DialoguePanel.RequestStartDialogue(NPCDialogue.DialogueController, null);
             */
            //On.DialoguePanel.AwakeInit += DialoguePanel_AwakeInit;
            //On.DialoguePanel.RefreshMultipleChoices += DialoguePanel_RefreshMultipleChoices;
            On.NPCInteraction.OnActivate += NPCInteraction_OnActivate;
            //On.DialogueAnswer.Select += DialogueAnswer_Select;
            //On.DialoguePanel.OnSelectDialogueOption += DialoguePanel_OnSelectDialogueOption;

            On.NetworkLevelLoader.JoinSequenceDone += CheckRentStatusOnLoading;
            //On.SNPC.Start += SNPC_Start;
        }

        private void SNPC_Start(On.SNPC.orig_Start orig, SNPC self)
        {
            orig(self);
            if (self.LocKey.Length > 0)
                OLogger.Log($"Start={self.LocKey}");
            if (self.LocKey == "name_unpc_monsooninnkeeper_01")
            {
            }
        }

        private void CheckRentStatusOnLoading(On.NetworkLevelLoader.orig_JoinSequenceDone orig, NetworkLevelLoader self)
        {
            OLogger.Log("CheckRentStatusOnLoading");
            orig(self);
            try
            {
                if (!StashAreaToStashUID.ContainsKey(currentArea) || CharacterManager.Instance.PlayerCharacters.Count == 0)
                {
                    return;
                }
                Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                if (stash == null)
                {
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
                }

                if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"PlayerHouse_{currentArea}_HouseAvailable") > 0)
                {
                    // House has been bought here, cancel rent event if present
                    OLogger.Log("  House bought here");
                    if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[currentArea]))
                    {
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[currentArea].EventUID);
                        stash.SetCanInteract(true);
                    }
                    return;
                }


                #region Move stash to inn
                Vector3 newPos = new Vector3(-366.3f, -1500.0f, 764.9f);
                Quaternion newRot = new Quaternion();
                switch (currentArea)
                {
                    case "Berg":
                        newPos = new Vector3(-366.3f, -1500.0f, 764.9f);
                        break;
                    //case "CierzoNewTerrain":                            break;
                    //case "Levant":                            break;
                    case "Monsoon":
                        newPos = new Vector3(-372.0f, -1500.0f, 560.7f);
                        break;
                        //default: return result;
                }
                //OLogger.Log($"X={newPos.x}, Y={newPos.y}, Z={newPos.z}");
                ItemVisual iv2 = ItemManager.GetVisuals(stash.ItemID);
                iv2.transform.SetPositionAndRotation(newPos, newRot);
                stash.OnContainerChangedOwner(character);
                stash.LinkVisuals(iv2, false);
                stash.InteractionHolder.transform.SetPositionAndRotation(newPos, newRot);
                #endregion

                stash.SetCanInteract(false);
                if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[currentArea]))
                {
                    if (QuestEventManager.Instance.CheckEventExpire(StashAreaToQuestEvent[currentArea].EventUID, 168))
                    {
                        character.CharacterUI.SmallNotificationPanel.ShowNotification($"Rent expired!", 5f);
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[currentArea].EventUID);
                        OLogger.Log("  Rent expired!");
                    }
                    else
                    {
                        stash.SetCanInteract(true);
                        OLogger.Log("  Rent still valid");
                    }
                }
                /*else // TESTING : auto rent
                {
                    if (character.Inventory.AvailableMoney >= 500)
                    {
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent started", 5f);
                        character.Inventory.AvailableMoney -= 20;
                        bool addOk = QuestEventManager.Instance.AddEvent(StashAreaToQuestEvent[currentArea]);
                        stash.SetCanInteract(true);
                        OLogger.Log($"  Rent started!");
                    }
                    else
                    {
                        character.CharacterUI.SmallNotificationPanel.ShowNotification($"Not enough money: {character.Inventory.AvailableMoney}", 5f);
                        OLogger.Log($"  Not enough money: {character.Inventory.AvailableMoney}");
                    }
                }*/
            }
            catch (Exception ex)
            {
                OLogger.Error("CheckRentStatusOnLoading:" + ex.Message);
            }
        }
        private bool CancelRentOnBuyHouse(On.QuestEventManager.orig_AddEvent_1 orig, QuestEventManager self, string _eventUID, int _stackAmount, bool _sendEvent)
        {
            bool res = orig(self, _eventUID, _stackAmount, _sendEvent);
            //OLogger.Log($"AddEvent({_eventUID})={res}");
            OLogger.Log($"CancelRentOnBuyHouse");
            try
            {
                // If event is house buying, cancel previous rent event
                if (res && StashAreaToStashUID.ContainsKey(currentArea) &&
                    QuestEventManager.Instance.GetQuestEvent(_eventUID).Name == $"PlayerHouse_{currentArea}_HouseAvailable" &&
                    QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[currentArea]))
                {
                    QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[currentArea].EventUID);
                    Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                    stash.SetCanInteract(true);
                    OLogger.Log("  Rent canceled (house bought)");
                }
            }
            catch (Exception ex)
            {
                OLogger.Log($"[{_modName}] QuestEventManager_AddEvent_1: {ex.Message}");
            }
            return res;
        }
        private void QuestEventDictionary_Load(On.QuestEventDictionary.orig_Load orig)
        {
            orig();
            try
            {
                QuestEventFamily innSection = QuestEventDictionary.Sections.FirstOrDefault(s => s.Name == "Inns");
                if (innSection != null)
                {
                    foreach (QuestEventSignature qes in StashAreaToQuestEvent.Values)
                    {
                        if (!innSection.Events.Contains(qes))
                        {
                            innSection.Events.Add(qes);
                        }
                        if (QuestEventDictionary.GetQuestEvent(qes.EventUID) == null)
                        {
                            QuestEventDictionary.RegisterEvent(qes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OLogger.Log($"[{_modName}] QuestEventDictionary_Load: {ex.Message}");
            }
        }

        private void NPCInteraction_OnActivate(On.NPCInteraction.orig_OnActivate orig, NPCInteraction self)
        {
            //OLogger.Log($"OnActivate={self.DialogueActor.actorRefKey}");
            //OLogger.Log($"OnActivate={self.DialogueActor.name}");
            try
            {
                if (self.ActorLocKey == "name_unpc_monsooninnkeeper_01" && !dialogSet)
                {
                    //OLogger.Log($"currentDialogue={self.NPCDialogue.DialogueTree.currentDialogue}");
                    //OLogger.Log($"CurrentExtDialogue={self.NPCDialogue.DialogueTree.CurrentExtDialogue}");
                    //OLogger.Log($"CurrentNodeStatus={self.NPCDialogue.DialogueTree.CurrentNodeStatus}");
                    /*OLogger.Log($"Nodes={self.NPCDialogue.DialogueController.graph.allNodes.Count}");
                    foreach (var item in self.NPCDialogue.DialogueController.graph.allNodes)
                    {
                        OLogger.Log($"{item.ID}={item.name}");
                    }*/

                    //var graphOwner = Helen.GetComponentInChildren<DialogueTreeController>(true);
                    var graphOwner = self.NPCDialogue.DialogueController;
                    var graph = (Graph)graphOwner.GetType().BaseType.GetField("_graph", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graphOwner as GraphOwner<DialogueTreeExt>);
                    var nodes = typeof(Graph).GetField("_nodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(graph as Graph) as List<Node>;
                    /*foreach (Node node in nodes)
                    {
                        //string aa = (string)AccessTools.Property(typeof(DTNode), "actorName").GetValue(node as DTNode, null);
                        //OLogger.Log($"Node {node.ID}: {node.name} ({aa})");
                        foreach (var item in node.inConnections)
                        {
                            OLogger.Log($"   From {item.sourceNode.ID}");
                        }
                        foreach (var item in node.outConnections)
                        {
                            OLogger.Log($"   To {item.targetNode.ID}");
                        }
                        if (node is StatementNodeExt statementNode)
                        {
                            OLogger.Log(" > "+statementNode.statement.text);
                        }
                        if (node is MultipleChoiceNodeExt)
                        {
                            foreach (var item in ((MultipleChoiceNodeExt)node).availableChoices)
                            {
                                OLogger.Log($" > {item.statement} {item.condition}");
                            }
                        }
                    }*/

                    ((MultipleChoiceNodeExt)nodes[2]).availableChoices.Insert(2, new MultipleChoiceNodeExt.Choice(new Statement("I want to rent a stash.")));

                    StatementNodeExt nSay = graph.AddNode<StatementNodeExt>();
                    nSay.statement = new Statement("Sure we can for 50 silver a week.");
                    nSay.SetActorName(self.ActorLocKey);
                    graph.ConnectNodes(nodes[2], nSay, 2);

                    MultipleChoiceNodeExt nConfirm = graph.AddNode<MultipleChoiceNodeExt>();
                    nConfirm.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("Sh*t up and take my money!")));
                    nConfirm.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("Forget about it.")));
                    graph.ConnectNodes(nSay, nConfirm);

                    StatementNodeExt nOk = graph.AddNode<StatementNodeExt>();
                    nOk.statement = new Statement("Here you go!");
                    nOk.SetActorName(self.ActorLocKey);
                    graph.ConnectNodes(nConfirm, nOk);
                    StatementNodeExt nCancel = graph.AddNode<StatementNodeExt>();
                    nCancel.statement = new Statement("Too bad...");
                    nCancel.SetActorName(self.ActorLocKey);
                    graph.ConnectNodes(nConfirm, nCancel);
                    FinishNode nFinish = graph.AddNode<FinishNode>();
                    graph.ConnectNodes(nOk, nFinish);
                    graph.ConnectNodes(nCancel, nFinish);

                    dialogSet = true;
                }
            }
            catch (Exception ex)
            {
                OLogger.Error("NPCInteraction_OnActivate:" + ex.Message);
            }
            //OLogger.Log($"OnActivate={self.NPCDialogue.DialogueTree.Cou}");
            orig(self);
        }

        private void DialoguePanel_OnSelectDialogueOption(On.DialoguePanel.orig_OnSelectDialogueOption orig, DialoguePanel self, int _value)
        {
            OLogger.Log($"OnSelectDialogueOption={_value}");
            orig(self, _value);
        }

        private void DialogueAnswer_Select(On.DialogueAnswer.orig_Select orig, DialogueAnswer self)
        {
            OLogger.Log($"DialogueAnswer_Select={self.index}");
            orig(self);
            return;
            try
            {
                if (self.index == 3)
                {
                    OLogger.Log("selected");
                    Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                    Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                    if (character.Inventory.AvailableMoney >= 50)
                    {
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent started", 5f);
                        character.Inventory.AvailableMoney -= 50;
                        bool addOk = QuestEventManager.Instance.AddEvent(StashAreaToQuestEvent[currentArea]);
                        stash.SetCanInteract(true);
                        OLogger.Log($"  Rent started!");
                    }
                    else
                    {
                        character.CharacterUI.SmallNotificationPanel.ShowNotification($"Not enough money: {character.Inventory.AvailableMoney}", 5f);
                        OLogger.Log($"  Not enough money: {character.Inventory.AvailableMoney}");
                    }
                    //self.CharacterUI.DialoguePanel.
                    AccessTools.Method(typeof(DialoguePanel), "RefreshMultipleChoices").Invoke(self.CharacterUI.DialoguePanel, null);
                }
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private void DialoguePanel_RefreshMultipleChoices(On.DialoguePanel.orig_RefreshMultipleChoices orig, DialoguePanel self)
        {
            OLogger.Log($"RefreshMultipleChoices");
            orig(self);
            try
            {
                if (!StashAreaToStashUID.ContainsKey(currentArea) || CharacterManager.Instance.PlayerCharacters.Count == 0)
                {
                    return;
                }
                Item stash = ItemManager.Instance.GetItem(StashAreaToStashUID[currentArea]);
                if (stash == null)
                {
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
                }

                MultipleChoiceRequestInfo m_currentOptions = (MultipleChoiceRequestInfo)AccessTools.Field(typeof(DialoguePanel), "m_currentOptions").GetValue(self);
                if (m_currentOptions == null)
                {
                    return;
                }
                /*foreach (var item in m_currentOptions.options)
                {
                    OLogger.Log($"  {item.Value}: {item.Key.meta}");
                    if (item.Key.meta == "Merchant_Generic_Inn_01")
                    {
                        dialogOrder = 2;
                    }
                }
                if (m_currentOptions.options.Count > 0 && m_currentOptions.options.Value.meta == "Merchant_Generic_Inn_01")
                {
                    dialogOrder = 2;
                }*/
                if (m_currentOptions.options.Count == 0)
                {
                    return;
                }

                List<DialogueAnswer> m_dialogueOptions = (List<DialogueAnswer>)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptions").GetValue(self);
                RectTransform m_dialogueOptionHolder = (RectTransform)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptionHolder").GetValue(self);
                int num = m_dialogueOptions.Count;
                foreach (var item in m_dialogueOptions)
                {
                    OLogger.Log($"  {item.dialogueChoiceIndex}: {item.text}");
                }

            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private void DialoguePanel_AwakeInit(On.DialoguePanel.orig_AwakeInit orig, DialoguePanel self)
        {
            OLogger.Log($"DialoguePanel_AwakeInit");
            orig(self);
            try
            {
                /*List<DialogueAnswer> m_dialogueOptions = (List<DialogueAnswer>)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptions").GetValue(self);
                RectTransform m_dialogueOptionHolder = (RectTransform)AccessTools.Field(typeof(DialoguePanel), "m_dialogueOptionHolder").GetValue(self);
                int num = m_dialogueOptions.Count;
                OLogger.Log($"{num}");
                DialogueAnswer dialogueAnswer = UnityEngine.Object.Instantiate(m_dialogueOptions[0]);
                dialogueAnswer.transform.SetParent(m_dialogueOptionHolder);
                dialogueAnswer.transform.ResetLocal();
                dialogueAnswer.name = "itm" + num.ToString();
                m_dialogueOptions.Add(dialogueAnswer);

                m_dialogueOptions[num].index = num + 1;
                m_dialogueOptions[num].dialogueChoiceIndex = 4;
                m_dialogueOptions[num].text = "coucou";*/
                /*m_dialogueOptions[num].onClick.RemoveAllListeners();
                int value = m_dialogueOptions[num].dialogueChoiceIndex;
                m_dialogueOptions[num].onClick.AddListener(delegate
                {
                    OnSelectDialogueOption(value);
                });*/
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
        }

        private bool SaveInstance_ApplyEnvironment(On.SaveInstance.orig_ApplyEnvironment orig, SaveInstance self, string _areaName)
        {
            //OLogger.Log($"SaveInstance_ApplyEnvironment={_areaName}");
            bool result = orig(self, _areaName);
            try
            {
                currentArea = _areaName;
                dialogSet = false;

                #region TODO : Thieves in town!
                // Parcourir tous les objets appartenant au joueur dans la ville
                /*for (int i = 0; i < ItemManager.Instance.WorldItems.Count; i++)
                {
                    string uid = ItemManager.Instance.WorldItems.Keys[i];
                    Item it = ItemManager.Instance.WorldItems.Values[i];
                    if (it.OwnerCharacter != null && it.OwnerCharacter.IsLocalPlayer && !it.IsEquipped &&
                        it.ParentContainer != it.OwnerCharacter.Inventory.EquippedBag && isinworld?
                        it.ParentContainer != it.OwnerCharacter.Inventory.Pouch)
                    {
                        OLogger.Log($"Delete {it.Name}");
                        ItemManager.Instance.DestroyItem(uid);
                    }
                }*/
                #endregion

            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }

            return result;
        }

        private string GameTimetoDays(double p_gametime)
        {
            double _realtime = p_gametime + 24 + EnvironmentConditions.Instance.TimeOfDay;
            string str = "";
            int days = (int)p_gametime / 24;
            if (days > 0) str = $"{days}d, ";
            int hours = (int)p_gametime % 24;
            str += $"{hours}h";
            return str;
        }
    }
}
