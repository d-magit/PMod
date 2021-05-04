using VRC;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VRC.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using System.Linq;
using MelonLoader;

namespace Client
{
    public class CopyAsset
    {
        private static string ToPath;

        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("CopyAsset", "Copy Asset");
            MelonPreferences.CreateEntry("CopyAsset", "ToPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Assets"), "Path to save");
            OnPreferencesSaved();
        }

        public static void OnPreferencesSaved() => ToPath = MelonPreferences.GetEntryValue<string>("CopyAsset", "ToPath");

        public static void OnUiManagerInit()
        {
            Transform UserInteract = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu").transform;
            Transform CopyAssetButton = Object.Instantiate(UserInteract.Find("ShowAuthorButton"), UserInteract);
            CopyAssetButton.position = UserInteract.Find("ShowAvatarStatsButton").position + UserInteract.Find("CloneAvatarButton").position - UserInteract.Find("ShowAuthorButton").position;

            CopyAssetButton.GetComponentInChildren<Text>().text = "Copy Asset\nto Path";
            CopyAssetButton.name = "CopyAssetButton";

            CopyAssetButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            CopyAssetButton.GetComponent<Button>().onClick.AddListener(new Action(() =>
            {
                ApiAvatar avatar = PlayerManager.Method_Public_Static_Player_String_PDM_0(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id)._vrcplayer.prop_ApiAvatar_0;
                if (!Directory.Exists(ToPath)) Directory.CreateDirectory(ToPath);
                try
                {
                    ToCopyAsset(avatar);
                    VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Copy Asset", $"Successfully copied avatar \"{avatar.name}\" to folder \"{ToPath}\"!", "Close", new Action(() => { VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Private_Void_PDM_0(); }));
                }
                catch (Exception e)
                {
                    VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Copy Asset", $"Failed to copy avatar \"{avatar.name}\" :(\nIf you see this message, please send Davi your last Melon log.", "Close", new Action(() => { VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Private_Void_PDM_0();  }));
                    MelonLogger.Error(e);
                }
            }));
        }

        // Ty gompo for the code below x3
        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static string ComputeVersionString(int version)
        {
            string result = "";
            foreach (byte b in BitConverter.GetBytes(version)) result += b.ToString("X2");
            return string.Concat(Enumerable.Repeat("0", (32 - result.Length))) + result;
        }

        private static void ToCopyAsset(ApiAvatar avatar) { foreach (var file in new DirectoryInfo(Path.Combine(AssetBundleDownloadManager.prop_AssetBundleDownloadManager_0.field_Private_Cache_0.path, ByteArrayToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(avatar.id))).ToUpper().Substring(0, 16), ComputeVersionString(avatar.version))).GetFiles("*.*", SearchOption.AllDirectories)) if (file.Name.Contains("__data")) File.Copy(file.FullName, Path.Combine(ToPath, $"{avatar.id}.vrca"), true); }
    }
}