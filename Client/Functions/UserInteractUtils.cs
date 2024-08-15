using Client.Functions.Utils;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using MelonLoader;
using VRC.Core;

namespace Client.Functions
{
    internal static class UserInteractUtils
    {
        private static string ToPath;

        public static QMNestedButton ActionMenu;
        
        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("UserInteractUtils", "PM - User Interact Utils");
            MelonPreferences.CreateEntry("UserInteractUtils", "ToPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Assets"), "Path to save Assets");
            OnPreferencesSaved();
        }

        public static void OnPreferencesSaved() => ToPath = MelonPreferences.GetEntryValue<string>("UserInteractUtils", "ToPath");

        public static void OnUiManagerInit()
        {
            ActionMenu = new QMNestedButton("UserInteractMenu", 0, 0, "<color=#ff0000>Client's\n</color>" + "Actions", "Open the Actions Menu", null, null, null, null);
            QMSingleButton CopyAssetButton = new QMSingleButton(ActionMenu, 1, 0, "Copy\nAsset", () => CopyAsset(), 
                "Copies the asset file to the destined folder.", null, null);
        }

        private static void CopyAsset()
        {
            ApiAvatar avatar = Utilities.GetPlayerFromID(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id).prop_ApiAvatar_0;
            if (!Directory.Exists(ToPath)) Directory.CreateDirectory(ToPath);
            try
            {
                ToCopyAsset(avatar);
                Methods.PopupV2(
                    "Copy Asset", 
                    $"Successfully copied avatar \"{avatar.name}\" to folder \"{ToPath}\"!", 
                    "Close", 
                    new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); }));
            }
            catch (Exception e)
            {
                Methods.PopupV2(
                    "Copy Asset", 
                    $"Failed to copy avatar \"{avatar.name}\" :(\nIf you see this message, please send the creator your last Melon log.", 
                    "Close", 
                    new Action(() => { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); }));
                MelonLogger.Error(e);
            }
        }

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

        private static void ToCopyAsset(ApiAvatar avatar) { 
            foreach (var file in new DirectoryInfo(Path.Combine(
                        AssetBundleDownloadManager.prop_AssetBundleDownloadManager_0.field_Private_Cache_0.path, 
                        ByteArrayToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(avatar.id))).ToUpper().Substring(0, 16), 
                        ComputeVersionString(avatar.version)))
                    .GetFiles("*.*", SearchOption.AllDirectories))
                if (file.Name.Contains("__data")) 
                    File.Copy(file.FullName, Path.Combine(ToPath, $"{avatar.id}.vrca"), true); 
        }
    }
}