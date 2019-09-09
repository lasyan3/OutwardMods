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
            On.DefeatScenariosManager.ActivateDefeatScenario += DefeatScenariosManager_ActivateDefeatScenario;

            //OLogger.Log("DisableDungeonsDefeatScenarios is enabled");
        }
        catch (Exception ex)
        {
            Debug.Log($"[{_modName}] Init: {ex.Message}");
        }
    }

    private void DefeatScenariosManager_ActivateDefeatScenario(On.DefeatScenariosManager.orig_ActivateDefeatScenario orig, DefeatScenariosManager self, DefeatScenario _scenario)
    {
        try
        {
            AreaManager.AreaEnum areaN = (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(SceneManagerHelper.ActiveSceneName);
            //OLogger.Log($"ActivateDefeatScenario={SceneManagerHelper.ActiveSceneName}");
            if (areaN != AreaManager.AreaEnum.CierzoOutside &&
               areaN != AreaManager.AreaEnum.Abrassar &&
               areaN != AreaManager.AreaEnum.Emercar &&
               areaN != AreaManager.AreaEnum.HallowedMarsh)
            {
                //OLogger.Log($"FailSafeDefeat!");
                SendNotificationToAllPlayers($"{SceneManagerHelper.ActiveSceneName}");
                //self.FailSafeDefeat();
                for (int i = 0; i < CharacterManager.Instance.PlayerCharacters.Count; i++)
                {
                    Character character = CharacterManager.Instance.GetCharacter(CharacterManager.Instance.PlayerCharacters.Values[i]);
                    if (!(bool)character) continue;
                    character.StatusEffectMngr.Purge();
                    character.Stats.RefreshVitalMaxStat();
                    PlayerSaveData playerSaveData = new PlayerSaveData(character);
                    playerSaveData.BurntHealth = Mathf.Clamp(playerSaveData.BurntHealth, 0f, character.Stats.MaxHealth * 0.9f);
                    playerSaveData.Health = character.Stats.MaxHealth;// Mathf.Clamp(playerSaveData.Health, character.Stats.MaxHealth * 0.1f, character.Stats.MaxHealth);
                    playerSaveData.BurntStamina = Mathf.Clamp(playerSaveData.BurntStamina, 0f, character.Stats.MaxStamina * 0.9f);
                    playerSaveData.Stamina = character.Stats.MaxStamina; // Mathf.Clamp(playerSaveData.Stamina, character.Stats.MaxStamina * 0.1f, character.Stats.MaxStamina);
                    playerSaveData.BurntMana = Mathf.Clamp(playerSaveData.BurntMana, 0f, character.Stats.MaxMana * 0.9f);
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
