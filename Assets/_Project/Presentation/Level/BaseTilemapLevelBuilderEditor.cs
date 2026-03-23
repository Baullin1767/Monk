#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class BaseTilemapLevelBuilderEditor
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Level 1.unity";
        private const string LevelRootName = "BaseLevel_Tilemap";
        private const string GroundTilePath = "Assets/2dAssetPack/Generated/Tiles/Tile_12.asset";
        private const string EdgeTilePath = "Assets/2dAssetPack/Generated/Tiles/Tile_02.asset";
        private const string AccentTilePath = "Assets/2dAssetPack/Generated/Tiles/Tile_10.asset";
        private const string GroundSecondRowTilePath = "Assets/2dAssetPack/Generated/Tiles/Tile_13.asset";
        private const string GroundThirdRowTilePath = "Assets/2dAssetPack/Generated/Tiles/Tile_14.asset";
        private const string CoinPrefabPath = "Assets/_Project/Prefabs/Collectibles/GoldCoin.prefab";

        [MenuItem("Tools/Monk/Setup/Build Base Level From 2dAssetPack Tiles")]
        public static void BuildBaseLevel()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to open '{GameScenePath}'.");
                return;
            }

            var root = GameObject.Find(LevelRootName);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            var legacySquare = GameObject.Find("Square");
            if (legacySquare != null)
            {
                Object.DestroyImmediate(legacySquare);
            }

            root = new GameObject(LevelRootName);

            var grid = root.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSize = Vector3.one;

            BuildParallaxBackground(root.transform);

            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(root.transform);
            groundGo.layer = ResolveGroundLayer();

            var tilemap = groundGo.AddComponent<Tilemap>();
            var tilemapRenderer = groundGo.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingOrder = -5;

            groundGo.AddComponent<TilemapCollider2D>();
            var rigidbody = groundGo.AddComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                rigidbody.bodyType = RigidbodyType2D.Static;
            }

            var groundTile = LoadTile(GroundTilePath);
            var edgeTile = LoadTile(EdgeTilePath) ?? groundTile;
            var accentTile = LoadTile(AccentTilePath) ?? groundTile;
            var groundSecondRowTile = LoadTile(GroundSecondRowTilePath) ?? groundTile;
            var groundThirdRowTile = LoadTile(GroundThirdRowTilePath) ?? groundTile;
            if (groundTile == null)
            {
                Debug.LogError("No valid 2dAssetPack tile asset found. Aborting level build.");
                return;
            }

            FillRect(tilemap, -48, -8, 96, 1, groundTile);
            FillRect(tilemap, -48, -7, 96, 1, groundSecondRowTile);
            FillRect(tilemap, -48, -6, 96, 1, groundThirdRowTile);
            PlacePlatform(tilemap, -38, -2, 14, edgeTile, groundTile);
            PlacePlatform(tilemap, -16, 0, 12, edgeTile, groundTile);
            PlacePlatform(tilemap, 6, -1, 12, edgeTile, groundTile);
            PlacePlatform(tilemap, 28, 1, 14, edgeTile, groundTile);
            PlacePlatform(tilemap, -4, 3, 10, edgeTile, groundTile);

            // A small stair section for jump cadence testing.
            FillRect(tilemap, 20, -5, 3, 1, accentTile);
            FillRect(tilemap, 23, -4, 3, 1, accentTile);
            FillRect(tilemap, 26, -3, 3, 1, accentTile);
            FillRect(tilemap, 29, -2, 3, 1, accentTile);

            tilemap.RefreshAllTiles();
            BuildCollisionSurfaces(root.transform, ResolveGroundLayer());
            EnsureCoinSystem(root.transform);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            GameSceneSetupEditor.SetupGameScene();
            var player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(-42f, -4f, 0f);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            Debug.Log("Base level generated from 2dAssetPack tile assets.");
        }

        private static TileBase LoadTile(string path)
        {
            return AssetDatabase.LoadAssetAtPath<TileBase>(path);
        }

        private static int ResolveGroundLayer()
        {
            var layer = LayerMask.NameToLayer("Ground");
            return layer >= 0 ? layer : 0;
        }

        private static void FillRect(Tilemap tilemap, int x, int y, int width, int height, TileBase tile)
        {
            for (var ix = x; ix < x + width; ix++)
            {
                for (var iy = y; iy < y + height; iy++)
                {
                    tilemap.SetTile(new Vector3Int(ix, iy, 0), tile);
                }
            }
        }

        private static void PlacePlatform(Tilemap tilemap, int x, int y, int width, TileBase edge, TileBase center)
        {
            for (var ix = 0; ix < width; ix++)
            {
                var tile = (ix == 0 || ix == width - 1) ? edge : center;
                tilemap.SetTile(new Vector3Int(x + ix, y, 0), tile);
            }
        }

        private static void BuildCollisionSurfaces(Transform parent, int layer)
        {
            var existing = parent.Find("Collision");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var collisionRoot = new GameObject("Collision");
            collisionRoot.transform.SetParent(parent);
            collisionRoot.layer = layer;

            var rb = collisionRoot.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            AddColliderRect(collisionRoot, new Vector2(0f, -6.5f), new Vector2(96f, 3f));
            AddColliderRect(collisionRoot, new Vector2(-31f, -2f), new Vector2(14f, 1f));
            AddColliderRect(collisionRoot, new Vector2(-10f, 0f), new Vector2(12f, 1f));
            AddColliderRect(collisionRoot, new Vector2(12f, -1f), new Vector2(12f, 1f));
            AddColliderRect(collisionRoot, new Vector2(35f, 1f), new Vector2(14f, 1f));
            AddColliderRect(collisionRoot, new Vector2(1f, 3f), new Vector2(10f, 1f));
            AddColliderRect(collisionRoot, new Vector2(21.5f, -5f), new Vector2(3f, 1f));
            AddColliderRect(collisionRoot, new Vector2(24.5f, -4f), new Vector2(3f, 1f));
            AddColliderRect(collisionRoot, new Vector2(27.5f, -3f), new Vector2(3f, 1f));
            AddColliderRect(collisionRoot, new Vector2(30.5f, -2f), new Vector2(3f, 1f));
        }

        private static void AddColliderRect(GameObject root, Vector2 center, Vector2 size)
        {
            var collider = root.AddComponent<BoxCollider2D>();
            collider.offset = center;
            collider.size = size;
        }

        private static void BuildParallaxBackground(Transform parent)
        {
            var oldParallax = parent.Find("ParallaxBackground");
            if (oldParallax != null)
            {
                Object.DestroyImmediate(oldParallax.gameObject);
            }

            var parallaxRoot = new GameObject("ParallaxBackground");
            parallaxRoot.transform.SetParent(parent);

            var controller = parallaxRoot.AddComponent<ParallaxBackgroundController>();

            var layerDefs = new[]
            {
                new { name = "1.png", x = 0.05f, y = 0.02f, order = -30, z = 14f, scale = 3.2f },
                new { name = "2.png", x = 0.12f, y = 0.04f, order = -28, z = 13f, scale = 3.0f },
                new { name = "3.png", x = 0.20f, y = 0.06f, order = -26, z = 12f, scale = 2.8f },
                new { name = "4.png", x = 0.32f, y = 0.08f, order = -24, z = 11f, scale = 2.6f },
                new { name = "5.png", x = 0.45f, y = 0.1f, order = -22, z = 10f, scale = 2.4f },
                new { name = "6.png", x = 0.58f, y = 0.12f, order = -20, z = 9f, scale = 2.2f }
            }; 

            var layerTransforms = new List<Transform>();
            var layerX = new List<float>();
            var layerY = new List<float>();
            foreach (var def in layerDefs)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/{def.name}");
                if (sprite == null)
                {
                    continue;
                }

                var layerGo = new GameObject(def.name.Replace(".png", string.Empty));
                layerGo.transform.SetParent(parallaxRoot.transform);
                layerGo.transform.position = new Vector3(0f, 0f, def.z);
                layerGo.transform.localScale = new Vector3(def.scale, def.scale, 1f);

                var sr = layerGo.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = def.order;

                layerTransforms.Add(layerGo.transform);
                layerX.Add(def.x);
                layerY.Add(def.y);
            }

            var so = new SerializedObject(controller);
            var layersProperty = so.FindProperty("layers");
            layersProperty.arraySize = layerTransforms.Count;
            for (var i = 0; i < layerTransforms.Count; i++)
            {
                var element = layersProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("transform").objectReferenceValue = layerTransforms[i];
                element.FindPropertyRelative("xMultiplier").floatValue = layerX[i];
                element.FindPropertyRelative("yMultiplier").floatValue = layerY[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureCoinSystem(Transform levelRoot)
        {
            var hud = EnsureCoinHud();
            EnsureCoinManager(hud);

            var coinsParent = levelRoot.Find("Coins");
            if (coinsParent != null)
            {
                Object.DestroyImmediate(coinsParent.gameObject);
            }

            coinsParent = new GameObject("Coins").transform;
            coinsParent.SetParent(levelRoot);

            var coinPrefab = EnsureCoinPrefab();
            if (coinPrefab == null)
            {
                return;
            }

            var coinPositions = new[]
            {
                new Vector3(-35f, -1.2f, 0f),
                new Vector3(-31f, -1.2f, 0f),
                new Vector3(-27f, -1.2f, 0f),
                new Vector3(-12f, 0.8f, 0f),
                new Vector3(-8f, 0.8f, 0f),
                new Vector3(10f, -0.2f, 0f),
                new Vector3(14f, -0.2f, 0f),
                new Vector3(1f, 3.8f, 0f),
                new Vector3(34f, 1.8f, 0f),
                new Vector3(38f, 1.8f, 0f)
            };

            foreach (var position in coinPositions)
            {
                var coin = PrefabUtility.InstantiatePrefab(coinPrefab) as GameObject;
                if (coin == null)
                {
                    continue;
                }

                coin.name = "GoldCoin";
                coin.transform.position = position;
                coin.transform.SetParent(coinsParent, true);
            }
        }

        private static void EnsureCoinManager(HUDView hudView)
        {
            var managerGo = GameObject.Find("CoinManager");
            if (managerGo == null)
            {
                managerGo = new GameObject("CoinManager");
            }

            var manager = managerGo.GetComponent<CoinManager>();
            if (manager == null)
            {
                manager = managerGo.AddComponent<CoinManager>();
            }

            if (hudView != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("hudView").objectReferenceValue = hudView;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static GameObject EnsureCoinPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoinPrefabPath);
            if (prefab != null)
            {
                return prefab;
            }

            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/Collectibles");

            var coin = new GameObject("GoldCoin");
            var renderer = coin.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 3;
            renderer.color = new Color(1f, 0.83f, 0.1f, 1f);
            renderer.sprite = LoadCoinSprite();

            var collider = coin.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.28f;

            coin.AddComponent<CoinCollectible>();
            prefab = PrefabUtility.SaveAsPrefabAsset(coin, CoinPrefabPath);
            Object.DestroyImmediate(coin);
            return prefab;
        }

        private static Sprite LoadCoinSprite()
        {
            var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/2dAssetPack/3 Objects/Gems/1.png");
            foreach (var asset in sprites)
            {
                if (asset is Sprite sprite)
                {
                    return sprite;
                }
            }

            var fallback = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2dAssetPack/3 Objects/Gems/1.png");
            if (fallback != null)
            {
                return fallback;
            }

            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        private static HUDView EnsureCoinHud()
        {
            var existingHud = Object.FindFirstObjectByType<HUDView>();
            if (existingHud != null)
            {
                return existingHud;
            }

            var canvasGo = new GameObject("HUDCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var hudGo = new GameObject("HUDView");
            hudGo.transform.SetParent(canvasGo.transform, false);
            var hudView = hudGo.AddComponent<HUDView>();

            var coinTextGo = new GameObject("CoinText");
            coinTextGo.transform.SetParent(hudGo.transform, false);
            var coinText = coinTextGo.AddComponent<TextMeshProUGUI>();
            coinText.text = "Coins: 0";
            coinText.fontSize = 42f;
            coinText.alignment = TextAlignmentOptions.TopLeft;
            coinText.color = new Color(1f, 0.92f, 0.15f, 1f);

            var rect = coinText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(32f, -32f);
            rect.sizeDelta = new Vector2(420f, 80f);

            var so = new SerializedObject(hudView);
            so.FindProperty("scoreText").objectReferenceValue = coinText;
            so.ApplyModifiedPropertiesWithoutUndo();

            return hudView;
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

