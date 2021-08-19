using Client.Utils;
using System;
using UnhollowerRuntimeLib;
using MelonLoader;
using UIExpansionKit.API;
using System.Collections;
using System.Reflection;
using VRC;

[assembly: AssemblyTitle(Client.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Client.BuildInfo.Author)]
[assembly: AssemblyVersion(Client.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Client.BuildInfo.Version)]
[assembly: MelonInfo(typeof(Client.Main), Client.BuildInfo.Name, Client.BuildInfo.Version, Client.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Client
{
    public static class BuildInfo
    {
        public const string Name = "Personal Client";
        public const string Author = "Me";
        public const string Version = "1.1.5";
    }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        internal static HarmonyLib.Harmony HInstance => Instance.HarmonyInstance;
        internal static EnableDisableListener listener;
        internal static ICustomShowableLayoutedMenu ClientMenu;

        public override void OnApplicationStart()
        {
            Instance = this;
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
            NetworkEvents.OnPlayerLeft += OnPlayerLeft;
            MelonCoroutines.Start(WaitForUIInit());
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            ModulesManager.Initialize();
            NativePatches.OnApplicationStart();
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Personal Client", () => ShowClientMenu());
            MelonLogger.Msg(ConsoleColor.Red, "Personal Client Loaded Successfully!");
        }

        internal static IEnumerator WaitForUIInit()
        {
            while (QuickMenu.prop_QuickMenu_0 == null)
                yield return null;

            listener = QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu").gameObject.AddComponent<EnableDisableListener>();
            ModulesManager.OnUiManagerInit();
            NetworkEvents.OnUiManagerInit();

            yield break;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) => ModulesManager.OnSceneWasLoaded(buildIndex, sceneName);

        public override void OnUpdate() => ModulesManager.OnUpdate();

        public override void OnPreferencesSaved() => ModulesManager.OnPreferencesSaved();

        internal static void OnPlayerJoined(Player player) => ModulesManager.OnPlayerJoined(player);

        internal static void OnPlayerLeft(Player player) => ModulesManager.OnPlayerLeft(player);

        private static void ShowClientMenu()
        {
            ClientMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            ClientMenu.AddSimpleButton("Close Menu", ClientMenu.Hide);
            ClientMenu.AddSimpleButton("Orbit", () =>
            {
                if (ModulesManager.orbit.IsOn.Value) ModulesManager.orbit.OrbitMenu.Show();
                else RiskyFuncAlert("Orbit");
            });
            ClientMenu.AddSimpleButton("ItemGrabber", () =>
            {
                if (ModulesManager.itemGrabber.IsOn.Value) ModulesManager.itemGrabber.PickupMenu.Show();
                else RiskyFuncAlert("ItemGrabber");
            });
            ClientMenu.AddSimpleButton("PhotonFreeze", () =>
            {
                if (ModulesManager.photonFreeze.IsOn.Value) ModulesManager.photonFreeze.ShowFreezeMenu();
                else RiskyFuncAlert("PhotonFreeze");
            });
            ClientMenu.AddSimpleButton("Triggers", () => ModulesManager.triggers.ShowTriggersMenu());
            ClientMenu.Show();
        }

        internal static void RiskyFuncAlert(string FuncName) => Methods.PopupV2(
            FuncName,
            "You have to first activate the mod on Melon Preferences menu! Be aware that this is a risky function.",
            "Close",
            new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); }));

    }
}