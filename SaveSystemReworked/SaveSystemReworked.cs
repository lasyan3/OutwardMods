using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CustomKeybindings;

public class SaveSystemReworked : PartialityMod
{

    public SaveSystemReworked()
    {
        this.ModID = "SaveSystemReworked";
        this.Version = "1.0.0";
        //this.loadPriority = 0;
        this.author = "lasyan3";
    }

    // Init() is called the moment the mod is loaded. Use this to set properties or load configs, and the like.
    public override void Init()
    {
        base.Init();
    }

    // OnLoad() is called after all mods have loaded. Do most of your first-time code here. Creating objects, getting stuff from other mods, etc.
    public override void OnLoad()
    {
        base.OnLoad();
    }

    // OnDisable() is called when a mod is disabled.
    public override void OnDisable()
    {
        base.OnDisable();
    }

    // OnEnable() is called when a mod is enabled (also when it's loaded)
    public override void OnEnable()
    {
        base.OnEnable();

        try
        {
            On.LocalCharacterControl.UpdateInteraction += LocalCharacterControl_UpdateInteraction;
            On.NetworkLevelLoader.OnApplicationQuit += NetworkLevelLoader_OnApplicationQuit; // Disable autosaving on quit

            CustomKeybindings.AddAction("QuickSave", KeybindingsCategory.Actions, ControlType.Both, 5);
            //CustomKeybindings.AddAction("QuickLoad", KeybindingsCategory.Actions, ControlType.Both, 5);

            //OLogger.Log("SaveSystemReworked is enabled");
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }

    private void NetworkLevelLoader_OnApplicationQuit(On.NetworkLevelLoader.orig_OnApplicationQuit orig, NetworkLevelLoader self) { }

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

            if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("QuickSave"))
            {
                SaveManager.Instance.Save(true, true);
                //QuickSave = SaveManager.Instance.ChooseCharacterSaveInstance(charUID, 0);

                if ((bool)Global.AudioManager)
                {
                    Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_NEWGAME_SelectSave);
                }
            }
            /*if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("QuickLoad"))
            {
                if (QuickSave == null)
                {
                    //SendNotificationToPlayer(self.Character, $"Missing QuickSave!");
                    //return;
                    QuickSave = SaveManager.Instance.ChooseCharacterSaveInstance(charUID, 0);
                }
                SplitScreenManager.Instance.LocalPlayers[0].SetChosenSave(QuickSave);

                if ((bool)Global.AudioManager)
                {
                    Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_NEWGAME_SelectSave);
                }

                /* NetworkLevelLoader.GoMainMenu
                 *   NetworkLevelLoader.EndRestart
                 *     SplitScreenManager.Instance.Restart --> RemoveLocalPlayer
                 *     NetworkLevelLoader.FinalizeRestart --> ClearAllCharacters
                 *       
                 */
            //MenuManager.Instance.BackToMainMenu();
            /*PhotonNetwork.LeaveRoom();
            //SplitScreenManager.Instance.Restart();
            NetworkLevelLoader.Instance.StartCoroutine(FinalizeRestart(self.Character));
            Global.AudioManager.StopAmbientMusic(1f);
            Global.AudioManager.MusicOnlySnapshot.TransitionTo(0.5f);*/

            /*BlackFade.Instance.StartFade(_toBlack: true, () =>
            {
                NetworkLevelLoader.Instance.StartCoroutine(ConnectionCoroutine(self.Character));
            });*/


            //}
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }
    private IEnumerator FinalizeRestart(Character selfChar)
    {
        OLogger.Log("FinalizeRestart START");
        //OLogger.Log("Finalize Restart| " + Time.time);
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
        //OLogger.Log("Finalize Restart - Done Saving| " + Time.time);
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
        SaveManager.Instance.ClearDemoSaves();*/
        //SceneManager.LoadSceneAsync(0);
        EnvironmentConditions.GameTime = 0.0;
        EnvironmentConditions.DeltaGameTime = 0f;
        OLogger.Log("FinalizeRestart END");
        NetworkLevelLoader.Instance.StartCoroutine(ConnectionCoroutine(selfChar));
        yield return null;
    }
    private IEnumerator ConnectionCoroutine2()
    {
        OLogger.Log("ConnectionCoroutine START");
        AccessTools.Field(typeof(NetworkLevelLoader), "m_hostLost").SetValue(NetworkLevelLoader.Instance, false);
        if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_lastLoadJoinedWorld").GetValue(NetworkLevelLoader.Instance))
        {
            OLogger.Log("Connected");
            MenuManager.Instance.SetConnectionScreenText(LocalizationManager.Instance.GetLoc("Connection_Preparation_Joining"));
            MenuManager.Instance.SetReturningToWorldScreen();
        }
        else if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").GetValue(NetworkLevelLoader.Instance))
        {
            yield return new WaitForSeconds(3f);
            OLogger.Log("Failed to connect");
            MenuManager.Instance.SetConnectionScreenText(LocalizationManager.Instance.GetLoc("Connection_Preparation_ReloadingWorld"));
            MenuManager.Instance.SetReturningToWorldScreen();
            yield return new WaitForSeconds(3f);
        }
        OLogger.Log("OK-01");
        bool firstCharacter = false;
        AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").SetValue(NetworkLevelLoader.Instance, false);
        bool playinscene = false;
        NetworkLevelLoader.Instance.PauseGameplay("JoinSequence", NetworkLevelLoader.Instance);
        OLogger.Log("OK-02");
        AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, false);
        if (!PhotonNetwork.offlineMode)
        {
            NetworkLevelLoader.Instance.photonView.RPC("RequestPauseGameplay", PhotonTargets.Others);
        }
        OLogger.Log("OK-03");
        if (SplitScreenManager.Instance.LocalPlayerCount == 0 || DemoManager.CurrentDemoStep == DemoManager.DemoStep.CharacterSelection)
        {
            if (!DemoManager.DemoIsActive || DemoManager.CurrentDemoStep == DemoManager.DemoStep.JustStarted)
            {
                SplitScreenManager.Instance.AddLocalPlayer();
            }
            //yield return NetworkLevelLoader.Instance.StartCoroutine(SplitScreenManager.Instance.WaitForCharacterSaveSelection());
            OLogger.Log("firstCharacter = true");
            firstCharacter = true;
        }
        OLogger.Log("OK-04");
        if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").GetValue(NetworkLevelLoader.Instance))
        {
            firstCharacter = true;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").SetValue(NetworkLevelLoader.Instance, false);
        }
        OLogger.Log("Start join sequence | " + Time.time.ToString());
        //NetworkLevelLoader.Instance.JoinSequenceStarted();
        AccessTools.Method(typeof(NetworkLevelLoader), "SequenceStarted").Invoke(NetworkLevelLoader.Instance, new object[] { });
        AccessTools.Field(typeof(NetworkLevelLoader), "m_joiningWorld").SetValue(NetworkLevelLoader.Instance, true);
        OLogger.Log("OK-05");
        while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").GetValue(NetworkLevelLoader.Instance))
        {
            yield return null;
        }
        OLogger.Log("OK-06");
        while (!ItemManager.Instance.PrefabsLoaded)
        {
            yield return null;
        }
        OLogger.Log("OK-07");
        Global.Lobby.IsWorldOwner = !PhotonNetwork.isNonMasterClientInRoom;
        if (PhotonNetwork.isNonMasterClientInRoom || firstCharacter)
        {
            OLogger.Log("Ask for instantiation data | " + Time.time.ToString());
            NetworkInstantiateManager.Instance.RequestInstantiationData();
            while (!NetworkInstantiateManager.Instance.InstantiationDataReceived)
            {
                yield return null;
            }
            OLogger.Log("Create player System | " + Time.time.ToString());
            NetworkInstantiateManager.Instance.CreatePlayerSystem();
            OLogger.Log("Create item character | " + Time.time.ToString());
            NetworkInstantiateManager.Instance.CreateItemCharacters();
            while (!NetworkInstantiateManager.Instance.CharacterInstantiationDone)
            {
                yield return null;
            }
        }
        OLogger.Log("Prepare Load level | " + Time.time.ToString());
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
        OLogger.Log("End join sequence | " + Time.time.ToString());
        if (!PhotonNetwork.offlineMode)
        {
            if (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                OLogger.Log("Waiting for all player + | " + Time.time.ToString());
                while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                {
                    yield return null;
                }
            }
            OLogger.Log("All player Done Loading + | " + Time.time.ToString());
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
        OLogger.Log("ConnectionCoroutine START");
        AccessTools.Field(typeof(NetworkLevelLoader), "m_hostLost").SetValue(NetworkLevelLoader.Instance, false);

        AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").SetValue(NetworkLevelLoader.Instance, false);
        bool playinscene = false;
        OLogger.Log("OK-05");
        NetworkLevelLoader.Instance.PauseGameplay("JoinSequence", NetworkLevelLoader.Instance);
        //m_loadingLevelFromSave = false;
        AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, false);
        OLogger.Log("OK-07");
        NetworkLevelLoader.Instance.JoinSequenceStarted();
        OLogger.Log("OK-08");
        while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").GetValue(NetworkLevelLoader.Instance))
        {
            yield return null;
        }
        OLogger.Log("OK-09");
        while (!ItemManager.Instance.PrefabsLoaded)
        {
            yield return null;
        }
        OLogger.Log("OK-10");

        /*AccessTools.Field(typeof(Character), "m_startInitDone").SetValue(selfChar, false);
        selfChar.ProcessInit();
        while (!(bool)AccessTools.Field(typeof(Character), "m_startInitDone").GetValue(selfChar))
        {
            yield return null;// new WaitForSeconds(0.2f);
        }*/
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
        OLogger.Log("OK-20");
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
        OLogger.Log("OK-30");
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
    }
}
