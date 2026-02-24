using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(PlatformEffector2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private float disableTime = 0.25f;

        private PlatformEffector2D effector;

        private void Awake()
        {
        }

        public void DisablePlatform()
        {
        }
    }
}
