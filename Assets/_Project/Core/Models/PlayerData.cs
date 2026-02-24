namespace Monk.Core
{
    public class PlayerData : IPlayerModel
    {
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsGrounded { get; set; }
        public PlayerState State { get; set; }

        public PlayerData(int maxHealth)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            State = PlayerState.Idle;
            IsGrounded = true;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || Health <= 0)
            {
                return;
            }

            Health -= amount;
            if (Health < 0)
            {
                Health = 0;
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || Health <= 0)
            {
                return;
            }

            Health += amount;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }
    }
}
