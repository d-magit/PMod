using Client.Utils;
using UnityEngine;
using MelonLoader;
using Photon.Pun;
using VRC;
using UIExpansionKit.API;
using Object = UnityEngine.Object;
//using FlatNetworkBufferSerializer = MonoBehaviour2PrivateHa1ObVeObAcVeSeAc1Unique;

namespace Client.Modules
{
    internal class PhotonFreeze : ModuleBase
    {
        private ICustomShowableLayoutedMenu FreezeMenu;
        private Transform CloneObj;
        private Vector3 OriginalPos;
        private Quaternion OriginalRot;
        internal int PhotonID = 0;
        internal bool IsFreeze = false;
        internal MelonPreferences_Entry<bool> IsOn;

        internal PhotonFreeze()
        {
            MelonPreferences.CreateCategory("PhotonFreeze", "PM - Photon Freeze");
            IsOn = MelonPreferences.CreateEntry("PhotonFreeze", "IsOn", false, "Activate Mod? This is a risky function.");
        }

        internal override void OnPlayerJoined(Player player) 
        { 
            if (player.prop_APIUser_0.id == Player.prop_Player_0.prop_APIUser_0.id) 
                PhotonID = player.gameObject.GetComponent<PhotonView>().viewIdField;
        }

        internal void ShowFreezeMenu()
        {
            FreezeMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            FreezeMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            FreezeMenu.AddToggleButton("PhotonFreeze", (_) => ToggleFreeze(), () => IsFreeze);
            if (IsFreeze) FreezeMenu.AddSimpleButton("TP To Frozen Position", () => { TPPlayerToPos(OriginalPos, OriginalRot); });
            FreezeMenu.Show();
        }

        private void TPPlayerToPos(Vector3 OriginalPos, Quaternion OriginalRot)
        {
            Utilities.GetLocalVRCPlayer().transform.position = OriginalPos;
            Utilities.GetLocalVRCPlayer().transform.rotation = OriginalRot;
        }

        private void ToggleFreeze()
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

        private void Clone(bool Toggle)
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