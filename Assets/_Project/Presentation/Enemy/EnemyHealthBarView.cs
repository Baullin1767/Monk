using UnityEngine;
using UnityEngine.UI;

namespace Monk.Presentation
{
    public class EnemyHealthBarView : MonoBehaviour
    {
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Image[] healthIcons;

        private void Awake()
        {
            if (enemyHealth == null)
            {
                enemyHealth = GetComponentInParent<EnemyHealth>();
            }
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged += UpdateHealthIcons;
            }
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged -= UpdateHealthIcons;
            }
        }

        private void UpdateHealthIcons(int currentHealth, int maxHealth)
        {
            if (healthIcons == null) return;

            for (var i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] == null) continue;
                healthIcons[i].enabled = i < currentHealth && i < maxHealth;
            }
        }
    }
}
