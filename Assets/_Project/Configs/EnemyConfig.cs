using UnityEngine;

namespace Monk.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Monk/Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int contactDamage = 1;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float detectionRange = 5f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Patrol")]
        [SerializeField] private float patrolDistance = 3f;

        public int MaxHealth => maxHealth;
        public int ContactDamage => contactDamage;
        public float MoveSpeed => moveSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float PatrolDistance => patrolDistance;
    }
}
