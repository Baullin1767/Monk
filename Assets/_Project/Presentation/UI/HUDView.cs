using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Monk.Presentation
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image[] healthIcons;

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Coins: {score}";
            }
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (healthIcons == null)
            {
                return;
            }

            for (var i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] == null)
                {
                    continue;
                }

                healthIcons[i].enabled = i < currentHealth && i < maxHealth;
            }
        }
    }
}
