using UnityEngine;

namespace Monk.Configs
{
    [CreateAssetMenu(fileName = "PhysicsConfig", menuName = "Monk/Configs/Physics Config")]
    public class PhysicsConfig : ScriptableObject
    {
        [SerializeField] private float globalGravity = -9.81f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.15f;

        public float GlobalGravity => globalGravity;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
    }
}
