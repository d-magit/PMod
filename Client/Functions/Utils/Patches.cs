using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using MelonLoader;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using VRC;

namespace Client.Functions.Utils
{
    internal static class HarmonyPatches
    {
        public static void PopupV2(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null) =>
            GetPopupV2Delegate(title, innertxt, buttontxt, buttonOk, action);
        private delegate void PopupV2Delegate(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null);
        private static PopupV2Delegate popupV2Delegate;
        private static PopupV2Delegate GetPopupV2Delegate
        {
            get
            {
                if (popupV2Delegate != null)  popupV2Delegate = (PopupV2Delegate)Delegate.CreateDelegate(typeof(PopupV2Delegate), 
                    VRCUiPopupManager.prop_VRCUiPopupManager_0, 
                    typeof(VRCUiPopupManager).GetMethods()
                        .First(methodBase => methodBase.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") &&
                        !methodBase.Name.Contains("PDM")));
                return popupV2Delegate;
            }
        }

        private static MethodInfo playerFromID;
        public static MethodInfo PlayerFromID
        {
            get
            {
                if (playerFromID != null) playerFromID = typeof(PlayerManager).GetMethods()
                    .Where(methodBase => methodBase.Name.StartsWith("Method_Public_Static_Player_String_") && !methodBase.Name.Contains("PDM"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();
                return playerFromID;
            }
        }
    }

    internal static class NativePatches
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FreezeSetupDelegate(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo);
        private static FreezeSetupDelegate freezeSetupDelegate;
        public static void OnApplicationStart()
        {
            unsafe
            {
                var RaiseEventMethod = typeof(PhotonNetwork).GetMethod(
                nameof(PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                null, new[] { typeof(byte), typeof(Il2CppSystem.Object), typeof(RaiseEventOptions), typeof(SendOptions) }, null);

                var originalMethod = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(RaiseEventMethod).GetValue(null);

                MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), typeof(NativePatches).GetMethod(nameof(FreezeSetup),
                    BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());

                freezeSetupDelegate = Marshal.GetDelegateForFunctionPointer<FreezeSetupDelegate>(originalMethod);
            }
        }

        private static object[] LastSent;
        public static IntPtr FreezeSetup(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo)
        {
            if (EType == 7)
            {
                try
                {
                    if (!Freeze.IsFreeze)
                    {
                        LastSent = new object[] { EType, Obj, EOptions, SOptions };
                    }
                    else
                    {
                        return IntPtr.Zero;
                        //return freezeSetupDelegate(LastSent[0], LastSent[1], LastSent[2], LastSent[3], nativeMethodInfo);
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg(ConsoleColor.Yellow, "Something went wrong in Freeze Patch");
                    MelonLogger.Error($"{e}");
                }
            }
            return freezeSetupDelegate(EType, Obj, EOptions, SOptions, nativeMethodInfo);
        }
    }
}