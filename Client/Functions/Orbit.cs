using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using UIExpansionKit.API;
using Main.Utils;
using Object = UnityEngine.Object;

namespace Main
{
    public class Orbit
    {
        private static VRCPlayerApi GetLocalVRCPlayerApi() => Player.prop_Player_0.prop_VRCPlayerApi_0;
        private static List<Player> Playerlist;
        private static ICustomShowableLayoutedMenu SelectionMenu;
        private static List<OrbitItem> Orbits;
        private static Player CurrentPlayer;
        private static bool patch;

        public static ICustomShowableLayoutedMenu OrbitMenu;
        public static UnhollowerBaseLib.Il2CppArrayBase<VRC_Pickup> Pickups;
        public static Orbit Instance { get; private set; }
        public static Quaternion rotation;
        public static Quaternion rotationy;
        public static Vector3 OrbitCenter;
        public static float PlayerHeight;
        public static float Timer = 0f;
        public static float radius;
        public static float speed;
        public static RotType rotType;
        public enum RotType
        {
            CircularRot,
            CylindricalRot,
            SphericalRot,
        }

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("Orbit", "Orbit");
            MelonPreferences.CreateEntry("Orbit", "Radius", 1.0f, "Radius");
            MelonPreferences.CreateEntry("Orbit", "RotationX", 0.0f, "X Rotation");
            MelonPreferences.CreateEntry("Orbit", "RotationY", 0.0f, "Y Rotation");
            MelonPreferences.CreateEntry("Orbit", "RotationZ", 0.0f, "Z Rotation");
            MelonPreferences.CreateEntry("Orbit", "Speed", 1.0f, "Speed");
            // MelonPreferences.CreateEntry("Orbit", "Log", false, "Log");
            MelonPreferences.CreateEntry("Orbit", "Patch", true, "Patch items on Orbit");
            OrbitMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
            OrbitMenu.AddSimpleButton("Go back", () => ModMain.ClientMenu.Show());
            OrbitMenu.AddSimpleButton("Stop Orbit", () => StopOrbit());
            OrbitMenu.AddSimpleButton("Circular Orbit", () => SelectOrbit("Circular"));
            OrbitMenu.AddSimpleButton("Spherical Orbit", () => SelectOrbit("Spherical"));
            OrbitMenu.AddSimpleButton("Cylindrical Orbit", () => SelectOrbit("Cylindrical"));
            OnPreferencesSaved();
        }

        public static void NetworkHook()
        {
            NetworkEvents.OnPlayerLeave += OnPlayerLeave;
            NetworkEvents.OnPlayerJoin += OnPlayerJoin;
            NetworkEvents.OnInstanceChange += OnInstanceChange;
        }

        public static void OnPreferencesSaved()
        {
            radius = MelonPreferences.GetEntryValue<float>("Orbit", "Radius");
            rotation = Quaternion.Euler(MelonPreferences.GetEntryValue<float>("Orbit", "RotationX"), 0, MelonPreferences.GetEntryValue<float>("Orbit", "RotationZ"));
            rotationy = Quaternion.Euler(0, MelonPreferences.GetEntryValue<float>("Orbit", "RotationY"), 0);
            speed = MelonPreferences.GetEntryValue<float>("Orbit", "Speed");
            // log = MelonPreferences.GetEntryValue<bool>("Orbit", "Log");
            patch = MelonPreferences.GetEntryValue<bool>("Orbit", "Patch");
        }

        public static void OnUpdate()
        {
            if (Pickups != null && Orbits != null && CurrentPlayer != null)
            {
                OrbitCenter = GetCenter();
                for (int i = 0; i < Pickups.Count; i++)
                {
                    if (patch) Patch(Pickups[i]);
                    Pickups[i].transform.position = Orbits[i].CurrentPos();
                    Pickups[i].transform.rotation = Orbits[i].CurrentRot();
                }
            }
            Timer += Time.deltaTime;
        }

        private static void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            // if (log) MelonLogger.Msg(ConsoleColor.Cyan, $"Changing instance...");
            Playerlist = new List<Player>();
            StopOrbit();
        }

        private static void OnPlayerJoin(Player player)
        {
            // if (log) MelonLogger.Msg(ConsoleColor.Cyan, $"Player {player.prop_APIUser_0.displayName} just joined, adding to the list...");
            Playerlist.Add(player);
        }

        private static void OnPlayerLeave(Player player)
        {
            // if (log) MelonLogger.Msg(ConsoleColor.Cyan, $"Player {player.prop_APIUser_0.displayName} just left, removing from the list...");
            Playerlist.Remove(player);
            if (CurrentPlayer == player) StopOrbit();
        }

        private static void SelectOrbit(string Type)
        {
            SelectionMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
            SelectionMenu.AddSimpleButton("Go back", () => OrbitMenu.Show());
            if (Type == "Circular") rotType = RotType.CircularRot;
            else if (Type == "Cylindrical") rotType = RotType.CylindricalRot;
            else rotType = RotType.SphericalRot;
            foreach (var player in Playerlist) SelectionMenu.AddSimpleButton($"{player.prop_APIUser_0.displayName}", () => ToOrbit(player));
            SelectionMenu.Show();
        }

        private static void ToOrbit(Player Player)
        {
            if (CurrentPlayer != null) StopOrbit();
            CurrentPlayer = Player;
            Timer = 0f;
            Pickups = Object.FindObjectsOfType<VRC_Pickup>();
            OrbitCenter = GetCenter();
            Orbits = new List<OrbitItem>();
            for (int i = 0; i < Pickups.Count; i++)
            {
                Orbits.Add(new OrbitItem(Pickups[i], i));
            }
        }

        private static void StopOrbit()
        {
            if (Pickups != null && Orbits != null)
            {
                for (int i = 0; i < Pickups.Count; i++)
                {
                    Orbits[i].IsOn = false;
                    if (Pickups[i])
                    {
                        Pickups[i].transform.position = Orbits[i].CurrentPos();
                        Pickups[i].transform.rotation = Orbits[i].CurrentRot();
                        Unpatch(i);
                    }
                }
            }
            Pickups = null;
            Orbits = null;
            CurrentPlayer = null;
        }

        private static void Patch(VRC_Pickup Item)
        {
            VRCPlayerApi GetOwner() => Networking.GetOwner(Item.gameObject);
            Item.GetComponent<VRC_Pickup>().DisallowTheft = true;
            Item.GetComponent<VRC_Pickup>().pickupable = false;
            Item.GetComponent<Rigidbody>().isKinematic = true;
            Item.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            Item.gameObject.SetActive(true);
            if (GetOwner().playerId != GetLocalVRCPlayerApi().playerId)
            {
                // if (log) MelonLogger.Msg(ConsoleColor.Cyan, $"Taking Ownership of item {Item.name}...");
                Item.GetComponent<VRC_Pickup>().currentlyHeldBy = null; 
                Networking.SetOwner(GetLocalVRCPlayerApi(), Item.gameObject);
            }
        }

        private static void Unpatch(int i)
        {
            Pickups[i].GetComponent<VRC_Pickup>().DisallowTheft = Orbits[i].InitialTheft;
            Pickups[i].GetComponent<VRC_Pickup>().pickupable = Orbits[i].InitialPickupable;
            Pickups[i].GetComponent<Rigidbody>().isKinematic = Orbits[i].InitialKinematic;
            Pickups[i].GetComponent<Rigidbody>().velocity = Orbits[i].InitialVelocity;
            Pickups[i].gameObject.SetActive(Orbits[i].InitialActive);
        }

        private static Vector3 GetCenter()
        {
            Vector3 Head = TransformOfBone(CurrentPlayer, HumanBodyBones.Head).position;
            if (rotType == RotType.CircularRot) return Head;
            else
            {
                Vector3 Pos = CurrentPlayer.transform.position;
                PlayerHeight = (Head - Pos).y;
                return Pos;
            }
        }

        // I took this from someone else from the mod community and I really don't remember who exactly to give the credits :( I'm very sorry.
        private static Transform TransformOfBone(Player player, HumanBodyBones bone)
        {
            Transform playerPosition = player.transform;
            VRCAvatarManager avatarManager = player.prop_VRCPlayer_0.prop_VRCAvatarManager_0;
            if (!avatarManager) return playerPosition;
            Animator animator = avatarManager.field_Private_Animator_0;
            if (!animator) return playerPosition;
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (!boneTransform) return playerPosition;
            return boneTransform;
        }
    }
}