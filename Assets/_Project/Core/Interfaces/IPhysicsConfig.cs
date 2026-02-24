namespace Monk.Core
{
    public interface IPhysicsConfig
    {
        float MoveSpeed { get; }
        float JumpForce { get; }
        float GravityScale { get; }
        float FallMultiplier { get; }
        float GroundCheckRadius { get; }
    }
}
