// Outdated and unfinished

//using System;
//using UnityEngine;
//using VRC;
//using UnityEngine.UI;
//using VRC.SDKBase;

//namespace Main
//{
//    public class TargetCrash
//    {
//        private static string CrashObject;

//        public static void VRChat_OnUiManagerInit()
//        {
//            var warnButton = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/WarnButton").transform;
//            var cameraButton = UnityEngine.Object.Instantiate(QuickMenu.prop_QuickMenu_0.transform.Find("CameraMenu/BackButton").gameObject, warnButton.parent);
//            cameraButton.transform.position = warnButton.position;
//            cameraButton.GetComponentInChildren<Text>().text = "Crash";
//            cameraButton.GetComponentInChildren<UiTooltip>().field_Public_Text_0.text = "Use hand object to crash player";
//            cameraButton.GetComponentInChildren<Text>().color = new Color(1, .8f, 1);
//            cameraButton.GetComponentInChildren<Image>().color = new Color(1, .8f, 1);
//            cameraButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
//            cameraButton.GetComponent<Button>().onClick.AddListener(new Action(() =>
//           {
//               var QMTransform = PlayerManager.Method_Public_Static_Player_String_0(QuickMenu.prop_QuickMenu_0.prop_APIUser_0.id).field_Internal_VRCPlayer_0.gameObject.transform;
//               foreach (var i in Resources.FindObjectsOfTypeAll<VRC_Pickup>())
//               {
//                   if (i.gameObject.name.ToLower().Contains(CrashObject.ToLower()))
//                   {
//                       Networking.SetOwner(Player.prop_Player_0.prop_VRCPlayerApi_0, i.gameObject);
//                       i.GetComponent<Rigidbody>().isKinematic = true;
//                       i.transform.SetPositionAndRotation(QMTransform.position, QMTransform.rotation);
//                       i.transform.parent = QMTransform;
//                   }
//               }
//           }));
//            warnButton.position += new Vector3(100, 100, 100);
//        }
//        private static void Popup(string title, string text, Action<string> okaction)
//        {
//            VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_PDM_0(title, null, InputField.InputType.Standard, false, text, DelegateSupport.ConvertDelegate<Il2CppSystem.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>>(new Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>(delegate (string s, Il2CppSystem.Collections.Generic.List<KeyCode> k, Text t) { okaction(s); })), null, "...", true, null);
//        }
//    }
//}