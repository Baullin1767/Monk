using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void SetSpeed(float speed)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(SpeedHash, Mathf.Abs(speed));
        }

        public void SetGrounded(bool grounded)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(GroundedHash, grounded);
        }

        public void SetVerticalVelocity(float velocity)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(VerticalVelocityHash, velocity);
        }

        public void TriggerAttack()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(AttackHash);
        }

        public void TriggerAttack2()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(Attack2Hash);
        }

        public void TriggerHurt()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(HurtHash);
        }

        public void TriggerDeath()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsDeadHash, true);
            animator.SetTrigger(DeathHash);
        }

        public void SetDead(bool isDead)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsDeadHash, isDead);
        }
    }
}
