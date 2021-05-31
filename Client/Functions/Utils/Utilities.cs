using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using VRC.UI;

namespace Client.Functions.Utils
{
    internal static class Utilities
    {
        public static VRCPlayer GetLocalVRCPlayer() => VRCPlayer.field_Internal_Static_VRCPlayer_0;
        public static VRCPlayerApi GetLocalVRCPlayerApi() => Player.prop_Player_0.prop_VRCPlayerApi_0;
        public static VRCPlayer GetPlayerFromID(string id) => ((Player)HarmonyPatches.PlayerFromID.Invoke(null, new object[] { id })).prop_VRCPlayer_0;
        public static void ChangeToAVByID(string id) => new PageAvatar
        {
            field_Public_SimpleAvatarPedestal_0 = new SimpleAvatarPedestal
            {
                field_Internal_ApiAvatar_0 = new ApiAvatar
                {
                    id = id
                }
            }
        }.ChangeToSelectedAvatar();

        public static bool ContainsStr(MethodBase methodBase, string match)
        {
            try
            {
                return XrefScanner.XrefScan(methodBase)
                    .Any(instance => instance.Type == XrefType.Global && instance.ReadAsObject() != null &&
                         instance.ReadAsObject().ToString().Equals(match, StringComparison.OrdinalIgnoreCase));
            }
            catch { }
            return false;
        }
        public static bool WasUsedBy(MethodBase methodBase, string methodName)
        {
            try
            {
                return XrefScanner.UsedBy(methodBase)
                    .Any(instance => instance.TryResolve() != null &&
                         instance.TryResolve().Name.Equals(methodName, StringComparison.Ordinal));
            }
            catch { }
            return false;
        }

        public static Transform GetBoneTransform(Player player, HumanBodyBones bone)
        {
            Transform playerPosition = player.transform;
            VRCAvatarManager avatarManager = player.prop_VRCPlayer_0.prop_VRCAvatarManager_0;
            if (!avatarManager) return playerPosition;
            Animator animator = avatarManager.field_Private_Animator_0;
            if (!animator) return playerPosition;
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (!boneTransform) return playerPosition;
            return boneTransform;
        }
    }
}
