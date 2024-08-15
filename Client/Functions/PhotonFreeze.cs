using Client.Functions.Utils;
using UnityEngine;
using MelonLoader;
using Photon.Pun;
using VRC;
using UIExpansionKit.API;
using Object = UnityEngine.Object;
//using FlatNetworkBufferSerializer = MonoBehaviour2PrivateHa1ObVeObAcVeSeAc1Unique;

namespace Client.Functions
{
    internal static class PhotonFreeze
    {
        private static ICustomShowableLayoutedMenu FreezeMenu;
        private static Transform CloneObj;
        private static Vector3 OriginalPos;
        private static Quaternion OriginalRot;
        public static int PhotonID = 0;
        public static bool IsFreeze = false;
        public static MelonPreferences_Entry<bool> IsOn;

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("PhotonFreeze", "PM - Photon Freeze");
            IsOn = MelonPreferences.CreateEntry("PhotonFreeze", "IsOn", false, "Activate Mod? This is a risky function.");
            NetworkEvents.OnJoin += OnJoin;
        }

        private static void OnJoin(Player player) 
        { 
            if (player.prop_APIUser_0.id == Player.prop_Player_0.prop_APIUser_0.id) 
                PhotonID = player.gameObject.GetComponent<PhotonView>().viewIdField;
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