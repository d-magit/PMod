using PMod.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using UnhollowerRuntimeLib;
using MelonLoader;
using UIExpansionKit.API;
using VRC;

[assembly: AssemblyTitle(PMod.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + PMod.BuildInfo.Author)]
[assembly: AssemblyVersion(PMod.BuildInfo.Version)]
[assembly: AssemblyFileVersion(PMod.BuildInfo.Version)]
[assembly: MelonInfo(typeof(PMod.Main), PMod.BuildInfo.Name, PMod.BuildInfo.Version, PMod.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace PMod
{
    public static class BuildInfo
    {
        public const string Name = "PMod";
        public const string Author = "Me";
        public const string Version = "1.1.6";
    }

    internal static class UIXManager 
    {
        public static void OnApplicationStart() => ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit;
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
            WaitForUiInit();
            ModulesManager.Initialize();
            NativePatches.OnApplicationStart();
            NetworkEvents.OnPlayerLeft += OnPlayerLeft;
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonLogger.Msg(ConsoleColor.Red, $"{BuildInfo.Name} Loaded Successfully!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"))) 
                typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
            else
            {
                MelonLogger.Warning("Error while using UiExpansionKit (UIX)'s UiInit. Using backup coroutine method.");
                static IEnumerator OnUiManagerInit()
                {
                    while (VRCUiManager.prop_VRCUiManager_0 == null)
                        yield return null;
                    VRChat_OnUiManagerInit();
                }
                MelonCoroutines.Start(OnUiManagerInit());
            }
        }

        internal static void VRChat_OnUiManagerInit()
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
                MelonLogger.Warning("Failed to initialize mod!");
                MelonLogger.Error(e);
            }
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