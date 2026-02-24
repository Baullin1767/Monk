using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerAttackHitbox : MonoBehaviour
    {
        [SerializeField] private int attackDamage = 1;

        private Collider2D hitboxCollider;

        private void Awake()
        {
            hitboxCollider = GetComponent<Collider2D>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }

        public void EnableHitbox()
        {
            if (hitboxCollider != null) hitboxCollider.enabled = true;
        }

        public void DisableHitbox()
        {
            if (hitboxCollider != null) hitboxCollider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemyController = other.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(attackDamage);
            }
        }
    }
}
