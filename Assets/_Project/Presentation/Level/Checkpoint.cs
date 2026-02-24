using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private bool isActivated;

        public Vector3 RespawnPosition => transform.position;
        public bool IsActivated => isActivated;

        private void OnTriggerEnter2D(Collider2D other)
        {
        }

        public void Activate()
        {
        }
    }
}
