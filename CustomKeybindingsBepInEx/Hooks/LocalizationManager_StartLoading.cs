using HarmonyLib;
using Rewired;
using System.Collections.Generic;
using System.Reflection;
using static CustomKeybindings;

namespace Hooks
{
    [HarmonyPatch(typeof(LocalizationManager), "StartLoading")]
    public class LocalizationManager_StartLoading
    {
        [HarmonyPostfix]
        public static void StartLoading(LocalizationManager __instance)
        {
            // Nab the localization dictionary that's used for keybind localization
            Dictionary<string, string> m_generalLocalization = (Dictionary<string, string>)__instance.GetType().GetField("m_generalLocalization", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            // Go through the added actions and use the user-created action descriptions to name them
            foreach (int myActionId in CustomKeybindings.myCustomActionIds.Keys)
            {
                InputAction myAction = ReInput.mapping.GetAction(myActionId);
                InputActionDescription myActionDescription = CustomKeybindings.myCustomActionIds[myActionId];

                // The first string is the technical name of the action, while the second string is what you want to display in the bindings menu
                m_generalLocalization.Add("InputAction_" + myAction.name, myActionDescription.name);
            }
        }
    }
}
