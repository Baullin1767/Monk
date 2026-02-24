namespace Monk.Core
{
    public class EnemyData : IEnemyModel
    {
        public int Health { get; private set; }
        public int ContactDamage { get; private set; }
        public EnemyState State { get; set; }

        public EnemyData(int health, int contactDamage)
        {
            Health = health;
            ContactDamage = contactDamage;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || Health <= 0) return;
            Health -= amount;
            if (Health < 0) Health = 0;
        }
    }
}
