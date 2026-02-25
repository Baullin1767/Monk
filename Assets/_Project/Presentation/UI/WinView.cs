using System.Collections;
using Monk.Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Monk.Presentation
{
    public class WinView : MonoBehaviour
    {
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TextMeshProUGUI winScoreText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private float showDelay = 2f;

        [SerializeField] private CoinManager coinManager;

        private EnemyHealth[] allEnemies;
        private int enemiesAlive;
        private Coroutine showCoroutine;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (coinManager == null)
            {
                coinManager = FindFirstObjectByType<CoinManager>();
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            Hide();
        }

        private void OnEnable()
        {
            allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            enemiesAlive = allEnemies.Length;

            for (int i = 0; i < allEnemies.Length; i++)
            {
                allEnemies[i].OnDied -= HandleEnemyDied;
                allEnemies[i].OnDied += HandleEnemyDied;
            }
        }

        private void OnDisable()
        {
            if (allEnemies != null)
            {
                for (int i = 0; i < allEnemies.Length; i++)
                {
                    if (allEnemies[i] != null)
                    {
                        allEnemies[i].OnDied -= HandleEnemyDied;
                    }
                }
            }

            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }

        private void HandleEnemyDied()
        {
            enemiesAlive--;

            if (enemiesAlive > 0) return;

            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
            }

            showCoroutine = StartCoroutine(ShowAfterDelay());
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSeconds(showDelay);
            showCoroutine = null;

            var finalScore = coinManager != null ? coinManager.TotalCoins : 0;
            Show(finalScore);
        }

        public void Show(int finalScore)
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }

            if (winScoreText != null)
            {
                winScoreText.text = $"Score: {finalScore}";
            }

            Time.timeScale = 0f;
        }

        public void Hide()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            Time.timeScale = 1f;
        }

        public void OnNextLevelClicked()
        {
            Time.timeScale = 1f;
            var currentIndex = SceneManager.GetActiveScene().buildIndex;
            var nextIndex = currentIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Screen.orientation = ScreenOrientation.Portrait;
                SceneManager.LoadScene(Constants.Scenes.MainMenu);
            }
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            Screen.orientation = ScreenOrientation.Portrait;
            SceneManager.LoadScene(Constants.Scenes.MainMenu);
        }
    }
}
