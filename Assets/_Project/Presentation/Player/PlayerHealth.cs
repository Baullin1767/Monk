using System;
using UnityEngine;
using Monk.Configs;
using Monk.Infrastructure;

namespace Monk.Presentation
{
    public class PlayerHealth : MonoBehaviour
    {
        public const string CurrentHealthKey = "monk.player.health.current";
        public const string MaxHealthKey = "monk.player.health.max";

        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invincibilityDuration = 1.5f;

        public event Action<int, int> OnHealthChanged;
        public event Action OnPlayerDied;

        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsDead { get; private set; }

        private float invincibilityTimer;
        private PlayerPrefsStorage storage;

        private void Awake()
        {
            storage = new PlayerPrefsStorage();
            MaxHealth = playerConfig != null ? playerConfig.MaxHealth : maxHealth;
            storage.SetInt(MaxHealthKey, MaxHealth);

            var savedHealth = storage.GetInt(CurrentHealthKey, MaxHealth);
            CurrentHealth = Mathf.Clamp(savedHealth, 0, MaxHealth);
            if (CurrentHealth <= 0)
            {
                CurrentHealth = MaxHealth;
            }
            IsDead = false;
            storage.SetInt(CurrentHealthKey, CurrentHealth);
            storage.Save();

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Update()
        {
            if (invincibilityTimer > 0f)
            {
                invincibilityTimer -= Time.deltaTime;
            }
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || invincibilityTimer > 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            invincibilityTimer = playerConfig != null ? playerConfig.InvincibilityDuration : invincibilityDuration;
            SaveCurrentHealth();
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
            {
                IsDead = true;
                OnPlayerDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            var nextHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            if (nextHealth == CurrentHealth)
            {
                return;
            }

            CurrentHealth = nextHealth;
            SaveCurrentHealth();
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Kill()
        {
            if (IsDead)
            {
                return;
            }

            CurrentHealth = 0;
            IsDead = true;
            SaveCurrentHealth();
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnPlayerDied?.Invoke();
        }

        private void SaveCurrentHealth()
        {
            storage ??= new PlayerPrefsStorage();
            storage.SetInt(MaxHealthKey, MaxHealth);
            storage.SetInt(CurrentHealthKey, CurrentHealth);
            storage.Save();
        }
    }
}
