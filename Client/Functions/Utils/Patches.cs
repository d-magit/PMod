using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using VRC;
using VRC.SDKBase;

namespace Client.Functions.Utils
{
    internal static class Methods
    {
        public static void PopupV2(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null) =>
            GetPopupV2Delegate(title, innertxt, buttontxt, buttonOk, action);
        private delegate void PopupV2Delegate(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null);
        private static PopupV2Delegate popupV2Delegate;
        private static PopupV2Delegate GetPopupV2Delegate
        {
            get
            {
                if (popupV2Delegate == null) popupV2Delegate = (PopupV2Delegate)Delegate.CreateDelegate(typeof(PopupV2Delegate), 
                    VRCUiPopupManager.prop_VRCUiPopupManager_0, 
                    typeof(VRCUiPopupManager).GetMethods()
                        .First(methodBase => methodBase.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") &&
                        !methodBase.Name.Contains("PDM") &&
                        Utilities.ContainsStr(methodBase, "UserInterface/MenuContent/Popups/StandardPopupV2") &&
                        Utilities.WasUsedBy(methodBase, "OpenSaveSearchPopup")));
                return popupV2Delegate;
            }
        }

        private static MethodInfo playerFromID;
        public static MethodInfo PlayerFromID
        {
            get
            {
                if (playerFromID == null) playerFromID = typeof(PlayerManager).GetMethods()
                    .Where(methodBase => methodBase.Name.StartsWith("Method_Public_Static_Player_String_") && !methodBase.Name.Contains("PDM"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();
                return playerFromID;
            }
        }
    }

    //internal static class HarmonyPatches { }

    internal class NativePatches
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FreezeSetupDelegate(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo);
        private delegate void ValidateAndTriggerEventDelegate(IntPtr instancePtr, IntPtr senderPtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo);
        private delegate void LocalToGlobalSetupDelegate(IntPtr instancePtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo);
        private static FreezeSetupDelegate freezeSetupDelegate;
        private static ValidateAndTriggerEventDelegate validateAndTriggerEventDelegate;
        private static LocalToGlobalSetupDelegate localToGlobalSetupDelegate;
        public static void OnApplicationStart()
        {
            unsafe
            {
                freezeSetupDelegate = NativePatchUtils.Patch<FreezeSetupDelegate>(typeof(PhotonNetwork)
                    .GetMethod(nameof(PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0),
                           BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                           null, new[] { typeof(byte), typeof(Il2CppSystem.Object), typeof(RaiseEventOptions), typeof(SendOptions) }, null),
                    NativePatchUtils.GetDetour<NativePatches>(nameof(FreezeSetup)));

                validateAndTriggerEventDelegate = NativePatchUtils.Patch<ValidateAndTriggerEventDelegate>(typeof(VRC_EventDispatcherRFC)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .First(m => m.ReturnType == typeof(void) && 
                          !m.Name.Contains("PDM") &&
                           m.GetParameters().Select(x => x.ParameterType.FullName).SequenceEqual(new Type[] 
                           { 
                               typeof(VRC.Player), 
                               typeof(VRC_EventHandler.VrcEvent), 
                               typeof(VRC_EventHandler.VrcBroadcastType), 
                               typeof(int), 
                               typeof(float) 
                           }.Select(x => x.FullName))),
                    NativePatchUtils.GetDetour<NativePatches>(nameof(ValidateAndTriggerEvent)));

                localToGlobalSetupDelegate = NativePatchUtils.Patch<LocalToGlobalSetupDelegate>(typeof(VRC_EventHandler)
                    .GetMethod(nameof(VRC_EventHandler.InternalTriggerEvent)),
                    NativePatchUtils.GetDetour<NativePatches>(nameof(LocalToGlobalSetup)));
            }
        }

        private static object[] LastSent;
        private static IntPtr FreezeSetup(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo)
        {
            if (EType == 7)
            {
                try
                {
                    if (!PhotonFreeze.IsFreeze)
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

        private static void ValidateAndTriggerEvent(IntPtr instancePtr, IntPtr senderPtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo)
        {
            try
            {
                VRC.Player senderPlayer = senderPtr != IntPtr.Zero ? UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<VRC.Player>(senderPtr) : null;
                VRC_EventHandler.VrcEvent @event = eventPtr != IntPtr.Zero ? UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<VRC_EventHandler.VrcEvent>(eventPtr) : null;
                if (Safety.CheckRPC(senderPlayer, @event, broadcast, instigatorId, fastForward))
                    validateAndTriggerEventDelegate(instancePtr, senderPtr, eventPtr, broadcast, instigatorId, fastForward, nativeMethodInfo);
                else MelonLogger.Msg(ConsoleColor.Green, $"Successfully blocked event {@event.ParameterString} from player {senderPlayer?.prop_APIUser_0.displayName}.");
            }
            catch (Exception e)
            {
                MelonLogger.Msg(ConsoleColor.Yellow, "Something went wrong in Validate Event Patch");
                MelonLogger.Error($"{e}");
            }
        }

        private static void LocalToGlobalSetup(IntPtr instancePtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo)
        {
            try
            {
                if (LocalToGlobal.IsForceGlobal && broadcast == VRC_EventHandler.VrcBroadcastType.Local)
                {
                    VRC_EventHandler.VrcEvent @event = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<VRC_EventHandler.VrcEvent>(eventPtr);
                    MelonLogger.Msg(ConsoleColor.Green, $"Successfully patched local event {@event.ParameterString} into AlwaysUnbuffered event.");
                    broadcast = VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg(ConsoleColor.Yellow, "Something went wrong in Local to Master Setup Patch");
                MelonLogger.Error($"{e}");
            }
            localToGlobalSetupDelegate(instancePtr, eventPtr, broadcast, instigatorId, fastForward, nativeMethodInfo);
        }
    }
}