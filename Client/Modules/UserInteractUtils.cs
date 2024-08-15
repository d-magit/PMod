using Client.Utils;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using MelonLoader;
using VRC.Core;

namespace Client.Modules
{
    internal class UserInteractUtils : ModuleBase
    {
        private MelonPreferences_Entry<string> ToPath;
        private MelonPreferences_Entry<int> ActionBtnX, ActionBtnY;
        private int tempXLoc, tempYLoc;

        internal static QMNestedButton ActionMenu;
        internal static QMSingleButton CopyAssetButton;

        internal UserInteractUtils()
        {
            MelonPreferences.CreateCategory("UserInteractUtils", "PM - User Interact Utils");
            ToPath = MelonPreferences.CreateEntry("UserInteractUtils", "ToPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Assets"), "Path to save Assets");
            ActionBtnX = MelonPreferences.CreateEntry("UserInteractUtils", "ActionBtnX", 0, "Actions Button Coord X");
            ActionBtnY = MelonPreferences.CreateEntry("UserInteractUtils", "ActionBtnY", 0, "Actions Button Coord Y");
        }

        internal override void OnUiManagerInit()
        {
            tempXLoc = ActionBtnX.Value; // cache temp X button location
            tempYLoc = ActionBtnY.Value; // cache temp Y button location
            ActionMenu = new QMNestedButton("UserInteractMenu", ActionBtnX.Value, ActionBtnY.Value, "<color=#ff0000>Client's\n</color>Actions", "Open the Actions Menu");
            CopyAssetButton = new QMSingleButton(ActionMenu, 1, 0, "Copy\nAsset", () => CopyAsset(),
                "Copies the asset file to the destined folder.", null, null);
        }

        internal override void OnPreferencesSaved()
        {
            if (ActionBtnX.Value != tempXLoc || ActionBtnY.Value != tempYLoc) { // Values Have changed on Pref Save
                ActionMenu.DestroyMe();         // Destroy
                CopyAssetButton.DestroyMe();    // Destroy
                OnUiManagerInit();              // Destroy and Recreate
            }
        }

        private void CopyAsset()
        {
            ApiAvatar avatar = Utilities.GetPlayerFromID(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id).prop_ApiAvatar_0;
            if (!Directory.Exists(ToPath.Value)) Directory.CreateDirectory(ToPath.Value);
            try
            {
                ToCopyAsset(avatar);
                Methods.PopupV2(
                    "Copy Asset",
                    $"Successfully copied avatar \"{avatar.name}\" to folder \"{ToPath.Value}\"!",
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

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private string ComputeVersionString(int version)
        {
            string result = "";
            foreach (byte b in BitConverter.GetBytes(version)) result += b.ToString("X2");
            return string.Concat(Enumerable.Repeat("0", (32 - result.Length))) + result;
        }

        private void ToCopyAsset(ApiAvatar avatar)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(
                        AssetBundleDownloadManager.prop_AssetBundleDownloadManager_0.field_Private_Cache_0.path,
                        ByteArrayToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(avatar.assetUrl.Substring(avatar.assetUrl.IndexOf("file_"), 41)))).ToUpper().Substring(0, 16),
                        ComputeVersionString(avatar.version)))
                    .GetFiles("*.*", SearchOption.AllDirectories))
                if (file.Name.Contains("__data"))
                    File.Copy(file.FullName, Path.Combine(ToPath.Value, $"{avatar.id}.vrca"), true);
        }
    }
}