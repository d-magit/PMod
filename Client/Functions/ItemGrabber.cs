using System;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using UIExpansionKit.API;
using Object = UnityEngine.Object;
using Utilities = Client.Functions.Utils.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Client.Functions
{
    internal static class ItemGrabber
    {
        private static ICustomShowableLayoutedMenu SelectionMenu;
        private static VRC_Pickup[] Pickups;
        private static Dictionary<VRC_Pickup, bool[]> PreviousStates = new();
        private static float min_distance;
        private static bool patch_all;
        private static bool take_ownership;
        public static ICustomShowableLayoutedMenu PickupMenu;
        public static bool IsOn;


        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("ItemGrabber", "PM - Item Grabber");
            MelonPreferences.CreateEntry("ItemGrabber", "IsOn", false, "Activate Mod? This is a risky function.");
            MelonPreferences.CreateEntry("ItemGrabber", "GrabDistance", -1.0f, "Distance (meters) for grabbing all, set to -1 for unlimited.");
            MelonPreferences.CreateEntry("ItemGrabber", "PatchAllOnLoad", false, "Patch All on Scene Load");
            MelonPreferences.CreateEntry("ItemGrabber", "TakeOwnership", true, "Take Ownership of Object on Grab");
            PickupMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            PickupMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            PickupMenu.AddSimpleButton("Patch", () => Select("Patch"));
            PickupMenu.AddSimpleButton("Unpatch", () => Select("Unpatch"));
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
            else if (Type == "Unpatch")
            {
                SelectionMenu.AddSimpleButton("Unpatch All", () => UnpatchAll());
                if (PreviousStates.Count != 0) foreach (var Pickup in PreviousStates.Keys) SelectionMenu.AddSimpleButton(Pickup.name, () => { Unpatch(Pickup); Select("Unpatch"); });
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
            var pickup = Item.GetComponent<VRC_Pickup>();
            if (!PreviousStates.ContainsKey(Item)) PreviousStates.Add(Item, new [] 
            {
                 pickup.DisallowTheft,
                 pickup.allowManipulationWhenEquipped,
                 pickup.pickupable,
                 Item.gameObject.active
            });
            pickup.DisallowTheft = false;
            pickup.allowManipulationWhenEquipped = true;
            pickup.pickupable = true;
            Item.gameObject.SetActive(true);
        }

        private static void PatchAll() { foreach (var Pickup in Pickups) Patch(Pickup); }

        private static void Unpatch(VRC_Pickup Item)
        {
            if (PreviousStates.ContainsKey(Item))
            {
                var pickup = Item.GetComponent<VRC_Pickup>();
                var PreviousState = PreviousStates[Item];
                pickup.DisallowTheft = PreviousState[0];
                pickup.allowManipulationWhenEquipped = PreviousState[1];
                pickup.pickupable = PreviousState[2];
                Item.gameObject.SetActive(PreviousState[3]);
                PreviousStates.Remove(Item);
            }
        }

        private static void UnpatchAll() 
        { 
            while (PreviousStates.Count != 0) Unpatch(PreviousStates.First().Key);
            Select("Unpatch");
        }

        private static void Trigger(VRC_Pickup Item)
        {
            if (Item == null) foreach (var Pickup in Pickups)
            {
                float dist = Vector3.Distance(Utilities.GetLocalVRCPlayer().transform.position, Pickup.transform.position);
                if (min_distance == -1 || dist <= min_distance) PickupItem(Pickup);
            }
            else PickupItem(Item);
        }
        
        private static void PickupItem(VRC_Pickup Item)
        {
            try
            {
                Patch(Item);
                if (Networking.GetOwner(Item.gameObject).playerId != Utilities.GetLocalVRCPlayerApi().playerId && take_ownership)
                {
                    Item.GetComponent<VRC_Pickup>().currentlyHeldBy = null;
                    Networking.SetOwner(Utilities.GetLocalVRCPlayerApi(), Item.gameObject);
                }
                Item.transform.position = Utilities.GetBoneTransform(Player.prop_Player_0, HumanBodyBones.Hips).position;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to grab item {Item.name}! {e}");
            }
        }
    }
}