using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static CustomKeybindings;

public class SaveSystemReworked : PartialityMod
{
    private bool m_isquicksaving = false;
    private bool m_isquickloading = false;

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
            CustomKeybindings.AddAction("QuickSave", KeybindingsCategory.Actions, ControlType.Both, 5);
            CustomKeybindings.AddAction("QuickLoad", KeybindingsCategory.Actions, ControlType.Both, 5);
            //On.NetworkLevelLoader.GoMainMenu += NetworkLevelLoader_GoMainMenu;
            //On.NetworkLevelLoader.EndRestart += NetworkLevelLoader_EndRestart;
            On.SaveManager.LoadWorld += SaveManager_LoadWorld;
            On.SaveManager.LoadEnvironment += SaveManager_LoadEnvironment;
            On.SaveManager.LocalCharStarted += SaveManager_LocalCharStarted;
            On.NetworkLevelLoader.OnApplicationQuit += NetworkLevelLoader_OnApplicationQuit; // Disable autosaving on quit
            //On.SaveManager.Save += SaveManager_Save;
            //On.SaveManager.Save_1 += SaveManager_Save_1;
            On.SaveManager.Save_2 += SaveManager_Save_2; // Disable autosave
            //On.SaveInstance.LoadInstance += SaveInstance_LoadInstance;
            //On.SaveManager.ChooseCharacterSaveInstance += SaveManager_ChooseCharacterSaveInstance;
            //On.CharacterSelectionPanel.OnSaveInstanceClicked_1 += CharacterSelectionPanel_OnSaveInstanceClicked_1;
            //On.CharacterUI.OnSaveSelected += CharacterUI_OnSaveSelected;
            //On.CharacterUI.DelayedSaveSelected += CharacterUI_DelayedSaveSelected;

            // Identify who uses CharacterLastSaveInstance
            //On.MajorElementSave.ProcessSave += MajorElementSave_ProcessSave;

            // TODO : désactiver les scénarios de défaite et reload auto save précédente --> NON car les scénarios peuvent être importants (ex: camping)
            //On.DefeatScenarioRespawn.OnActivate += DefeatScenarioRespawn_OnActivate;
            //On.DefeatScenariosManager.ActivateDefeatScenario += DefeatScenariosManager_ActivateDefeatScenario;

            // TODO : quickLoad
            On.NetworkLevelLoader.LoadLevel += NetworkLevelLoader_LoadLevel;
            On.NetworkLevelLoader.LoadLevel_1 += NetworkLevelLoader_LoadLevel_1;
            On.NetworkLevelLoader.LoadLevel_2 += NetworkLevelLoader_LoadLevel_2;
            //On.NetworkLevelLoader.LoadLevelOneShot += NetworkLevelLoader_LoadLevelOneShot;
            //On.NetworkLevelLoader.LoadLevelOneShot_1 += NetworkLevelLoader_LoadLevelOneShot_1;
            //On.NetworkLevelLoader.OnLevelWasLoaded += NetworkLevelLoader_OnLevelWasLoaded;
            On.NetworkLevelLoader.MidLoadLevel += NetworkLevelLoader_MidLoadLevel;
            On.NetworkLevelLoader.FinishLoadLevel += NetworkLevelLoader_FinishLoadLevel;
            On.NetworkLevelLoader.BaseLoadLevel += NetworkLevelLoader_BaseLoadLevel;
            On.Character.CheckLoadPosition += Character_CheckLoadPosition;
            //On.NetworkLevelLoader.LocalCharStarted += NetworkLevelLoader_LocalCharStarted;
            //On.CharacterManager.UpdateCharacterInitialization += CharacterManager_UpdateCharacterInitialization;
            //On.Character.Start += Character_Start;
            //On.Character.ProcessInit += Character_ProcessInit;

            On.NetworkLevelLoader.JoinSequenceStarted += NetworkLevelLoader_JoinSequenceStarted;
            On.NetworkLevelLoader.JoinSequenceDone += NetworkLevelLoader_JoinSequenceDone;

            //OLogger.Log("SaveSystemReworked is enabled");
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }

    private void NetworkLevelLoader_LocalCharStarted(On.NetworkLevelLoader.orig_LocalCharStarted orig, NetworkLevelLoader self, Character _char)
    {
        OLogger.Log($"NetworkLevelLoader_LocalCharStarted");
        orig(self, _char);
    }

    private void NetworkLevelLoader_OnJoinedRoom(On.NetworkLevelLoader.orig_OnJoinedRoom orig, NetworkLevelLoader self)
    {
        OLogger.Log($"OnJoinedRoom");
        orig(self);
    }

    private void NetworkLevelLoader_JoinSequenceDone(On.NetworkLevelLoader.orig_JoinSequenceDone orig, NetworkLevelLoader self)
    {
        OLogger.Log($"JoinSequenceDone");
        orig(self);
    }

    private void NetworkLevelLoader_JoinSequenceStarted(On.NetworkLevelLoader.orig_JoinSequenceStarted orig, NetworkLevelLoader self)
    {
        OLogger.Log($"JoinSequenceStarted");
        orig(self);
    }

    private void NetworkLevelLoader_BaseLoadLevel(On.NetworkLevelLoader.orig_BaseLoadLevel orig, NetworkLevelLoader self, int _spawnPoint, float _spawnOffset, bool _save)
    {
        OLogger.Log($"BaseLoadLevel");
        orig(self, _spawnPoint, _spawnOffset, _save);
        if (m_isquickloading)
        {
            AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(self, true);
            m_isquickloading = false;
        }
    }

    private void Character_CheckLoadPosition(On.Character.orig_CheckLoadPosition orig, Character self)
    {
        OLogger.Log($"CheckLoadPosition");
        orig(self);
    }

    private void Character_ProcessInit(On.Character.orig_ProcessInit orig, Character self)
    {
        bool m_startInitDone = (bool)AccessTools.Field(typeof(Character), "m_startInitDone").GetValue(self);
        if (!m_startInitDone)
        {
            OLogger.Log($"{self.Name} Init");
        }
        orig(self);
        if (!m_startInitDone)
        {
            OLogger.Log($"{self.Name}={(bool)AccessTools.Field(typeof(Character), "m_startInitDone").GetValue(self)}");
        }
    }

    private void CharacterManager_UpdateCharacterInitialization(On.CharacterManager.orig_UpdateCharacterInitialization orig, CharacterManager self)
    {
        List<Character> m_characterToInitialized = (List<Character>)AccessTools.Field(typeof(CharacterManager), "m_characterToInitialized").GetValue(self);
        if (m_characterToInitialized.Count > 0)
        {
            OLogger.Log($"UpdateCharacterInitialization={m_characterToInitialized.Count}");
        }
        orig(self);
    }

    private void Character_Start(On.Character.orig_Start orig, Character self)
    {
        OLogger.Log($"Character_Start");
        orig(self);
    }

    private IEnumerator NetworkLevelLoader_FinishLoadLevel(On.NetworkLevelLoader.orig_FinishLoadLevel orig, NetworkLevelLoader self)
    {
        OLogger.Log($"FinishLoadLevel");
        IEnumerator res = orig(self);
        if (m_isquickloading)
        {
            m_isquickloading = false;
        }
        return res;
    }

    private void NetworkLevelLoader_MidLoadLevel(On.NetworkLevelLoader.orig_MidLoadLevel orig, NetworkLevelLoader self)
    {
        OLogger.Log($"MidLoadLevel");
        orig(self);
    }

    private void NetworkLevelLoader_OnLevelWasLoaded(On.NetworkLevelLoader.orig_OnLevelWasLoaded orig, NetworkLevelLoader self)
    {
        OLogger.Log($"OnLevelWasLoaded");
        orig(self);
    }

    private void NetworkLevelLoader_LoadLevelOneShot_1(On.NetworkLevelLoader.orig_LoadLevelOneShot_1 orig, NetworkLevelLoader self, string _levelName)
    {
        OLogger.Log($"LoadLevelOneShot_1");
        orig(self, _levelName);
    }

    private void NetworkLevelLoader_LoadLevelOneShot(On.NetworkLevelLoader.orig_LoadLevelOneShot orig, NetworkLevelLoader self, int _buildIndex)
    {
        OLogger.Log($"LoadLevelOneShot");
        orig(self, _buildIndex);
    }

    private void NetworkLevelLoader_LoadLevel_2(On.NetworkLevelLoader.orig_LoadLevel_2 orig, NetworkLevelLoader self, int _buildIndex, int _spawnPoint, float _spawnOffset, bool _save)
    {
        OLogger.Log($"LoadLevel_2");
        orig(self, _buildIndex, _spawnPoint, _spawnOffset, _save);
    }

    private void NetworkLevelLoader_LoadLevel_1(On.NetworkLevelLoader.orig_LoadLevel_1 orig, NetworkLevelLoader self, float _increaseTime, string _levelName, int _spawnPoint, float _spawnOffset, bool _save)
    {
        OLogger.Log($"LoadLevel_1");
        orig(self, _increaseTime, _levelName, _spawnPoint, _spawnOffset, _save);
    }

    private void NetworkLevelLoader_LoadLevel(On.NetworkLevelLoader.orig_LoadLevel orig, NetworkLevelLoader self, string _levelName, int _spawnPoint, float _spawnOffset, bool _save)
    {
        OLogger.Log($"LoadLevel");
        orig(self, _levelName, _spawnPoint, _spawnOffset, _save);
    }

    private void CharacterUI_DelayedSaveSelected(On.CharacterUI.orig_DelayedSaveSelected orig, CharacterUI self, CharacterSave _save)
    {
        OLogger.Log($"CharacterUI_DelayedSaveSelected");
        //orig(self, _save);
        int m_rewiredID = (int)AccessTools.Field(typeof(CharacterUI), "m_rewiredID").GetValue(self);
        SplitScreenManager.Instance.PrepareCharacterSelection(m_rewiredID, _save);
    }

    private void CharacterUI_OnSaveSelected(On.CharacterUI.orig_OnSaveSelected orig, CharacterUI self, CharacterSave _save)
    {
        OLogger.Log($"CharacterUI_OnSaveSelected");
        orig(self, _save);
    }

    private void CharacterSelectionPanel_OnSaveInstanceClicked_1(On.CharacterSelectionPanel.orig_OnSaveInstanceClicked_1 orig, CharacterSelectionPanel self, SaveInstance _instance, int _index)
    {
        OLogger.Log($"OnSaveInstanceClicked");
        orig(self, _instance, _index);
        /*if (self.PlayerID == 0 && !_instance.CharSave.PSave.NewSave)
        {
            _instance.LegacyChestSave.ApplyData();
        }
        CharacterSave arg = NetworkLevelLoader.Instance.ChooseCharacterSaveInstance(_instance.CharSave.CharacterUID, _index);
        if ((bool)Global.AudioManager)
        {
            Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_NEWGAME_SelectSave);
        }
        if (self.onSaveSelected != null)
        {
            self.onSaveSelected.Invoke(arg);
        }*/
    }

    private void SaveManager_LocalCharStarted(On.SaveManager.orig_LocalCharStarted orig, SaveManager self, Character _char)
    {
        OLogger.Log("LocalCharStarted");
        orig(self, _char);
    }

    private bool SaveManager_LoadEnvironment(On.SaveManager.orig_LoadEnvironment orig, SaveManager self)
    {
        OLogger.Log("SaveManager_LoadEnvironment");
        return orig(self);
    }

    private void SaveManager_LoadWorld(On.SaveManager.orig_LoadWorld orig, SaveManager self)
    {
        OLogger.Log("SaveManager_LoadWorld");
        orig(self);
    }

    private void DefeatScenariosManager_ActivateDefeatScenario(On.DefeatScenariosManager.orig_ActivateDefeatScenario orig, DefeatScenariosManager self, DefeatScenario _scenario)
    {
        OLogger.Log("DefeatScenariosManager_ActivateDefeatScenario");
        orig(self, _scenario);
    }

    private void DefeatScenarioRespawn_OnActivate(On.DefeatScenarioRespawn.orig_OnActivate orig, DefeatScenarioRespawn self)
    {
        OLogger.Log("DefeatScenarioRespawn_OnActivate");
        orig(self);
    }

    private bool MajorElementSave_ProcessSave(On.MajorElementSave.orig_ProcessSave orig, MajorElementSave self)
    {
        return true;
    }

    private void NetworkLevelLoader_OnApplicationQuit(On.NetworkLevelLoader.orig_OnApplicationQuit orig, NetworkLevelLoader self)
    {
    }

    private void SaveManager_Save_2(On.SaveManager.orig_Save_2 orig, SaveManager self, bool _async, bool _forceSaveEnvironment)
    {
        try
        {
            if (!_forceSaveEnvironment)
                return;
            //OLogger.Log($"Save async={_async} forceEnv={_forceSaveEnvironment}");
            orig(self, _async, _forceSaveEnvironment);
            return;
            #region tests
            if (m_isquicksaving)
            {
                m_isquicksaving = false;
                return;
            }
            while (SaveManager.Instance.SaveInProgress)
            {
                OLogger.Log($"Wait saving...");
                new UnityEngine.WaitForSeconds(0.5f);
            }
            bool m_savingPending = (bool)AccessTools.Field(typeof(SaveManager), "m_savingPending").GetValue(self);
            bool m_saveRequired = (bool)AccessTools.Field(typeof(SaveManager), "m_saveRequired").GetValue(self);
            DictionaryExt<string, List<SaveInstance>> m_charSaves = (DictionaryExt<string, List<SaveInstance>>)AccessTools.Field(typeof(SaveManager), "m_charSaves").GetValue(self);
            for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
            {
                if (!(bool)SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter)
                {
                    continue;
                }
                UID uID = SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.UID;
                SaveInstance saveInstance2 = m_charSaves[uID][0];
                OLogger.Log($"Delete {saveInstance2.InstancePath}");
                if (saveInstance2.DeleteSave())
                {
                    //m_charSaves[uID].Remove(saveInstance2);
                }
            }
            return;
            if (NetworkLevelLoader.Instance.InLoading || NetworkLevelLoader.Instance.IsJoiningWorld)
            {
                return;
            }
            if (m_savingPending && _async && !Global.IsApplicationClosing)
            {
                m_saveRequired = true;
                return;
            }
            m_savingPending = true;
            bool flag = true;
            /*if (Global.CheatsEnabled)
            {
                flag = (NetworkLevelLoader.Instance.AutoSaveEnvironment || _forceSaveEnvironment);
            }*/
            for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
            {
                if (!(bool)SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter)
                {
                    continue;
                }
                UID uID = SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.UID;
                if (self.CharacterLastSaveInstance.ContainsKey(uID))
                {
                    int index = self.CharacterLastSaveInstance[uID];
                    SaveInstance saveInstance = null;
                    if (!string.IsNullOrEmpty(m_charSaves[uID][index].InstancePath))
                    {
                        OLogger.Log($"Based on {m_charSaves[uID][index].InstancePath}");
                        saveInstance = new SaveInstance();
                        saveInstance.CopyInstance(m_charSaves[uID][index]);
                    }
                    else
                    {
                        OLogger.Log($"Use direct {m_charSaves[uID][index].SavePath}");
                        saveInstance = m_charSaves[uID][index];
                    }
                    saveInstance.InstancePath = DateTime.Now.ToString("yyyyMMddHHmmss");
                    bool flag2 = flag && uID == self.LastLoadedSaveID;
                    flag2 &= Global.Lobby.IsWorldOwner;
                    /*if (!saveInstance.Save(flag && flag2))
                    {
                        continue;
                    }*/
                    //continue;
                    /*if (DemoManager.DemoIsActive && !m_demoCharacters.Contains(uID))
                    {
                        m_demoCharacters.Add(uID);
                    }*/
                    saveInstance = m_charSaves[uID][index];
                    m_charSaves[uID].Remove(saveInstance);
                    m_charSaves[uID].Insert(0, saveInstance);
                    self.CharacterLastSaveInstance[uID] = 0;
                    SplitScreenManager.Instance.LocalPlayers[i].SetChosenSave(saveInstance.CharSave, _firstSelect: false);
                    /*if (m_charSaves[uID].Count > 15)
                    {
                        SaveInstance saveInstance2 = m_charSaves[uID][m_charSaves[uID].Count - 1];
                        if (saveInstance2.DeleteSave())
                        {
                            m_charSaves[uID].Remove(saveInstance2);
                        }
                    }*/
                }
                /*else
                {
                    Debug.LogErrorFormat("{0} - {1}: Trying to save but no Save Instance was chosen.", SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.Name, SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.UID.Value);
                }*/
            }
            /*if (_async)
            {
                CharacterManager.Instance.SavingPending(_pending: true);
            }*/
            AccessTools.Field(typeof(SaveManager), "m_savingPending").SetValue(self, m_savingPending);
            AccessTools.Field(typeof(SaveManager), "m_saveRequired").SetValue(self, m_saveRequired);
            AccessTools.Field(typeof(SaveManager), "m_charSaves").SetValue(self, m_charSaves);
            //DoneSaving();

            //orig(self, _async, _forceSaveEnvironment);
            //var fctDoneSaving = AccessTools.Method(typeof(void), "DoneSaving", new Type[] { });
            /*for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
            {
                UID uID = SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.UID;
                DictionaryExt<string, List<SaveInstance>> charSaves = (DictionaryExt<string, List<SaveInstance>>)AccessTools.Field(typeof(SaveManager), "m_charSaves").GetValue(self);
                SaveInstance QuickSaveInstance = charSaves[uID][0];
            }*/
            #endregion
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }

    private void NetworkLevelLoader_GoMainMenu(On.NetworkLevelLoader.orig_GoMainMenu orig, NetworkLevelLoader self)
    {
        OLogger.Log("GoMainMenu");
        orig(self);
        /*MenuManager.Instance.ShowSavingIcon(_show: true);
        PhotonNetwork.LeaveRoom();
        BlackFade.Instance.StartFade(_toBlack: true, EndRestart);
        Global.AudioManager.StopAmbientMusic(1f);
        Global.AudioManager.MusicOnlySnapshot.TransitionTo(0.5f);*/
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

            if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("QuickSave"))
            {
                m_isquicksaving = true;
                SaveManager.Instance.Save(true, true);
                //QuickSaveInstance = SaveManager.Instance.ChooseCharacterSaveInstance(charUID, 0);

                /*DictionaryExt<string, List<SaveInstance>> charSaves = (DictionaryExt<string, List<SaveInstance>>)AccessTools.Field(typeof(SaveManager), "m_charSaves").GetValue(self);
                for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
                {
                    UID uID = SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.UID;
                    SaveInstance QuickSaveInstance = charSaves[uID][0];
                    //QuickSaveInstance.CharSave.PSave.Position
                }*/
                SendNotificationToAllPlayers($"Saved!");
                if ((bool)Global.AudioManager)
                {
                    Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_NEWGAME_SelectSave);
                }
            }
            if (CustomKeybindings.m_playerInputManager[playerID].GetButtonDown("QuickLoad"))
            {
                // Clean previous data
                //MenuManager.Instance.BackToMainMenu();
                //DefeatScenariosManager.Instance.FailSafeDefeat();
                //int p = NetworkLevelLoader.Instance.LastSpawnPointID;

                CharacterSave lastCharSave = SaveManager.Instance.ChooseCharacterSaveInstance(self.Character.UID, 1);
                SplitScreenManager.Instance.LocalPlayers[0].SetChosenSave(lastCharSave);
                //new CharacterUI().OnSaveSelected(arg); --> DelayedSaveSelected
                //SplitScreenManager.Instance.PrepareCharacterSelection(playerID, arg);

                //SaveManager.Instance.LocalCharStarted(self.Character);*/
                //NetworkLevelLoader.Instance.ReloadLevel(0f, false);

                /* LoadLevel
                 * LoadLevelOneShot_1
                 * OnLevelWasLoaded
                 * LoadLevelOneShot_1
                 * OnLevelWasLoaded
                 * MidLoadLevel
                 */

                /* SaveInstance:
                 *  - CharacterSave
                 *     - PlayerSaveData
                 *      --> CharacterSave.ApplyLoadedSaveToChar(Character)
                 *          --> SaveManager.LocalCharStarted(Character)
                 *  - WorldSave 
                 *      --> SaveInstance.ApplyWorld() 
                 *          --> SaveManager.LoadWorld()
                 *              --> NetworkLevelLoader.MidLoadLevel()
                 *  - LegacyChestSave
                 *      --> SaveInstance.ApplyLegacy()
                 *  - SceneSaves
                 */
                //OLogger.Log("start fade");
                BlackFade.Instance.StartFade(_toBlack: true, () =>
                {
                    if ((bool)Global.AudioManager)
                    {
                        Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_NEWGAME_SelectSave);
                    }
                    //OLogger.Log("_____QUICKLOAD_____");
                    //CharacterManager.Instance.AddCharacterToInitialized(self.Character);
                    #region working
                    //m_isquickloading = true;
                    //CharacterManager.Instance.ClearAllCharacters();
                    //self.Character.OwnerPlayerSys.AllowInitialize();
                    //NetworkLevelLoader.Instance.JoinSequenceStarted();
                    //NetworkLevelLoader.Instance.LoadLevel(lastCharSave.PSave.AreaName, -1, _save: false);//*/
                    #endregion

                    NetworkLevelLoader.Instance.StartCoroutine(ConnectionCoroutine(self.Character));

                    //SaveManager.Instance.LocalCharStarted(self.Character);
                    //bool m_loadingLevelFromSave = (bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").GetValue(NetworkLevelLoader.Instance);
                    //OLogger.Log($"m_loadingLevelFromSave={m_loadingLevelFromSave}");
                    //AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, true);
                    //SaveManager.Instance.LocalCharStarted(self.Character);
                    //self.Character.CheckLoadPosition();
                    /*while (NetworkLevelLoader.Instance.InLoading)
                    {
                        OLogger.Log("loading...");
                        new WaitForSeconds(2.0f);
                    }*/
                    //self.Character.LoadPlayerSave(arg.PSave);
                    //self.Character.CheckLoadPosition();
                    //NetworkLevelLoader.Instance.MidLoadLevel();
                    /*SplitScreenManager.Instance.Restart();
                    /*string m_autoRoomName = (string)AccessTools.Field(typeof(ConnectPhotonMaster), "m_autoRoomName").GetValue(ConnectPhotonMaster.Instance);
                    AccessTools.Field(typeof(SplitPlayer), "m_chosenSave").SetValue(SplitScreenManager.Instance.LocalPlayers[0], arg);
                    Debug.Log("Clear Previous Data");
                    NetworkInstantiateManager.Instance.Reset();
                    CharacterManager.Instance.ClearAllCharacters();
                    Global.Lobby.ClearPlayerSystems();
                    QuestEventManager.Instance.ClearEvents();
                    ItemManager.Instance.CleanUp();
                    EnvironmentConditions.GameTime = 0.0;
                    EnvironmentConditions.DeltaGameTime = 0f;

                    //SplitScreenManager.Instance.RemoveLocalPlayer(0);
                    //NetworkLevelLoader.Instance.StartConnectionCoroutine();
                    Debug.Log("Create item character | " + Time.time.ToString());
                    NetworkInstantiateManager.Instance.CreateItemCharacters();
                    while (!NetworkInstantiateManager.Instance.CharacterInstantiationDone)
                    {
                        new WaitForSeconds(0.3f);
                    }
                    Debug.Log("Prepare Load level | " + Time.time.ToString());
                    while (!BlackFade.Instance.FadeDone)
                    {
                        new WaitForSeconds(0.3f);
                    }
                    CharacterSave chosenSave = SplitScreenManager.Instance.LocalPlayers[0].ChosenSave;
                    QuestEventManager.Instance.SyncQuestRequired();
                    NetworkLevelLoader.Instance.LoadLevel(chosenSave.PSave.AreaName, 0, 1.5f, _save: false);
                    while (NetworkLevelLoader.Instance.IsGameplayLoading)
                    {
                        new WaitForSeconds(0.3f);
                    }
                    NetworkLevelLoader.Instance.JoinSequenceDone();
                    NetworkLevelLoader.Instance.UnPauseGameplay("JoinSequence");
                    if ((bool)TOD_Sky.Instance)
                    {
                        TOD_Sky.Instance.TODTime.ProgressTime = true;
                    }
                    */
                });
            }
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }
    private IEnumerator ConnectionCoroutine(Character selfChar)
    {
        //OLogger.Log($"ConnectionCoroutine");
        //m_hostLost = false;
        AccessTools.Field(typeof(NetworkLevelLoader), "m_hostLost").SetValue(NetworkLevelLoader.Instance, false);
        /*if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_lastLoadJoinedWorld").GetValue(NetworkLevelLoader.Instance))
        {
            OLogger.Log("Connected");
            MenuManager.Instance.SetConnectionScreenText(LocalizationManager.Instance.GetLoc("Connection_Preparation_Joining"));
            MenuManager.Instance.SetReturningToWorldScreen();
        }
        else if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").GetValue(NetworkLevelLoader.Instance))
        {
            yield return new WaitForSeconds(3f);
            OLogger.Log($"Failed to connect");
            MenuManager.Instance.SetConnectionScreenText(LocalizationManager.Instance.GetLoc("Connection_Preparation_ReloadingWorld"));
            MenuManager.Instance.SetReturningToWorldScreen();
            yield return new WaitForSeconds(3f);
        }*/
        //bool firstCharacter = false;

        AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").SetValue(NetworkLevelLoader.Instance, false);
        bool playinscene = false;
        NetworkLevelLoader.Instance.PauseGameplay("JoinSequence", NetworkLevelLoader.Instance);
        //m_loadingLevelFromSave = false;
        AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, false);
        //OLogger.Log($"offlineMode={PhotonNetwork.offlineMode}");
        /*if (!PhotonNetwork.offlineMode)
        {
            NetworkLevelLoader.Instance.photonView.RPC("RequestPauseGameplay", PhotonTargets.Others);
        }*/
        /*if (SplitScreenManager.Instance.LocalPlayerCount == 0 || DemoManager.CurrentDemoStep == DemoManager.DemoStep.CharacterSelection)
        {
            OLogger.Log($"WaitForCharacterSaveSelection");
            if (!DemoManager.DemoIsActive || DemoManager.CurrentDemoStep == DemoManager.DemoStep.JustStarted)
            {
                SplitScreenManager.Instance.AddLocalPlayer();
            }
            yield return NetworkLevelLoader.Instance.StartCoroutine(SplitScreenManager.Instance.WaitForCharacterSaveSelection());
            OLogger.Log("firstCharacter = true");
            firstCharacter = true;
        }
        if ((bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").GetValue(NetworkLevelLoader.Instance))
        {
            OLogger.Log($"m_failedJoin");
            firstCharacter = true;
            //m_failedJoin = false;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_failedJoin").SetValue(NetworkLevelLoader.Instance, false);
        }*/
        //OLogger.Log("Start join sequence | " + Time.time.ToString());
        NetworkLevelLoader.Instance.JoinSequenceStarted();
        while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_sequenceStarted").GetValue(NetworkLevelLoader.Instance))
        {
            yield return null;
        }
        while (!ItemManager.Instance.PrefabsLoaded)
        {
            yield return null;
        }

        AccessTools.Field(typeof(Character), "m_startInitDone").SetValue(selfChar, false);
        selfChar.ProcessInit();
        while (!(bool)AccessTools.Field(typeof(Character), "m_startInitDone").GetValue(selfChar))
        {
            //OLogger.Log($"Wait Init {selfChar.Name}");
            yield return null;// new WaitForSeconds(0.2f);
        }

        Global.Lobby.IsWorldOwner = !PhotonNetwork.isNonMasterClientInRoom;
        /*if (PhotonNetwork.isNonMasterClientInRoom || firstCharacter)
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
        }*/
        //OLogger.Log("Prepare Load level | " + Time.time.ToString());
        //firstCharacter = true; // LASYAN3
        /*if (PhotonNetwork.isNonMasterClientInRoom)
        {
            OLogger.Log($"isNonMasterClientInRoom");
            QuestEventManager.Instance.SyncQuestRequired();
            NetworkInstantiateManager.Instance.StartLoadLevel();
            yield return new WaitForSeconds(2f);
        }
        else*/ /*if (firstCharacter)
        {*/
        //OLogger.Log($"firstCharacter");
        while (!BlackFade.Instance.FadeDone)
        {
            yield return null;
        }
        //if (NetworkLevelLoader.Instance.AutoLoadFirstNextScene)
        //{
        //OLogger.Log($"AutoLoadFirstNextScene");
        CharacterSave chosenSave = SplitScreenManager.Instance.LocalPlayers[0].ChosenSave;
        /*if (!DemoManager.DemoIsActive || (chosenSave != null && !chosenSave.PSave.NewSave))
        {
            if (chosenSave == null || chosenSave.PSave.NewSave || string.IsNullOrEmpty(chosenSave.PSave.AreaName))
            {
                if (!DemoManager.DemoIsActive)
                {
                    //m_showPrologueScreens = true;
                    AccessTools.Field(typeof(NetworkLevelLoader), "m_showPrologueScreens").SetValue(NetworkLevelLoader.Instance, true);
                }
                NetworkLevelLoader.Instance.LoadLevel(1, 0, 1.5f, _save: false);
            }
            else
            {*/
        //OLogger.Log($"m_loadingLevelFromSave");
        QuestEventManager.Instance.SyncQuestRequired();
        NetworkLevelLoader.Instance.LoadLevel(chosenSave.PSave.AreaName, 0, 1.5f, _save: false);
        //m_loadingLevelFromSave = true;
        AccessTools.Field(typeof(NetworkLevelLoader), "m_loadingLevelFromSave").SetValue(NetworkLevelLoader.Instance, true);
        /*}
    }
    else if (DemoManager.IsFirstSelection)
    {
        NetworkLevelLoader.Instance.LoadLevel(DemoManager.StartSceneName, 0, 1.5f, _save: false);
    }
    else
    {
        OLogger.Log($"LoadLevelDemo");
        NetworkLevelLoader.Instance.LoadLevel(DemoManager.NextDemoScene, DemoManager.NextSceneSpawnPoint, 1.5f, _save: false);
    }*/
        /*}
        else
        {
            OLogger.Log($"DoneLoadingLevel");
            //m_continueAfterLoading = true;
            //m_allPlayerDoneLoading = true;
            //m_allPlayerReadyToContinue = true;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_continueAfterLoading").SetValue(NetworkLevelLoader.Instance, true);
            AccessTools.Field(typeof(NetworkLevelLoader), "m_allPlayerDoneLoading").SetValue(NetworkLevelLoader.Instance, true);
            AccessTools.Field(typeof(NetworkLevelLoader), "m_allPlayerReadyToContinue").SetValue(NetworkLevelLoader.Instance, true);
            playinscene = true;
            ItemManager.Instance.DoneLoadingLevel();
            SceneInteractionManager.Instance.SetInteractionIsLoaded();
            //m_lastLoadedLevelName = SceneManagerHelper.ActiveSceneName;
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
            //m_gameplayLoading = false;
            AccessTools.Field(typeof(NetworkLevelLoader), "m_gameplayLoading").SetValue(NetworkLevelLoader.Instance, false);
            if (NetworkLevelLoader.Instance.onGameplayLoadingDone != null)
            {
                NetworkLevelLoader.Instance.onGameplayLoadingDone();
            }
            NetworkLevelLoader.Instance.StartCoroutine(CharacterManager.Instance.LoadCharactersDefaultVisuals());
        }*/
        //}
        while (NetworkLevelLoader.Instance.IsGameplayLoading)
        {
            yield return null;
        }
        //OLogger.Log($"DoneGameplayLoading");
        NetworkLevelLoader.Instance.photonView.RPC("SendReceiveTrivialRPCs", PhotonTargets.All, PhotonNetwork.player);
        yield return new WaitForSeconds(0.51f);
        /*if (PhotonNetwork.isNonMasterClientInRoom)
        {
            OLogger.Log($"GetWorldHostCharacter");
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
                    Debug.DrawLine(vector2, vector + Vector3.down * 100f, Color.blue, 5f);
                }
                controlledCharacter.Teleport(vector, worldHostCharacter.transform.rotation);
            }
        }*/
        //OLogger.Log("End join sequence | " + Time.time.ToString());
        /*if (!PhotonNetwork.offlineMode)
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
        }*/
        /*if (PhotonNetwork.isNonMasterClientInRoom && (bool)QuestEventManager.Instance)
        {
            QuestEventManager.Instance.RequestEvents();
        }*/
        while (!(bool)AccessTools.Field(typeof(NetworkLevelLoader), "m_continueAfterLoading").GetValue(NetworkLevelLoader.Instance))
        {
            yield return null;
        }
        NetworkLevelLoader.Instance.JoinSequenceDone();
        /*if (PhotonNetwork.isNonMasterClientInRoom)
        {
            NetworkLevelLoader.Instance.photonView.RPC("SendRequestGameTime", PhotonTargets.MasterClient);
        }*/
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

    private void CharacterManager_LoadAiCharactersFromSave(On.CharacterManager.orig_LoadAiCharactersFromSave orig, CharacterManager self, CharacterSaveData[] _saves)
    {
        // do nothing --> Enemies always respawn
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
