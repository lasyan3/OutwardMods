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
using static Character;
using static CustomKeybindings;
using static DefeatScenarioRespawn;
using static DefeatScenarioRespawn.StatAffect;

public class DisableDungeonsDefeatScenarios : PartialityMod
{
    private readonly string _modName = "DisableDungeonsDefeatScenarios";

    public DisableDungeonsDefeatScenarios()
    {
        this.ModID = _modName;
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
            //On.DefeatScenariosManager.ActivateDefeatScenario += DefeatScenariosManager_ActivateDefeatScenario; // FailSafeDefeat in dungeons

            //On.CharacterManager.LoadAiCharactersFromSave += CharacterManager_LoadAiCharactersFromSave; // Restore stats and remove debuffs for alive enemies
            //On.DefeatScenariosContainer.StartInit += DefeatScenariosContainer_StartInit;
            On.DefeatScenariosContainer.AwakeInit += DefeatScenariosContainer_AwakeInit;
            //On.DefeatScenariosManager.CheckForLoadSceneDefeat += DefeatScenariosManager_CheckForLoadSceneDefeat;
            //On.DefeatScenariosManager.ActivateDefeatScenario += DefeatScenariosManager_ActivateDefeatScenario1;
            //On.DefeatScenariosContainer.ChooseScenario += DefeatScenariosContainer_ChooseScenario;

            //OLogger.Log("DisableDungeonsDefeatScenarios is enabled");
        }
        catch (Exception ex)
        {
            Debug.Log($"[{_modName}] Init: {ex.Message}");
        }
    }

    private DefeatScenario DefeatScenariosContainer_ChooseScenario(On.DefeatScenariosContainer.orig_ChooseScenario orig, DefeatScenariosContainer self)
    {
        DefeatScenario d = orig(self);
        OLogger.Log($"ChooseScenario={d}");
        return d;
    }

    private void DefeatScenariosManager_ActivateDefeatScenario1(On.DefeatScenariosManager.orig_ActivateDefeatScenario orig, DefeatScenariosManager self, DefeatScenario _scenario)
    {
        OLogger.Log("ActivateDefeatScenario1");
        orig(self, null);
    }

    private void DefeatScenariosContainer_AwakeInit(On.DefeatScenariosContainer.orig_AwakeInit orig, DefeatScenariosContainer self)
    {
        orig(self);
        try
        {
            AreaManager.AreaEnum areaN = (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            if (areaN != AreaManager.AreaEnum.CierzoOutside &&
               areaN != AreaManager.AreaEnum.Abrassar &&
               areaN != AreaManager.AreaEnum.Emercar &&
               areaN != AreaManager.AreaEnum.HallowedMarsh &&
               areaN != AreaManager.AreaEnum.Tutorial &&
               areaN != AreaManager.AreaEnum.CierzoDungeon)
            {
                //OLogger.Log($"{areaN}");
                DefeatTable m_tbl = (DefeatTable)AccessTools.Field(typeof(DefeatScenariosContainer), "m_eventChances").GetValue(self);
                List<RandomChance> lstCh = new List<RandomChance>((List<RandomChance>)AccessTools.Field(typeof(DefeatTable), "m_chances").GetValue(m_tbl));
                //OLogger.Log($"DefeatScenarioRespawn_CNT={lstCh.Count(c => c.Element is DefeatScenarioRespawn && c.HasValidConditions())}");
                if (lstCh.Count(c => (c.Element is DefeatScenarioRespawn || c.Element is DefeatScenarioChangeScene && (c.Element as DefeatScenarioChangeScene).Area == areaN)
                    && c.HasValidConditions()) == 0)
                {
                    return;
                }
                m_tbl.Clear();
                foreach (var item in lstCh)
                {
                    //OLogger.Log($"   {item.Element.name} {(item.Element is DefeatScenarioChangeScene ? (item.Element as DefeatScenarioChangeScene).Area.ToString() : "")}");
                    if (item.Element is DefeatScenarioRespawn || item.Element is DefeatScenarioChangeScene && (item.Element as DefeatScenarioChangeScene).Area == areaN)
                    {
                        //OLogger.Log($"{item.ChanceReduction} {item.ConditionCount} {item.InitialChance} {item.Chance}");
                        m_tbl.AddChance(item);
                    }
                }
                /*DefeatScenario d = m_tbl.Throw();
                if (d == null)
                {
                    OLogger.Log($"RESET");
                    m_tbl.Clear();
                    foreach (var item in lstCh)
                    {
                        m_tbl.AddChance(item);
                    }
                }
                else
                {
                    OLogger.Log($"SEL={d.GetType().Name}");
                }*/
            }
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }

    private void DefeatScenariosContainer_StartInit(On.DefeatScenariosContainer.orig_StartInit orig, DefeatScenariosContainer self)
    {
        orig(self);
        try
        {
            DictionaryExt<string, DefeatScenario> m_events = (DictionaryExt<string, DefeatScenario>)AccessTools.Field(typeof(DefeatScenariosContainer), "m_events").GetValue(self);
            //OLogger.Log($"Count={m_events.Count}");
            for (int i = 0; i < m_events.Count; i++)
            {
                OLogger.Log($"{i}={m_events.Values[i].GetType().Name}");
                DefeatScenario _scenario = m_events.Values[i];
                if (_scenario is DefeatScenarioRespawn)
                {
                    //OLogger.Log($"DefeatScenarioRespawn");
                    DefeatScenarioRespawn test = _scenario as DefeatScenarioRespawn;
                    /*OLogger.Log($"AffectBackpack={test.AffectBackpack}");
                    OLogger.Log($"RespawnAnimation={test.RespawnAnimation}");
                    OLogger.Log($"SpawnSelectionType={test.SpawnSelectionType}");
                    OLogger.Log($"m_respawnTimeLapse={(RespawnTimeLapseType)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_respawnTimeLapse").GetValue(test)}");*/
                    StatAffect s;
                    ChangeType c;
                    Vector2 r;
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_burntHealthModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_burntHealthModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_burntManaModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_burntManaModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_burntStaminaModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_burntStaminaModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_drinkModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_drinkModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_foodModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_foodModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_healthModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_healthModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_manaModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_manaModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_sleepModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_sleepModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_staminaModifer").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_staminaModifier={c} - {r}");
                    s = (StatAffect)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_temperatureModifier").GetValue(test);
                    c = (ChangeType)AccessTools.Field(typeof(StatAffect), "m_changeType").GetValue(s);
                    r = (Vector2)AccessTools.Field(typeof(StatAffect), "m_ratio").GetValue(s);
                    if (!s.IsUnchanged) OLogger.Log($"m_temperatureModifier={c} - {r}");

                    //OLogger.Log($"ContractedStatuses_CNT={((StatusEffectContraction[])AccessTools.Field(typeof(DefeatScenarioRespawn), "ContractedStatuses").GetValue(test)).Length}");
                    foreach (var item in (StatusEffectContraction[])AccessTools.Field(typeof(DefeatScenarioRespawn), "ContractedStatuses").GetValue(test))
                    {
                        StatusEffect se = (StatusEffect)AccessTools.Field(typeof(StatusEffectContraction), "m_statusEffect").GetValue(item);
                        int m_chancesToContract = (int)AccessTools.Field(typeof(StatusEffectContraction), "m_chancesToContract").GetValue(item);
                        OLogger.Log($"Status={se} ({m_chancesToContract}%)");
                    }

                    /*OLogger.Log($"m_addedQuestEvents_CNT={((QuestEventReference[])AccessTools.Field(typeof(DefeatScenarioRespawn), "m_addedQuestEvents").GetValue(test)).Length}");
                    OLogger.Log($"m_defeatSpawns_CNT={((DefeatSpawn[])AccessTools.Field(typeof(DefeatScenarioRespawn), "m_defeatSpawns").GetValue(test)).Length}");
                    foreach (var item in (DefeatSpawn[])AccessTools.Field(typeof(DefeatScenarioRespawn), "m_defeatSpawns").GetValue(test))
                    {
                        OLogger.Log($"   {item.DefeatScenario.GetType().Name}");
                    }

                    OLogger.Log($"m_nextTimeSlots_CNT={((EnvironmentConditions.TimeOfDayTimeSlot[])AccessTools.Field(typeof(DefeatScenarioRespawn), "m_nextTimeSlots").GetValue(test)).Length}");
                    OLogger.Log($"m_removedQuestEvents_CNT={((QuestEventReference[])AccessTools.Field(typeof(DefeatScenarioRespawn), "m_removedQuestEvents").GetValue(test)).Length}");

                    /*OLogger.Log($"m_hourRange={(Vector2)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_hourRange").GetValue(test)}");
                    OLogger.Log($"m_lastSpawn={(int)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_lastSpawn").GetValue(test)}");
                    OLogger.Log($"m_nextTimeSlotJumpRange={(Vector2)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_nextTimeSlotJumpRange").GetValue(test)}");*/
                    //OLogger.Log($"m_samePlacePos={(Vector3)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_samePlacePos").GetValue(test)}");
                    //OLogger.Log($"m_samePlaceSpawnIndex={(int)AccessTools.Field(typeof(DefeatScenarioRespawn), "m_samePlaceSpawnIndex").GetValue(test)}");
                }
            }
        }
        catch (Exception ex)
        {
            OLogger.Error(ex.Message);
        }
    }

    private void CharacterManager_LoadAiCharactersFromSave(On.CharacterManager.orig_LoadAiCharactersFromSave orig, CharacterManager self, CharacterSaveData[] _saves)
    {
        orig(self, _saves);
        if (self.PlayerCharacters.Count == 0) return;
        Character player = self.Characters[CharacterManager.Instance.PlayerCharacters.Values[0]];
        //OLogger.Log($"Engaged={ player.EngagedCharacters.Count}");
        for (int i = 0; i < self.Characters.Count; i++)
        {
            Character character = self.Characters.Values[i];
            //OLogger.Log($"AddChar={_char.Name} {_char.IsAlly(player)} {_char.IsDead}");
            if (!character.IsLocalPlayer && !character.IsAlly(player) && !character.IsDead)
            {
                //OLogger.Log($"Reset={character.Name} ({character.Health}/{character.Stats.MaxHealth})");
                character.ResetStats();
                character.StatusEffectMngr.RemoveShortStatuses(); // Remove debuffs
            }

        }
    }

    private void DefeatScenariosManager_ActivateDefeatScenario(On.DefeatScenariosManager.orig_ActivateDefeatScenario orig, DefeatScenariosManager self, DefeatScenario _scenario)
    {
        try
        {
            AreaManager.AreaEnum areaN = (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            if (_scenario is DefeatScenarioRespawn)
            {
                OLogger.Log($"DefeatScenarioRespawn");
            }
            //OLogger.Log($"ActivateDefeatScenario={SceneManagerHelper.ActiveSceneName}");
            if (areaN != AreaManager.AreaEnum.CierzoOutside &&
               areaN != AreaManager.AreaEnum.Abrassar &&
               areaN != AreaManager.AreaEnum.Emercar &&
               areaN != AreaManager.AreaEnum.HallowedMarsh &&
               areaN != AreaManager.AreaEnum.Tutorial &&
               areaN != AreaManager.AreaEnum.CierzoDungeon /*&&
               (_scenario is DefeatScenarioChangeScene)*/)
            {
                OLogger.Log($"FailSafeDefeat!");

                /*_scenario = new DefeatScenarioRespawn();
                (_scenario as DefeatScenarioRespawn).AffectBackpack = false;
                (_scenario as DefeatScenarioRespawn).RespawnAnimation = SpellCastType.GetUpBelly;
                (_scenario as DefeatScenarioRespawn).SpawnSelectionType = SpawnSelectionTypes.Random;
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_respawnTimeLapse").SetValue(_scenario, RespawnTimeLapseType.Hour);

                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_burntHealthModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.3f, 0.4f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_burntManaModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.0f, 0.0f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_burntStaminaModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.0f, 0.0f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_drinkModifier").SetValue(_scenario, new StatAffect(ChangeType.Multiplier, new Vector2(1.5f, 2.5f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_foodModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.0f, 0.0f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_healthModifier").SetValue(_scenario, new StatAffect(ChangeType.TargetRangeRatio, new Vector2(0.4f, 0.5f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_manaModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.0f, 0.0f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_sleepModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.0f, 0.0f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_staminaModifier").SetValue(_scenario, new StatAffect(ChangeType.Unchanged, new Vector2(0.0f, 0.0f)));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_temperatureModifier").SetValue(_scenario, new StatAffect(ChangeType.TargetRangeRatio, new Vector2(0.5f, 0.5f)));

                //AccessTools.Field(typeof(DefeatScenarioRespawn), "ContractedStatuses").SetValue(_scenario, new StatAffect(ChangeType.TargetRangeRatio, new Vector2(0.5f, 0.5f)));

                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_hourRange").SetValue(_scenario, new Vector2(8.0f, 12.0f));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_lastSpawn").SetValue(_scenario, 0);
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_nextTimeSlotJumpRange").SetValue(_scenario, new Vector2(1.0f, 1.0f));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_samePlacePos").SetValue(_scenario, new Vector3(0.0f, 0.0f, 0.0f));
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_samePlaceSpawnIndex").SetValue(_scenario, -1);
                AccessTools.Field(typeof(DefeatScenarioRespawn), "m_respawnTimeLapse").SetValue(_scenario, DefeatScenarioRespawn.RespawnTimeLapseType.Hour);
                orig(self, _scenario);
                return;*/

                SendNotificationToAllPlayers($"{SceneManagerHelper.ActiveSceneName}");
                //self.FailSafeDefeat();
                for (int i = 0; i < CharacterManager.Instance.PlayerCharacters.Count; i++)
                {
                    Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[i]);
                    if (!(bool)character) continue;
                    character.StatusEffectMngr.Purge();
                    character.Stats.RefreshVitalMaxStat();
                    PlayerSaveData playerSaveData = new PlayerSaveData(character);
                    /*playerSaveData.BurntHealth += character.Stats.MaxHealth * 0.10f; // Reduce burnt health by 10%
                    playerSaveData.BurntHealth = Mathf.Clamp(playerSaveData.BurntHealth, 0f, character.Stats.MaxHealth * 0.9f);
                    playerSaveData.Health = character.Stats.MaxHealth;// Mathf.Clamp(playerSaveData.Health, character.Stats.MaxHealth * 0.1f, character.Stats.MaxHealth);
                    playerSaveData.BurntStamina += character.Stats.MaxStamina * 0.10f; // Reduce burnt stamina by 10%
                    playerSaveData.BurntStamina = Mathf.Clamp(playerSaveData.BurntStamina, 0f, character.Stats.MaxStamina * 0.9f);
                    playerSaveData.Stamina = character.Stats.MaxStamina; // Mathf.Clamp(playerSaveData.Stamina, character.Stats.MaxStamina * 0.1f, character.Stats.MaxStamina);
                    playerSaveData.BurntMana = Mathf.Clamp(playerSaveData.BurntMana, 0f, character.Stats.MaxMana * 0.9f);*/

                    StatAffect m_burntHealthModifier = new StatAffect(ChangeType.TargetRangeRatio, new Vector2(0.7f, 0.8f));
                    playerSaveData.BurntHealth = Mathf.Clamp(m_burntHealthModifier.ProcessStat(character.Stats.MaxHealth), 0f, character.Stats.MaxHealth);
                    StatAffect m_healthModifier = new StatAffect(ChangeType.TargetRangeRatio, new Vector2(0.3f, 0.4f));
                    playerSaveData.Health = Mathf.Clamp(m_healthModifier.ProcessStat(character.Stats.MaxHealth), 0f, character.Stats.MaxHealth);


                    character.Resurrect(playerSaveData, _playAnim: true);
                    character.PlayerStats.RestLastNeedsUpdateTime();
                    character.ResetCombat();
                }
                NetworkLevelLoader.Instance.RequestReloadLevel(0);
                return;
            }
        }
        catch (Exception ex)
        {
            OLogger.Log($"[{_modName}] DefeatScenariosManager_ActivateDefeatScenario: {ex.Message}");
            Debug.Log($"[{_modName}] DefeatScenariosManager_ActivateDefeatScenario: {ex.Message}");
        }
        orig(self, _scenario);
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
