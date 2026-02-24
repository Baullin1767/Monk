using UnityEngine;
using UnityEngine.SceneManagement;
using Monk.Common;

namespace Monk.Presentation
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        public void Show()
        {
        }

        public void Hide()
        {
        }

        public void OnResumeClicked()
        {
        }

        public void OnMainMenuClicked()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            SceneManager.LoadScene(Constants.Scenes.MainMenu);
        }
    }
}
