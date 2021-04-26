using System;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using ApiAvatar = VRC.Core.ApiAvatar;
using APIUser = VRC.Core.APIUser;

namespace Main
{
    public class ForceClone
    {
        private static GameObject cloneButton;
        private static ApiAvatar toClone;
        private static QuickMenu QM;
        private static APIUser QMUser;
        private static GameObject cameraClone;
        private static GameObject platformAny;
        private static GameObject platformPC;
        private static GameObject platformOculus;
        private static GameObject platformIconAny;
        private static GameObject platformIconPC;
        private static GameObject platformIconOculus;
        private static bool isFar = true;
        private static bool a = true;

        public static void OnUiManagerInit() => CreateButtons();

        public static void OnUpdate()
        {
            if (QM != null)
            {
                if (QM.prop_Boolean_0)
                {
                    QMUser = QM.prop_APIUser_0;
                    if (QMUser != null)
                    {
                        if (!a) a = true;
                        UpdatePlatforms();
                        toClone = PlayerManager.Method_Public_Static_Player_String_0(QMUser.id).field_Internal_VRCPlayer_0.prop_ApiAvatar_0;
                        if (toClone.releaseStatus == "public" && !QMUser.allowAvatarCopying && toClone.id != Player.prop_Player_0.field_Internal_VRCPlayer_0.prop_ApiAvatar_0.id)
                        {
                            if (!isFar)
                            {
                                cloneButton.transform.position += new Vector3(100, 100, 100);
                                cameraClone.SetActive(true);
                                isFar = true;
                            }
                        }
                        else if (isFar)
                        {
                            cloneButton.transform.position -= new Vector3(100, 100, 100);
                            cameraClone.SetActive(false);
                            isFar = false;
                        }
                    }
                }
                else if (!QM.prop_Boolean_0 && a)
                {
                    if (isFar)
                    {
                        cloneButton.transform.position -= new Vector3(100, 100, 100);
                        cameraClone.SetActive(false);
                        isFar = false;
                    }
                    a = false;
                }
            }
        }

        private static void UpdatePlatforms()
        {
            if (platformAny.active != platformIconAny.active || platformPC.active != platformIconPC.active || platformOculus.active != platformIconOculus.active)
            {
                platformAny.SetActive(platformIconAny.active);
                platformPC.SetActive(platformIconPC.active);
                platformOculus.SetActive(platformIconOculus.active);
            }
        }

        private static void CreateButtons()
        {
            cloneButton = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/CloneAvatarButton");
            QM = QuickMenu.prop_QuickMenu_0;
            cameraClone = Object.Instantiate(QM.transform.Find("CameraMenu/BackButton").gameObject, cloneButton.transform.parent);
            GameObject platform = Object.Instantiate(cloneButton.transform.Find("PlatformIcon").gameObject, cameraClone.transform);
            Transform platformIcon = cloneButton.transform.Find("PlatformIcon");
            platformAny = platform.transform.Find("AnyIcon").gameObject;
            platformPC = platform.transform.Find("PCIcon").gameObject;
            platformOculus = platform.transform.Find("QuestIcon").gameObject;
            platformIconAny = platformIcon.Find("AnyIcon").gameObject;
            platformIconPC = platformIcon.Find("PCIcon").gameObject;
            platformIconOculus = platformIcon.Find("QuestIcon").gameObject;
            cameraClone.transform.position = cloneButton.transform.position;
            platform.transform.position = cloneButton.transform.Find("PlatformIcon").position;
            cameraClone.GetComponentInChildren<Text>().text = "Clone\nPublic\nAvatar";
            cameraClone.GetComponentInChildren<Text>().color = new Color(1, .8f, 1);
            cameraClone.GetComponentInChildren<Image>().color = new Color(1, .8f, 1);
            cameraClone.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            cameraClone.GetComponent<Button>().onClick.AddListener(new Action(() =>
            {
                var avatar = new PageAvatar
                {
                    field_Public_SimpleAvatarPedestal_0 = new SimpleAvatarPedestal
                    {
                        field_Internal_ApiAvatar_0 = new ApiAvatar
                        {
                            id = toClone.id
                        }
                    }
                };
                avatar.ChangeToSelectedAvatar();
                MelonLogger.Msg(ConsoleColor.Red, "Avatar cloned: " + toClone.id);
            }));
            cloneButton.transform.position += new Vector3(100, 100, 100);
            cameraClone.name = "ForceCloneAvatarButton";
            platform.name = "PlatformIcon";
        }
    }
}