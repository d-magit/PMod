﻿using System;
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

namespace PMod.Utils
{
    internal static class Methods
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
        private delegate void LocalToGlobalSetupDelegate(IntPtr instancePtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo);
        private delegate IntPtr OnPlayerNetDecodeDelegate(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer);
        private static FreezeSetupDelegate freezeSetupDelegate;
        private static LocalToGlobalSetupDelegate localToGlobalSetupDelegate;
        private static readonly List<OnPlayerNetDecodeDelegate> dontGarbageCollectDelegates = new();
        internal static void OnApplicationStart()
        {
            unsafe
            {
                freezeSetupDelegate = NativePatchUtils.Patch<FreezeSetupDelegate>(typeof(PhotonNetwork)
                    .GetMethod(nameof(PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0),
                           BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                           null, new[] { typeof(byte), typeof(Il2CppSystem.Object), typeof(RaiseEventOptions), typeof(SendOptions) }, null),
                    NativePatchUtils.GetDetour<NativePatches>(nameof(FreezeSetup)));

                localToGlobalSetupDelegate = NativePatchUtils.Patch<LocalToGlobalSetupDelegate>(typeof(VRC_EventHandler)
                    .GetMethod(nameof(VRC_EventHandler.InternalTriggerEvent)),
                    NativePatchUtils.GetDetour<NativePatches>(nameof(LocalToGlobalSetup)));

                foreach (MethodInfo method in typeof(PlayerNet).GetMethods().Where(mi => mi.GetParameters().Length == 3 && mi.Name.StartsWith("Method_Public_Virtual_Final_New_Void_ValueTypePublicSealed")))
                {
                    var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method).GetValue(null);

                    OnPlayerNetDecodeDelegate originalDecodeDelegate = null;

                    OnPlayerNetDecodeDelegate replacement = (instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer) => PlayerNetPatch(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer, originalDecodeDelegate);

                    dontGarbageCollectDelegates.Add(replacement);

                    MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), Marshal.GetFunctionPointerForDelegate(replacement));

                    originalDecodeDelegate = Marshal.GetDelegateForFunctionPointer<OnPlayerNetDecodeDelegate>(originalMethodPointer);
                }
            }
        }

        private static Il2CppSystem.Object LastSent;
        private static IntPtr FreezeSetup(byte EType, IntPtr Obj, IntPtr EOptions, IntPtr SOptions, IntPtr nativeMethodInfo)
        {
            if (EType == 7 && Il2CppArrayBase<int>.WrapNativeGenericArrayPointer(Obj)[0] == ModulesManager.photonFreeze.PhotonID)
            {
                try
                {
                    if (!ModulesManager.photonFreeze.IsFreeze)
                        LastSent = new Il2CppSystem.Object(Obj);
                    else
                        return freezeSetupDelegate(EType, LastSent.Pointer, EOptions, SOptions, nativeMethodInfo);
                }
                catch (Exception e)
                {
                    MelonLogger.Warning("Something went wrong in Freeze Patch");
                    MelonLogger.Error($"{e}");
                }
            }
            return freezeSetupDelegate(EType, Obj, EOptions, SOptions, nativeMethodInfo);
        }

        internal static bool triggerOnce = false;
        private static void LocalToGlobalSetup(IntPtr instancePtr, IntPtr eventPtr, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward, IntPtr nativeMethodInfo)
        {
            try
            {
                if ((ModulesManager.triggers.IsAlwaysForceGlobal || triggerOnce) && broadcast == VRC_EventHandler.VrcBroadcastType.Local)
                {
                    VRC_EventHandler.VrcEvent @event = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<VRC_EventHandler.VrcEvent>(eventPtr);
                    broadcast = VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered;
                    if (triggerOnce) triggerOnce = false;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Warning("Something went wrong in Local to Master Setup Patch");
                MelonLogger.Error($"{e}");
            }
            localToGlobalSetupDelegate(instancePtr, eventPtr, broadcast, instigatorId, fastForward, nativeMethodInfo);
        }

        private static IntPtr PlayerNetPatch(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer, OnPlayerNetDecodeDelegate originalDecodeDelegate)
        {
            IntPtr result = originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);
            try
            {
                if (result != IntPtr.Zero)
                {
                    PlayerNet playerNet = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<PlayerNet>(instancePointer);
                    if (playerNet != null)
                    {
                        Timer entry = null;
                        try { entry = ModulesManager.frozenPlayersManager.EntryDict[playerNet.prop_Player_0.prop_APIUser_0.id]; } catch { };
                        if (entry != null)
                            entry.RestartTimer();
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Warning("Something went wrong in OnPlayerNetPatch");
                MelonLogger.Error($"{e}");
            }
            return result;
        }
    }
}