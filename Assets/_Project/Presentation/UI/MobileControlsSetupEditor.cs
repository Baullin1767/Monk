#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class MobileControlsSetupEditor
    {
        private const string Level1ScenePath = "Assets/_Project/Scenes/Level 1.unity";
        private const string Level2ScenePath = "Assets/_Project/Scenes/Level 2.unity";
        private const string FixedJoystickPrefabPath = "Assets/Joystick Pack/Prefabs/Fixed Joystick.prefab";

        [MenuItem("Tools/Monk/Setup/Ensure Mobile Controls (Joystick + Attack + Jump)")]
        public static void EnsureMobileControls()
        {
            EnsureMobileControlsInScene(Level1ScenePath);
            EnsureMobileControlsInScene(Level2ScenePath);
            Debug.Log("Mobile controls configured for Level 1 and Level 2.");
        }

        private static void EnsureMobileControlsInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to open '{scenePath}'.");
                return;
            }

            var canvas = EnsureCanvas();
            var root = EnsureChild(canvas.transform, "MobileControlsRoot");

            var joystick = EnsureJoystick(root.transform);
            var jumpButton = EnsureActionButton(root.transform, "JumpButton", new Vector2(1f, 0f), new Vector2(-300f, 130f), "Jump");
            var attackButton = EnsureActionButton(root.transform, "AttackButton", new Vector2(1f, 0f), new Vector2(-130f, 130f), "Attack");

            var inputManagerGo = GameObject.Find("InputManager");
            if (inputManagerGo == null)
            {
                inputManagerGo = new GameObject("InputManager");
            }

            var inputManager = inputManagerGo.GetComponent<Monk.Input.InputManager>();
            if (inputManager == null)
            {
                inputManager = inputManagerGo.AddComponent<Monk.Input.InputManager>();
            }

            var desktopProvider = inputManagerGo.GetComponent<Monk.Input.DesktopInputProvider>();
            if (desktopProvider == null)
            {
                desktopProvider = inputManagerGo.AddComponent<Monk.Input.DesktopInputProvider>();
            }

            var mobileProvider = inputManagerGo.GetComponent<Monk.Input.MobileInputProvider>();
            if (mobileProvider == null)
            {
                mobileProvider = inputManagerGo.AddComponent<Monk.Input.MobileInputProvider>();
            }

            var inputSo = new SerializedObject(inputManager);
            inputSo.FindProperty("desktopInputProvider").objectReferenceValue = desktopProvider;
            inputSo.FindProperty("mobileInputProvider").objectReferenceValue = mobileProvider;
            inputSo.ApplyModifiedPropertiesWithoutUndo();

            var mobileSo = new SerializedObject(mobileProvider);
            mobileSo.FindProperty("movementJoystick").objectReferenceValue = joystick;
            mobileSo.FindProperty("jumpButton").objectReferenceValue = jumpButton;
            mobileSo.FindProperty("attackButton").objectReferenceValue = attackButton;
            mobileSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static Canvas EnsureCanvas()
        {
            var canvasGo = GameObject.Find("MobileControlsCanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("MobileControlsCanvas");
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasGo.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

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

        private static Joystick EnsureJoystick(Transform parent)
        {
            var existing = parent.Find("MoveJoystick");
            if (existing != null)
            {
                return existing.GetComponent<Joystick>();
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FixedJoystickPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Joystick prefab not found at '{FixedJoystickPrefabPath}'.");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate joystick prefab.");
                return null;
            }

            instance.name = "MoveJoystick";
            instance.transform.SetParent(parent, false);

            var rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(140f, 140f);
            }

            return instance.GetComponent<Joystick>();
        }

        private static Monk.Input.MobileActionButton EnsureActionButton(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPosition,
            string label)
        {
            var go = EnsureChild(parent, name);
            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = new Color(0.14f, 0.22f, 0.33f, 0.92f);

            var button = go.GetComponent<Button>();
            if (button == null)
            {
                button = go.AddComponent<Button>();
            }

            var actionButton = go.GetComponent<Monk.Input.MobileActionButton>();
            if (actionButton == null)
            {
                actionButton = go.AddComponent<Monk.Input.MobileActionButton>();
            }

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(150f, 90f);

            EnsureLabel(go.transform, label);
            return actionButton;
        }

        private static void EnsureLabel(Transform parent, string text)
        {
            var labelGo = EnsureChild(parent, "Label");
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = labelGo.AddComponent<TextMeshProUGUI>();
            }

            label.text = text;
            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;

            var rect = labelGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
