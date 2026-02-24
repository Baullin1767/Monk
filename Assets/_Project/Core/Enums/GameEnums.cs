namespace Monk.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }

    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Attacking,
        Hurt,
        Dead
    }

    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    public enum FacingDirection
    {
        Left,
        Right
    }
}
