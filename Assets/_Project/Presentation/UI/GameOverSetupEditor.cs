#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class GameOverSetupEditor
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Level 1.unity";

        [MenuItem("Tools/Monk/Setup/Ensure Game Over Window")]
        public static void EnsureGameOverWindow()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to open '{GameScenePath}'.");
                return;
            }

            var canvas = EnsureGameOverCanvas();
            var root = EnsureChild(canvas.transform, "GameOverUIRoot");
            var gameOverView = root.GetComponent<GameOverView>();
            if (gameOverView == null)
            {
                gameOverView = root.AddComponent<GameOverView>();
            }

            var panel = EnsurePanel(root.transform, "GameOverPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 350f));
            panel.SetActive(false);

            EnsureText(panel.transform, "TitleText", "YOU DIED", 42f, new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(420f, 60f), TextAlignmentOptions.Center);
            var scoreText = EnsureText(panel.transform, "ScoreText", "Score: 0", 30f, new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(420f, 48f), TextAlignmentOptions.Center);

            var retryButton = EnsureButton(panel.transform, "RestartButton", new Vector2(0.5f, 0f), new Vector2(-95f, 52f), "Restart");
            var mainMenuButton = EnsureButton(panel.transform, "MainMenuButton", new Vector2(0.5f, 0f), new Vector2(95f, 52f), "Main Menu");

            ApplyGameOverBindings(gameOverView, panel, scoreText, retryButton, mainMenuButton);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            Debug.Log("Game Over Window configured in Level 1.");
        }

        private static Canvas EnsureGameOverCanvas()
        {
            var canvasGo = GameObject.Find("GameOverCanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("GameOverCanvas");
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

            image.color = new Color(0f, 0f, 0f, 0.82f);

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
            rect.sizeDelta = new Vector2(170f, 56f);

            EnsureText(go.transform, "Label", text, 28f, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 50f), TextAlignmentOptions.Center);
            return button;
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
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return tmp;
        }

        private static void ApplyGameOverBindings(
            GameOverView gameOverView,
            GameObject panel,
            TextMeshProUGUI scoreText,
            Button retryButton,
            Button mainMenuButton)
        {
            var so = new SerializedObject(gameOverView);
            so.FindProperty("gameOverPanel").objectReferenceValue = panel;
            so.FindProperty("finalScoreText").objectReferenceValue = scoreText;
            so.FindProperty("retryButton").objectReferenceValue = retryButton;
            so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
            so.FindProperty("showDelay").floatValue = 2f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
