using BepInEx;
using BepInEx.Logging;
using SideLoader;
using System;

namespace SaveSystemReworked
{

    [BepInPlugin(ID, NAME, VERSION)]
    public class SaveSystemReworked : BaseUnityPlugin
    {
        const string ID = "fr.lasyan3.savesystemreworked";
        const string NAME = "SaveSystemReworked";
        const string VERSION = "1.0.0";

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);
        public static bool IsQuickSave = false;

        internal void Awake()
        {
            try
            {
                var harmony = new HarmonyLib.Harmony(ID);
                harmony.PatchAll();

                CustomKeybindings.AddAction("QuickSave", KeybindingsCategory.CustomKeybindings, ControlType.Keyboard);
                //CustomKeybindings.AddAction("QuickLoad", KeybindingsCategory.Actions, ControlType.Both, 5);

                MyLogger.LogDebug("Awaken");
            }
            catch (Exception ex)
            {
                MyLogger.LogError(ex.Message);
            }
        }

        /*
         * NetworkLevelLoader
         * 
         * CurrentSaveInstance
         *   CharacterSaveInstanceHolder.ApplyLoadedSaveToChar
         *     SaveManager.LocalCharStarted
         *       NetworkLevelLoader.LocalCharStarted
         *         Character.ProcessInit
         *   CharacterSaveInstanceHolder.ApplyEnvironment
         *     SaveManager.LoadEnvironment
         *       NetworkLevelLoader.MidLoadLevel
         *   CharacterSaveInstanceHolder.ApplyWorld
         *     SaveManager.LoadWorld
         *       NetworkLevelLoader.MidLoadLevel
         *   CharacterSaveInstanceHolder.ApplyLegacy
         *     SaveManager.LoadLegacyChestData
         *       NetworkLevelLoader.MidLoadLevel
         */

        /*private IEnumerator FinalizeRestart(Character selfChar)
        {
            DoOloggerLog("FinalizeRestart START");
            //DoOloggerLog("Finalize Restart| " + Time.time);
            yield return new WaitForSeconds(0.5f);
            while (SaveManager.Instance.SaveInProgress)
            {
                yield return null;
            }
            while (PhotonNetwork.inRoom)
            {
                yield return null;
            }
            CharacterManager.Instance.ClearAllCharacters();
            MenuManager.Instance.ShowSavingIcon(_show: false);
            //DoOloggerLog("Finalize Restart - Done Saving| " + Time.time);
            PhotonNetwork.Disconnect();
            while (PhotonNetwork.connectionState != 0)
            {
                yield return null;
            }
            //UnityEngine.Object.Destroy(CharacterManager.Instance.gameObject);
            /*if (DemoManager.DemoIsActive)
            {
                if (DemoManager.CurrentDemoStep == DemoManager.DemoStep.Playing)
                {
                    SaveManager.Instance.DeleteSave();
                }
                DemoManager.Reset();
                DemoManager.StopDemo();
            }
            SaveManager.Instance.ClearDemoSaves();*
            //SceneManager.LoadSceneAsync(0);
            EnvironmentConditions.GameTime = 0.0;
            EnvironmentConditions.DeltaGameTime = 0f;
            DoOloggerLog("FinalizeRestart END");
            NetworkLevelLoader.Instance.StartCoroutine(ConnectionCoroutine(selfChar));
            yield return null;
        }
        private IEnumerator ConnectionCoroutine2()
        {
            DoOloggerLog("ConnectionCoroutine START");
            AccessTools.Field(typeof(NetworkLevelLoader), "m_hostLost").SetValue(NetworkLevelLoader.Instance, false);
            if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_lastLoadJoinedWorld").GetValue(NetworkLevelLoader.Instance))
            {
                DoOloggerLog("Connected");
                MenuManager.Instance.SetConnectionScreenText(LocalizationManager.Instance.GetLoc("Connection_Preparation_Joining"));
                MenuManager.Instance.SetReturningToWorldScreen();
            }
            else if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").GetValue(NetworkLevelLoader.Instance))
            {
                yield return new WaitForSeconds(3f);
                DoOloggerLog("Failed to connect");
                MenuManager.Instance.SetConnectionScreenText(LocalizationManager.Instance.GetLoc("Connection_Preparation_ReloadingWorld"));
                MenuManager.Instance.SetReturningToWorldScreen();
                yield return new WaitForSeconds(3f);
            }
            DoOloggerLog("OK-01");
            bool firstCharacter = false;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").SetValue(NetworkLevelLoader.Instance, false);
            bool playinscene = false;
            NetworkLevelLoader.Instance.PauseGameplay("JoinSequence", NetworkLevelLoader.Instance);
            DoOloggerLog("OK-02");
            AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, false);
            if (!PhotonNetwork.offlineMode)
            {
                NetworkLevelLoader.Instance.photonView.RPC("RequestPauseGameplay", PhotonTargets.Others);
            }
            DoOloggerLog("OK-03");
            if (SplitScreenManager.Instance.LocalPlayerCount == 0 || DemoManager.CurrentDemoStep == DemoManager.DemoStep.CharacterSelection)
            {
                if (!DemoManager.DemoIsActive || DemoManager.CurrentDemoStep == DemoManager.DemoStep.JustStarted)
                {
                    SplitScreenManager.Instance.AddLocalPlayer();
                }
                //yield return NetworkLevelLoader.Instance.StartCoroutine(SplitScreenManager.Instance.WaitForCharacterSaveSelection());
                DoOloggerLog("firstCharacter = true");
                firstCharacter = true;
            }
            DoOloggerLog("OK-04");
            if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").GetValue(NetworkLevelLoader.Instance))
            {
                firstCharacter = true;
                AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").SetValue(NetworkLevelLoader.Instance, false);
            }
            DoOloggerLog("Start join sequence | " + Time.time.ToString());
            //NetworkLevelLoader.Instance.JoinSequenceStarted();
            AccessTools.Method(typeof(NetworkLevelLoader), "SequenceStarted").Invoke(NetworkLevelLoader.Instance, new object[] { });
            AccessTools.Field(typeof(NetworkLevelLoader), "m_joiningWorld").SetValue(NetworkLevelLoader.Instance, true);
            DoOloggerLog("OK-05");
            while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").GetValue(NetworkLevelLoader.Instance))
            {
                yield return null;
            }
            DoOloggerLog("OK-06");
            while (!ItemManager.Instance.PrefabsLoaded)
            {
                yield return null;
            }
            DoOloggerLog("OK-07");
            Global.Lobby.IsWorldOwner = !PhotonNetwork.isNonMasterClientInRoom;
            if (PhotonNetwork.isNonMasterClientInRoom || firstCharacter)
            {
                DoOloggerLog("Ask for instantiation data | " + Time.time.ToString());
                NetworkInstantiateManager.Instance.RequestInstantiationData();
                while (!NetworkInstantiateManager.Instance.InstantiationDataReceived)
                {
                    yield return null;
                }
                DoOloggerLog("Create player System | " + Time.time.ToString());
                NetworkInstantiateManager.Instance.CreatePlayerSystem();
                DoOloggerLog("Create item character | " + Time.time.ToString());
                NetworkInstantiateManager.Instance.CreateItemCharacters();
                while (!NetworkInstantiateManager.Instance.CharacterInstantiationDone)
                {
                    yield return null;
                }
            }
            DoOloggerLog("Prepare Load level | " + Time.time.ToString());
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                QuestEventManager.Instance.SyncQuestRequired();
                NetworkInstantiateManager.Instance.StartLoadLevel();
                yield return new WaitForSeconds(2f);
            }
            else if (firstCharacter)
            {
                while (!BlackFade.Instance.FadeDone)
                {
                    yield return null;
                }
                if (NetworkLevelLoader.Instance.AutoLoadFirstNextScene)
                {
                    CharacterSave chosenSave = SplitScreenManager.Instance.LocalPlayers[0].ChosenSave;
                    if (!DemoManager.DemoIsActive || (chosenSave != null && !chosenSave.PSave.NewSave))
                    {
                        if (chosenSave == null || chosenSave.PSave.NewSave || string.IsNullOrEmpty(chosenSave.PSave.AreaName))
                        {
                            if (!DemoManager.DemoIsActive)
                            {
                                AccessTools.Field(typeof(NetworkLevelLoader), "m_showPrologueScreens").SetValue(NetworkLevelLoader.Instance, true);
                            }
                            NetworkLevelLoader.Instance.LoadLevel(1, 0, 1.5f, _save: false);
                        }
                        else
                        {
                            QuestEventManager.Instance.SyncQuestRequired();
                            NetworkLevelLoader.Instance.LoadLevel(chosenSave.PSave.AreaName, 0, 1.5f, _save: false);
                            AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, true);
                        }
                    }
                    else if (DemoManager.IsFirstSelection)
                    {
                        NetworkLevelLoader.Instance.LoadLevel(DemoManager.StartSceneName, 0, 1.5f, _save: false);
                    }
                    else
                    {
                        NetworkLevelLoader.Instance.LoadLevel(DemoManager.NextDemoScene, DemoManager.NextSceneSpawnPoint, 1.5f, _save: false);
                    }
                }
                else
                {
                    AccessTools.Field(typeof(NetworkLevelLoader), "m_continueAfterLoading").SetValue(NetworkLevelLoader.Instance, true);
                    AccessTools.Field(typeof(NetworkLevelLoader), "m_allPlayerDoneLoading").SetValue(NetworkLevelLoader.Instance, true);
                    AccessTools.Field(typeof(NetworkLevelLoader), "m_allPlayerReadyToContinue").SetValue(NetworkLevelLoader.Instance, true);
                    playinscene = true;
                    ItemManager.Instance.DoneLoadingLevel();
                    SceneInteractionManager.Instance.SetInteractionIsLoaded();
                    AccessTools.Field(typeof(NetworkLevelLoader), "m_lastLoadedLevelName").SetValue(NetworkLevelLoader.Instance, SceneManagerHelper.ActiveSceneName);
                    if (NetworkLevelLoader.Instance.onSceneLoadingDone != null)
                    {
                        NetworkLevelLoader.Instance.onSceneLoadingDone();
                    }
                    while (!ItemManager.Instance.IsAllItemSynced)
                    {
                        yield return null;
                    }
                    for (int i = 0; i < Global.Lobby.PlayersInLobby.Count; i++)
                    {
                        Global.Lobby.PlayersInLobby[i].ControlledCharacter.CharacterUI.ShowGameplayPanel();
                        CharacterManager.Instance.ApplyQuickSlots(Global.Lobby.PlayersInLobby[i].ControlledCharacter);
                        Global.Lobby.PlayersInLobby[i].ControlledCharacter.SendMessage("OnFinalizeLoading");
                    }
                    AccessTools.Field(typeof(NetworkLevelLoader), "m_gameplayLoading").SetValue(NetworkLevelLoader.Instance, false);
                    if (NetworkLevelLoader.Instance.onGameplayLoadingDone != null)
                    {
                        NetworkLevelLoader.Instance.onGameplayLoadingDone();
                    }
                    NetworkLevelLoader.Instance.StartCoroutine(CharacterManager.Instance.LoadCharactersDefaultVisuals());
                }
            }
            while (NetworkLevelLoader.Instance.IsGameplayLoading)
            {
                yield return null;
            }
            NetworkLevelLoader.Instance.photonView.RPC("SendReceiveTrivialRPCs", PhotonTargets.All, PhotonNetwork.player);
            yield return new WaitForSeconds(0.51f);
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                Character worldHostCharacter = CharacterManager.Instance.GetWorldHostCharacter();
                for (int j = 0; j < Global.Lobby.LocalPlayersUID.Length; j++)
                {
                    Character controlledCharacter = Global.Lobby.GetPlayerSystem(Global.Lobby.LocalPlayersUID[j]).ControlledCharacter;
                    Vector3 vector = worldHostCharacter.transform.position + controlledCharacter.transform.right * 0.1f * j;
                    if (controlledCharacter.IsInElevator)
                    {
                        vector.y += 0.5f;
                    }
                    else
                    {
                        Vector3 vector2 = vector + Vector3.up;
                        if (Physics.Raycast(vector2, Vector3.down, out RaycastHit hitInfo, 100f, Global.LargeEnvironmentMask))
                        {
                            vector = hitInfo.point;
                        }
                        //Debug.DrawLine(vector2, vector + Vector3.down * 100f, Color.blue, 5f);
                    }
                    controlledCharacter.Teleport(vector, worldHostCharacter.transform.rotation);
                }
            }
            DoOloggerLog("End join sequence | " + Time.time.ToString());
            if (!PhotonNetwork.offlineMode)
            {
                if (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                {
                    DoOloggerLog("Waiting for all player + | " + Time.time.ToString());
                    while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                    {
                        yield return null;
                    }
                }
                DoOloggerLog("All player Done Loading + | " + Time.time.ToString());
            }
            if (PhotonNetwork.isNonMasterClientInRoom && (bool)QuestEventManager.Instance)
            {
                QuestEventManager.Instance.RequestEvents();
            }
            while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_continueAfterLoading").GetValue(NetworkLevelLoader.Instance))
            {
                yield return null;
            }
            NetworkLevelLoader.Instance.JoinSequenceDone();
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                NetworkLevelLoader.Instance.photonView.RPC("SendRequestGameTime", PhotonTargets.MasterClient);
            }
            NetworkLevelLoader.Instance.UnPauseGameplay("JoinSequence");
            if ((bool)TOD_Sky.Instance)
            {
                TOD_Sky.Instance.TODTime.ProgressTime = true;
            }
            AccessTools.Field(typeof(NetworkLevelLoader), "m_transitionSceneLoaded").SetValue(NetworkLevelLoader.Instance, false);
            if (playinscene && NetworkLevelLoader.Instance.onOverallLoadingDone != null)
            {
                NetworkLevelLoader.Instance.onOverallLoadingDone();
            }
        }
        private IEnumerator ConnectionCoroutine(Character selfChar)
        {
            DoOloggerLog("ConnectionCoroutine START");
            AccessTools.Field(typeof(NetworkLevelLoader), "m_hostLost").SetValue(NetworkLevelLoader.Instance, false);

            AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").SetValue(NetworkLevelLoader.Instance, false);
            bool playinscene = false;
            DoOloggerLog("OK-05");
            NetworkLevelLoader.Instance.PauseGameplay("JoinSequence", NetworkLevelLoader.Instance);
            //m_loadingLevelFromSave = false;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, false);
            DoOloggerLog("OK-07");
            NetworkLevelLoader.Instance.JoinSequenceStarted();
            DoOloggerLog("OK-08");
            while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").GetValue(NetworkLevelLoader.Instance))
            {
                yield return null;
            }
            DoOloggerLog("OK-09");
            while (!ItemManager.Instance.PrefabsLoaded)
            {
                yield return null;
            }
            DoOloggerLog("OK-10");

            /*AccessTools.Field(typeof(Character), "m_startInitDone").SetValue(selfChar, false);
            selfChar.ProcessInit();
            while (!(bool)AccessTools.Field(typeof(Character), "m_startInitDone").GetValue(selfChar))
            {
                yield return null;// new WaitForSeconds(0.2f);
            }*
            NetworkInstantiateManager.Instance.CreateItemCharacters();
            while (!NetworkInstantiateManager.Instance.CharacterInstantiationDone)
            {
                yield return null;
            }

            Global.Lobby.IsWorldOwner = !PhotonNetwork.isNonMasterClientInRoom;
            while (!BlackFade.Instance.FadeDone)
            {
                yield return null;
            }
            DoOloggerLog("OK-20");
            CharacterSave chosenSave = SplitScreenManager.Instance.LocalPlayers[0].ChosenSave;
            QuestEventManager.Instance.SyncQuestRequired();
            NetworkLevelLoader.Instance.LoadLevel(chosenSave.PSave.AreaName, 0, 1.5f, _save: false);
            //m_loadingLevelFromSave = true;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, true);
            while (NetworkLevelLoader.Instance.IsGameplayLoading)
            {
                yield return null;
            }
            NetworkLevelLoader.Instance.photonView.RPC("SendReceiveTrivialRPCs", PhotonTargets.All, PhotonNetwork.player);
            yield return new WaitForSeconds(0.51f);
            DoOloggerLog("OK-30");
            while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_continueAfterLoading").GetValue(NetworkLevelLoader.Instance))
            {
                yield return null;
            }
            NetworkLevelLoader.Instance.JoinSequenceDone();
            NetworkLevelLoader.Instance.UnPauseGameplay("JoinSequence");
            if ((bool)TOD_Sky.Instance)
            {
                TOD_Sky.Instance.TODTime.ProgressTime = true;
            }
            //m_transitionSceneLoaded = false;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_transitionSceneLoaded").SetValue(NetworkLevelLoader.Instance, false);
            if (playinscene && NetworkLevelLoader.Instance.onOverallLoadingDone != null)
            {
                NetworkLevelLoader.Instance.onOverallLoadingDone();
            }
        }

        private void SendNotificationToPlayer(Character player, string p_notif)
        {
            if (player != null)
            {
                player.CharacterUI.SmallNotificationPanel.ShowNotification(p_notif, 8f);
            }
        }
        private void SendNotificationToAllPlayers(string p_notif)
        {
            for (int i = 0; i < CharacterManager.Instance.PlayerCharacters.Count; i++)
            {
                Character player = CharacterManager.Instance.Characters[CharacterManager.Instance.PlayerCharacters.Values[i]];
                if (player != null)
                {
                    player.CharacterUI.SmallNotificationPanel.ShowNotification(p_notif, 8f);
                }
            }
        }//*/
    }
}