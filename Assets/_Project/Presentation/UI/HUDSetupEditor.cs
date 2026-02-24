#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class HUDSetupEditor
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Level 1.unity";

        [MenuItem("Tools/Monk/Setup/Ensure HUD")]
        public static void EnsureHud()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to open '{GameScenePath}'.");
                return;
            }

            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("Player not found in scene. Run 'Setup Level 1 Player' first.");
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(player))
            {
                PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            var canvas = EnsureHudCanvas(player.transform);
            var hud = EnsureChild(canvas.transform, "HUD");

            var hudView = hud.GetComponent<HUDView>();
            if (hudView == null)
            {
                hudView = hud.AddComponent<HUDView>();
            }

            var presenter = hud.GetComponent<PlayerHudPresenter>();
            if (presenter == null)
            {
                presenter = hud.AddComponent<PlayerHudPresenter>();
            }

            var coinText = EnsureText(
                hud.transform, "CoinText", "Coins: 0", 0.4f,
                new Vector2(0f, 1f), new Vector2(0.1f, -0.1f),
                new Vector2(2f, 0.5f), TextAlignmentOptions.Left);

            var healthIcons = new Image[3];
            for (var i = 0; i < 3; i++)
            {
                var iconGo = EnsureChild(hud.transform, $"HealthIcon_{i}");
                var image = iconGo.GetComponent<Image>();
                if (image == null)
                {
                    image = iconGo.AddComponent<Image>();
                }

                image.color = Color.red;

                var rect = iconGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(0.1f + i * 0.45f, -0.65f);
                rect.sizeDelta = new Vector2(0.35f, 0.35f);

                healthIcons[i] = image;
            }

            var so = new SerializedObject(hudView);
            so.FindProperty("scoreText").objectReferenceValue = coinText;
            var healthProp = so.FindProperty("healthIcons");
            healthProp.arraySize = 3;
            for (var i = 0; i < 3; i++)
            {
                healthProp.GetArrayElementAtIndex(i).objectReferenceValue = healthIcons[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            var presenterSo = new SerializedObject(presenter);
            presenterSo.FindProperty("hudView").objectReferenceValue = hudView;
            presenterSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Selection.activeGameObject = hud;

            Debug.Log("HUD (World Space) configured under Player in Level 1.");
        }

        private static Canvas EnsureHudCanvas(Transform player)
        {
            var existing = player.Find("PlayerHUDCanvas");
            var canvasGo = existing != null ? existing.gameObject : null;
            if (canvasGo == null)
            {
                canvasGo = new GameObject("PlayerHUDCanvas");
            }

            canvasGo.transform.SetParent(player, false);
            canvasGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            canvasGo.transform.localRotation = Quaternion.identity;
            canvasGo.transform.localScale = Vector3.one;

            var canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasGo.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.WorldSpace;

            var rectTransform = canvasGo.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(3f, 1.2f);
            rectTransform.pivot = new Vector2(0.5f, 0f);

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
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return tmp;
        }
    }
}
#endif
