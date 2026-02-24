namespace Monk.Core
{
    public interface ILevelData
    {
        string LevelId { get; }
        string SceneName { get; }
        bool IsUnlocked { get; set; }
    }
}
