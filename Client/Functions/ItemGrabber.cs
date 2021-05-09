using System;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using UIExpansionKit.API;
using Object = UnityEngine.Object;

namespace Client
{
    public class ItemGrabber
    {
        private static VRCPlayer GetLocalVRCPlayer() => VRCPlayer.field_Internal_Static_VRCPlayer_0;
        private static VRCPlayerApi GetLocalVRCPlayerApi() => Player.prop_Player_0.prop_VRCPlayerApi_0;
        private static ICustomShowableLayoutedMenu SelectionMenu;
        private static VRC_Pickup[] Pickups;
        private static float min_distance;
        private static bool patch_all;
        private static bool take_ownership;
        public static ICustomShowableLayoutedMenu PickupMenu;
        public static bool IsOn;


        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("ItemGrabber", "Item Grabber");
            MelonPreferences.CreateEntry("ItemGrabber", "IsOn", false, "Activate Mod? This is a risky function.");
            MelonPreferences.CreateEntry("ItemGrabber", "GrabDistance", -1.0f, "Distance (meters) for grabbing all, set to -1 for unlimited.");
            MelonPreferences.CreateEntry("ItemGrabber", "PatchAllOnLoad", false, "Patch All on Scene Load");
            MelonPreferences.CreateEntry("ItemGrabber", "TakeOwnership", true, "Take Ownership of Object on Grab");
            PickupMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            PickupMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            PickupMenu.AddSimpleButton("Patch", () => Select("Patch"));
            PickupMenu.AddSimpleButton("Grab", () => Select("Grab"));
            OnPreferencesSaved();
        }

        public static void OnPreferencesSaved()
        {
            IsOn = MelonPreferences.GetEntryValue<bool>("ItemGrabber", "IsOn");
            min_distance = MelonPreferences.GetEntryValue<float>("ItemGrabber", "GrabDistance");
            patch_all = MelonPreferences.GetEntryValue<bool>("ItemGrabber", "PatchAllOnLoad");
            take_ownership = MelonPreferences.GetEntryValue<bool>("ItemGrabber", "TakeOwnership");
        }

        public static void OnSceneWasLoaded()
        {
            if (patch_all)
            {
                Pickups = Object.FindObjectsOfType<VRC_Pickup>();
                PatchAll();
            }
        }

        private static void Select(string Type)
        {
            SelectionMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
            SelectionMenu.AddSimpleButton("Go back", () => PickupMenu.Show());
            Pickups = Object.FindObjectsOfType<VRC_Pickup>();
            if (Type == "Patch")
            {
                SelectionMenu.AddSimpleButton("Patch All", () => PatchAll());
                foreach (var Pickup in Pickups) SelectionMenu.AddSimpleButton(Pickup.name, () => Patch(Pickup));

            }
            else
            {
                SelectionMenu.AddSimpleButton("Grab in Range", () => Trigger(null));
                foreach (var Pickup in Pickups) SelectionMenu.AddSimpleButton(Pickup.name, () => Trigger(Pickup));
            }
            SelectionMenu.Show();
        }

        private static void Patch(VRC_Pickup Item)
        {
            Item.GetComponent<VRC_Pickup>().DisallowTheft = false;
            Item.GetComponent<VRC_Pickup>().allowManipulationWhenEquipped = true;
            Item.GetComponent<VRC_Pickup>().pickupable = true;
            Item.gameObject.SetActive(true);
        }

        private static void PatchAll() { foreach (var Pickup in Pickups) Patch(Pickup); }

        private static void Trigger(VRC_Pickup Item)
        {
            if (Item == null) foreach (var Pickup in Pickups)
            {
                float dist = Vector3.Distance(GetLocalVRCPlayer().transform.position, Pickup.transform.position);
                if (min_distance == -1 || dist <= min_distance) PickupItem(Pickup);
            }
            else PickupItem(Item);
        }
        
        private static void PickupItem(VRC_Pickup Item)
        {
            try
            {
                VRCPlayerApi GetOwner() => Networking.GetOwner(Item.gameObject);
                Patch(Item);
                if (GetOwner().playerId != GetLocalVRCPlayerApi().playerId && take_ownership)
                {
                    Item.GetComponent<VRC_Pickup>().currentlyHeldBy = null;
                    Networking.SetOwner(GetLocalVRCPlayerApi(), Item.gameObject);
                }
                Item.transform.position = TransformOfBone(Player.prop_Player_0, HumanBodyBones.Hips).position;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to grab item {Item.name}! {e}");
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