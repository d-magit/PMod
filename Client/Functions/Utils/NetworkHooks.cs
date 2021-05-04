﻿using System;
using System.Linq;
using System.Reflection;
using Harmony;
using VRC;
using VRC.Core;

namespace Client.Utils
{
    // Copied from loukylor that copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier). Me 🤝 knah 🤝 literally entire modding community. Ty lord and savior knah for all your superior knowledge
    public class NetworkEvents
    {
        public static event Action<Player> OnPlayerLeave;
        public static event Action<Player> OnPlayerJoin;
        public static event Action<ApiWorld, ApiWorldInstance> OnInstanceChange;
        public static event Action<ApiAvatar, VRCAvatarManager> OnAvatarChange;

        private static void OnInstanceChangeMethod(ApiWorld __0, ApiWorldInstance __1)
        {
            OnInstanceChange?.Invoke(__0, __1);
        }
        private static void OnAvatarChangeMethod(ApiAvatar __0, VRCAvatarManager __instance)
        {
            OnAvatarChange?.Invoke(__0, __instance);
        }

        public static void NetworkInit()
        {
            if (NetworkManager.field_Internal_Static_NetworkManager_0 == null) return;
            
            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;
            Main.HarmonyInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));
            Main.HarmonyInstance.Patch(typeof(VRCAvatarManager).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Boolean_ApiAvatar_String_Single_")), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => OnPlayerJoin?.Invoke(player)));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => OnPlayerLeave?.Invoke(player)));
        }
    }
}