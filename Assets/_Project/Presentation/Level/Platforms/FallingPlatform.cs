using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingPlatform : MonoBehaviour
    {
        [SerializeField] private float shakeDelay = 0.5f;
        [SerializeField] private float fallDelay = 1f;
        [SerializeField] private float respawnDelay = 3f;

        private Rigidbody2D rb;
        private Vector3 initialPosition;

        private void Awake()
        {
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
        }
    }
}
