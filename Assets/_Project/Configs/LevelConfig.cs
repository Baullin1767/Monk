using UnityEngine;
using Monk.Core;

namespace Monk.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Monk/Configs/Level Config")]
    public class LevelConfig : ScriptableObject, ILevelData
    {
        [SerializeField] private string levelId;
        [SerializeField] private string sceneName;
        [SerializeField] private bool isUnlocked;

        public string LevelId => levelId;
        public string SceneName => sceneName;
        public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }
    }
}
