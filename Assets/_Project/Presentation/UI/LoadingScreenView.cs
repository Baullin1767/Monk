using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Monk.Presentation
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image barEmpty;
        [SerializeField] private Image barFill;
        [SerializeField] private TextMeshProUGUI loadingText;

        [SerializeField] private float loadDuration = 3f;
        [SerializeField] private string nextSceneName = "MainMenu";

        private void Start()
        {
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;

            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            barFill.fillAmount = 0f;

            StartCoroutine(LoadSequence());
        }

        private IEnumerator LoadSequence()
        {
            float elapsed = 0f;
            int dotCount = 0;
            float dotTimer = 0f;

            while (elapsed < loadDuration)
            {
                elapsed += Time.deltaTime;
                barFill.fillAmount = Mathf.Clamp01(elapsed / loadDuration);

                dotTimer += Time.deltaTime;
                if (dotTimer >= 0.4f)
                {
                    dotTimer = 0f;
                    dotCount = (dotCount + 1) % 4;
                    loadingText.text = "LOADING" + new string('.', dotCount);
                }

                yield return null;
            }

            barFill.fillAmount = 1f;
            loadingText.text = "LOADING...";

            yield return new WaitForSeconds(0.3f);

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
