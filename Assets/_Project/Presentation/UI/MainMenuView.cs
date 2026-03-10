using UnityEngine;
using UnityEngine.SceneManagement;
using Monk.Infrastructure;

namespace Monk.Presentation
{
    public class MainMenuView : MonoBehaviour
    {
        private void Awake()
        {
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        public void OnStartGameClicked()
        {
            var storage = new PlayerPrefsStorage();
            PlayerHealth.SyncHealthWithRealtime(storage, 3);
            var maxHealth = Mathf.Max(1, storage.GetInt(PlayerHealth.MaxHealthKey, 3));
            var currentHealth = storage.GetInt(PlayerHealth.CurrentHealthKey, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 1;
                storage.SetInt(PlayerHealth.MaxHealthKey, maxHealth);
                storage.SetInt(PlayerHealth.CurrentHealthKey, currentHealth);
                storage.Save();
            }

            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            var levelProgress = new LevelProgressService(storage);
            SceneManager.LoadScene(levelProgress.GetNextSceneToPlay());
        }
    }
}
