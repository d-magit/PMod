using System;
using UnityEngine;
using VRC.SDKBase;

namespace Client.Functions.Utils
{
    public class OrbitItem
    {
        private Vector3 InitialPos;
        private Quaternion InitialRot;
        private readonly double Index;
        public readonly bool InitialTheft;
        public readonly bool InitialPickupable;
        public readonly bool InitialKinematic;
        public readonly Vector3 InitialVelocity;
        public readonly bool InitialActive;
        public bool IsOn { get; set; } = true;

        public OrbitItem(VRC_Pickup pickup, double i)
        {
            InitialTheft = pickup.GetComponent<VRC_Pickup>().DisallowTheft;
            InitialPickupable = pickup.GetComponent<VRC_Pickup>().pickupable;
            InitialKinematic = pickup.GetComponent<Rigidbody>().isKinematic;
            InitialVelocity = pickup.GetComponent<Rigidbody>().velocity;
            InitialActive = pickup.gameObject.active;
            InitialPos = pickup.transform.position;
            InitialRot = pickup.transform.rotation;
            Index = i / Orbit.Pickups.Count;
        }

        private Vector3 CircularRot()
        {
            double Angle = Orbit.Timer * Orbit.speed + 2 * Math.PI * Index;
            return Orbit.OrbitCenter + Orbit.rotationy * (Orbit.rotation * new Vector3((float)Math.Cos(Angle) * Orbit.radius, 0, (float)Math.Sin(Angle) * Orbit.radius));
        }

        private Vector3 CylindricalRot()
        {
            double Angle = Orbit.Timer * Orbit.speed + 2 * Math.PI * Index;
            return Orbit.OrbitCenter + new Vector3(0, (float)(Orbit.PlayerHeight * Index), 0) + Orbit.rotationy * 
                (Orbit.rotation * new Vector3((float)Math.Cos(Angle) * Orbit.radius, 0, (float)Math.Sin(Angle) * Orbit.radius));
        }

        private Vector3 SphericalRot()
        {
            double Angle = (Orbit.Timer * Orbit.speed) / (4 * Math.PI) + Index * 360;
            double Height = Orbit.PlayerHeight * ((Orbit.Timer * Orbit.speed / 2 + Index) % 1);
            Quaternion Rotation = Quaternion.Euler(0, (float)Angle, 0);
            return Orbit.OrbitCenter + Orbit.rotationy * (Orbit.rotation * 
                (Rotation * new Vector3((float)(4 * Math.Sqrt(Height * Orbit.PlayerHeight - Math.Pow(Height, 2)) * Orbit.radius), (float)Height, 0)));
        }

        public Vector3 CurrentPos()
        {
            if (IsOn)
            {
                if (Orbit.rotType == Orbit.RotType.CircularRot) return CircularRot();
                else if (Orbit.rotType == Orbit.RotType.CylindricalRot) return CylindricalRot();
                else return SphericalRot();
            }
            return InitialPos;
        }

        public Quaternion CurrentRot()
        {
            float Angle = (float)(Orbit.Timer * 50f * Orbit.speed + 2 * Math.PI * Index);
            if (IsOn) return Quaternion.Euler(-Angle, 0, -Angle);
            else return InitialRot;
        }
    }
}