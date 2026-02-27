using System.Collections.Generic;
using UnityEngine;
using Monk.Infrastructure;

namespace Monk.Presentation
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerAttackHitbox : MonoBehaviour
    {
        [SerializeField] private int attackDamage = 1;

        private BoxCollider2D boxCollider;
        private bool isActive;
        private readonly HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();
        private readonly Collider2D[] overlapBuffer = new Collider2D[16];

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.enabled = false;
        }

        public void EnableHitbox()
        {
            hitEnemies.Clear();
            isActive = true;
        }

        public void DisableHitbox()
        {
            isActive = false;
            hitEnemies.Clear();
        }

        private void FixedUpdate()
        {
            if (!isActive) return;

            var worldCenter = (Vector2)transform.TransformPoint(boxCollider.offset);
            var lossyScale = transform.lossyScale;
            var worldSize = new Vector2(
                boxCollider.size.x * Mathf.Abs(lossyScale.x),
                boxCollider.size.y * Mathf.Abs(lossyScale.y)
            );

            int count = Physics2D.OverlapBoxNonAlloc(worldCenter, worldSize, 0f, overlapBuffer);

            for (int i = 0; i < count; i++)
            {
                var col = overlapBuffer[i];
                if (!col.CompareTag("Enemy")) continue;

                var enemyController = col.GetComponent<EnemyController>();
                if (enemyController == null) continue;
                if (!hitEnemies.Add(enemyController)) continue;

                enemyController.TakeDamage(attackDamage);
            }
        }
    }
}
