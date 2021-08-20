using PMod.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;
using MelonLoader;
using UIExpansionKit.API;
using VRC;
using PMod.Loader;

[assembly: AssemblyTitle(PMod.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + PMod.BuildInfo.Author)]
[assembly: AssemblyVersion(PMod.BuildInfo.Version)]
[assembly: AssemblyFileVersion(PMod.BuildInfo.Version)]

namespace PMod
{
    public static class BuildInfo
    {
        public const string Name = "PMod";
        public const string Author = "Me";
        public const string Version = "1.1.6";
    }

    public static class PMod
    {
        internal static HarmonyLib.Harmony HInstance => MelonHandler.Mods.First(m => m.Info.Name == Info.Name).HarmonyInstance;
        internal static EnableDisableListener listener;
        internal static ICustomShowableLayoutedMenu ClientMenu;

        public static void OnApplicationStart()
        {
            ModulesManager.Initialize();
            NativePatches.OnApplicationStart();
            NetworkEvents.OnPlayerLeft += OnPlayerLeft;
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            PLogger.Msg(ConsoleColor.DarkMagenta, $"{BuildInfo.Name} Loaded Successfully!");
        }

        public static void VRChat_OnUiManagerInit()
        {
            try
            {
                ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton($"{BuildInfo.Name}", () => ShowClientMenu());
                listener = QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu").gameObject.AddComponent<EnableDisableListener>();
                ModulesManager.OnUiManagerInit();
                NetworkEvents.OnUiManagerInit();
            }
            catch (Exception e)
            {
                PLogger.Warning("Failed to initialize mod!");
                PLogger.Error(e);
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName) => ModulesManager.OnSceneWasLoaded(buildIndex, sceneName);

        public static void OnUpdate() => ModulesManager.OnUpdate();

        public static void OnPreferencesSaved() => ModulesManager.OnPreferencesSaved();
        public static void OnFixedUpdate() { }
        public static void OnLateUpdate() { }
        public static void OnGUI() { }
        public static void OnApplicationQuit() { }
        public static void OnPreferencesLoaded() { }
        public static void OnSceneWasInitialized(int buildIndex, string sceneName) { }

        internal static void OnPlayerJoined(Player player) => ModulesManager.OnPlayerJoined(player);

        internal static void OnPlayerLeft(Player player) => ModulesManager.OnPlayerLeft(player);

        internal static void RiskyFuncAlert(string FuncName) => Methods.PopupV2(
            FuncName,
            "You have to first activate the mod on Melon Preferences menu! Be aware that this is a risky function.",
            "Close",
            new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); }));

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
    }
}