using Client.Utils;
using UIExpansionKit.API;
using UnityEngine;
using VRC;
using Object = UnityEngine.Object;

namespace Client
{
    public class Freeze
    {
        private static ICustomShowableLayoutedMenu FreezeMenu;
        //private static QMNestedButton Menu;
        //private static QMToggleButton PhotonFreeze;
        //private static QMSingleButton TPFrozen;
        //private static FlatBufferNetworkSerializer Serializer;
        private static Transform CloneObj;
        private static Vector3 TempPos;
        private static Quaternion TempRot;
        private static bool IsFreeze;

        //public static void OnUiManagerInit()
        //{
        //    Menu = new QMNestedButton(Main.ClientMenu0, 3, 0, "PhotonFreeze", "Freeze Mod", null, null, null, null);
        //    PhotonFreeze = new QMToggleButton(Menu, 1, 0, "Freeze", () => { ToggleFreeze(); }, "UnFreeze", () => { ToggleFreeze(); }, "TOGGLE: Freeze Photon info for others", null, null, false, false);
        //    TPFrozen = new QMSingleButton(Menu, 2, 0, "TP To Frozen Position", () => { TPPlayerToPos(TempPos, TempRot); }, "Teleports you to the position where you started being frozen at.", null, null);
        //    TPFrozen.getGameObject().SetActive(false);
        //}
        
        public static void ShowFreezeMenu()
        {
            FreezeMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            FreezeMenu.AddSimpleButton("Go back", () => Main.ClientMenu.Show());
            FreezeMenu.AddToggleButton("Freeze", (_) => { ToggleFreeze(); }, () => { return IsFreeze; });
            if (IsFreeze) FreezeMenu.AddSimpleButton("TP To Frozen Position", () => { TPPlayerToPos(TempPos, TempRot); });
            FreezeMenu.Show();
        }

        private static void TPPlayerToPos(Vector3 vec3, Quaternion rot)
        {
            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = vec3;
            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.rotation = rot;
        }

        private static void ToggleFreeze()
        {
            IsFreeze = !IsFreeze;
            //Serializer = Player.prop_Player_0.prop_VRCPlayerApi_0.gameObject.GetComponent<FlatBufferNetworkSerializer>();
            //Serializer.enabled = !IsFreeze;
            if (IsFreeze)
            {
                TempPos = Player.prop_Player_0.prop_VRCPlayerApi_0.GetPosition();
                TempRot = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.rotation;
            }
            //TPFrozen.getGameObject().SetActive(IsFreeze);
            Clone(IsFreeze);
            //PhotonFreeze.setToggleState(IsFreeze, false);
            if (IsFreeze) QuickMenu.prop_QuickMenu_0.Method_Private_Void_1();
        }

        private static void Clone(bool Toggle)
        {
            if (Toggle)
            {
                CloneObj = Object.Instantiate(VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.transform.Find("Avatar"), null, true);
                Animator animator = CloneObj.GetComponent<Animator>();
                if (animator != null && animator.isHuman)
                {
                    Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (boneTransform != null)
                    {
                        boneTransform.localScale = Vector3.one;
                    }
                }
                CloneObj.name = "Cloned Frozen Avatar";
                foreach (Component component in CloneObj.GetComponents<Component>())
                {
                    if (!(component is Transform))
                    {
                        Object.Destroy(component);
                    }
                }
                CloneObj.position = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position;
                CloneObj.rotation = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.rotation;
            }
            if (!Toggle)
            {
                Object.Destroy(CloneObj.gameObject);
            }
        }
    }
}