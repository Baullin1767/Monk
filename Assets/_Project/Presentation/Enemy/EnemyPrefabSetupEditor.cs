#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class EnemyPrefabSetupEditor
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Enemies/Tufei.prefab";
        private const string SpriteSheetPath = "Assets/_Project/Sprites/Enemy/tufei.png";
        private const string DefaultSpriteName = "tufei_0";
        private const string ControllerPath = "Assets/_Project/Animations/Enemy/Enemy.controller";

        [MenuItem("Tools/Monk/Setup/Create Enemy Prefab")]
        public static void CreateEnemyPrefab()
        {
            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/Enemies");

            var root = new GameObject("Tufei");

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(root.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.22f, 0f);

            var healthIcons = CreateHUDCanvas(root.transform);

            var spriteRenderer = root.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = LoadDefaultSprite();

            var rigidbody = root.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 3f;
            rigidbody.freezeRotation = true;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = root.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.32f, 0.43f);
            collider.offset = new Vector2(0f, -0.01f);

            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

            var enemyAnimator = root.AddComponent<EnemyAnimator>();
            var enemyHealth = root.AddComponent<EnemyHealth>();
            var enemyView = root.AddComponent<EnemyView>();
            var enemyController = root.AddComponent<EnemyController>();

            var enemyConfig = LoadEnemyConfig();
            var groundLayerIndex = LayerMask.NameToLayer("Ground");
            var groundLayerMask = groundLayerIndex >= 0 ? (1 << groundLayerIndex) : (1 << 0);

            var healthSo = new SerializedObject(enemyHealth);
            healthSo.FindProperty("config").objectReferenceValue = enemyConfig;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            var viewSo = new SerializedObject(enemyView);
            viewSo.FindProperty("spriteRenderer").objectReferenceValue = spriteRenderer;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            var controllerSo = new SerializedObject(enemyController);
            controllerSo.FindProperty("config").objectReferenceValue = enemyConfig;
            controllerSo.FindProperty("health").objectReferenceValue = enemyHealth;
            controllerSo.FindProperty("enemyAnimator").objectReferenceValue = enemyAnimator;
            controllerSo.FindProperty("view").objectReferenceValue = enemyView;
            controllerSo.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            controllerSo.FindProperty("groundLayer").intValue = groundLayerMask;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            var healthBarView = root.AddComponent<EnemyHealthBarView>();
            var healthBarViewSo = new SerializedObject(healthBarView);
            healthBarViewSo.FindProperty("enemyHealth").objectReferenceValue = enemyHealth;
            var iconsProp = healthBarViewSo.FindProperty("healthIcons");
            iconsProp.arraySize = healthIcons.Length;
            for (var i = 0; i < healthIcons.Length; i++)
            {
                iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = healthIcons[i];
            }
            healthBarViewSo.ApplyModifiedPropertiesWithoutUndo();

            root.tag = "Enemy";

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            if (prefab == null)
            {
                Debug.LogError("Failed to create enemy prefab.");
                return;
            }

            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Enemy prefab created at '{PrefabPath}'.");
        }

        private static Image[] CreateHUDCanvas(Transform parent)
        {
            var hudCanvas = new GameObject("HUDCanvas");
            hudCanvas.transform.SetParent(parent);
            hudCanvas.transform.localPosition = new Vector3(0f, 0.35f, 0f);

            var canvas = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            var canvasScaler = hudCanvas.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 100f;

            var rectTransform = hudCanvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0.5f, 0.1f);
            rectTransform.localScale = Vector3.one;

            var healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(hudCanvas.transform);
            healthBar.transform.localPosition = Vector3.zero;

            var healthBarRect = healthBar.AddComponent<RectTransform>();
            healthBarRect.sizeDelta = new Vector2(0.5f, 0.1f);
            healthBarRect.localScale = Vector3.one;

            var layout = healthBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 0.02f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var icons = new Image[3];
            for (var i = 0; i < 3; i++)
            {
                var icon = new GameObject($"HealthIcon_{i}");
                icon.transform.SetParent(healthBar.transform);
                icon.transform.localScale = Vector3.one;

                var image = icon.AddComponent<Image>();
                image.color = new Color(0.9f, 0.15f, 0.15f, 1f);
                icons[i] = image;

                var iconRect = icon.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(0.08f, 0.08f);
            }

            return icons;
        }

        private static Sprite LoadDefaultSprite()
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(SpriteSheetPath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sprite && sprite.name == DefaultSpriteName)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static ScriptableObject LoadEnemyConfig()
        {
            var guids = AssetDatabase.FindAssets("t:EnemyConfig");
            if (guids.Length == 0)
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = folderPath[..folderPath.LastIndexOf('/')];
            var folderName = folderPath[(folderPath.LastIndexOf('/') + 1)..];

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
