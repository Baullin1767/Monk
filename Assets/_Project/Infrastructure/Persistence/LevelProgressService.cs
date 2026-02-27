using Monk.Common;

namespace Monk.Infrastructure
{
    public class LevelProgressService
    {
        private static readonly string[] LevelOrder =
        {
            Constants.Scenes.Level1,
            Constants.Scenes.Level2,
            Constants.Scenes.Level3,
            Constants.Scenes.Level4
        };

        private readonly PlayerPrefsStorage storage;

        public LevelProgressService(PlayerPrefsStorage storage)
        {
            this.storage = storage;
        }

        public string GetNextSceneToPlay()
        {
            var completed = storage.GetString(Constants.PrefsKeys.HighestCompletedLevel, "");

            if (string.IsNullOrEmpty(completed))
                return LevelOrder[0];

            for (int i = 0; i < LevelOrder.Length; i++)
            {
                if (LevelOrder[i] == completed)
                {
                    int next = i + 1;
                    return next < LevelOrder.Length ? LevelOrder[next] : LevelOrder[0];
                }
            }

            return LevelOrder[0];
        }

        public void MarkLevelCompleted(string sceneName)
        {
            int completedIndex = -1;
            for (int i = 0; i < LevelOrder.Length; i++)
            {
                if (LevelOrder[i] == sceneName)
                {
                    completedIndex = i;
                    break;
                }
            }

            if (completedIndex < 0)
                return;

            var saved = storage.GetString(Constants.PrefsKeys.HighestCompletedLevel, "");
            int savedIndex = -1;
            for (int i = 0; i < LevelOrder.Length; i++)
            {
                if (LevelOrder[i] == saved)
                {
                    savedIndex = i;
                    break;
                }
            }

            if (completedIndex > savedIndex)
            {
                storage.SetString(Constants.PrefsKeys.HighestCompletedLevel, sceneName);
                storage.Save();
            }
        }
    }
}
