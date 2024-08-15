using Client.Functions.Utils;
using System;
using MelonLoader;
using UIExpansionKit.API;

namespace Client.Functions
{
    internal static class LocalToGlobal
    {
        private static ICustomShowableLayoutedMenu LocalToGlobalMenu;
        public static bool IsOn;
        public static bool IsForceGlobal = false;

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("LocalToGlobal", "PM - Local To Global");
            MelonPreferences.CreateEntry("LocalToGlobal", "IsOn", false, "Activate Mod? This is a risky function.");
        }

        public static void OnPreferencesSaved()
        {
            IsOn = MelonPreferences.GetEntryValue<bool>("LocalToGlobal", "IsOn");
        }
        public static void ShowLocalToMasterMenu()
        {
            LocalToGlobalMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            LocalToGlobalMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            if (Utilities.IsSDK2World()) LocalToGlobalMenu.AddToggleButton("Force Local to Global - On", (_) => IsForceGlobal = !IsForceGlobal, () => IsForceGlobal);
            else LocalToGlobalMenu.AddSimpleButton("Force Local to Global - Off", () => Methods.PopupV2(
                "LocalToGlobal", 
                "Sorry, this world is an SDK3 Udon world.\nThis function won't work in here.", 
                "Close", 
                new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); })));
            LocalToGlobalMenu.Show();
        }
    }
}