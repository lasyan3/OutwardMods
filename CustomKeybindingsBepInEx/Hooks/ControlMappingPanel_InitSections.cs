using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using UnityEngine;
using static CustomKeybindings;

namespace Hooks
{
    [HarmonyPatch(typeof(ControlMappingPanel), "InitSections")]
    public class ControlMappingPanel_InitSections
    {
        [HarmonyPrefix]
        public static bool InitSections( ControllerMap _controllerMap)
        {
            MyLogger.LogDebug("InitSections");
            // Loop through our custom actions we added via Rewired
            foreach (int myActionId in CustomKeybindings.myCustomActionIds.Keys)
            {

                // The info that the user specified for this action
                CustomKeybindings.InputActionDescription myActionDescription = CustomKeybindings.myCustomActionIds[myActionId];

                // There are separate keybinding maps for keyboard, mouse, & controllers
                // We only add our action-to-element mappings to the keybind maps that make sense
                // For example, if you are adding a key that doesn't make sense to have on a controller,
                // then skip when _controllerMap is JoystickMap
                //
                // (Optional)
                // You can check if this method is being called for the Keyboard/Mouse bindings panel or
                // the Controller bindings panel, but I prefer to check the class of the _controllerMap
                //   if (self.ControllerType == ControlMappingPanel.ControlType.Keyboard) {
                //

                bool shouldLog = false;
                if (shouldLog)
                {
                    MyLogger.LogDebug("_controllerMap is keyboard or mouse: " + (_controllerMap is KeyboardMap || _controllerMap is MouseMap));
                    MyLogger.LogDebug("_controllerMap is joystick: " + (_controllerMap is JoystickMap));
                    MyLogger.LogDebug("_controllerMap.categoryId: " + _controllerMap.categoryId);
                    MyLogger.LogDebug("action is keyboard: " + (myActionDescription.controlType == ControlType.Keyboard));
                    MyLogger.LogDebug("action is gamepad: " + (myActionDescription.controlType == ControlType.Gamepad));
                    MyLogger.LogDebug("action is both: " + (myActionDescription.controlType == ControlType.Both));
                    MyLogger.LogDebug("action.sectionId: " + myActionDescription.sectionId);
                }

                // If the controller map's control type does not match our action
                if (!(myActionDescription.controlType == ControlType.Keyboard && (_controllerMap is KeyboardMap || _controllerMap is MouseMap) ||
                      myActionDescription.controlType == ControlType.Gamepad && (_controllerMap is JoystickMap) ||
                      myActionDescription.controlType == ControlType.Both))
                {
                    // Then skip to next action
                    continue;
                }

                // If the categoryId of this controller map does not match our action's
                if (_controllerMap.categoryId != myActionDescription.sectionId)
                {
                    // Skip to next action
                    continue;
                }

                // If we pass the tests, create & add the action-to-element map for this particular action
                _controllerMap.CreateElementMap(myActionId, Pole.Positive, KeyCode.None, ModifierKeyFlags.None);

                // Continue the loop...
            }

            // We're done here. Call original implementation
            return true;
        }
    }
}
