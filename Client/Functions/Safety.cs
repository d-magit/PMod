using UnhollowerBaseLib;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace Client.Functions
{
    // Incomplete
    internal static class Safety
    {
        public static bool CheckRPC(Player senderPlayer, VRC_EventHandler.VrcEvent @event, VRC_EventHandler.VrcBroadcastType broadcast, int instigatorId, float fastForward)
        {
            if (@event?.EventType == VRC_EventHandler.VrcEventType.SendRPC)
            {
                Il2CppReferenceArray<Il2CppSystem.Object> decodedObjects;
                switch (@event?.ParameterString)
                {
                    case "_InstantiateObject":
                        decodedObjects = Networking.DecodeParameters(@event?.ParameterBytes);
                        var name = decodedObjects[0].ToString();
                        Vector3 position = decodedObjects[1].Unbox<Vector3>();
                        Quaternion rotation = decodedObjects[2].Unbox<Quaternion>();
                        bool flag = false;
                        if (flag)
                        {
                            MelonLogger.Warning($"{senderPlayer?.prop_APIUser_0.displayName} tried to instantiate '{name}' with an invalid position/rotation.");
                            return false;
                        }
                        break;
                }
            }
            return true;
        }
    }
}
