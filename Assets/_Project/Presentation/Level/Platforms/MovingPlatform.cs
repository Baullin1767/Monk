using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float waitTime = 0.5f;

        private Rigidbody2D rb;

        private void Awake()
        {
        }

        private void FixedUpdate()
        {
        }
    }
}
