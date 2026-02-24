namespace Monk.Core
{
    public interface IGameState
    {
        GameState CurrentState { get; set; }
        int Score { get; set; }
        int Lives { get; set; }
        void Reset();
    }
}
