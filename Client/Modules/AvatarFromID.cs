﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UIExpansionKit.API;
using PMod.Utils;

namespace PMod.Modules
{
    internal class AvatarFromID : ModuleBase
    {
        internal override void OnUiManagerInit()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.AvatarMenu).AddSimpleButton("Avatar from ID", () =>
            {
                VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.
                Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_0
                (
                    "Avatar from ID", null, InputField.InputType.Standard, false, "Change Avatar",
                    (Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>)((ID, _, _) => Utilities.ChangeToAVByID(ID)),
                    null, "Insert Avatar ID"
                );
            });
        }
    }
}