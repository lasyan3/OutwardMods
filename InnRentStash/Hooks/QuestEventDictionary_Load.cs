using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnRentStash.Hooks
{
    [HarmonyPatch(typeof(QuestEventDictionary), "Load")]
    public class QuestEventDictionary_Load
    {
        [HarmonyPostfix]
        public static void Load()
        {
            try
            {
                // Get PlayerHouse Event UIDs
                foreach (var sect in QuestEventDictionary.Sections)
                {
                    foreach (var qevt in sect.Events)
                    {
                        foreach (var dicStash in InnRentStash.StashAreaToQuestEvent)
                        {
                            if (qevt.EventName == $"PlayerHouse_{dicStash.Key}_HouseAvailable")
                            {
                                InnRentStash.StashAreaToQuestEvent[dicStash.Key].PlayerHouseQuestEventUID = qevt.EventUID;
                                //InnRentStash.MyLogger.LogDebug($" > {dicStash.Key}={qevt.EventUID}");
                            }
                        }
                    }
                }

                // Add Rent Events
                QuestEventFamily innSection = QuestEventDictionary.Sections.FirstOrDefault(s => s.Name == "Inns");
                if (innSection != null)
                {
                    foreach (StrRent item in InnRentStash.StashAreaToQuestEvent.Values)
                    {
                        if (!innSection.Events.Contains(item.QuestEvent))
                        {
                            //InnRentStash.MyLogger.LogDebug($"Add QuestEvent: {item.QuestEvent.EventName}");
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
                InnRentStash.MyLogger.LogError("Load: " + ex.Message);
            }
        }
    }
}
