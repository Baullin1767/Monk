using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Monk.Presentation
{
    public class StoreWindowView : MonoBehaviour
    {
        [Serializable]
        private class HealthKitOffer
        {
            [SerializeField] private string displayName = "Health Kit";
            [SerializeField] private int healAmount = 1;
            [SerializeField] private int cost = 5;
            [SerializeField] private Button buyButton;
            [SerializeField] private TextMeshProUGUI labelText;

            public string DisplayName => displayName;
            public int HealAmount => healAmount;
            public int Cost => cost;
            public Button BuyButton => buyButton;
            public TextMeshProUGUI LabelText => labelText;
        }

        [Header("Window")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private bool hideOnStart = true;

        [Header("Bindings")]
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private HealthKitOffer[] offers;
        private UnityAction[] buyHandlers;

        private void Awake()
        {
            ResolveReferences();
            BindButtons();
            RefreshCoins();
            RefreshOfferLabels();

            if (hideOnStart && windowRoot != null)
            {
                windowRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (coinManager != null)
            {
                coinManager.OnCoinsChanged += HandleCoinsChanged;
            }
        }

        private void Start()
        {
            ResolveReferences();
            RefreshCoins();
            RefreshOfferLabels();
        }

        private void OnDisable()
        {
            if (coinManager != null)
            {
                coinManager.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void OnDestroy()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OpenWindow);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseWindow);
            }

            if (offers == null || buyHandlers == null)
            {
                return;
            }

            for (var i = 0; i < offers.Length && i < buyHandlers.Length; i++)
            {
                var buyButton = offers[i]?.BuyButton;
                if (buyButton == null || buyHandlers[i] == null)
                {
                    continue;
                }

                buyButton.onClick.RemoveListener(buyHandlers[i]);
            }
        }

        public void OpenWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(true);
            }

            ShowFeedback(string.Empty);
            RefreshCoins();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }
        }

        private void ResolveReferences()
        {
            if (coinManager == null)
            {
                coinManager = FindFirstObjectByType<CoinManager>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (windowRoot == null)
            {
                windowRoot = gameObject;
            }
        }

        private void BindButtons()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OpenWindow);
                openButton.onClick.AddListener(OpenWindow);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseWindow);
                closeButton.onClick.AddListener(CloseWindow);
            }

            if (offers == null)
            {
                return;
            }

            buyHandlers = new UnityAction[offers.Length];
            for (var i = 0; i < offers.Length; i++)
            {
                var offerIndex = i;
                var buyButton = offers[i]?.BuyButton;
                if (buyButton == null)
                {
                    continue;
                }

                buyHandlers[i] = () => BuyOffer(offerIndex);
                buyButton.onClick.AddListener(buyHandlers[i]);
            }
        }

        private void RefreshCoins()
        {
            if (coinsText == null || coinManager == null)
            {
                return;
            }

            coinsText.text = $"Coins: {coinManager.TotalCoins}";
        }

        private void RefreshOfferLabels()
        {
            if (offers == null)
            {
                return;
            }

            foreach (var offer in offers)
            {
                if (offer == null || offer.LabelText == null)
                {
                    continue;
                }

                offer.LabelText.text = $"{offer.DisplayName} (+{offer.HealAmount} HP) - {offer.Cost} coins";
            }
        }

        private void BuyOffer(int index)
        {
            if (offers == null || index < 0 || index >= offers.Length)
            {
                return;
            }

            var offer = offers[index];
            if (offer == null)
            {
                return;
            }

            ResolveReferences();

            if (coinManager == null)
            {
                ShowFeedback("Coin manager not found.");
                return;
            }

            if (playerHealth == null)
            {
                ShowFeedback("Player health not found.");
                return;
            }

            if (playerHealth.IsDead)
            {
                ShowFeedback("Cannot heal after death.");
                return;
            }

            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                ShowFeedback("Health is already full.");
                return;
            }

            if (!coinManager.CanAfford(offer.Cost))
            {
                ShowFeedback("Not enough coins.");
                return;
            }

            var beforeHealth = playerHealth.CurrentHealth;
            if (!coinManager.SpendCoins(offer.Cost))
            {
                ShowFeedback("Purchase failed.");
                return;
            }

            playerHealth.Heal(offer.HealAmount);
            var restored = playerHealth.CurrentHealth - beforeHealth;
            ShowFeedback(restored > 0
                ? $"Bought {offer.DisplayName}. Restored {restored} HP."
                : $"{offer.DisplayName} purchased.");
        }

        private void HandleCoinsChanged(int _)
        {
            RefreshCoins();
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }
    }
}
