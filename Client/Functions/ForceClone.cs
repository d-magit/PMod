using System;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using ApiAvatar = VRC.Core.ApiAvatar;
using APIUser = VRC.Core.APIUser;

namespace Client
{
    public class ForceClone
    {
        private static QuickMenu QM;
        private static ApiAvatar toClone;
        private static GameObject cameraClone;
        private static GameObject platformAny;
        private static GameObject platformPC;
        private static GameObject platformOculus;
        private static GameObject platformIconAny;
        private static GameObject platformIconPC;
        private static GameObject platformIconOculus;
        private static Transform cloneButton;
        private static Transform UserInteract => GameObject.Find("UserInterface/QuickMenu/UserInteractMenu").transform;
        private static Vector3 GetOriginalPos() => UserInteract.Find("ShowAuthorButton").position + UserInteract.Find("ViewAvatarThreeToggle/Button_UseSafetySettings").position - UserInteract.Find("MuteButton").position;
        private static bool isFar = true;

        public static void OnUiManagerInit()
        {
            CreateButtons();
            Main.listener.OnEnabled += delegate 
            {
                APIUser QMUser = QM.field_Private_APIUser_0;
                toClone = PlayerManager.Method_Public_Static_Player_String_PDM_0(QMUser.id)._vrcplayer.prop_ApiAvatar_0;
                if (toClone.releaseStatus == "public" && !QMUser.allowAvatarCopying && toClone.id != VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_ApiAvatar_0.id)
                {
                    if (!isFar)
                    {
                        cloneButton.position += new Vector3(100, 100, 100);
                        cameraClone.SetActive(true);
                        isFar = true;
                    }
                }
                else if (isFar)
                {
                    cloneButton.position = GetOriginalPos();
                    cameraClone.SetActive(false);
                    isFar = false;
                }
            };
            Main.listener.OnDisabled += delegate 
            {
                if (isFar)
                {
                    cloneButton.position = GetOriginalPos();
                    cameraClone.SetActive(false);
                    isFar = false;
                }
            };
        }

        public static void OnUpdate()
        {
            if (platformAny != null)
            {
                if (platformAny.active != platformIconAny.active || platformPC.active != platformIconPC.active || platformOculus.active != platformIconOculus.active)
                {
                    platformAny.SetActive(platformIconAny.active);
                    platformPC.SetActive(platformIconPC.active);
                    platformOculus.SetActive(platformIconOculus.active);
                }
            }
        }

        private static void CreateButtons()
        {
            QM = QuickMenu.prop_QuickMenu_0;
            cloneButton = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/CloneAvatarButton").transform;
            cameraClone = Object.Instantiate(QM.transform.Find("CameraMenu/BackButton").gameObject, cloneButton.parent);

            Transform platform = Object.Instantiate(cloneButton.Find("PlatformIcon"), cameraClone.transform);
            Transform platformIcon = cloneButton.Find("PlatformIcon");

            platformAny = platform.Find("AnyIcon").gameObject;
            platformPC = platform.Find("PCIcon").gameObject;
            platformOculus = platform.Find("QuestIcon").gameObject;
            platformIconAny = platformIcon.Find("AnyIcon").gameObject;
            platformIconPC = platformIcon.Find("PCIcon").gameObject;
            platformIconOculus = platformIcon.Find("QuestIcon").gameObject;

            cameraClone.transform.position = cloneButton.position;
            platform.position = cloneButton.Find("PlatformIcon").position;

            cameraClone.GetComponentInChildren<Text>().text = "Clone\nPublic\nAvatar";
            cameraClone.GetComponentInChildren<Text>().color = new Color(1, .8f, 1);
            cameraClone.GetComponentInChildren<Image>().color = new Color(1, .8f, 1);

            cameraClone.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            cameraClone.GetComponent<Button>().onClick.AddListener(new Action(() =>
            {
                new PageAvatar { field_Public_SimpleAvatarPedestal_0 = new SimpleAvatarPedestal { field_Internal_ApiAvatar_0 = new ApiAvatar { id = toClone.id } } }.ChangeToSelectedAvatar();
                MelonLogger.Msg(ConsoleColor.Red, "Avatar cloned: " + toClone.id);
            }));

            cameraClone.name = "ForceCloneAvatarButton";
            platform.name = "PlatformIcon";
        }
    }
}