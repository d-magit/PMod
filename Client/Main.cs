using Client.Functions;
using Client.Functions.Utils;
using System;
using Harmony;
using UnhollowerRuntimeLib;
using MelonLoader;
using UIExpansionKit.API;
using System.Collections;

namespace Client
{
    public static class BuildInfo
    {
        public const string Name = "Personal Client";
        public const string Author = "Me";
        public const string Version = "1.1.1";
    }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        public static HarmonyInstance HarmonyInstance => Instance.Harmony;
        public static EnableDisableListener listener;
        public static ICustomShowableLayoutedMenu ClientMenu;

        public override void OnApplicationStart()
        {
            Instance = this;
            MelonCoroutines.Start(WaitForUIInit());
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            NativePatches.OnApplicationStart();
            FrozenPlayersManager.OnApplicationStart();
            ItemGrabber.OnApplicationStart();
            Triggers.OnApplicationStart();
            Orbit.OnApplicationStart();
            PhotonFreeze.OnApplicationStart();
            UserInteractUtils.OnApplicationStart();
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Personal Client", () => ShowClientMenu());
            MelonLogger.Msg(ConsoleColor.Red, "Personal Client Loaded Successfully!");
        }

        private static void ShowClientMenu()
        {
            ClientMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            ClientMenu.AddSimpleButton("Close Menu", ClientMenu.Hide);
            ClientMenu.AddSimpleButton("Orbit", () => 
            {
                if (Orbit.IsOn) Orbit.OrbitMenu.Show();
                else RiskyFuncAlert("Orbit");
            });
            ClientMenu.AddSimpleButton("ItemGrabber", () =>
            {
                if (ItemGrabber.IsOn) ItemGrabber.PickupMenu.Show();
                else RiskyFuncAlert("ItemGrabber");
            });
            ClientMenu.AddSimpleButton("PhotonFreeze", () => 
            {
                if (PhotonFreeze.IsOn) PhotonFreeze.ShowFreezeMenu();
                else RiskyFuncAlert("PhotonFreeze");
            });
            ClientMenu.AddSimpleButton("Triggers", () => Triggers.ShowTriggersMenu());
            ClientMenu.Show();
        }

        public static void RiskyFuncAlert(string FuncName) => Methods.PopupV2(
            FuncName,
            "You have to first activate the mod on Melon Preferences menu! Be aware that this is a risky function.",
            "Close",
            new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); }));

        public static IEnumerator WaitForUIInit()
        {
            while (QuickMenu.prop_QuickMenu_0 == null)
                yield return null;

            listener = QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu").gameObject.AddComponent<EnableDisableListener>();
            NetworkEvents.OnUiManagerInit();
            AvatarFromID.OnUiManagerInit();
            ForceClone.OnUiManagerInit();
            Orbit.OnUiManagerInit();
            UserInteractUtils.OnUiManagerInit();
            yield break;
        }

        public override void OnPreferencesSaved()
        {
            ItemGrabber.OnPreferencesSaved();
            Triggers.OnPreferencesSaved();
            Orbit.OnPreferencesSaved();
            PhotonFreeze.OnPreferencesSaved();
            UserInteractUtils.OnPreferencesSaved();
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