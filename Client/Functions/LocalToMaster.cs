using Client.Functions.Utils;
using System;
using MelonLoader;
using UIExpansionKit.API;

namespace Client.Functions
{
    // Incomplete AF
    internal static class LocalToMaster
    {
        private static ICustomShowableLayoutedMenu LocalToMasterMenu;
        public static bool IsOn;
        public static bool IsForceMaster = false;

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("LocalToMaster", "Local To Master");
            MelonPreferences.CreateEntry("LocalToMaster", "IsOn", false, "Activate Mod? This is a risky function.");
        }

        public static void OnPreferencesSaved()
        {
            IsOn = MelonPreferences.GetEntryValue<bool>("LocalToMaster", "IsOn");
        }
        public static void ShowLocalToMasterMenu()
        {
            LocalToMasterMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            LocalToMasterMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            if (Utilities.IsSDK2World()) LocalToMasterMenu.AddToggleButton("Force Local to Master - On", (_) => IsForceMaster = !IsForceMaster, () => IsForceMaster);
            else LocalToMasterMenu.AddSimpleButton("Force Local to Master - Off", () => Methods.PopupV2(
                "LocalToMaster", 
                "Sorry, this world is an SDK3 Udon world. This function won't work in here.", 
                "Close", 
                new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); })));
            LocalToMasterMenu.Show();
        }
    }
}
