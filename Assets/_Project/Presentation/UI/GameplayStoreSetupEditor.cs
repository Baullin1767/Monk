#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class GameplayStoreSetupEditor
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Level 1.unity";

        [MenuItem("Tools/Monk/Setup/Ensure Gameplay Store Window")]
        public static void EnsureGameplayStoreWindow()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to open '{GameScenePath}'.");
                return;
            }

            var canvas = EnsureStoreCanvas();
            var storeRoot = EnsureChild(canvas.transform, "StoreUIRoot");
            var storeView = storeRoot.GetComponent<StoreWindowView>();
            if (storeView == null)
            {
                storeView = storeRoot.AddComponent<StoreWindowView>();
            }

            var openButton = EnsureButton(storeRoot.transform, "OpenStoreButton", new Vector2(1f, 1f), new Vector2(-110f, -36f), "Store");

            var panel = EnsurePanel(storeRoot.transform, "StoreWindowPanel", new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(680f, 430f));
            panel.SetActive(false);

            EnsureText(panel.transform, "Title", "Store", 42f, new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(560f, 60f), TextAlignmentOptions.Center);
            var coinsText = EnsureText(panel.transform, "CoinsText", "Coins: 0", 30f, new Vector2(0f, 1f), new Vector2(32f, -74f), new Vector2(280f, 48f), TextAlignmentOptions.Left);
            var feedbackText = EnsureText(panel.transform, "FeedbackText", string.Empty, 24f, new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(620f, 48f), TextAlignmentOptions.Center);
            var closeButton = EnsureButton(panel.transform, "CloseButton", new Vector2(1f, 1f), new Vector2(-34f, -28f), "X");

            var offer1 = EnsureOffer(panel.transform, "Offer_Small", "Small Kit", new Vector2(0f, -130f));
            var offer2 = EnsureOffer(panel.transform, "Offer_Medium", "Medium Kit", new Vector2(0f, -200f));
            var offer3 = EnsureOffer(panel.transform, "Offer_Large", "Large Kit", new Vector2(0f, -270f));

            var presenter = storeRoot.GetComponent<PlayerHudPresenter>();
            if (presenter == null)
            {
                storeRoot.AddComponent<PlayerHudPresenter>();
            }

            ApplyStoreViewBindings(storeView, panel, openButton, closeButton, coinsText, feedbackText, offer1, offer2, offer3);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Selection.activeGameObject = storeRoot;

            Debug.Log("Gameplay Store Window configured in Level 1.");
        }

        private static Canvas EnsureStoreCanvas()
        {
            var canvasGo = GameObject.Find("StoreCanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("StoreCanvas");
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasGo.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (canvasGo.GetComponent<CanvasScaler>() == null)
            {
                canvasGo.AddComponent<CanvasScaler>();
            }

            if (canvasGo.GetComponent<GraphicRaycaster>() == null)
            {
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static GameObject EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static GameObject EnsurePanel(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            var go = EnsureChild(parent, name);
            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = new Color(0f, 0f, 0f, 0.85f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return go;
        }

        private static Button EnsureButton(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos, string text)
        {
            var go = EnsureChild(parent, name);
            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = new Color(0.14f, 0.22f, 0.33f, 0.96f);

            var button = go.GetComponent<Button>();
            if (button == null)
            {
                button = go.AddComponent<Button>();
            }

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(140f, 54f);

            EnsureText(go.transform, "Label", text, 26f, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(132f, 50f), TextAlignmentOptions.Center);
            return button;
        }

        private static OfferUi EnsureOffer(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var row = EnsureChild(parent, name);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = anchoredPos;
            rowRect.sizeDelta = new Vector2(620f, 58f);

            var buyButton = EnsureButton(row.transform, "BuyButton", new Vector2(1f, 0.5f), new Vector2(-74f, 0f), "Buy");
            var labelText = EnsureText(row.transform, "OfferText", label, 24f, new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(450f, 54f), TextAlignmentOptions.Left);

            return new OfferUi(buyButton, labelText);
        }

        private static TextMeshProUGUI EnsureText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            TextAlignmentOptions alignment)
        {
            var go = EnsureChild(parent, name);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                tmp = go.AddComponent<TextMeshProUGUI>();
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(anchor.x, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return tmp;
        }

        private static void ApplyStoreViewBindings(
            StoreWindowView storeView,
            GameObject panel,
            Button openButton,
            Button closeButton,
            TextMeshProUGUI coinsText,
            TextMeshProUGUI feedbackText,
            OfferUi offer1,
            OfferUi offer2,
            OfferUi offer3)
        {
            var so = new SerializedObject(storeView);
            so.FindProperty("windowRoot").objectReferenceValue = panel;
            so.FindProperty("openButton").objectReferenceValue = openButton;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.FindProperty("hideOnStart").boolValue = true;
            so.FindProperty("coinsText").objectReferenceValue = coinsText;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackText;

            var offersProperty = so.FindProperty("offers");
            offersProperty.arraySize = 3;

            SetOffer(offersProperty.GetArrayElementAtIndex(0), "Small Kit", 1, 5, offer1);
            SetOffer(offersProperty.GetArrayElementAtIndex(1), "Medium Kit", 2, 9, offer2);
            SetOffer(offersProperty.GetArrayElementAtIndex(2), "Large Kit", 3, 12, offer3);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetOffer(SerializedProperty offer, string displayName, int healAmount, int cost, OfferUi ui)
        {
            offer.FindPropertyRelative("displayName").stringValue = displayName;
            offer.FindPropertyRelative("healAmount").intValue = healAmount;
            offer.FindPropertyRelative("cost").intValue = cost;
            offer.FindPropertyRelative("buyButton").objectReferenceValue = ui.Button;
            offer.FindPropertyRelative("labelText").objectReferenceValue = ui.Text;
        }

        private struct OfferUi
        {
            public Button Button;
            public TextMeshProUGUI Text;

            public OfferUi(Button button, TextMeshProUGUI text)
            {
                Button = button;
                Text = text;
            }
        }
    }
}
#endif
