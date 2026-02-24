using UnityEngine;

namespace Monk.Presentation
{
    public class PlayerHudPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private HUDView hudView;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void ResolveReferences()
        {
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (hudView == null)
            {
                hudView = FindFirstObjectByType<HUDView>();
            }
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            hudView?.UpdateHealth(currentHealth, maxHealth);
        }
    }
}
