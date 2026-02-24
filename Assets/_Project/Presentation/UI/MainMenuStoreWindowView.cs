using System.Collections.Generic;
using Monk.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monk.Presentation
{
    public class MainMenuStoreWindowView : MonoBehaviour
    {
        [System.Serializable]
        private struct HealthKit
        {
            public string title;
            public int healAmount;
            public int coinCost;
        }

        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private HealthKit[] kits =
        {
            new HealthKit { title = "Small Kit", healAmount = 1, coinCost = 5 },
            new HealthKit { title = "Medium Kit", healAmount = 2, coinCost = 9 },
            new HealthKit { title = "Large Kit", healAmount = 3, coinCost = 12 }
        };
        [SerializeField] private Sprite buyBG;

        private readonly List<Button> buyButtons = new();
        private PlayerPrefsStorage storage;

        private void Awake()
        {
            storage = new PlayerPrefsStorage();
            EnsureUi();
            BuildRows();
            RefreshUi();
        }

        private void OnEnable()
        {
            RefreshUi();
        }

        private void EnsureUi()
        {
            if (rowsRoot == null)
            {
                var existing = transform.Find("StoreRows");
                if (existing != null)
                {
                    rowsRoot = existing;
                }
                else
                {
                    var rowsGo = new GameObject("StoreRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    rowsGo.transform.SetParent(transform, false);
                    rowsRoot = rowsGo.transform;

                    var rowsRect = rowsGo.GetComponent<RectTransform>();
                    rowsRect.anchorMin = new Vector2(0.5f, 1f);
                    rowsRect.anchorMax = new Vector2(0.5f, 1f);
                    rowsRect.pivot = new Vector2(0.5f, 1f);
                    rowsRect.anchoredPosition = new Vector2(0f, -340f);
                    rowsRect.sizeDelta = new Vector2(900f, 560f);

                    var layout = rowsGo.GetComponent<VerticalLayoutGroup>();
                    layout.padding = new RectOffset(0, 0, 0, 0);
                    layout.spacing = 18f;
                    layout.childControlHeight = true;
                    layout.childControlWidth = true;
                    layout.childForceExpandHeight = false;
                    layout.childForceExpandWidth = true;
                }
            }

            if (coinsText == null)
            {
                coinsText = CreateOrFindText("CoinsText", new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(700f, 60f), 38f, TextAlignmentOptions.Center);
            }

            if (healthText == null)
            {
                healthText = CreateOrFindText("HealthText", new Vector2(0.5f, 1f), new Vector2(0f, -305f), new Vector2(700f, 52f), 30f, TextAlignmentOptions.Center);
            }

            if (feedbackText == null)
            {
                feedbackText = CreateOrFindText("FeedbackText", new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(900f, 50f), 28f, TextAlignmentOptions.Center);
            }
        }

        private TextMeshProUGUI CreateOrFindText(string name, Vector2 anchor, Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            var existing = transform.Find(name);
            var go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            if (existing == null)
            {
                go.transform.SetParent(transform, false);
            }

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private void BuildRows()
        {
            for (var i = 0; i < rowsRoot.childCount; i++)
            {
                Destroy(rowsRoot.GetChild(i).gameObject);
            }

            buyButtons.Clear();
            for (var i = 0; i < kits.Length; i++)
            {
                var kitIndex = i;
                var row = new GameObject($"Row_{i + 1}", typeof(RectTransform), typeof(LayoutElement));
                row.transform.SetParent(rowsRoot, false);

                var rowRect = row.GetComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(0f, 96f);

                var layoutElement = row.GetComponent<LayoutElement>();
                layoutElement.minHeight = 96f;
                layoutElement.preferredHeight = 96f;

                var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                label.transform.SetParent(row.transform, false);
                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 0.5f);
                labelRect.anchorMax = new Vector2(1f, 0.5f);
                labelRect.pivot = new Vector2(0f, 0.5f);
                labelRect.offsetMin = new Vector2(20f, -42f);
                labelRect.offsetMax = new Vector2(-260f, 42f);

                var labelText = label.GetComponent<TextMeshProUGUI>();
                labelText.fontSize = 30f;
                labelText.alignment = TextAlignmentOptions.Left;
                labelText.color = Color.white;
                labelText.text = $"{kits[i].title}: +{kits[i].healAmount} HP ({kits[i].coinCost} coins)";

                var buttonGo = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonGo.transform.SetParent(row.transform, false);
                var buttonRect = buttonGo.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(1f, 0.5f);
                buttonRect.anchorMax = new Vector2(1f, 0.5f);
                buttonRect.pivot = new Vector2(1f, 0.5f);
                buttonRect.anchoredPosition = new Vector2(-20f, 0f);
                buttonRect.sizeDelta = new Vector2(220f, 74f);

                var buttonImage = buttonGo.GetComponent<Image>();
                buttonImage.sprite = buyBG;

                var button = buttonGo.GetComponent<Button>();
                button.onClick.AddListener(() => BuyKit(kitIndex));

                var buttonLabel = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                buttonLabel.transform.SetParent(buttonGo.transform, false);
                var buttonLabelRect = buttonLabel.GetComponent<RectTransform>();
                buttonLabelRect.anchorMin = Vector2.zero;
                buttonLabelRect.anchorMax = Vector2.one;
                buttonLabelRect.offsetMin = Vector2.zero;
                buttonLabelRect.offsetMax = Vector2.zero;

                var buttonLabelText = buttonLabel.GetComponent<TextMeshProUGUI>();
                buttonLabelText.fontSize = 30f;
                buttonLabelText.alignment = TextAlignmentOptions.Center;
                buttonLabelText.color = new Color(0.25f, 0.15f, 0.05f, 1f);
                buttonLabelText.text = "BUY";

                buyButtons.Add(button);
            }
        }

        private void BuyKit(int index)
        {
            if (index < 0 || index >= kits.Length)
            {
                return;
            }

            var kit = kits[index];
            var coins = storage.GetInt(CoinManager.CoinTotalKey, 0);
            var maxHealth = storage.GetInt(PlayerHealth.MaxHealthKey, 3);
            var currentHealth = storage.GetInt(PlayerHealth.CurrentHealthKey, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth >= maxHealth)
            {
                feedbackText.text = "Health is already full.";
                return;
            }

            if (coins < kit.coinCost)
            {
                feedbackText.text = "Not enough coins.";
                return;
            }

            coins -= kit.coinCost;
            currentHealth = Mathf.Min(currentHealth + kit.healAmount, maxHealth);

            storage.SetInt(CoinManager.CoinTotalKey, coins);
            storage.SetInt(PlayerHealth.CurrentHealthKey, currentHealth);
            storage.Save();

            feedbackText.text = $"Bought {kit.title}. Health restored.";
            RefreshUi();
        }

        private void RefreshUi()
        {
            var coins = storage.GetInt(CoinManager.CoinTotalKey, 0);
            var maxHealth = storage.GetInt(PlayerHealth.MaxHealthKey, 3);
            var currentHealth = storage.GetInt(PlayerHealth.CurrentHealthKey, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            coinsText.text = $"Coins: {coins}";
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
            if (string.IsNullOrWhiteSpace(feedbackText.text))
            {
                feedbackText.text = "Buy health kits to restore saved health.";
            }
        }

        public void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }
    }
}
