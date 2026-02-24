using UnityEngine;
using UnityEngine.InputSystem;
using Monk.Core;

namespace Monk.Input
{
    public class DesktopInputProvider : MonoBehaviour, IInputProvider
    {
        public float HorizontalAxis { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                HorizontalAxis = 0f;
                JumpPressed = false;
                AttackPressed = false;
                return;
            }

            var moveLeft = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            var moveRight = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;

            HorizontalAxis = 0f;
            if (moveLeft)
            {
                HorizontalAxis -= 1f;
            }

            if (moveRight)
            {
                HorizontalAxis += 1f;
            }

            HorizontalAxis = Mathf.Clamp(HorizontalAxis, -1f, 1f);
            JumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            AttackPressed = keyboard.eKey.wasPressedThisFrame;
        }
    }
}
