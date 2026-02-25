using UnityEngine;
using Monk.Core;
using Monk.Configs;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Components")]
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyAnimator enemyAnimator;
        [SerializeField] private EnemyView view;

        [Header("Patrol")]
        [SerializeField] private float patrolDistance = 3f;
        [SerializeField] private float patrolWaitTime = 1.5f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float stopDistance = 0.4f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDuration = 0.5f;
        [SerializeField] private float hurtDuration = 0.3f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.1f;

        private Rigidbody2D rb;
        private EnemyState currentState;
        private Transform playerTransform;
        private Vector2 spawnPosition;
        private Vector2 patrolPointA;
        private Vector2 patrolPointB;
        private bool patrollingToB = true;
        private float patrolWaitTimer;
        private float attackCooldownTimer;
        private float attackTimer;
        private float hurtTimer;
        private float deathCleanupDelay = 2f;
        private float contactDamageCooldown;
        private const float ContactDamageInterval = 1f;
        private FacingDirection facing = FacingDirection.Left;

        private float MoveSpeed => config != null ? config.MoveSpeed : 2f;
        private float ChaseSpeed => config != null ? config.ChaseSpeed : 4f;
        private float DetectionRange => config != null ? config.DetectionRange : 5f;
        private float ScaledAttackRange => attackRange;
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (health == null) health = GetComponent<EnemyHealth>();
            if (enemyAnimator == null) enemyAnimator = GetComponent<EnemyAnimator>();
            if (view == null) view = GetComponent<EnemyView>();

            if (config != null)
            {
                patrolDistance = config.PatrolDistance;
                attackRange = config.AttackRange;
                attackCooldown = config.AttackCooldown;
            }
        }

        private void Start()
        {
            spawnPosition = rb.position;
            patrolPointA = spawnPosition + Vector2.left * patrolDistance * 0.5f;
            patrolPointB = spawnPosition + Vector2.right * patrolDistance * 0.5f;

            if (health != null)
            {
                health.OnDied += HandleDeath;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;

            SetState(EnemyState.Idle);
        }

        private void Update()
        {
            if (currentState == EnemyState.Dead) return;

            if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
            if (contactDamageCooldown > 0f) contactDamageCooldown -= Time.deltaTime;

            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;
                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyState.Chase:
                    UpdateChase();
                    break;
                case EnemyState.Attack:
                    UpdateAttack();
                    break;
                case EnemyState.Hurt:
                    UpdateHurt();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (currentState == EnemyState.Dead)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            switch (currentState)
            {
                case EnemyState.Patrol:
                    FixedUpdatePatrol();
                    break;
                case EnemyState.Chase:
                    FixedUpdateChase();
                    break;
                case EnemyState.Idle:
                case EnemyState.Attack:
                case EnemyState.Hurt:
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    break;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDeath;
            }
        }

        public void TakeDamage(int amount)
        {
            if (currentState == EnemyState.Dead) return;

            if (health != null)
            {
                health.TakeDamage(amount);
            }

            if (health != null && health.IsDead) return;

            SetState(EnemyState.Hurt);
            hurtTimer = hurtDuration;
            enemyAnimator?.TriggerHurt();
            view?.PlayDamageFlash();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (currentState == EnemyState.Dead) return;
            if (contactDamageCooldown > 0f) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(config != null ? config.ContactDamage : 1);
                    contactDamageCooldown = ContactDamageInterval;
                }
            }
        }

        private void SetState(EnemyState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case EnemyState.Idle:
                    enemyAnimator?.SetSpeed(0f);
                    patrolWaitTimer = patrolWaitTime;
                    break;
                case EnemyState.Patrol:
                    enemyAnimator?.SetSpeed(1f);
                    break;
                case EnemyState.Chase:
                    enemyAnimator?.SetSpeed(1f);
                    break;
                case EnemyState.Attack:
                    enemyAnimator?.SetSpeed(0f);
                    break;
                case EnemyState.Hurt:
                    enemyAnimator?.SetSpeed(0f);
                    break;
                case EnemyState.Dead:
                    enemyAnimator?.SetSpeed(0f);
                    break;
            }
        }

        private void UpdateIdle()
        {
            if (IsPlayerInDetectionRange())
            {
                SetState(EnemyState.Chase);
                return;
            }

            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                SetState(EnemyState.Patrol);
            }
        }

        private void UpdatePatrol()
        {
            if (IsPlayerInDetectionRange())
            {
                SetState(EnemyState.Chase);
                return;
            }

            var target = patrollingToB ? patrolPointB : patrolPointA;
            var distToTarget = Mathf.Abs(rb.position.x - target.x);

            if (distToTarget < 0.2f)
            {
                patrollingToB = !patrollingToB;
                SetState(EnemyState.Idle);
                return;
            }

            UpdateFacing(target.x > rb.position.x ? FacingDirection.Right : FacingDirection.Left);
        }

        private void FixedUpdatePatrol()
        {
            var target = patrollingToB ? patrolPointB : patrolPointA;
            var direction = Mathf.Sign(target.x - rb.position.x);
            rb.linearVelocity = new Vector2(direction * MoveSpeed, rb.linearVelocity.y);
        }

        private void UpdateChase()
        {
            if (playerTransform == null)
            {
                SetState(EnemyState.Patrol);
                return;
            }

            var playerPosition = playerTransform.position;
            var enemyPosition = rb.position;
            playerPosition.y = 0;
            playerPosition.z = 0;
            enemyPosition.y = 0;
            
            var distToPlayer = Vector2.Distance(playerPosition, enemyPosition);

            if (distToPlayer > DetectionRange * 1.5f)
            {
                SetState(EnemyState.Patrol);
                return;
            }

            if (distToPlayer <= ScaledAttackRange && attackCooldownTimer <= 0f)
            {
                SetState(EnemyState.Attack);
                attackTimer = attackDuration;
                attackCooldownTimer = attackCooldown;
                enemyAnimator?.TriggerAttack();
                DealAttackDamage();
                return;
            }

            UpdateFacing(playerTransform.position.x > rb.position.x ? FacingDirection.Right : FacingDirection.Left);
        }

        private void FixedUpdateChase()
        {
            if (playerTransform == null) return;

            var distToPlayer = Vector2.Distance(rb.position, (Vector2)playerTransform.position);
            if (distToPlayer <= stopDistance)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            var direction = Mathf.Sign(playerTransform.position.x - rb.position.x);
            rb.linearVelocity = new Vector2(direction * ChaseSpeed, rb.linearVelocity.y);
        }

        private void UpdateAttack()
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                if (IsPlayerInDetectionRange())
                    SetState(EnemyState.Chase);
                else
                    SetState(EnemyState.Idle);
            }
        }

        private void UpdateHurt()
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0f)
            {
                if (IsPlayerInDetectionRange())
                    SetState(EnemyState.Chase);
                else
                    SetState(EnemyState.Idle);
            }
        }

        private void HandleDeath()
        {
            SetState(EnemyState.Dead);
            enemyAnimator?.TriggerDeath();
            view?.PlayDeathEffect();

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            Destroy(gameObject, deathCleanupDelay);
        }

        private void DealAttackDamage()
        {
            if (playerTransform == null) return;

            var distToPlayer = Vector2.Distance(rb.position, (Vector2)playerTransform.position);
            if (distToPlayer <= ScaledAttackRange)
            {
                var playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(config != null ? config.ContactDamage : 1);
                }
            }
        }

        private bool IsPlayerInDetectionRange()
        {
            if (playerTransform == null) return false;
            return Vector2.Distance(rb.position, (Vector2)playerTransform.position) <= DetectionRange;
        }

        private void UpdateFacing(FacingDirection direction)
        {
            if (facing == direction) return;
            facing = direction;
            view?.SetFacing(direction);
        }
    }
}
