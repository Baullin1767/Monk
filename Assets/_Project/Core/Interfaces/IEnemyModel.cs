namespace Monk.Core
{
    public interface IEnemyModel
    {
        int Health { get; }
        int ContactDamage { get; }
        EnemyState State { get; set; }
        void TakeDamage(int amount);
    }
}
