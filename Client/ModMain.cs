using MelonLoader;
using Main.Utils;
using UIExpansionKit.API;

namespace Main
{
    public static class BuildInfo
    {
        public const string Name = "Davi's Client";
        public const string Author = "Davi";
        public const string Version = "1.0.0";
    }

    public class ModMain : MelonMod
    {
        public static ICustomShowableLayoutedMenu ClientMenu;
        public static ModMain Instance { get; private set; }

        public override void OnApplicationStart()
        {
            Instance = this;
            MelonLogger.Msg(System.ConsoleColor.Red, "Davi's Client Loaded Successfully!");
            Orbit.OnApplicationStart();
            ItemGrabber.OnApplicationStart();
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Davi's Client", () => ClientMenu.Show());
            ClientMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            ClientMenu.AddSimpleButton("ItemGrabber", () => ItemGrabber.PickupMenu.Show());
            ClientMenu.AddSimpleButton("Orbit", () => Orbit.OrbitMenu.Show());
        }

        public override void VRChat_OnUiManagerInit()
        {
            NetworkEvents.NetworkInit();
            Orbit.NetworkHook();
            ForceClone.OnUiManagerInit();
        }

        public override void OnPreferencesSaved()
        {
            Orbit.OnPreferencesSaved();
            ItemGrabber.OnPreferencesSaved();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ItemGrabber.OnSceneWasLoaded();
        }

        public override void OnUpdate()
        {
            Orbit.OnUpdate();
            ForceClone.OnUpdate();
        }
    }
}