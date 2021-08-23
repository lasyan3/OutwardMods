using Harmony;
using ODebug;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaRespawnMoreFrequently : PartialityMod
{
    private float RESPAWN_TIME = 24 * 3;//120f;

    public AreaRespawnMoreFrequently()
    {
        this.ModID = "AreaRespawnMoreFrequently";
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
            On.AreaManager.IsAreaExpired += AreaManager_IsAreaExpired; // Reduce respawn timer and disable respawning for specific areas (all dungeons)
            //On.EnvironmentSave.PrepareSave += EnvironmentSave_PrepareSave; // Areas's respawn timer won't stop while inside the area.
            //On.EnvironmentSave.ApplyData += EnvironmentSave_ApplyData; // Disable respawn of items and loot
            //On.CharacterManager.LoadAiCharactersFromSave += CharacterManager_LoadAiCharactersFromSave;

            // TODO: increase reserve of enemis in the area
            //On.AISquadManager.FirstSpawn += AISquadManager_FirstSpawn;
            //On.AISquadManager.FindSpawn += AISquadManager_FindSpawn;
            /*On.AISquadManager.TryActivateSquad += AISquadManager_TryActivateSquad;
            On.AISquadManager.TryDeactivateSquad += AISquadManager_TryDeactivateSquad;
            On.AISquadManager.CheckActiveSquadsCap += AISquadManager_CheckActiveSquadsCap;*/

            // TODO: fix days count between area travel

            //On.CharacterManager.AddCharacter += CharacterManager_AddCharacter;
            //On.NDPool.Update += NDPool_Update;
            //On.SceneInteractionManager.AddInteractionActivator += SceneInteractionManager_AddInteractionActivator;
            /*On.QuestEventManager.AddEvent_1 += QuestEventManager_AddEvent_1;
            On.Unlock_QuestEventOccured.CheckIsValid += Unlock_QuestEventOccured_CheckIsValid;
            On.NodeCanvas.Tasks.Conditions.Condition_QuestEventOccured.OnCheck += Condition_QuestEventOccured_OnCheck;
            On.SendQuestEventInteraction.OnActivate += SendQuestEventInteraction_OnActivate;
            On.HideObjectManager.GameObjectList.UpdateVisibility += GameObjectList_UpdateVisibility;
            On.QuestEventAreaCondition.CheckIsValid += QuestEventAreaCondition_CheckIsValid;
            On.QuestRelated.ItemParentChanged += QuestRelated_ItemParentChanged;*/
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }

    private bool AISquadManager_CheckActiveSquadsCap(On.AISquadManager.orig_CheckActiveSquadsCap orig, AISquadManager self)
    {
        bool res = orig(self);
        OLogger.Log($"CheckActiveSquadsCap {res}");
        return res;
    }

    private void AISquadManager_TryDeactivateSquad(On.AISquadManager.orig_TryDeactivateSquad orig, AISquadManager self, AISquad _squad)
    {
        List<AISquad> _reserve = (List<AISquad>)AccessTools.Field(typeof(AISquadManager), "m_squadsInReserve").GetValue(self);
        List<AISquad> _play = (List<AISquad>)AccessTools.Field(typeof(AISquadManager), "m_squadsInPlay").GetValue(self);
        DictionaryExt<UID, AISquad> _all = (DictionaryExt<UID, AISquad>)AccessTools.Field(typeof(AISquadManager), "m_allSquads").GetValue(self);
        OLogger.Log($"TryDeactivateSquad {(_squad != null ? _squad.name : "(null)")}");
        OLogger.Log($" > All={_all.Count} Play={_play.Count} Reserve={_reserve.Count}");
        orig(self, _squad);
    }

    private void AISquadManager_TryActivateSquad(On.AISquadManager.orig_TryActivateSquad orig, AISquadManager self, AISquad _squad, bool _resetPositions)
    {
        List<AISquad> _reserve = (List<AISquad>)AccessTools.Field(typeof(AISquadManager), "m_squadsInReserve").GetValue(self);
        List<AISquad> _play = (List<AISquad>)AccessTools.Field(typeof(AISquadManager), "m_squadsInPlay").GetValue(self);
        DictionaryExt<UID, AISquad> _all = (DictionaryExt<UID, AISquad>)AccessTools.Field(typeof(AISquadManager), "m_allSquads").GetValue(self);
        OLogger.Log($"TryActivateSquad {(_squad != null ? _squad.name : "(null)")}");
        OLogger.Log($" > All={_all.Count} Play={_play.Count} Reserve={_reserve.Count}");
        OLogger.Log($" > InReserve={_reserve.Contains(_squad)}");
        /*if (_squad != null && !_reserve.Contains(_squad))
        {
            _reserve.Add(_squad);
            AccessTools.Field(typeof(AISquadManager), "m_squadsInReserve").SetValue(self, _reserve);
        }*/
        orig(self, _squad, _resetPositions);
    }

    private void AISquadManager_FindSpawn(On.AISquadManager.orig_FindSpawn orig, AISquadManager self, bool _ignoreSight)
    {
        OLogger.Log("FindSpawn");
        orig(self, _ignoreSight);
    }

    private void AISquadManager_FirstSpawn(On.AISquadManager.orig_FirstSpawn orig, AISquadManager self)
    {
        OLogger.Log("FirstSpawn");
        orig(self);
    }

    private void QuestRelated_ItemParentChanged(On.QuestRelated.orig_ItemParentChanged orig, QuestRelated self)
    {
        OLogger.Log($"ItemParentChanged");
    }

    private bool QuestEventAreaCondition_CheckIsValid(On.QuestEventAreaCondition.orig_CheckIsValid orig, QuestEventAreaCondition self, Character _affectedCharacter)
    {
        OLogger.Log($"CheckIsValid2");
        return orig.Invoke(self, _affectedCharacter);
    }

    private void GameObjectList_UpdateVisibility(On.HideObjectManager.GameObjectList.orig_UpdateVisibility orig, HideObjectManager.GameObjectList self)
    {
        OLogger.Log($"UpdateVisibility");
    }

    private void SendQuestEventInteraction_OnActivate(On.SendQuestEventInteraction.orig_OnActivate orig, SendQuestEventInteraction self)
    {
        OLogger.Log($"OnActivate");
    }

    private bool Condition_QuestEventOccured_OnCheck(On.NodeCanvas.Tasks.Conditions.Condition_QuestEventOccured.orig_OnCheck orig, NodeCanvas.Tasks.Conditions.Condition_QuestEventOccured self)
    {
        if (self.QuestEventRef != null && self.QuestEventRef.Event != null
            && self.QuestEventRef.Event.EventName == "Faction_HolyMission")
        {
            //OLogger.Log($"OnCheck ${self.QuestEventRef.Event.EventName}");
            //return false; // NPC not spawned
        }
        return orig.Invoke(self);
    }

    private bool Unlock_QuestEventOccured_CheckIsValid(On.Unlock_QuestEventOccured.orig_CheckIsValid orig, Unlock_QuestEventOccured self)
    {
        OLogger.Log($"CheckIsValid");
        return orig.Invoke(self);
    }

    private bool QuestEventManager_AddEvent_1(On.QuestEventManager.orig_AddEvent_1 orig, QuestEventManager self, string _eventUID, int _stackAmount, bool _sendEvent)
    {
        OLogger.Log($"AddEvent {_stackAmount} {_sendEvent}");
        Dictionary<UID, List<IQuestEventListener>> l = (Dictionary<UID, List<IQuestEventListener>>)AccessTools.Field(typeof(QuestEventManager), "m_listeners").GetValue(self);
        return orig.Invoke(self, _eventUID, _stackAmount, _sendEvent);
    }

    private void SceneInteractionManager_AddInteractionActivator(On.SceneInteractionManager.orig_AddInteractionActivator orig, SceneInteractionManager self, EventActivator _activator)
    {
        OLogger.Log($"AddInteractionActivator");
    }

    private void NDPool_Update(On.NDPool.orig_Update orig, NDPool self)
    {
        OLogger.Log($"NDPool_Update {self.name}");
    }

    private void EnvironmentSave_ApplyData(On.EnvironmentSave.orig_ApplyData orig, EnvironmentSave self)
    {
        bool resetEnemies = false;
        AreaManager.AreaEnum areaN = (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(self.AreaName);
        if (areaN == AreaManager.AreaEnum.CierzoOutside ||
            areaN == AreaManager.AreaEnum.Abrassar ||
            areaN == AreaManager.AreaEnum.Emercar ||
            areaN == AreaManager.AreaEnum.HallowedMarsh)
        {
            resetEnemies = true;
        }

        float num = (float)(self.GameTime - EnvironmentConditions.GameTime);
        if (num > 0f)
        {
            EnvironmentConditions.GameTime = self.GameTime;
        }

        if (!AreaManager.Instance.IsAreaExpired(self.AreaName, num))
        {
            ItemManager.Instance.LoadItems(self.ItemList, _clearAllItems: true);
            CharacterManager.Instance.LoadAiCharactersFromSave(self.CharList.ToArray());
            SceneInteractionManager.Instance.LoadInteractableStates(self.InteractionActivatorList);
            SceneInteractionManager.Instance.LoadDropTableStates(self.DropTablesList);
            CampingEventManager.Instance.LoadEventTableData(self.CampingEventSaveData);
            DefeatScenariosManager.Instance.LoadSaveData(self.DefeatScenarioSaveData);
            EnvironmentConditions.Instance.LoadSoulSpots(self.UsedSoulSpots);
            MapDisplay.Instance.Load(self.MapSaveData);
        }
        else if (resetEnemies)
        {
            CharacterManager.Instance.LoadAiCharactersFromSave(self.CharList.ToArray());
        }
        //OLogger.Log($"Enter {self.AreaName} at {GameTimetoDays(EnvironmentConditions.GameTime)}");
        //OLogger.Log($" > Last visit was {GameTimetoDays(self.GameTime)})");
        //orig.Invoke(self);
        //ItemManager.Instance.LoadItems(self.ItemList, _clearAllItems: true);
        //SceneInteractionManager.Instance.LoadInteractableStates(self.InteractionActivatorList);
        //SceneInteractionManager.Instance.LoadDropTableStates(self.DropTablesList);
    }

    private void CharacterManager_LoadAiCharactersFromSave(On.CharacterManager.orig_LoadAiCharactersFromSave orig, CharacterManager self, CharacterSaveData[] _saves)
    {
        // do nothing --> Enemies always respawn
    }

    private void EnvironmentSave_PrepareSave(On.EnvironmentSave.orig_PrepareSave orig, EnvironmentSave self)
    {
        OLogger.Log($"Quit {self.AreaName} at {GameTimetoDays(EnvironmentConditions.GameTime)}");
        //OLogger.Log($" > what is:  {GameTimetoDays(self.GameTime)})");
        double bkp = self.GameTime;
        OLogger.Log($" > 1) {GameTimetoDays(self.GameTime)})");
        orig.Invoke(self);
        OLogger.Log($" > 2) {GameTimetoDays(self.GameTime)})");
        //if (bkp > 0)
        self.GameTime = bkp;
        OLogger.Log($" > 3) {GameTimetoDays(self.GameTime)})");
    }

    private bool AreaManager_IsAreaExpired(On.AreaManager.orig_IsAreaExpired orig, AreaManager self, string _areaName, float _diff)
    {
        //OLogger.Log($"IsAreaExpired {_areaName}={GameTimetoDays(-_diff)})");
        //return orig(self, _areaName, _diff);
        Area areaFromSceneName = self.GetAreaFromSceneName(_areaName);
        /*AreaManager.AreaEnum areaN = (AreaManager.AreaEnum)self.GetAreaIndexFromSceneName(_areaName);
        if (areaN != AreaManager.AreaEnum.CierzoOutside &&
            areaN != AreaManager.AreaEnum.Abrassar &&
            areaN != AreaManager.AreaEnum.Emercar &&
            areaN != AreaManager.AreaEnum.HallowedMarsh)
        {
            SendNotificationToAllPlayers($"{areaN} NEVER expire");
            return false;
        }*/
        float _resetTime = areaFromSceneName.ResetTime == 168f ? RESPAWN_TIME : areaFromSceneName.ResetTime;
        if (areaFromSceneName != null && !self.PermenantAreas.Contains((AreaManager.AreaEnum)areaFromSceneName.ID) && 0f - _diff > _resetTime)
        {
            SendNotificationToAllPlayers($"RESPAWNED ({GameTimetoDays(-_diff)})");
            return true;
        }
        /*if (_diff < 0)
        {
            SendNotificationToAllPlayers($"{areaN} has not expired yet ({GameTimetoDays(-_diff)})");
        }*/
        /*else
        {
            SendNotificationToAllPlayers($"{areaN} has not expired yet");
        }*/
        return false;
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
