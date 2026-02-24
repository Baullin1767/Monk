using UnityEngine;
using UnityEngine.SceneManagement;
using Monk.Common;
using Monk.Infrastructure;

namespace Monk.Presentation
{
    public class MainMenuView : MonoBehaviour
    {
        private const string NoHealthMessage = "You dont have enoth Health go to Store to buy some health Kit";

        public void OnStartGameClicked()
        {
            var storage = new PlayerPrefsStorage();
            var maxHealth = storage.GetInt(PlayerHealth.MaxHealthKey, 3);
            var currentHealth = storage.GetInt(PlayerHealth.CurrentHealthKey, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0)
            {
                var windowManager = FindFirstObjectByType<UIWindowManager>();
                windowManager?.OpenStore();

                var storeWindow = FindFirstObjectByType<MainMenuStoreWindowView>();
                storeWindow?.ShowFeedback(NoHealthMessage);
                return;
            }

            Screen.orientation = ScreenOrientation.LandscapeLeft;
            SceneManager.LoadScene(Constants.Scenes.Level1);
        }
    }
}
