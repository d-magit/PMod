using Client.Functions.Utils;
using System;
using MelonLoader;
using UIExpansionKit.API;
using VRC.SDKBase;
using UnityEngine;

namespace Client.Functions
{
    internal static class Triggers
    {
        private static bool IsForceGlobal = false;
        public static MelonPreferences_Entry<bool> IsOn;
        public static bool IsAlwaysForceGlobal = false;

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("LocalToGlobal", "PM - Local To Global");
            IsOn = MelonPreferences.CreateEntry("LocalToGlobal", "IsOn", false, "Activate Mod? This is a risky function.");
        }

        public static void ShowTriggersMenu()
        {
            ICustomShowableLayoutedMenu TriggersMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            TriggersMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            string btnName = "LocalToGlobal - ";
            Action action = null;
            if (IsOn.Value && Utils.Utilities.GetWorldSDKVersion() == Utils.Utilities.WorldSDKVersion.SDK2)
            {
                btnName += "On";
                action = () => ShowLocalToGlobalMenu();
            }
            else if (!IsOn.Value)
            {
                btnName += "Off";
                action = () => Main.RiskyFuncAlert("LocalToGlobal");
            }
            else
            {
                btnName += "Off";
                action = () => Methods.PopupV2(
                    "LocalToGlobal",
                    "Sorry, this world is an SDK3 Udon world.\nThis function won't work in here.",
                    "Close",
                    new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); }));
            }
            TriggersMenu.AddSimpleButton(btnName, action);
            foreach (VRC_Trigger trigger in Resources.FindObjectsOfTypeAll<VRC_Trigger>()) TriggersMenu.AddSimpleButton(trigger.name, () => TriggerMenu(trigger));
            TriggersMenu.Show();
        }

        private static void ShowLocalToGlobalMenu()
        {
            ICustomShowableLayoutedMenu LocalToGlobalMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            LocalToGlobalMenu.AddSimpleButton("Go back", () => ShowTriggersMenu());
            LocalToGlobalMenu.AddToggleButton("Always Force Local to Global", (_) => IsAlwaysForceGlobal = !IsAlwaysForceGlobal, () => IsAlwaysForceGlobal);
            LocalToGlobalMenu.AddToggleButton("Force Local to Global on Trigger", (_) => IsForceGlobal = !IsForceGlobal, () => IsForceGlobal);
            LocalToGlobalMenu.Show();
        }

        private static void TriggerMenu(VRC_Trigger trigger)
        {
            ICustomShowableLayoutedMenu TriggerMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
            TriggerMenu.AddSimpleButton("Go back", () => ShowTriggersMenu());
            foreach (VRC_Trigger.TriggerEvent @event in trigger.Triggers) TriggerMenu.AddSimpleButton(@event.Name, () => 
            { 
                try 
                { 
                    if (IsForceGlobal)
                        NativePatches.triggerOnce = true; 
                    trigger.ExecuteTrigger.Invoke(@event); 
                } catch { } 
            });
            TriggerMenu.Show();
        }
    }
}