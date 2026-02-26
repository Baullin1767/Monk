using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Monk.Common;
using System.Collections;

namespace Monk.Presentation
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private float showDelay = 2f;

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private CoinManager coinManager;

        private Coroutine showCoroutine;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (coinManager == null)
            {
                coinManager = FindFirstObjectByType<CoinManager>();
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
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }

            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }

        private void HandlePlayerDied()
        {
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
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score: {finalScore}";
            }

            Time.timeScale = 0f;
        }

        public void Hide()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            Time.timeScale = 1f;
        }

        public void OnRetryClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
