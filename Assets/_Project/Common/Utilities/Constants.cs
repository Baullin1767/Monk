namespace Monk.Common
{
    public static class Constants
    {
        public static class Layers
        {
            public const string Ground = "Ground";
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string Platform = "Platform";
        }

        public static class Tags
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string Checkpoint = "Checkpoint";
        }

        public static class Scenes
        {
            public const string MainMenu = "MainMenu";
            public const string Level1 = "Level 1";
            public const string Level2 = "Level 2";
            public const string Level3 = "Level 3";
            public const string Level4 = "Level 4";
        }

        public static class PrefsKeys
        {
            public const string MusicVolume = "MusicVolume";
            public const string SFXVolume = "SFXVolume";
            public const string HighScore = "HighScore";
            public const string HighestCompletedLevel = "monk.progress.highestLevelCompleted";
        }
    }
}
