using System;
using System.Reflection;
using HarmonyLib;
using VRC;
using VRC.Core;

namespace Client.Utils
{
    internal static class NetworkEvents
    {
        internal static event Action<Player> OnPlayerJoined;
        internal static event Action<Player> OnPlayerLeft;
        internal static event Action<ApiWorld, ApiWorldInstance> OnInstanceChange;
        internal static event Action<Photon.Realtime.Player> OnMasterChanged;

        private static bool SeenFire;
        private static bool AFiredFirst;

        private static void EventHandlerA(Player player)
        {
            if (!SeenFire)
            {
                AFiredFirst = true;
                SeenFire = true;
            }

            (AFiredFirst ? OnPlayerJoined : OnPlayerLeft)?.Invoke(player);
        }

        private static void EventHandlerB(Player player)
        {
            if (!SeenFire)
            {
                AFiredFirst = false;
                SeenFire = true;
            }

            (AFiredFirst ? OnPlayerLeft : OnPlayerJoined)?.Invoke(player);
        }

        private static void OnInstanceChangeMethod(ApiWorld __0, ApiWorldInstance __1) => OnInstanceChange?.Invoke(__0, __1);

        private static void OnMasterChange(Photon.Realtime.Player __0) => OnMasterChanged?.Invoke(__0);

        private static void AddDelegate(VRCEventDelegate<Player> field, Action<Player> eventHandler) => field.field_Private_HashSet_1_UnityAction_1_T_0.Add(eventHandler);

        internal static void OnUiManagerInit()
        {
            AddDelegate(NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0, EventHandlerA);
            AddDelegate(NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1, EventHandlerB);

            Main.HInstance.Patch(typeof(NetworkManager).GetMethod("OnMasterClientSwitched"),
                new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnMasterChange), BindingFlags.NonPublic | BindingFlags.Static)));
            Main.HInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, 
                new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));
        }
    }
}