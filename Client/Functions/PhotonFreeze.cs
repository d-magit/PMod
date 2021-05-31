using Client.Functions.Utils;
using UnityEngine;
using VRC;
using UIExpansionKit.API;
using Object = UnityEngine.Object;
using MelonLoader;
//using FlatNetworkBufferSerializer = MonoBehaviour2PrivateHa1ObVeObAcVeSeAc1Unique;

namespace Client.Functions
{
    // Incomplete
    internal static class PhotonFreeze
    {
        private static ICustomShowableLayoutedMenu FreezeMenu;
        private static Transform CloneObj;
        private static Vector3 OriginalPos;
        private static Quaternion OriginalRot;
        public static bool IsFreeze = false;
        public static bool IsOn;

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("PhotonFreeze", "Photon Freeze");
            MelonPreferences.CreateEntry("PhotonFreeze", "IsOn", false, "Activate Mod? This is a risky function.");
        }

        public static void OnPreferencesSaved()
        {
            IsOn = MelonPreferences.GetEntryValue<bool>("PhotonFreeze", "IsOn");
        }

        public static void ShowFreezeMenu()
        {
            FreezeMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            FreezeMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            FreezeMenu.AddToggleButton("PhotonFreeze", (_) => ToggleFreeze(), () => IsFreeze);
            if (IsFreeze) FreezeMenu.AddSimpleButton("TP To Frozen Position", () => { TPPlayerToPos(OriginalPos, OriginalRot); });
            FreezeMenu.Show();
        }

        private static void TPPlayerToPos(Vector3 OriginalPos, Quaternion OriginalRot)
        {
            Utilities.GetLocalVRCPlayer().transform.position = OriginalPos;
            Utilities.GetLocalVRCPlayer().transform.rotation = OriginalRot;
        }

        private static void ToggleFreeze()
        {
            IsFreeze = !IsFreeze;
            if (IsFreeze)
            {
                OriginalPos = Utilities.GetLocalVRCPlayerApi().GetPosition();
                OriginalRot = Utilities.GetLocalVRCPlayer().transform.rotation;
            }
            Clone(IsFreeze);
            ShowFreezeMenu();
        }

        private static void Clone(bool Toggle)
        {
            if (Toggle)
            {
                CloneObj = Object.Instantiate(Utilities.GetLocalVRCPlayer().prop_VRCAvatarManager_0.transform.Find("Avatar"), null, true);
                CloneObj.name = "Cloned Frozen Avatar";
                CloneObj.position = Utilities.GetLocalVRCPlayer().transform.position;
                CloneObj.rotation = Utilities.GetLocalVRCPlayer().transform.rotation;

                Animator animator = CloneObj.GetComponent<Animator>();
                if (animator != null && animator.isHuman)
                {
                    Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (boneTransform != null) boneTransform.localScale = Vector3.one;
                }
                foreach (Component component in CloneObj.GetComponents<Component>())
                    if (!(component is Transform)) Object.Destroy(component);
                Tools.SetLayerRecursively(CloneObj.gameObject, LayerMask.NameToLayer("Player"));
            }
            else Object.Destroy(CloneObj.gameObject);
        }
    }
}