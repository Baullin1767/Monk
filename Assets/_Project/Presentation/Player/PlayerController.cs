using UnityEngine;
using Monk.Core;
using Monk.Infrastructure;
using Monk.Input;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerAnimator playerAnimator;
        [SerializeField] private PlayerView view;
        [SerializeField] private PlayerAttackHitbox attackHitbox;
        [SerializeField] private float firstAttackDuration = 0.2f;
        [SerializeField] private float secondAttackDuration = 0.2f;
        [SerializeField] private float comboChainTimeout = 0.8f;
        [SerializeField] private float fallDeathY = -12f;

        private IInputProvider inputProvider;
        private float horizontalInput;
        private bool jumpQueued;
        private bool queueSecondAttack;
        private bool waitingForSecondAttack;
        private float attackTimer;
        private float comboChainTimer;
        private bool wasGrounded;
        private bool isDead;
        private AudioManager audioManager;

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<PlayerMovement>();
            }

            if (health == null)
            {
                health = GetComponent<PlayerHealth>();
            }

            if (playerAnimator == null)
            {
                playerAnimator = GetComponent<PlayerAnimator>();
            }

            if (view == null)
            {
                view = GetComponent<PlayerView>();
            }

            if (inputManager == null)
            {
                inputManager = FindFirstObjectByType<InputManager>();
            }

            inputProvider = inputManager != null ? inputManager.ActiveProvider : null;

            if (health != null)
            {
                health.OnPlayerDied += HandlePlayerDied;
            }
            if (audioManager == null)
            {
                audioManager = FindFirstObjectByType<AudioManager>();
            }
        }

        private void Update()
        {
            if (isDead)
            {
                SyncAnimationState();
                return;
            }

            if (transform.position.y < fallDeathY && health != null && !health.IsDead)
            {
                health.Kill();
                SyncAnimationState();
                return;
            }

            if (inputManager != null && inputProvider == null)
            {
                inputProvider = inputManager.ActiveProvider;
            }

            horizontalInput = inputProvider != null ? Mathf.Clamp(inputProvider.HorizontalAxis, -1f, 1f) : 0f;

            if (inputProvider != null && inputProvider.JumpPressed)
            {
                jumpQueued = true;
            }

            if (inputProvider != null && inputProvider.AttackPressed)
            {
                HandleAttackInput();
            }

            if (Mathf.Abs(horizontalInput) > 0.01f && view != null)
            {
                view.SetFacing(horizontalInput < 0f ? FacingDirection.Left : FacingDirection.Right);
            }

            SyncAnimationState();
        }

        private void FixedUpdate()
        {
            if (movement == null)
            {
                return;
            }

            if (isDead)
            {
                movement.Move(0f);
                return;
            }

            if (attackTimer > 0f)
            {
                attackTimer -= Time.fixedDeltaTime;
                if (attackTimer <= 0f && attackHitbox != null)
                {
                    attackHitbox.DisableHitbox();
                }
            }

            if (comboChainTimer > 0f)
            {
                comboChainTimer -= Time.fixedDeltaTime;
            }
            else
            {
                waitingForSecondAttack = false;
            }

            movement.Move(horizontalInput);

            if (jumpQueued)
            {
                movement.Jump();
                jumpQueued = false;
            }

            if (attackTimer <= 0f && queueSecondAttack)
            {
                queueSecondAttack = false;
                waitingForSecondAttack = false;
                comboChainTimer = 0f;
                attackTimer = secondAttackDuration;
                playerAnimator?.TriggerAttack2();
                attackHitbox?.EnableHitbox();
                if (audioManager != null)
                    audioManager.PlayHitEnemy();
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnPlayerDied -= HandlePlayerDied;
            }
        }

        private void SyncAnimationState()
        {
            if (movement == null || playerAnimator == null)
            {
                return;
            }

            var grounded = movement.CheckGrounded();
            var verticalVelocity = movement.VerticalVelocity;

            playerAnimator.SetSpeed(Mathf.Abs(horizontalInput));
            playerAnimator.SetGrounded(grounded);
            playerAnimator.SetVerticalVelocity(verticalVelocity);

            if (!wasGrounded && grounded)
            {
                view?.PlayLandEffect();
            }

            wasGrounded = grounded;
        }

        private void HandlePlayerDied()
        {
            isDead = true;
            horizontalInput = 0f;
            jumpQueued = false;
            queueSecondAttack = false;
            waitingForSecondAttack = false;
            attackHitbox?.DisableHitbox();
            playerAnimator?.SetDead(true);
            playerAnimator?.TriggerDeath();
        }

        private void HandleAttackInput()
        {
            if (playerAnimator == null)
            {
                return;
            }

            if (waitingForSecondAttack)
            {
                if (attackTimer > 0f)
                {
                    queueSecondAttack = true;
                }
                else
                {
                    queueSecondAttack = false;
                    waitingForSecondAttack = false;
                    comboChainTimer = 0f;
                    attackTimer = secondAttackDuration;
                    playerAnimator.TriggerAttack2();
                    attackHitbox?.EnableHitbox();
                    if (audioManager != null)
                        audioManager.PlayHitEnemy();
                }

                return;
            }

            if (attackTimer <= 0f)
            {
                attackTimer = firstAttackDuration;
                comboChainTimer = comboChainTimeout;
                waitingForSecondAttack = true;
                queueSecondAttack = false;
                playerAnimator.TriggerAttack();
                attackHitbox?.EnableHitbox();
                if (audioManager != null)
                    audioManager.PlayHitEnemy();
            }
        }
    }
}
