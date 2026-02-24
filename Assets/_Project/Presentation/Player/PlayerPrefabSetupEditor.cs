#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Monk.Presentation.Editor
{
    public static class PlayerPrefabSetupEditor
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string SpriteSheetPath = "Assets/nemaycojohn/player and enemy/PL.png";
        private const string DefaultSpriteName = "PL_11";
        private const string ControllerPath = "Assets/_Project/Animations/Player/Player.controller";
        private const float DefaultMoveSpeed = 3f;

        [MenuItem("Tools/Monk/Setup/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/Player");

            var root = new GameObject("Player");
            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(root.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.45f, 0f);

            var spriteRenderer = root.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = LoadDefaultSprite();

            var rigidbody = root.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 3f;
            rigidbody.freezeRotation = true;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = root.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.28f, 0.4f);
            collider.offset = new Vector2(0f, -0.02f);

            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

            var movement = root.AddComponent<PlayerMovement>();
            var playerAnimator = root.AddComponent<PlayerAnimator>();
            var playerHealth = root.AddComponent<PlayerHealth>();
            var playerView = root.AddComponent<PlayerView>();
            var attackHitboxObj = new GameObject("AttackHitbox");
            attackHitboxObj.transform.SetParent(root.transform);
            attackHitboxObj.transform.localPosition = new Vector3(0.2f, 0f, 0f);
            var hitboxCollider = attackHitboxObj.AddComponent<BoxCollider2D>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector2(0.25f, 0.3f);
            hitboxCollider.enabled = false;
            var attackHitbox = attackHitboxObj.AddComponent<PlayerAttackHitbox>();

            var playerController = root.AddComponent<PlayerController>();

            var playerConfig = LoadPlayerConfig();
            var groundLayerIndex = LayerMask.NameToLayer("Ground");
            var groundLayerMask = groundLayerIndex >= 0 ? (1 << groundLayerIndex) : (1 << 0);

            var movementSo = new SerializedObject(movement);
            movementSo.FindProperty("playerConfig").objectReferenceValue = playerConfig;
            movementSo.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            movementSo.FindProperty("groundLayer").intValue = groundLayerMask;
            movementSo.FindProperty("moveSpeed").floatValue = DefaultMoveSpeed;
            movementSo.ApplyModifiedPropertiesWithoutUndo();

            var healthSo = new SerializedObject(playerHealth);
            healthSo.FindProperty("playerConfig").objectReferenceValue = playerConfig;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            var viewSo = new SerializedObject(playerView);
            viewSo.FindProperty("spriteRenderer").objectReferenceValue = spriteRenderer;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            var controllerSo = new SerializedObject(playerController);
            controllerSo.FindProperty("movement").objectReferenceValue = movement;
            controllerSo.FindProperty("health").objectReferenceValue = playerHealth;
            controllerSo.FindProperty("playerAnimator").objectReferenceValue = playerAnimator;
            controllerSo.FindProperty("view").objectReferenceValue = playerView;
            controllerSo.FindProperty("attackHitbox").objectReferenceValue = attackHitbox;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            root.tag = "Player";

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            if (prefab == null)
            {
                Debug.LogError("Failed to create player prefab.");
                return;
            }

            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Player prefab created at '{PrefabPath}'.");
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

        private static ScriptableObject LoadPlayerConfig()
        {
            var guids = AssetDatabase.FindAssets("t:PlayerConfig");
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
