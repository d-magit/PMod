using System;
using System.Linq;
using System.Reflection;
using Harmony;
using VRC;
using VRC.Core;

namespace Client.Utils
{
    // Copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier). Me 🤝 knah 🤝 literally entire modding community. Ty lord and savior knah for all your superior knowledge.
    public class NetworkEvents
    {
        public static event Action<Player> OnJoin;
        public static event Action<Player> OnLeave;
        public static event Action<ApiWorld, ApiWorldInstance> OnInstanceChange;
        public static event Action<ApiAvatar, VRCAvatarManager> OnAvatarChange;

        private static bool IsInitialized;
        private static bool SeenFire;
        private static bool AFiredFirst;

        private static void EventHandlerA(Player player)
        {
            if (!SeenFire)
            {
                AFiredFirst = true;
                SeenFire = true;
            }

            (AFiredFirst ? OnJoin : OnLeave)?.Invoke(player);
        }

        private static void EventHandlerB(Player player)
        {
            if (!SeenFire)
            {
                AFiredFirst = false;
                SeenFire = true;
            }

            (AFiredFirst ? OnLeave : OnJoin)?.Invoke(player);
        }

        private static void AddDelegate(VRCEventDelegate<Player> field, Action<Player> eventHandler) => field.field_Private_HashSet_1_UnityAction_1_T_0.Add(eventHandler);

        private static void OnInstanceChangeMethod(ApiWorld __0, ApiWorldInstance __1) => OnInstanceChange?.Invoke(__0, __1);

        private static void OnAvatarChangeMethod(ApiAvatar __0, VRCAvatarManager __instance) => OnAvatarChange?.Invoke(__0, __instance);

        public static void OnUiManagerInit()
        {
            if (IsInitialized || !NetworkManager.field_Internal_Static_NetworkManager_0) return;

            AddDelegate(NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0, EventHandlerA);
            AddDelegate(NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1, EventHandlerB);

            Main.HarmonyInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));
            Main.HarmonyInstance.Patch(typeof(VRCAvatarManager).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Boolean_ApiAvatar_String_Single_")), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));

            IsInitialized = true;
        }
    }
}