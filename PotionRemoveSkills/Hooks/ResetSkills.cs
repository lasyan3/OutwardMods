using HarmonyLib;
using System;

namespace EraserElixir.Hooks
{
    [HarmonyPatch(typeof(Item), "OnUse")]
    public class RemoveSkillsOnUse
    {
        [HarmonyPrefix]
        public static bool OnUse(Item __instance, Character _targetChar)
        {
            try
            {
                if (_targetChar != null && __instance.ItemID == 4300191)
                {
                    SkillSchool[] m_skillTrees = (SkillSchool[])AccessTools.Field(typeof(SkillTreeHolder), "m_skillTrees").GetValue(SkillTreeHolder.Instance);
                    int fgSkills = 0;
                    foreach (var school in m_skillTrees)
                    {
                        //PotionResetSkills.Instance.MyLogger.LogDebug($"{school.Name}");
                        int idxBreakThrough = 42;
                        foreach (var skill in school.SkillSlots)
                        {
                            if (skill is SkillSlot)
                            {
                                if (skill.IsBreakthrough)
                                {
                                    idxBreakThrough = school.SkillSlots.IndexOf(skill);
                                }
                                SkillSlot ssk = (SkillSlot)skill;
                                if (_targetChar.Inventory.LearnedSkill(ssk.Skill) && school.SkillSlots.IndexOf(skill) >= idxBreakThrough)
                                {
                                    //PotionResetSkills.Instance.MyLogger.LogDebug($" > {ssk.ColumnIndex}:{ssk.ParentBranch.Index}:{ssk.Skill.Name}");
                                    Item it = _targetChar.Inventory.SkillKnowledge.GetItemFromItemID(ssk.Skill.ItemID);
                                    ItemManager.Instance.DestroyItem(it.UID);
                                    fgSkills++;
                                    if (skill.IsBreakthrough)
                                    {
                                        AccessTools.Field(typeof(PlayerCharacterStats), "m_usedBreakthroughCount").SetValue(_targetChar.PlayerStats, _targetChar.PlayerStats.UsedBreakthroughCount - 1);
                                    }
                                    //_targetChar.Inventory.AddMoney(ssk.RequiredMoney);
                                }
                            }
                            else if (skill is SkillSlotFork)
                            {
                                if (skill.IsBreakthrough)
                                {
                                    idxBreakThrough = school.SkillSlots.IndexOf(skill);
                                }
                                SkillSlotFork sskf = (SkillSlotFork)skill;
                                foreach (var ssk in sskf.SkillsToChooseFrom)
                                {
                                    if (_targetChar.Inventory.LearnedSkill(ssk.Skill) && school.SkillSlots.IndexOf(skill) >= idxBreakThrough)
                                    {
                                        //PotionResetSkills.Instance.MyLogger.LogDebug($" > {ssk.ColumnIndex}:{ssk.ParentBranch.Index}:{ssk.Skill.Name}");
                                        Item it = _targetChar.Inventory.SkillKnowledge.GetItemFromItemID(ssk.Skill.ItemID);
                                        ItemManager.Instance.DestroyItem(it.UID);
                                        fgSkills++;
                                        if (skill.IsBreakthrough)
                                        {
                                            AccessTools.Field(typeof(PlayerCharacterStats), "m_usedBreakthroughCount").SetValue(_targetChar.PlayerStats, _targetChar.PlayerStats.UsedBreakthroughCount - 1);
                                        }
                                        //_targetChar.Inventory.AddMoney(ssk.RequiredMoney);
                                    }
                                }
                            }
                        }
                    }
                    _targetChar.CharacterUI.ShowInfoNotification($"{fgSkills} skills forgotten!", __instance.ItemIcon);
                    __instance.RemoveQuantity(1);
                    return false;
                }
            }
            catch (Exception ex)
            {
                PotionResetSkills.Instance.MyLogger.LogError("Item_OnUse: " + ex.Message);
            }
            return true;
        }
    }
}
