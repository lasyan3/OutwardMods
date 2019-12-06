using Harmony;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Conditions;
//using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static CustomKeybindings;

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
    public class InnRentStash : PartialityMod
    {
        private readonly string m_modName = "InnRentStash";
        private string m_currentArea;
        private bool m_dialogIsSet;
        private readonly bool m_debugLog = false;
        private TreasureChest m_currentStash;
        private bool m_isStashSharing = true;
        private readonly Dictionary<string, string> StashAreaToStashUID = new Dictionary<string, string>
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
        private readonly Dictionary<string, StrRent> StashAreaToQuestEvent = new Dictionary<string, StrRent>
        {
            {
                "Berg",
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
                "Levant",
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
                    }
                }
           },
            {
                "Monsoon",
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
                    }
                }
           }
        };

        public InnRentStash()
        {
            this.ModID = m_modName;
            this.Version = "1.0.0";
            //this.loadPriority = 0;
            this.author = "lasyan3";
        }

        public override void Init() { base.Init(); }

        public override void OnLoad() { base.OnLoad(); }

        public override void OnDisable()
        {
            base.OnDisable();

            On.SaveInstance.ApplyEnvironment -= SaveInstance_ApplyEnvironment;
            On.QuestEventDictionary.Load -= QuestEventDictionary_Load;
            On.QuestEventManager.AddEvent_1 -= AddEvent;
            On.NPCInteraction.OnActivate -= NPCInteraction_OnActivate;
            On.NetworkLevelLoader.UnPauseGameplay -= NetworkLevelLoader_UnPauseGameplay;

            On.CraftingMenu.GetPlayersOwnItems -= CraftingMenu_GetPlayersOwnItems;
            On.CraftingMenu.GetPlayersOwnItems_1 -= CraftingMenu_GetPlayersOwnItems_1;
            On.RecipeDisplay.SetReferencedRecipe -= RecipeDisplay_SetReferencedRecipe;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            try
            {
                On.SaveInstance.ApplyEnvironment += SaveInstance_ApplyEnvironment; // Replace stashs
                On.QuestEventDictionary.Load += QuestEventDictionary_Load;
                On.QuestEventManager.AddEvent_1 += AddEvent;
                On.NPCInteraction.OnActivate += NPCInteraction_OnActivate;
                On.NetworkLevelLoader.UnPauseGameplay += NetworkLevelLoader_UnPauseGameplay;

                On.CraftingMenu.GetPlayersOwnItems += CraftingMenu_GetPlayersOwnItems; // Get ingredients from the stash (by ItemID)
                On.CraftingMenu.GetPlayersOwnItems_1 += CraftingMenu_GetPlayersOwnItems_1; // Get ingredients from the stash (by tag)

                On.RecipeDisplay.SetReferencedRecipe += RecipeDisplay_SetReferencedRecipe; // Show quantity of owned objects in recipes' name

                On.LocalCharacterControl.UpdateInteraction += LocalCharacterControl_UpdateInteraction;
                CustomKeybindings.AddAction("StashSharing", KeybindingsCategory.Actions, ControlType.Both, 5);
            }
            catch (Exception ex)
            {
                DoOloggerError(ex.Message);
                Debug.Log($"[{m_modName}] OnEnable: {ex.Message}");
            }
        }


        private void ItemDisplay_UpdateQuantityDisplay(On.ItemDisplay.orig_UpdateQuantityDisplay orig, ItemDisplay self)
        {
            try
            {
                if (self.RefItem != null)
                {
                    DoOloggerLog($"UpdateQuantityDisplay={self.RefItem.Name}");
                    Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(self);
                    Transform transform = self.transform.Find("lblQuantity");
                    if (m_lblQuantity == null && transform == null)
                    {
                        DoOloggerLog($"fuck");
                        Text t = self.transform.GetOrAddComponent<Text>();
                        t.name = "lblQuantity";
                        DoOloggerLog($"Try={self.transform.Find("lblQuantity") != null}");
                        //t.text = "coucou";
                        //m_lblQuantity = transform.GetComponent<Text>();
                        //m_lblQuantity.text = string.Empty;
                    }
                    /*	transform = base.transform.Find("lblQuantity");
		                m_lblQuantity = transform.GetComponent<Text>();
		                m_lblQuantity.text = string.Empty;
                    */
                    if (m_lblQuantity != null)
                    {
                        m_lblQuantity.text = "5";
                        DoOloggerLog($"ok");
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                DoOloggerError(ex.Message);
                Debug.Log($"[{m_modName}] OnEnable: {ex.Message}");
            }
            orig(self);
        }

        private void CraftingMenu_OnRecipeSelected(On.CraftingMenu.orig_OnRecipeSelected orig, CraftingMenu self, int _index, bool _forceRefresh)
        {
            orig(self, _index, _forceRefresh);
            try
            {
                IngredientSelector[] ing = (IngredientSelector[])AccessTools.Field(typeof(CraftingMenu), "m_ingredientSelectors").GetValue(self);
                if (ing.Length > 0)
                {
                    /*DoOloggerLog($"IngredientSelector={ing.Length}");
                    ItemDisplay m_itd = (ItemDisplay)AccessTools.Field(typeof(IngredientSelector), "m_itemDisplay").GetValue(ing[0]);
                    DoOloggerLog($"{m_itd.RefItem.Name}");
                    Text m_lblQuantity = (Text)AccessTools.Field(typeof(ItemDisplay), "m_lblQuantity").GetValue(m_itd);
                    Text m_txtAnyIngredient = (Text)AccessTools.Field(typeof(IngredientSelector), "m_txtAnyIngredient").GetValue(ing[0]);
                    if (m_lblQuantity != null)
                    {
                        m_lblQuantity.text = "5";
                        DoOloggerLog($"ok");
                    }
                    if (m_txtAnyIngredient != null)
                    {
                        m_txtAnyIngredient.text = "5";
                        DoOloggerLog($"ok2");
                    }*/
                    //ing[0].SelectedIngredient.MaxStackAmount
                    //ing[0].
                }
            }
            catch (Exception ex)
            {
                DoOloggerError(ex.Message);
                Debug.Log($"[{m_modName}] CraftingMenu_OnRecipeSelected: {ex.Message}");
            }
        }

        private void CraftingMenu_GetPlayersOwnItems_1(On.CraftingMenu.orig_GetPlayersOwnItems_1 orig, CraftingMenu self, ref List<Item> _list, Tag _tag)
        {
            orig(self, ref _list, _tag);
            try
            {
                if (m_currentStash == null || !m_currentStash.IsInteractable || !m_isStashSharing)
                {
                    return;
                }
                _list.AddRange(m_currentStash.GetItemsFromTag(_tag));
            }
            catch (Exception ex)
            {
                DoOloggerError($"CraftingMenu_GetPlayersOwnItems_1: {ex.Message}");
                Debug.Log($"[{m_modName}] CraftingMenu_GetPlayersOwnItems_1: {ex.Message}");
            }
        }

        private void RecipeDisplay_SetReferencedRecipe(On.RecipeDisplay.orig_SetReferencedRecipe orig, RecipeDisplay self, Recipe _recipe, bool _canBeCompleted, IList<Item>[] _compatibleIngredients, IList<Item> _ingredients)
        {
            orig(self, _recipe, _canBeCompleted, _compatibleIngredients, _ingredients);
            try
            {
                if (_recipe.Results.Length == 0)
                {
                    return;
                }
                Text m_lblRecipeName = (Text)AccessTools.Field(typeof(RecipeDisplay), "m_lblRecipeName").GetValue(self);
                int invQty = self.LocalCharacter.Inventory.GetOwnedItems(_recipe.Results[0].Item.ItemID).Count;
                int stashQty = 0;
                if (m_currentStash != null && m_currentStash.IsInteractable)
                {
                    stashQty = m_currentStash.GetItemsFromID(_recipe.Results[0].Item.ItemID).Count;
                }
                self.SetName(m_lblRecipeName.text += $" ({invQty + stashQty})");
            }
            catch (Exception ex)
            {
                DoOloggerError($"RecipeDisplay_SetReferencedRecipe: {ex.Message}");
                Debug.Log($"[{m_modName}] RecipeDisplay_SetReferencedRecipe: {ex.Message}");
            }
        }

        private void CraftingMenu_GetPlayersOwnItems(On.CraftingMenu.orig_GetPlayersOwnItems orig, CraftingMenu self, ref List<Item> _list, int _itemID)
        {
            orig(self, ref _list, _itemID);
            try
            {
                if (m_currentStash == null || !m_currentStash.IsInteractable || !m_isStashSharing)
                {
                    return;
                }
                //DoLog($"CraftingMenu={m_currentStash.IsInteractable}");
                if (_itemID == 4000050)
                    DoOloggerLog($"{_itemID}={m_currentStash.GetItemsFromID(_itemID).Count}");
                _list.AddRange(m_currentStash.GetItemsFromID(_itemID));
            }
            catch (Exception ex)
            {
                DoOloggerError($"CraftingMenu_GetPlayersOwnItems: {ex.Message}");
                Debug.Log($"[{m_modName}] CraftingMenu_GetPlayersOwnItems: {ex.Message}");
            }
        }

        private void NetworkLevelLoader_UnPauseGameplay(On.NetworkLevelLoader.orig_UnPauseGameplay orig, NetworkLevelLoader self, string _identifier)
        {
            orig(self, _identifier);
            try
            {
                DoOloggerLog($"UnPauseGameplay={_identifier}");
                if (_identifier == "Loading")
                {
                    CheckRentStatus();
                }
            }
            catch (Exception ex)
            {
                DoOloggerError($"NetworkLevelLoader_UnPauseGameplay: {ex.Message}");
                Debug.Log($"[{m_modName}] NetworkLevelLoader_UnPauseGameplay: {ex.Message}");
            }
        }

        private bool AddEvent(On.QuestEventManager.orig_AddEvent_1 orig, QuestEventManager self, string _eventUID, int _stackAmount, bool _sendEvent)
        {
            bool res = orig(self, _eventUID, _stackAmount, _sendEvent);
            DoOloggerLog($"AddEvent({_eventUID})={res}");
            if (!res) return res;
            //DoLog($"CancelRentOnBuyHouse");
            try
            {
                if (string.IsNullOrEmpty(m_currentArea)) return res;
                if (!StashAreaToQuestEvent.ContainsKey(m_currentArea)) return res;
                if (m_currentStash == null) return res;
                // If event is house buying, cancel previous rent event
                /*if (QuestEventManager.Instance.GetQuestEvent(_eventUID).Name == $"PlayerHouse_{m_currentArea}_HouseAvailable" &&
                    QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent))
                {
                    QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID);
                    m_currentStash.SetCanInteract(true);
                    //character.CharacterUI.SmallNotificationPanel.ShowNotification($"Rent canceled", 5f);
                    //DoLog("  Rent canceled (house bought)");
                }*/
                // If event is rent, activate the stash
                if (_eventUID == StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID)
                {
                    m_currentStash.SetCanInteract(true);
                    DoOloggerLog("Activate stash");
                }
            }
            catch (Exception ex)
            {
                DoOloggerError($"[{m_modName}] AddEvent: {ex.Message}");
                Debug.Log($"[{m_modName}] AddEvent: {ex.Message}");
            }
            return res;
        }
        private void QuestEventDictionary_Load(On.QuestEventDictionary.orig_Load orig)
        {
            orig();
            try
            {
                DoOloggerLog("QuestEventDictionary_Load");

                // Get PlayerHouse Event UIDs
                foreach (var sect in QuestEventDictionary.Sections)
                {
                    foreach (var qevt in sect.Events)
                    {
                        foreach (var dicStash in StashAreaToQuestEvent)
                        {
                            if (qevt.EventName == $"PlayerHouse_{dicStash.Key}_HouseAvailable")
                            {
                                StashAreaToQuestEvent[dicStash.Key].PlayerHouseQuestEventUID = qevt.EventUID;
                                DoOloggerLog($" > {dicStash.Key}={qevt.EventUID}");
                            }
                        }
                    }
                }

                // Add Rent Events
                QuestEventFamily innSection = QuestEventDictionary.Sections.FirstOrDefault(s => s.Name == "Inns");
                if (innSection != null)
                {
                    foreach (StrRent item in StashAreaToQuestEvent.Values)
                    {
                        if (!innSection.Events.Contains(item.QuestEvent))
                        {
                            innSection.Events.Add(item.QuestEvent);
                        }
                        if (QuestEventDictionary.GetQuestEvent(item.QuestEvent.EventUID) == null)
                        {
                            QuestEventDictionary.RegisterEvent(item.QuestEvent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DoOloggerError($"[{m_modName}] QuestEventDictionary_Load: {ex.Message}");
                Debug.Log($"[{m_modName}] QuestEventDictionary_Load: {ex.Message}");
            }
        }

        private void NPCInteraction_OnActivate(On.NPCInteraction.orig_OnActivate orig, NPCInteraction self)
        {
            orig(self);
            try
            {
                if (!StashAreaToQuestEvent.ContainsKey(m_currentArea) || CharacterManager.Instance.PlayerCharacters.Count == 0)
                {
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    return;
                }
                if (self.ActorLocKey != StashAreaToQuestEvent[m_currentArea].NpcName || m_dialogIsSet)
                {
                    return;
                }
                var graphOwner = self.NPCDialogue.DialogueController;
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
                nStart.statement = new Statement($"Of course! Renting a stash costs only {StashAreaToQuestEvent[m_currentArea].RentPrice} silver for one week.");
                nStart.SetActorName(self.ActorLocKey);
                MultipleChoiceNodeExt nChoose = graph.AddNode<MultipleChoiceNodeExt>();
                nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("Sh*t up and take my money!")));
                nChoose.availableChoices.Add(new MultipleChoiceNodeExt.Choice(new Statement("I'll be back.")));
                ConditionNode nCheckMoney = graph.AddNode<ConditionNode>();
                nCheckMoney.condition = new Condition_OwnsItem()
                {
                    character = character,
                    item = new ItemReference { ItemID = 9000010 },
                    minAmount = StashAreaToQuestEvent[m_currentArea].RentPrice
                };
                ConditionNode nCheckHouse = graph.AddNode<ConditionNode>();

                nCheckHouse.condition = new Condition_QuestEventOccured()
                {
                    QuestEventRef = new QuestEventReference { EventUID = StashAreaToQuestEvent[m_currentArea].PlayerHouseQuestEventUID }
                };
                StatementNodeExt nCheckHouseBad = graph.AddNode<StatementNodeExt>();
                nCheckHouseBad.statement = new Statement("You have a house, no need to rent a stash anymore.");
                nCheckHouseBad.SetActorName(self.ActorLocKey);
                ConditionNode nCheckAlready = graph.AddNode<ConditionNode>();
                nCheckAlready.condition = new Condition_QuestEventOccured()
                {
                    QuestEventRef = new QuestEventReference { EventUID = StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID }
                };
                StatementNodeExt nCheckAlreadyBad = graph.AddNode<StatementNodeExt>();
                nCheckAlreadyBad.statement = new Statement("You've already rented a stash for the week! Just use it.");
                nCheckAlreadyBad.SetActorName(self.ActorLocKey);

                ActionNode nWishRent = graph.AddNode<ActionNode>();
                ActionList action = new ActionList();
                action.AddAction(new NodeCanvas.Tasks.Actions.RemoveItem()
                {
                    fromCharacter = new BBParameter<Character>(character),
                    Items = new List<BBParameter<ItemReference>>() { new ItemReference { ItemID = 9000010 } },
                    Amount = new List<BBParameter<int>>() { new BBParameter<int>(StashAreaToQuestEvent[m_currentArea].RentPrice) },
                });
                action.AddAction(new NodeCanvas.Tasks.Actions.SendQuestEvent()
                {
                    QuestEventRef = new QuestEventReference { EventUID = StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID }
                });
                action.AddAction(new NodeCanvas.Tasks.Actions.PlaySound()
                {
                    Sound = GlobalAudioManager.Sounds.UI_MERCHANT_CompleteTransaction
                });
                nWishRent.action = action;

                StatementNodeExt nResultOk = graph.AddNode<StatementNodeExt>();
                nResultOk.statement = new Statement("Thanks, I've unlocked the stash for you.");
                nResultOk.SetActorName(self.ActorLocKey);
                StatementNodeExt nResultBad = graph.AddNode<StatementNodeExt>();
                nResultBad.statement = new Statement("Sorry, you don't have enough silver. Come back when you can afford it!");
                nResultBad.SetActorName(self.ActorLocKey);
                StatementNodeExt nCancel = graph.AddNode<StatementNodeExt>();
                nCancel.statement = new Statement("See you soon!");
                nCancel.SetActorName(self.ActorLocKey);
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

                m_dialogIsSet = true;
            }
            catch (Exception ex)
            {
                DoOloggerError("NPCInteraction_OnActivate:" + ex.Message);
                Debug.Log($"[{m_modName}] NPCInteraction_OnActivate: {ex.Message}");
            }
        }

        /// <summary>
        /// This event is called each time the environment changes (aka the player as a loading screen).
        /// I use this to save the name of the current area, and to reset the flag indicating the dialog for
        /// the innkeepers needs to be reset.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="_areaName"></param>
        /// <returns></returns>
        private bool SaveInstance_ApplyEnvironment(On.SaveInstance.orig_ApplyEnvironment orig, SaveInstance self, string _areaName)
        {
            DoOloggerLog($"Area={_areaName}");
            bool result = orig(self, _areaName);
            try
            {
                m_currentArea = _areaName;
                m_dialogIsSet = false;

                #region TODO : Thieves in town!
                // Parcourir tous les objets appartenant au joueur dans la ville
                /*for (int i = 0; i < ItemManager.Instance.WorldItems.Count; i++)
                {
                    string uid = ItemManager.Instance.WorldItems.Keys[i];
                    Item it = ItemManager.Instance.WorldItems.Values[i];
                    if (it.OwnerCharacter != null && it.OwnerCharacter.IsLocalPlayer && !it.IsEquipped &&
                        /*it.ParentContainer != it.OwnerCharacter.Inventory.EquippedBag &&* it.IsInWorld
                        /*it.ParentContainer != it.OwnerCharacter.Inventory.Pouch*)
                    {
                        DoLog($"Delete {it.Name}");
                        ItemManager.Instance.DestroyItem(uid);
                    }
                }*/
                #endregion

            }
            catch (Exception ex)
            {
                DoOloggerError($"[SaveInstance_ApplyEnvironment: {ex.Message}");
                Debug.Log($"[{m_modName}] SaveInstance_ApplyEnvironment: {ex.Message}");
            }

            return result;
        }

        private void LocalCharacterControl_UpdateInteraction(On.LocalCharacterControl.orig_UpdateInteraction orig, LocalCharacterControl self)
        {
            orig(self);
            if (self.InputLocked)
            {
                return;
            }

            try
            {
                UID charUID = self.Character.UID;
                int playerID = self.Character.OwnerPlayerSys.PlayerID;

                if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("StashSharing"))
                {
                    m_isStashSharing = !m_isStashSharing;
                    if (m_isStashSharing)
                    {
                        self.Character.CharacterUI.SmallNotificationPanel.ShowNotification("Sharing enabled", 2f);
                    }
                    else
                    {
                        self.Character.CharacterUI.SmallNotificationPanel.ShowNotification("Sharing disabled", 2f);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[{m_modName}] CheckRentStatus: {ex.Message}");
            }
        }

        private void CheckRentStatus()
        {
            try
            {
                DoOloggerLog($"CheckRentStatus");
                if (CharacterManager.Instance.PlayerCharacters.Count == 0)
                {
                    DoOloggerLog($" > PlayerCharacters=0");
                    return;
                }
                if (CharacterManager.Instance.PlayerCharacters.Values.Count == 0)
                {
                    DoOloggerLog($" > PlayerCharacters.Values=0");
                    return;
                }
                Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[0]);
                if (character == null)
                {
                    DoOloggerLog($" > NullCharacter");
                    return;
                }
                m_currentStash = null;
                if (!StashAreaToStashUID.ContainsKey(m_currentArea) || CharacterManager.Instance.PlayerCharacters.Count == 0)
                {
                    DoOloggerLog($" > UnkStash");
                    return;
                }
                m_currentStash = (TreasureChest)ItemManager.Instance.GetItem(StashAreaToStashUID[m_currentArea]);
                if (m_currentStash == null)
                {
                    DoOloggerLog($" > NullStash");
                    return;
                }

                // If we are in Cierzo, check quest failure
                if (m_currentArea == "CierzoNewTerrain")
                {
                    if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"General_NotLighthouseOwner") > 0)
                    {
                        m_currentStash = null;
                    }
                    return;
                }

                // Disable generation content for house stash
                AccessTools.Field(typeof(TreasureChest), "m_hasGeneratedContent").SetValue(m_currentStash, true);

                if (QuestEventManager.Instance.CurrentQuestEvents.Count(q => q.Name == $"PlayerHouse_{m_currentArea}_HouseAvailable") > 0)
                {
                    // House has been bought here, cancel rent event if present
                    DoOloggerLog(" > House bought here");
                    if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent))
                    {
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID);
                        //m_currentStash.SetCanInteract(true);
                    }
                    return;
                }


                #region Move stash to inn
                Vector3 newPos = StashAreaToQuestEvent[m_currentArea].StashPosition;
                Quaternion newRot = StashAreaToQuestEvent[m_currentArea].StashRotation;
                //ItemVisual iv2 = ItemManager.GetVisuals(m_currentStash.ItemID);
                //ItemVisual iv2 = ItemManager.GetVisuals(m_currentStash);
                Transform transform = UnityEngine.Object.Instantiate(m_currentStash.VisualPrefab);
                ItemVisual iv2 = transform.GetComponent<ItemVisual>();
                iv2.ItemID = m_currentStash.ItemID;
                DoOloggerLog($"Item {m_currentStash.UID}={(m_currentStash as ItemContainer).ItemCount}");
                iv2.transform.SetPositionAndRotation(newPos, newRot);
                m_currentStash.OnContainerChangedOwner(character);
                m_currentStash.LinkVisuals(iv2, false);
                m_currentStash.InteractionHolder.transform.SetPositionAndRotation(newPos, newRot);
                #endregion

                m_currentStash.SetCanInteract(false);
                if (QuestEventManager.Instance.HasQuestEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent))
                {
                    if (QuestEventManager.Instance.CheckEventExpire(StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID, StashAreaToQuestEvent[m_currentArea].RentDuration))
                    {
                        //m_notification = $"Rent has expired!";
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent has expired!", 8f);
                        QuestEventManager.Instance.RemoveEvent(StashAreaToQuestEvent[m_currentArea].QuestEvent.EventUID);
                        DoOloggerLog(" > Rent expired!");
                    }
                    else
                    {
                        //m_notification = "Rent ongoing";
                        character.CharacterUI.SmallNotificationPanel.ShowNotification("Rent ongoing", 8f);
                        m_currentStash.SetCanInteract(true);
                        DoOloggerLog(" > Rent still valid");
                    }
                }
                else
                {
                    DoOloggerLog($" > NoQuestEvent");
                }

            }
            catch (Exception ex)
            {
                DoOloggerError("CheckRentStatus:" + ex.Message);
                Debug.Log($"[{m_modName}] CheckRentStatus: {ex.Message}");
            }
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

        private void DoOloggerLog(string p_message)
        {
            if (m_debugLog)
            {
                //OLogger.Log(p_message);
            }
        }
        private void DoOloggerError(string p_message)
        {
            if (m_debugLog)
            {
                //OLogger.Error(p_message);
            }
        }
    }
}
