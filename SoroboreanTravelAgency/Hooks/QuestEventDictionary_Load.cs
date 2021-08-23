using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardMods.Hooks
{
    [HarmonyPatch(typeof(QuestEventDictionary), "Load")]
    public class QuestEventDictionary_Load
    {
        [HarmonyPostfix]
        public static void Load()
        {
            try
            {
                // Add Fast Travel events
                /*foreach (var item in QuestEventDictionary.Sections)
                {
                    OLogger.Log(item.Name);
                }*/
                QuestEventFamily innSection = QuestEventDictionary.Sections.FirstOrDefault(s => s.Name == "Neutral_General");
                if (innSection != null)
                {
                    foreach (var item in SoroboreanTravelAgency.AreaToQuestEvent)
                    {
                        if (!innSection.Events.Contains(item.Value))
                        {
                            innSection.Events.Add(item.Value);
                        }
                        if (QuestEventDictionary.GetQuestEvent(item.Value.EventUID) == null)
                        {
                            QuestEventDictionary.RegisterEvent(item.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
            }
        }
    }
}
