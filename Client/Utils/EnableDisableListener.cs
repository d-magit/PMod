using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Client.Utils
{
    public class EnableDisableListener : MonoBehaviour
    {
        public EnableDisableListener(IntPtr obj0) : base(obj0) {}

        [method: HideFromIl2Cpp]
        public event Action OnEnabled;

        [method: HideFromIl2Cpp]
        public event Action OnDisabled;

        private void OnEnable() => OnEnabled?.Invoke();

        private void OnDisable() => OnDisabled?.Invoke();
    }
}