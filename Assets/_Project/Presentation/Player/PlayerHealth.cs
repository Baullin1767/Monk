using System;
using System.Globalization;
using UnityEngine;
using Monk.Configs;
using Monk.Infrastructure;

namespace Monk.Presentation
{
    public class PlayerHealth : MonoBehaviour
    {
        public const string CurrentHealthKey = "monk.player.health.current";
        public const string MaxHealthKey = "monk.player.health.max";
        public const string LastHealthRegenUtcTicksKey = "monk.player.health.regen.lastUtcTicks";
        public const int HealthRegenAmountPerStep = 1;
        public const int HealthRegenMinutesPerStep = 10;

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

            SyncHealthWithRealtime(storage, MaxHealth);

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

        public static void SyncHealthWithRealtime(PlayerPrefsStorage storage, int fallbackMaxHealth = 3)
        {
            storage ??= new PlayerPrefsStorage();

            var max = Mathf.Max(1, storage.GetInt(MaxHealthKey, fallbackMaxHealth));
            var current = Mathf.Clamp(storage.GetInt(CurrentHealthKey, max), 0, max);
            var nowUtc = DateTime.UtcNow;
            var hasLastTick = long.TryParse(
                storage.GetString(LastHealthRegenUtcTicksKey, string.Empty),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var lastTickValue);

            var lastTickUtc = hasLastTick ? new DateTime(lastTickValue, DateTimeKind.Utc) : nowUtc;
            if (lastTickUtc > nowUtc)
            {
                lastTickUtc = nowUtc;
            }

            if (current < max)
            {
                var elapsed = nowUtc - lastTickUtc;
                var intervals = Mathf.Max(0, Mathf.FloorToInt((float)(elapsed.TotalMinutes / HealthRegenMinutesPerStep)));
                if (intervals > 0)
                {
                    current = Mathf.Min(max, current + intervals * HealthRegenAmountPerStep);
                    lastTickUtc = lastTickUtc.AddMinutes(intervals * HealthRegenMinutesPerStep);
                }
            }
            else
            {
                // Prevent stockpiling regen intervals while already full.
                lastTickUtc = nowUtc;
            }

            storage.SetInt(MaxHealthKey, max);
            storage.SetInt(CurrentHealthKey, current);
            storage.SetString(LastHealthRegenUtcTicksKey, lastTickUtc.Ticks.ToString(CultureInfo.InvariantCulture));
            storage.Save();
        }

        private void SaveCurrentHealth()
        {
            storage ??= new PlayerPrefsStorage();
            storage.SetInt(MaxHealthKey, MaxHealth);
            storage.SetInt(CurrentHealthKey, CurrentHealth);
            storage.SetString(LastHealthRegenUtcTicksKey, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
            storage.Save();
        }
    }
}
