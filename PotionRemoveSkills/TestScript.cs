using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wip
{
    public class TestScript : MonoBehaviour
    {
        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                GenerateSimpleNPC(CharacterManager.Instance.GetFirstLocalCharacter().MiddlePos);
            }
        }

        public GameObject GenerateSimpleNPC(Vector3 position)
        {
            var panel = Resources.FindObjectsOfTypeAll<CharacterCreationPanel>()[0];

            // OR var panel = CharacterManager.Instance.GetFirstLocalCharacter().CharacterUI.MainMenu.CurrentCharacterCreationPanel;

            /*GameObject prefab = AccessTools.Field(typeof(CharacterCreationPanel), "CharacterCreationPrefab").GetValue(panel) as GameObject;

            if (prefab && prefab.transform.Find("HumanSNPC") is Transform npc)
            {
                Transform npc2 = Instantiate(npc);

                // move npc to player
                npc2.position = position;
                return npc2.gameObject;
            }*/
            try
            {
                //AccessTools.Method(typeof(CharacterCreationPanel), "InitHumanSNPC").Invoke(panel, null);
                //panel.OnRandomize();
                GameObject prefab = AccessTools.Field(typeof(CharacterCreationPanel), "CharacterCreationPrefab").GetValue(panel) as GameObject;
                if (prefab && prefab.transform.Find("HumanSNPC") is Transform npc)
                {
                    //npc.position = position;
                    //SNPC testNC = npc.GetOrAddComponent<SNPC>();

                    // _SNPC/CharactersToDesactivate/_Immobile
                    GameObject gameObject = GameObject.Find("_SNPC");
                    if (gameObject != null)
                    {
                        //OLogger.Log("_SNPC", "ffffffff", "wip");
                        Transform t1 = gameObject.transform.Find("CharactersToDesactivate");
                        if (t1 != null)
                        {
                            //OLogger.Log("CharactersToDesactivate", "ffffffff", "wip");
                            Transform t2 = t1.transform.Find("_Immobile");
                            if (t2 != null)
                            {
                                //OLogger.Log("_Immobile", "ffffffff", "wip");
                                //testNC = t2.GetOrAddComponent<SNPC>();
                                Transform npc2 = Instantiate(npc);
                                npc2.transform.parent = t2;
                                npc2.name = "HumanSNPC_lasyan3";
                                npc2.position = position;

                                #region BasicInteraction
                                /*GameObject gameObject2 = new GameObject("Interaction_Holder");
                                gameObject2.transform.parent = npc2.transform;
                                gameObject2.transform.position = position;
                                InteractionTriggerBase interactionTriggerBase = gameObject2.AddComponent<InteractionTriggerBase>();
                                InteractionActivator interactionActivator = gameObject2.AddComponent<InteractionActivator>();
                                InteractionBase interaction = gameObject2.AddComponent<InteractionBase>();
                                interactionTriggerBase.GenerateColliderIfNone = true;
                                interactionTriggerBase.IsLargeTrigger = true;
                                interactionTriggerBase.DetectionPriority = -9999;
                                interactionTriggerBase.SetActivator(interactionActivator);
                                AccessTools.Field(typeof(InteractionActivator), "m_overrideBasicText").SetValue(interactionActivator, "Rest at <color=#fc4e03>Bonfire</color>");
                                AccessTools.Field(typeof(InteractionActivator), "m_sceneBasicInteraction").SetValue(interactionActivator, interaction);
                                interaction.OnActivationEvent = testEvent;//*/
                                #endregion

                                #region NPCInteraction
                                // InteractionTriggerBase   --> _SNPC/CharactersToDesactivate/_UNPC/Elinarasaved/UNPC_EllinaraA/Dialogue_EllinaraA/InteractionAccessPoint_pO7WB - InteractionTriggerBase
                                // InteractionActivator     --> _SNPC/CharactersToDesactivate/_UNPC/Elinarasaved/UNPC_EllinaraA/Dialogue_EllinaraA/NPC/InteractionActivatorSettings - Component
                                // NPCInteraction           --> _SNPC/CharactersToDesactivate/_UNPC/Elinarasaved/UNPC_EllinaraA/Dialogue_EllinaraA/NPC/InteractionActivatorSettings - Component
                                GameObject gameObject2 = new GameObject("Interaction_Holder");
                                gameObject2.transform.parent = npc2.transform;
                                gameObject2.transform.position = position;

                                Transform InteractionAccessPoint = FindPath("_SNPC", "CharactersToDesactivate/_UNPC/Elinarasaved/UNPC_EllinaraA/Dialogue_EllinaraA/InteractionAccessPoint_pO7WB");
                                InteractionTriggerBase interactionTriggerBase = InteractionAccessPoint.GetComponent<InteractionTriggerBase>(); // gameObject2.AddComponent<InteractionTriggerBase>();
                                interactionTriggerBase.transform.parent = gameObject2.transform;
                                Transform InteractionActivatorSettings = FindPath("_SNPC", "CharactersToDesactivate/_UNPC/Elinarasaved/UNPC_EllinaraA/Dialogue_EllinaraA/NPC/InteractionActivatorSettings");
                                InteractionActivator interactionActivator = InteractionActivatorSettings.GetComponent<InteractionActivator>(); // gameObject2.AddComponent<InteractionActivator>();
                                interactionActivator.transform.parent = gameObject2.transform;
                                NPCInteraction interaction = InteractionActivatorSettings.GetComponent<NPCInteraction>(); // gameObject2.AddComponent<NPCInteraction>();
                                interaction.transform.parent = gameObject2.transform;
                                // DialogueActor
                                // m_dialogueTree --> NPCDialogue.DialogueTree
                                // m_character --> DialogueActor.gameObject.GetComponent<Character>()
                                // m_dialogueActorLocalize --> DialogueActor.GetComponent<DialogueActorLocalize>()
                                interactionTriggerBase.GenerateColliderIfNone = true;
                                interactionTriggerBase.IsLargeTrigger = true;
                                interactionTriggerBase.DetectionPriority = -9999;
                                interactionTriggerBase.SetActivator(interactionActivator);
                                AccessTools.Field(typeof(InteractionActivator), "m_overrideBasicText").SetValue(interactionActivator, "Rest at <color=#fc4e03>Bonfire</color>");
                                AccessTools.Field(typeof(InteractionActivator), "m_sceneBasicInteraction").SetValue(interactionActivator, interaction);
                                interaction.OnActivationEvent = testEvent;//*/
                                #endregion
                                OLogger.Log("DONE");
                            }
                        }
                    }

                    //OLogger.Log($"SNPC={testNC.UID}");
                    //AccessTools.Method(typeof(SNPC), "Start").Invoke(testNC, null);
                    //CharacterVisuals cv = AccessTools.Field(typeof(SNPC), "m_visuals").GetValue(testNC) as CharacterVisuals;
                    //OLogger.Log($"visuals={cv == null}");
                    //testNC.transform.position = position;

                    //testNC.StartTalking(panel.pl
                    /*OLogger.Log($"CharacterVisuals");
                    CharacterVisuals cv = AccessTools.Field(typeof(CharacterCreationPanel), "m_characterVisuals").GetValue(panel) as CharacterVisuals;
                    OLogger.Log($"testSN");
                    SNPC testSN = new SNPC();
                    OLogger.Log($"m_visuals");
                    AccessTools.Field(typeof(SNPC), "m_visuals").SetValue(testSN, cv);
                    OLogger.Log($"Generate");
                    //testSN.SetUID(UID.Generate());
                    Transform npc2 = Instantiate(npc);
                    //OLogger.Log($"transform={testSN.transform}");
                    npc2.transform.position = position;*/

                    return npc.gameObject;
                }
            }
            catch (Exception ex)
            {
                OLogger.Error(ex.Message);
            }
            return null;
        }
        public void testEvent()
        {
            OLogger.Log("interaction called!");
        }

        public Transform FindPath(string root, string path)
        {
            Transform trans = null;
            GameObject gameObject = GameObject.Find(root);
            if (gameObject == null)
            {
                return trans;
            }
            foreach (string p in path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (trans == null)
                {
                    trans = gameObject.transform.Find(p);
                }
                else
                {
                    trans = trans.transform.Find(p);
                }
                if (trans == null)
                {
                    OLogger.Error($"{p} is null!");
                }
            }
            return trans;
        }
    }
}
