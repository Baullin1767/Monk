using UnityEngine;
using Monk.Core;

namespace Monk.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Monk/Configs/Player Config")]
    public class PlayerConfig : ScriptableObject, IPhysicsConfig
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float gravityScale = 3f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Header("Health")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invincibilityDuration = 1.5f;

        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float GravityScale => gravityScale;
        public float FallMultiplier => fallMultiplier;
        public float GroundCheckRadius => groundCheckRadius;
        public int MaxHealth => maxHealth;
        public float InvincibilityDuration => invincibilityDuration;
    }
}
