using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Monk.Common;

namespace Monk.Presentation
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnDestroy()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            SceneManager.LoadScene(Constants.Scenes.MainMenu);
        }
    }
}
