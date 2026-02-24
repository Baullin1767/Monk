using UnityEngine;
using Monk.Configs;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [Header("Surface Friction")]
        [SerializeField] private float airFriction = 0f;
        [SerializeField] private float groundFriction = 1f;

        private Rigidbody2D rb;
        private Collider2D playerCollider;
        private PhysicsMaterial2D groundMaterial;
        private PhysicsMaterial2D airMaterial;
        private bool lastGrounded;

        public float VerticalVelocity => rb != null ? rb.linearVelocity.y : 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();

            if (playerConfig != null)
            {
                rb.gravityScale = playerConfig.GravityScale;
            }

            InitializePhysicsMaterials();
            lastGrounded = CheckGrounded();
            ApplyFrictionMaterial(lastGrounded);
        }

        private void FixedUpdate()
        {
            var grounded = CheckGrounded();
            if (grounded == lastGrounded)
            {
                return;
            }

            lastGrounded = grounded;
            ApplyFrictionMaterial(grounded);
        }

        public void Move(float horizontalInput)
        {
            if (rb == null)
            {
                return;
            }

            var velocity = rb.linearVelocity;
            velocity.x = horizontalInput * GetMoveSpeed();
            rb.linearVelocity = velocity;
        }

        public void Jump()
        {
            if (rb == null || !CheckGrounded())
            {
                return;
            }

            var velocity = rb.linearVelocity;
            velocity.y = GetJumpForce();
            rb.linearVelocity = velocity;
        }

        public bool CheckGrounded()
        {
            var checkPoint = groundCheck != null ? groundCheck.position : transform.position;
            return Physics2D.OverlapCircle(checkPoint, GetGroundCheckRadius(), groundLayer) != null;
        }

        private float GetMoveSpeed()
        {
            return playerConfig != null ? playerConfig.MoveSpeed : moveSpeed;
        }

        private float GetJumpForce()
        {
            return playerConfig != null ? playerConfig.JumpForce : jumpForce;
        }

        private float GetGroundCheckRadius()
        {
            return playerConfig != null ? playerConfig.GroundCheckRadius : groundCheckRadius;
        }

        private void InitializePhysicsMaterials()
        {
            groundMaterial = new PhysicsMaterial2D("PlayerGroundMaterial")
            {
                friction = Mathf.Max(0f, groundFriction),
                bounciness = 0f
            };

            airMaterial = new PhysicsMaterial2D("PlayerAirMaterial")
            {
                friction = Mathf.Max(0f, airFriction),
                bounciness = 0f
            };
        }

        private void ApplyFrictionMaterial(bool grounded)
        {
            if (playerCollider == null)
            {
                return;
            }

            if (!grounded)
            {
                Debug.Log("Jimp");
            }

            playerCollider.sharedMaterial = grounded ? groundMaterial : airMaterial;
        }
    }
}
