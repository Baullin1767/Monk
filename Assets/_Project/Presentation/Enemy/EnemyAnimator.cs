using UnityEngine;
using Monk.Core;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
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
            if (animator != null) animator.SetFloat(SpeedHash, Mathf.Abs(speed));
        }

        public void TriggerAttack()
        {
            if (animator != null) animator.SetTrigger(AttackHash);
        }

        public void TriggerHurt()
        {
            if (animator != null) animator.SetTrigger(HurtHash);
        }

        public void TriggerDeath()
        {
            if (animator != null)
            {
                animator.SetBool(IsDeadHash, true);
                animator.SetTrigger(DeathHash);
            }
        }

        public void SetDead(bool isDead)
        {
            if (animator != null) animator.SetBool(IsDeadHash, isDead);
        }
    }
}
