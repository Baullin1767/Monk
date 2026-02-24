using System;
using UnityEngine;
using Monk.Configs;

namespace Monk.Presentation
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDied;

        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsDead { get; private set; }

        private void Awake()
        {
            MaxHealth = config != null ? config.MaxHealth : 3;
            CurrentHealth = MaxHealth;
            IsDead = false;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
            {
                IsDead = true;
                OnDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;

            var nextHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            if (nextHealth == CurrentHealth) return;

            CurrentHealth = nextHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
