namespace Monk.Core
{
    public interface IInputProvider
    {
        float HorizontalAxis { get; }
        bool JumpPressed { get; }
        bool AttackPressed { get; }
    }
}
