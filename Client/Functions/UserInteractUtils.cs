using VRC;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VRC.Core;
using System.Linq;
using MelonLoader;
using System.Diagnostics;
using Client.Utils;

namespace Client
{
    public class UserInteractUtils
    {
        private static VRCPlayer GetPlayerFromID(string id) => PlayerManager.Method_Public_Static_Player_String_1(id)._vrcplayer;
        private static VRCUiPopupManager GetPopupManager() => VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0;
        private static string ToPath;

        public static QMNestedButton ActionMenu;
        
        public static void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("CopyAsset", "Copy Asset");
            MelonPreferences.CreateEntry("CopyAsset", "ToPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Assets"), "Path to save");
            OnPreferencesSaved();
        }

        public static void OnPreferencesSaved() => ToPath = MelonPreferences.GetEntryValue<string>("CopyAsset", "ToPath");

        public static void OnUiManagerInit()
        {
            ActionMenu = new QMNestedButton("UserInteractMenu", 0, 0, "<color=#ff0000>Client's\n</color>" + "Actions", "Open the Actions Menu", null, null, null, null);
            QMSingleButton CopyAssetButton = new QMSingleButton(ActionMenu, 1, 0, "Copy\nAsset", () => { CopyAsset(); }, "Copies the asset file to the destined folder.", null, null);
            //QMSingleButton OpenSteamPageButton = new QMSingleButton(ActionMenu, 2, 0, "Open\nSteam\nPage", () => { OpenSteamPage(); }, "Opens the Steam page for the current Quick Menu user if found.", null, null);
        }

        private static void CopyAsset()
        {
            ApiAvatar avatar = GetPlayerFromID(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id).prop_ApiAvatar_0;
            if (!Directory.Exists(ToPath)) Directory.CreateDirectory(ToPath);
            try
            {
                ToCopyAsset(avatar);
                GetPopupManager().Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Copy Asset", $"Successfully copied avatar \"{avatar.name}\" to folder \"{ToPath}\"!", "Close", new Action(() => { GetPopupManager().Method_Private_Void_PDM_0(); }));
            }
            catch (Exception e)
            {
                GetPopupManager().Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Copy Asset", $"Failed to copy avatar \"{avatar.name}\" :(\nIf you see this message, please send the creator your last Melon log.", "Close", new Action(() => { GetPopupManager().Method_Private_Void_PDM_0(); }));
                MelonLogger.Error(e);
            }
        }

        private static void OpenSteamPage()
        {
            VRCPlayer User = GetPlayerFromID(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id);
            string PlayerSteamID = User.field_Private_UInt64_0.ToString();
            if (!String.IsNullOrEmpty(PlayerSteamID) && PlayerSteamID != "0")
            {
                try
                {
                    Process.Start("https://steamcommunity.com/profiles/" + PlayerSteamID);
                    GetPopupManager().Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Steam", $"Successfully opened Steam page for user \"{User.prop_String_0}\"!", "Close", new Action(() => { GetPopupManager().Method_Private_Void_PDM_0(); }));
                }
                catch (Exception e)
                {
                    GetPopupManager().Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Steam", $"Failed to open Steam page! :(\nIf you see this message, please send the creator your last Melon log.", "Close", new Action(() => { GetPopupManager().Method_Private_Void_PDM_0(); }));
                    MelonLogger.Error(e);
                }
            }
            else
            {
                GetPopupManager().Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_0("Steam", $"Oops! This user does not have a Steam associated to it's profile.", "Close", new Action(() => { GetPopupManager().Method_Private_Void_PDM_0(); }));
            }
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