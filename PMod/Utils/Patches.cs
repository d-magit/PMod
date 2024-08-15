using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using MelonLoader;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using VRC;
using VRC.SDKBase;
using PMod.Loader;

namespace PMod.Utils
{
    internal static class DelegateMethods
    {
        internal static void PopupV2(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null) =>
            GetPopupV2Delegate(title, innertxt, buttontxt, buttonOk, action);
        private delegate void PopupV2Delegate(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null);
        private static PopupV2Delegate popupV2Delegate;
        private static PopupV2Delegate GetPopupV2Delegate => 
            popupV2Delegate ??= (PopupV2Delegate)Delegate.CreateDelegate(typeof(PopupV2Delegate), 
                    VRCUiPopupManager.prop_VRCUiPopupManager_0, 
                    typeof(VRCUiPopupManager).GetMethods()
                        .First(methodBase => methodBase.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") &&
                        !methodBase.Name.Contains("PDM") &&
                        Utilities.ContainsStr(methodBase, "UserInterface/MenuContent/Popups/StandardPopupV2") &&
                        Utilities.WasUsedBy(methodBase, "OpenSaveSearchPopup")));

        private static MethodInfo playerFromID;
        internal static MethodInfo PlayerFromID =>
                playerFromID ??= typeof(PlayerManager).GetMethods()
                    .Where(methodBase => methodBase.Name.StartsWith("Method_Public_Static_Player_String_") && !methodBase.Name.Contains("PDM"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();
    }

    internal class NativePatches
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FreezeSetupDelegate(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr InvisibleJoinDelegate(IntPtr instancePointer, byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OnPlayerNetDecodeDelegate(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LocalToGlobalSetupDelegate(IntPtr instancePtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo);
        private static FreezeSetupDelegate freezeSetupDelegate;
        private static InvisibleJoinDelegate invisibleJoinDelegate;
        private static LocalToGlobalSetupDelegate localToGlobalSetupDelegate;
        private static readonly List<OnPlayerNetDecodeDelegate> dontGCDelegates = new();
        internal unsafe static void OnApplicationStart()
        {
            freezeSetupDelegate = NativePatchUtils.Patch<FreezeSetupDelegate>(typeof(PhotonNetwork)
                .GetMethod(nameof(PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0),
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                        null, new[] { typeof(byte), typeof(Il2CppSystem.Object), typeof(RaiseEventOptions), typeof(SendOptions) }, null),
                NativePatchUtils.GetDetour<NativePatches>(nameof(FreezeSetup)));

            // Please don't use this it's dangerous af lol u r gonna get banned XD // Also, why would u even use this? creep
            invisibleJoinDelegate = NativePatchUtils.Patch<InvisibleJoinDelegate>(typeof(LoadBalancingClient)
                .GetMethod(nameof(LoadBalancingClient.Method_Public_Virtual_New_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0),
                        BindingFlags.Public | BindingFlags.Instance,
                        null, new[] { typeof(byte), typeof(Il2CppSystem.Object), typeof(RaiseEventOptions), typeof(SendOptions) }, null),
                NativePatchUtils.GetDetour<NativePatches>(nameof(InvisibleJoinSetup)));

            localToGlobalSetupDelegate = NativePatchUtils.Patch<LocalToGlobalSetupDelegate>(typeof(VRC_EventHandler)
                .GetMethod(nameof(VRC_EventHandler.InternalTriggerEvent)),
                NativePatchUtils.GetDetour<NativePatches>(nameof(LocalToGlobalSetup)));

            //(from p in m.GetParameters() select p.ParameterType).ToArray() == new[] { typeof(byte), typeof(Il2CppSystem.Object), typeof(RaiseEventOptions), typeof(SendOptions) }
            var mIEnum = typeof(PlayerNet).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetParameters().Length == 3 && m.Name.StartsWith("Method_Public_Virtual_Final_New_Void_ValueTypePublicSealed"));
            foreach (var mInfo in mIEnum)
            {
                OnPlayerNetDecodeDelegate tempMethod, originalMethod = null;
                dontGCDelegates.Add(tempMethod = (instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer) =>
                    PlayerNetPatch(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer, originalMethod));
                originalMethod = NativePatchUtils.Patch<OnPlayerNetDecodeDelegate>(mInfo, Marshal.GetFunctionPointerForDelegate(tempMethod));
            }
        }

        private static Il2CppSystem.Object LastSent;
        private static IntPtr FreezeSetup(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo)
        {
            switch (EType)
            {
                case 7:
                    if (Il2CppArrayBase<int>.WrapNativeGenericArrayPointer(Obj)[0] != ModulesManager.photonFreeze.PhotonID) break;
                    try
                    {
                        if (!ModulesManager.photonFreeze.IsFreeze)
                            LastSent = new Il2CppSystem.Object(Obj);
                        else
                            return freezeSetupDelegate(EType, LastSent.Pointer, EOptions, SOptions, nativeMethodInfo);
                    }
                    catch (Exception e)
                    {
                        PLogger.Warning("Something went wrong in FreezeSetup Patch");
                        PLogger.Error($"{e}");
                    }
                    break;
            }
            return freezeSetupDelegate(EType, Obj, EOptions, SOptions, nativeMethodInfo);
        }

        // Please don't use this it's dangerous af lol u r gonna get banned XD // Also, why would u even use this? creep
        internal static bool triggerOnceInvisible = false;
        private static IntPtr InvisibleJoinSetup(IntPtr instancePointer, byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo)
        {
            RaiseEventOptions REOptions = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<RaiseEventOptions>(EOptions);
            IntPtr _return = IntPtr.Zero;
            bool ran = false;
            switch (EType)
            {
                case 202:
                    try
                    {
                        if (!triggerOnceInvisible) break;
                        REOptions.field_Public_ReceiverGroup_0 = ReceiverGroup.MasterClient;
                        _return = invisibleJoinDelegate(instancePointer, EType, Obj, EOptions, SOptions, nativeMethodInfo);
                        REOptions.field_Public_ReceiverGroup_0 = ReceiverGroup.Others;
                        if (ModulesManager.invisibleJoin.onceOnly) triggerOnceInvisible = false;
                        ran = true;
                    }
                    catch (Exception e)
                    {
                        PLogger.Warning("Something went wrong in InvisibleJoinSetup Patch");
                        PLogger.Error($"{e}");
                    }
                    break;
            }
            return ran ? _return : invisibleJoinDelegate(instancePointer, EType, Obj, EOptions, SOptions, nativeMethodInfo);
        }

        internal static bool triggerOnceLTG = false;
        private static void LocalToGlobalSetup(IntPtr instancePtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo)
        {
            try
            {
                if ((ModulesManager.triggers.IsAlwaysForceGlobal || triggerOnceLTG) && broadcast == VRC_EventHandler.VrcBroadcastType.Local)
                {
                    VRC_EventHandler.VrcEvent @event = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<VRC_EventHandler.VrcEvent>(eventPtr);
                    broadcast = VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered;
                    if (triggerOnceLTG) triggerOnceLTG = false;
                }
            }
            catch (Exception e)
            {
                PLogger.Warning("Something went wrong in LocalToGlobalSetup Patch");
                PLogger.Error($"{e}");
            }
            localToGlobalSetupDelegate(instancePtr, eventPtr, broadcast, instigatorId, fastForward, nativeMethodInfo);
        }

        private static IntPtr PlayerNetPatch(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer, OnPlayerNetDecodeDelegate originalDecodeDelegate)
        {
            IntPtr result = originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);
            try
            {
                if (result == IntPtr.Zero) return result;

                PlayerNet playerNet = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<PlayerNet>(instancePointer);
                if (playerNet == null) return result;

                Timer entry = null;
                try { entry = ModulesManager.frozenPlayersManager.EntryDict[playerNet.prop_Player_0.prop_APIUser_0.id]; } catch { };
                if (entry != null) entry.RestartTimer();
            }
            catch (Exception e)
            {
                PLogger.Warning("Something went wrong in PlayerNetPatch");
                PLogger.Error($"{e}");
            }
            return result;
        }
    }
}