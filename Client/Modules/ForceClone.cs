using Client.Utils;
using System;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;
using ApiAvatar = VRC.Core.ApiAvatar;
using APIUser = VRC.Core.APIUser;

namespace Client.Modules
{
    internal class ForceClone : ModuleBase
    {
        private QuickMenu QM;
        private ApiAvatar toClone;
        private QMSingleButton cameraClone;
        private GameObject platformAny;
        private GameObject platformPC;
        private GameObject platformOculus;
        private GameObject platformIconAny;
        private GameObject platformIconPC;
        private GameObject platformIconOculus;
        private Transform cloneButton;
        private bool isFar = true;
        private Transform UserInteract() => GameObject.Find("UserInterface/QuickMenu/UserInteractMenu").transform;
        private Vector3 GetOriginalPos() => UserInteract().Find("ShowAuthorButton").position + 
            UserInteract().Find("ViewAvatarThreeToggle/Button_UseSafetySettings").position - UserInteract().Find("MuteButton").position;

        internal override void OnUiManagerInit()
        {
            CreateButtons();
            Main.listener.OnEnabled += delegate 
            {
                APIUser QMUser = QM.field_Private_APIUser_0;
                toClone = Utilities.GetPlayerFromID(QMUser.id).prop_ApiAvatar_0;
                if (toClone.releaseStatus == "public" && !QMUser.allowAvatarCopying && toClone.id != Utilities.GetLocalVRCPlayer().prop_ApiAvatar_0.id)
                {
                    if (!isFar)
                    {
                        cloneButton.position += new Vector3(100, 100, 100);
                        cameraClone.setActive(true);
                        isFar = true;
                    }
                }
                else if (isFar)
                {
                    cloneButton.position = GetOriginalPos();
                    cameraClone.setActive(false);
                    isFar = false;
                }
            };
            Main.listener.OnDisabled += delegate 
            {
                if (isFar)
                {
                    cloneButton.position = GetOriginalPos();
                    cameraClone.setActive(false);
                    isFar = false;
                }
            };
        }

        internal override void OnUpdate()
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

        private void CreateButtons()
        {
            QM = QuickMenu.prop_QuickMenu_0;
            cloneButton = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/CloneAvatarButton").transform;
            cameraClone = new QMSingleButton("UserInteractMenu", 5, 0, "Clone\nPublic\nAvatar", () => {
                Utilities.ChangeToAVByID(toClone.id);
                MelonLogger.Msg(ConsoleColor.Red, "Avatar cloned: " + toClone.id);
            }, "Force Clone Avatar.", null, new Color(1, .8f, 1));

            Transform platform = Object.Instantiate(cloneButton.Find("PlatformIcon"), cameraClone.getGameObject().transform);
            Transform platformIcon = cloneButton.Find("PlatformIcon");

            platformAny = platform.Find("AnyIcon").gameObject;
            platformPC = platform.Find("PCIcon").gameObject;
            platformOculus = platform.Find("QuestIcon").gameObject;
            platformIconAny = platformIcon.Find("AnyIcon").gameObject;
            platformIconPC = platformIcon.Find("PCIcon").gameObject;
            platformIconOculus = platformIcon.Find("QuestIcon").gameObject;
            platform.position = cloneButton.Find("PlatformIcon").position;
        }
    }
}