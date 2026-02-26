using System;
using UnityEngine;

namespace Monk.Infrastructure
{
    public class OrientationListener : MonoBehaviour
    {
        public event Action<DeviceOrientation> OnDeviceOrientationChanged;

        public DeviceOrientation CurrentOrientation { get; private set; }

        private DeviceOrientation lastOrientation;

        private void Start()
        {
            lastOrientation = Input.deviceOrientation;
            CurrentOrientation = lastOrientation;
        }

        private void Update()
        {
            var current = Input.deviceOrientation;
            if (current != lastOrientation)
            {
                lastOrientation = current;
                CurrentOrientation = current;
                OnDeviceOrientationChanged?.Invoke(current);
            }
        }
    }
}
