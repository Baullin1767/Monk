using UnityEngine;
using Monk.Core;
using UnityEngine.InputSystem;

namespace Monk.Input
{
    public class MobileInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private Joystick movementJoystick;
        [SerializeField] private MobileActionButton jumpButton;
        [SerializeField] private MobileActionButton attackButton;
        [SerializeField] private bool keyboardFallbackInEditor;

        public float HorizontalAxis { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }

        private bool jumpQueued;
        private bool attackQueued;

        private void Awake()
        {
            if (movementJoystick == null)
            {
                movementJoystick = FindFirstObjectByType<Joystick>();
            }

            if (jumpButton == null || attackButton == null)
            {
                var buttons = FindObjectsByType<MobileActionButton>(FindObjectsSortMode.None);
                foreach (var button in buttons)
                {
                    if (button == null)
                    {
                        continue;
                    }

                    var nameLower = button.name.ToLowerInvariant();
                    if (jumpButton == null && nameLower.Contains("jump"))
                    {
                        jumpButton = button;
                        continue;
                    }

                    if (attackButton == null && nameLower.Contains("attack"))
                    {
                        attackButton = button;
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (jumpButton != null)
            {
                jumpButton.OnButtonPressed -= HandleJumpPressed;
                jumpButton.OnButtonPressed += HandleJumpPressed;
            }

            if (attackButton != null)
            {
                attackButton.OnButtonPressed -= HandleAttackPressed;
                attackButton.OnButtonPressed += HandleAttackPressed;
            }
        }

        private void OnDisable()
        {
            if (jumpButton != null)
            {
                jumpButton.OnButtonPressed -= HandleJumpPressed;
            }

            if (attackButton != null)
            {
                attackButton.OnButtonPressed -= HandleAttackPressed;
            }
        }

        private void Update()
        {
            HorizontalAxis = movementJoystick != null ? Mathf.Clamp(movementJoystick.Horizontal, -1f, 1f) : 0f;

            if (keyboardFallbackInEditor && Application.isEditor)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    {
                        HorizontalAxis = -1f;
                    }
                    else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    {
                        HorizontalAxis = 1f;
                    }

                    if (keyboard.spaceKey.wasPressedThisFrame)
                    {
                        jumpQueued = true;
                    }

                    if (keyboard.eKey.wasPressedThisFrame)
                    {
                        attackQueued = true;
                    }
                }
            }

            JumpPressed = jumpQueued;
            AttackPressed = attackQueued;
            jumpQueued = false;
            attackQueued = false;
        }

        private void HandleJumpPressed()
        {
            jumpQueued = true;
        }

        private void HandleAttackPressed()
        {
            attackQueued = true;
        }
    }
}
