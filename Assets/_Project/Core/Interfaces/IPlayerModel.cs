namespace Monk.Core
{
    public interface IPlayerModel
    {
        int Health { get; }
        int MaxHealth { get; }
        bool IsGrounded { get; set; }
        PlayerState State { get; set; }
        void TakeDamage(int amount);
        void Heal(int amount);
    }
}
