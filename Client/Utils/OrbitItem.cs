using System;
using UnityEngine;
using VRC.SDKBase;

namespace PMod.Utils
{
    internal class OrbitItem
    {
        private Vector3 InitialPos;
        private Quaternion InitialRot;
        private readonly double Index;
        internal readonly bool InitialTheft;
        internal readonly bool InitialPickupable;
        internal readonly bool InitialKinematic;
        internal readonly Vector3 InitialVelocity;
        internal readonly bool InitialActive;
        internal bool IsOn { get; set; } = true;

        internal OrbitItem(VRC_Pickup pickup, double i)
        {
            InitialTheft = pickup.GetComponent<VRC_Pickup>().DisallowTheft;
            InitialPickupable = pickup.GetComponent<VRC_Pickup>().pickupable;
            InitialKinematic = pickup.GetComponent<Rigidbody>().isKinematic;
            InitialVelocity = pickup.GetComponent<Rigidbody>().velocity;
            InitialActive = pickup.gameObject.active;
            InitialPos = pickup.transform.position;
            InitialRot = pickup.transform.rotation;
            Index = i / ModulesManager.orbit.Pickups.Count;
        }

        private Vector3 CircularRot()
        {
            double Angle = ModulesManager.orbit.Timer * ModulesManager.orbit.speed.Value + 2 * Math.PI * Index;
            return ModulesManager.orbit.OrbitCenter + ModulesManager.orbit.rotationy * (ModulesManager.orbit.rotation * new Vector3((float)Math.Cos(Angle) * ModulesManager.orbit.radius.Value, 0, 
                (float)Math.Sin(Angle) * ModulesManager.orbit.radius.Value));
        }

        private Vector3 CylindricalRot()
        {
            double Angle = ModulesManager.orbit.Timer * ModulesManager.orbit.speed.Value + 2 * Math.PI * Index;
            return ModulesManager.orbit.OrbitCenter + new Vector3(0, (float)(ModulesManager.orbit.PlayerHeight * Index), 0) + ModulesManager.orbit.rotationy * 
                (ModulesManager.orbit.rotation * new Vector3((float)Math.Cos(Angle) * ModulesManager.orbit.radius.Value, 0, (float)Math.Sin(Angle) * ModulesManager.orbit.radius.Value));
        }

        private Vector3 SphericalRot()
        {
            double Angle = (ModulesManager.orbit.Timer * ModulesManager.orbit.speed.Value) / (4 * Math.PI) + Index * 360;
            double Height = ModulesManager.orbit.PlayerHeight * ((ModulesManager.orbit.Timer * ModulesManager.orbit.speed.Value / 2 + Index) % 1);
            Quaternion Rotation = Quaternion.Euler(0, (float)Angle, 0);
            return ModulesManager.orbit.OrbitCenter + ModulesManager.orbit.rotationy * (ModulesManager.orbit.rotation * 
                (Rotation * new Vector3((float)(4 * Math.Sqrt(Height * ModulesManager.orbit.PlayerHeight - Math.Pow(Height, 2)) * ModulesManager.orbit.radius.Value), (float)Height, 0)));
        }

        internal Vector3 CurrentPos()
        {
            if (IsOn)
            {
                if (ModulesManager.orbit.rotType == Modules.Orbit.RotType.CircularRot) return CircularRot();
                else if (ModulesManager.orbit.rotType == Modules.Orbit.RotType.CylindricalRot) return CylindricalRot();
                else return SphericalRot();
            }
            return InitialPos;
        }

        internal Quaternion CurrentRot()
        {
            float Angle = (float)(ModulesManager.orbit.Timer * 50f * ModulesManager.orbit.speed.Value + 2 * Math.PI * Index);
            if (IsOn) return Quaternion.Euler(-Angle, 0, -Angle);
            else return InitialRot;
        }
    }
}