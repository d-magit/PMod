using MelonLoader;
using Client.Utils;
using UIExpansionKit.API;
using UnhollowerRuntimeLib;
using UnityEngine;
using Harmony;
using System;

namespace Client
{
    public static class BuildInfo
    {
        public const string Name = "Personal Client";
        public const string Author = "Me";
        public const string Version = "1.0.1";
    }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        public static HarmonyInstance HarmonyInstance => Instance.Harmony;
        public static ICustomShowableLayoutedMenu ClientMenu;
        public static EnableDisableListener listener;

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            CopyAsset.OnApplicationStart();
            ItemGrabber.OnApplicationStart();
            Orbit.OnApplicationStart();
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Personal Client", () => ShowClientMenu());
            MelonLogger.Msg(ConsoleColor.Red, "Personal Client Loaded Successfully!");
        }

        private static void ShowClientMenu()
        {
            ClientMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            ClientMenu.AddSimpleButton("Orbit", () => 
            {
                if (Orbit.IsOn) Orbit.OrbitMenu.Show();
                else VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Orbit", $"You have to first activate the mod on Melon Preferences menu! Be aware that this is a risky function.", "Close", new Action(() => { VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Private_Void_PDM_0(); }));
            });
            ClientMenu.AddSimpleButton("ItemGrabber", () =>
            {
                if (ItemGrabber.IsOn) ItemGrabber.PickupMenu.Show();
                else VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("ItemGrabber", $"You have to first activate the mod on Melon Preferences menu! Be aware that this is a risky function.", "Close", new Action(() => { VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Private_Void_PDM_0(); }));
            });
            ClientMenu.Show();
        }

        public override void VRChat_OnUiManagerInit()
        {
            listener = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu").AddComponent<EnableDisableListener>();
            NetworkEvents.NetworkInit();
            CopyAsset.OnUiManagerInit();
            ForceClone.OnUiManagerInit();
            Orbit.NetworkHook();
        }

        public override void OnPreferencesSaved()
        {
            CopyAsset.OnPreferencesSaved();
            ItemGrabber.OnPreferencesSaved();
            Orbit.OnPreferencesSaved();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            EmmAllower.OnSceneWasLoaded();
            ItemGrabber.OnSceneWasLoaded();
        }

        public override void OnUpdate()
        {
            ForceClone.OnUpdate();
            Orbit.OnUpdate();
        }
    }
}