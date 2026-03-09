#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Monk.Presentation.Editor
{
    public static class TwoDAssetPackMigrationEditor
    {
        private const string PackRoot = "Assets/2dAssetPack";

        private const string PlayerIdlePath = "Assets/2dAssetPack/1 Main Characters/1/Idle.png";
        private const string PlayerRunPath = "Assets/2dAssetPack/1 Main Characters/1/Run.png";
        private const string PlayerJumpPath = "Assets/2dAssetPack/1 Main Characters/1/Jump.png";
        private const string PlayerFallPath = "Assets/2dAssetPack/1 Main Characters/1/Fall.png";
        private const string PlayerHitPath = "Assets/2dAssetPack/1 Main Characters/1/Hit.png";
        private const string PlayerDoubleJumpPath = "Assets/2dAssetPack/1 Main Characters/1/Double_Jump.png";
        private const string PlayerWallJumpPath = "Assets/2dAssetPack/1 Main Characters/1/Wall_Jump.png";

        private const string EnemyIdlePath = "Assets/2dAssetPack/4 Enemies/4/Idle.png";
        private const string EnemyWalkPath = "Assets/2dAssetPack/4 Enemies/4/Walk.png";
        private const string EnemyAttackPath = "Assets/2dAssetPack/4 Enemies/4/Attack.png";
        private const string EnemyHitPath = "Assets/2dAssetPack/4 Enemies/4/Hit.png";

        private const string OldPlayerSheetPath = "Assets/_Project/Sprites/Player/PL.png";
        private const string OldEnemySheetPath = "Assets/_Project/Sprites/Enemy/tufei.png";
        private const string OldButtonPath = "Assets/_Project/Sprites/UI/MainMenu/ButtonBG.png";
        private const string OldBackgroundPath = "Assets/_Project/Sprites/UI/Background.png";
        private const string OldLoadingBarPath = "Assets/_Project/Sprites/UI/LoadingScreen/LoadingBarScpriteSheet.png";
        private const string OldTilesheetPath = "Assets/karsiori/TileMap/Tiles/Tilesheet - WOODS.png";
        private const string OldTilesheetGuid = "b425613db706e0a49821a35253ec9db1";

        private const string PreviewBackground1Path = "Assets/2dAssetPack/7 Levels/Preview/1lvl.jpg";
        private const string PreviewBackground2Path = "Assets/2dAssetPack/7 Levels/Preview/2lvl.jpg";
        private const string ButtonSpritePath = "Assets/2dAssetPack/5 GUI/Buttons/Button_20.png";
        private const string LoadingBarEmptyPath = "Assets/2dAssetPack/5 GUI/Interface/Tile_24.png";
        private const string LoadingBarFillPath = "Assets/2dAssetPack/5 GUI/Interface/Tile_25.png";
        private const string CoinSpritePath = "Assets/2dAssetPack/3 Objects/Gems/1.png";

        private static readonly Regex TrailingNumberRegex = new("(\\d+)$", RegexOptions.Compiled);
        private static readonly Regex LegacyTileSpriteDataRegex = new(
            "(m_Data:\\s*\\{fileID:\\s*)(-?\\d+)(,\\s*guid:\\s*)b425613db706e0a49821a35253ec9db1(\\s*,\\s*type:\\s*3\\s*\\})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly string[] SerializedExtensions = { ".unity", ".prefab", ".anim", ".asset" };

        private static List<Sprite> playerIdleSprites = new();
        private static List<Sprite> playerRunSprites = new();
        private static List<Sprite> playerJumpSprites = new();
        private static List<Sprite> playerFallSprites = new();
        private static List<Sprite> playerHitSprites = new();
        private static List<Sprite> playerDoubleJumpSprites = new();
        private static List<Sprite> playerWallJumpSprites = new();

        private static List<Sprite> enemyIdleSprites = new();
        private static List<Sprite> enemyWalkSprites = new();
        private static List<Sprite> enemyAttackSprites = new();
        private static List<Sprite> enemyHitSprites = new();

        private static List<Sprite> tileSprites = new();
        private static Dictionary<string, Sprite> backgroundMap = new(StringComparer.OrdinalIgnoreCase);

        private static Sprite buttonSprite;
        private static Sprite menuBackgroundSprite;
        private static Sprite loadingBarEmptySprite;
        private static Sprite loadingBarFillSprite;
        private static Sprite coinSprite;

        private readonly struct SpriteReference
        {
            public SpriteReference(string guid, long localId)
            {
                Guid = guid;
                LocalId = localId;
            }

            public string Guid { get; }
            public long LocalId { get; }
        }

        [MenuItem("Tools/Monk/Setup/Migrate Visuals To 2dAssetPack (Preset A)")]
        public static void MigratePresetA()
        {
            if (!AssetDatabase.IsValidFolder(PackRoot))
            {
                Debug.LogError($"Cannot run migration because '{PackRoot}' is missing.");
                return;
            }

            Configure2dAssetPackImports();
            EnsurePresetASpriteSlices();
            EnsureGeneratedTileAssets();

            BuildRuntimeSpriteCache();
            RemapAllSpriteReferences();
            RemapLegacyTileSpriteReferencesInSerializedFiles();

            PlayerAnimationSetupEditor.GeneratePlayerAnimations();
            EnemyAnimationSetupEditor.GenerateEnemyAnimations();
            PlayerPrefabSetupEditor.CreatePlayerPrefab();
            EnemyPrefabSetupEditor.CreateEnemyPrefab();

            RemapAllSpriteReferences();
            RemapLegacyTileSpriteReferencesInSerializedFiles();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("2dAssetPack migration completed. Import, slicing, remap, and animation regeneration are done.");
        }

        public static void EnsurePresetASpriteSlices()
        {
            SliceHorizontalStrip(PlayerIdlePath, 32, 32, "PL_Idle");
            SliceHorizontalStrip(PlayerRunPath, 32, 32, "PL_Run");
            SliceHorizontalStrip(PlayerJumpPath, 32, 32, "PL_Jump");
            SliceHorizontalStrip(PlayerFallPath, 32, 32, "PL_Fall");
            SliceHorizontalStrip(PlayerHitPath, 32, 32, "PL_Hit");
            SliceHorizontalStrip(PlayerDoubleJumpPath, 32, 32, "PL_DoubleJump");
            SliceHorizontalStrip(PlayerWallJumpPath, 32, 32, "PL_WallJump");

            SliceHorizontalStrip(EnemyIdlePath, 48, 48, "EN_Idle");
            SliceHorizontalStrip(EnemyWalkPath, 48, 48, "EN_Walk");
            SliceHorizontalStrip(EnemyAttackPath, 48, 48, "EN_Attack");
            SliceHorizontalStrip(EnemyHitPath, 48, 48, "EN_Hit");

            var gemPaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/2dAssetPack/3 Objects/Gems" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => ExtractTrailingNumber(Path.GetFileNameWithoutExtension(path)))
                .ToArray();

            for (var i = 0; i < gemPaths.Length; i++)
            {
                SliceHorizontalStrip(gemPaths[i], 16, 16, $"GEM_{i + 1}");
            }
        }

        private static void Configure2dAssetPackImports()
        {
            var textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { PackRoot });
            foreach (var guid in textureGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                ConfigureTextureImport(path);
            }
        }

        private static void ConfigureTextureImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            var changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (importer.spritePixelsPerUnit != 100f)
            {
                importer.spritePixelsPerUnit = 100f;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void SliceHorizontalStrip(string path, int frameWidth, int frameHeight, string namePrefix)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (importer == null || texture == null)
            {
                return;
            }

            if (frameWidth <= 0 || frameHeight <= 0)
            {
                return;
            }

            if (texture.width < frameWidth || texture.height < frameHeight)
            {
                return;
            }

            var columns = texture.width / frameWidth;
            var rows = texture.height / frameHeight;
            if (columns <= 0 || rows <= 0)
            {
                return;
            }

            var metas = new List<SpriteMetaData>(columns * rows);
            var index = 0;
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = $"{namePrefix}_{index}",
                        rect = new Rect(col * frameWidth, texture.height - ((row + 1) * frameHeight), frameWidth, frameHeight),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });

                    index++;
                }
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();
        }

        private static void EnsureGeneratedTileAssets()
        {
            EnsureFolder("Assets/2dAssetPack/Generated");
            EnsureFolder("Assets/2dAssetPack/Generated/Tiles");

            var tilePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/2dAssetPack/2 Locations/Tiles" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetFileName(path).StartsWith("Tile_", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => ExtractTrailingNumber(Path.GetFileNameWithoutExtension(path)))
                .ToArray();

            foreach (var spritePath in tilePaths)
            {
                var sprite = LoadFirstSprite(spritePath);
                if (sprite == null)
                {
                    continue;
                }

                var tileAssetPath = $"Assets/2dAssetPack/Generated/Tiles/{Path.GetFileNameWithoutExtension(spritePath)}.asset";
                var tileAsset = AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath);
                if (tileAsset == null)
                {
                    tileAsset = ScriptableObject.CreateInstance<Tile>();
                    tileAsset.sprite = sprite;
                    AssetDatabase.CreateAsset(tileAsset, tileAssetPath);
                }
                else
                {
                    tileAsset.sprite = sprite;
                    EditorUtility.SetDirty(tileAsset);
                }
            }
        }

        private static void BuildRuntimeSpriteCache()
        {
            playerIdleSprites = LoadSpritesSorted(PlayerIdlePath);
            playerRunSprites = LoadSpritesSorted(PlayerRunPath);
            playerJumpSprites = LoadSpritesSorted(PlayerJumpPath);
            playerFallSprites = LoadSpritesSorted(PlayerFallPath);
            playerHitSprites = LoadSpritesSorted(PlayerHitPath);
            playerDoubleJumpSprites = LoadSpritesSorted(PlayerDoubleJumpPath);
            playerWallJumpSprites = LoadSpritesSorted(PlayerWallJumpPath);

            enemyIdleSprites = LoadSpritesSorted(EnemyIdlePath);
            enemyWalkSprites = LoadSpritesSorted(EnemyWalkPath);
            enemyAttackSprites = LoadSpritesSorted(EnemyAttackPath);
            enemyHitSprites = LoadSpritesSorted(EnemyHitPath);

            tileSprites = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/2dAssetPack/2 Locations/Tiles" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetFileName(path).StartsWith("Tile_", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => ExtractTrailingNumber(Path.GetFileNameWithoutExtension(path)))
                .Select(LoadFirstSprite)
                .Where(sprite => sprite != null)
                .ToList();

            backgroundMap = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase)
            {
                ["BACKGROUND.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/1.png"),
                ["WOODS - Fourth.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/2.png"),
                ["WOODS - Third.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/3.png"),
                ["WOODS - Second.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/4.png"),
                ["WOODS - First.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/5.png"),
                ["VINES - Second.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/6.png"),
                ["BUSH - BACKGROUND.png"] = LoadFirstSprite("Assets/2dAssetPack/7 Levels/Tiled/Backgrounds/6.png")
            };

            buttonSprite = LoadFirstSprite(ButtonSpritePath);
            menuBackgroundSprite = LoadFirstSprite(PreviewBackground1Path);
            loadingBarEmptySprite = LoadFirstSprite(LoadingBarEmptyPath);
            loadingBarFillSprite = LoadFirstSprite(LoadingBarFillPath);
            coinSprite = LoadSpritesSorted(CoinSpritePath).FirstOrDefault() ?? LoadFirstSprite(CoinSpritePath);

            if (menuBackgroundSprite == null)
            {
                menuBackgroundSprite = LoadFirstSprite(PreviewBackground2Path);
            }
        }

        private static void RemapAllSpriteReferences()
        {
            RemapPrefabs();
            RemapScenes();
            RemapAnimationClips();
            RemapScriptableObjects();
        }

        private static void RemapLegacyTileSpriteReferencesInSerializedFiles()
        {
            var replacementMap = BuildLegacyTileSpriteReplacementMap();
            if (replacementMap.Count == 0)
            {
                return;
            }

            var changedFiles = 0;
            foreach (var fullPath in EnumerateSerializedFiles())
            {
                var content = File.ReadAllText(fullPath);
                if (content.IndexOf(OldTilesheetGuid, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var replaced = false;
                var remapped = LegacyTileSpriteDataRegex.Replace(content, match =>
                {
                    if (!long.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var oldLocalId))
                    {
                        return match.Value;
                    }

                    if (!replacementMap.TryGetValue(oldLocalId, out var replacement))
                    {
                        return match.Value;
                    }

                    replaced = true;
                    return string.Concat(
                        match.Groups[1].Value,
                        replacement.LocalId.ToString(CultureInfo.InvariantCulture),
                        match.Groups[3].Value,
                        replacement.Guid,
                        match.Groups[4].Value);
                });

                if (!replaced || string.Equals(remapped, content, StringComparison.Ordinal))
                {
                    continue;
                }

                File.WriteAllText(fullPath, remapped);
                changedFiles++;
            }

            if (changedFiles > 0)
            {
                AssetDatabase.Refresh();
            }
        }

        private static Dictionary<long, SpriteReference> BuildLegacyTileSpriteReplacementMap()
        {
            var map = new Dictionary<long, SpriteReference>();
            var oldTileSprites = LoadSpritesSorted(OldTilesheetPath);
            for (var i = 0; i < oldTileSprites.Count; i++)
            {
                var oldSprite = oldTileSprites[i];
                if (oldSprite == null)
                {
                    continue;
                }

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(oldSprite, out _, out long oldLocalId))
                {
                    continue;
                }

                var newSprite = MapTileSprite(oldSprite.name);
                if (newSprite == null)
                {
                    continue;
                }

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newSprite, out var newGuid, out long newLocalId))
                {
                    continue;
                }

                map[oldLocalId] = new SpriteReference(newGuid, newLocalId);
            }

            return map;
        }

        private static IEnumerable<string> EnumerateSerializedFiles()
        {
            var files = Directory.EnumerateFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (SerializedExtensions.Any(item => extension.Equals(item, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return file;
                }
            }
        }

        private static void RemapPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                var changed = RemapHierarchy(root);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }

                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RemapScenes()
        {
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var originalScenePath = SceneManager.GetActiveScene().path;

            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var changed = false;

                var roots = scene.GetRootGameObjects();
                for (var i = 0; i < roots.Length; i++)
                {
                    changed |= RemapHierarchy(roots[i]);
                }

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!string.IsNullOrWhiteSpace(originalScenePath) && File.Exists(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }

        private static void RemapAnimationClips()
        {
            var clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets" });
            foreach (var guid in clipGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                {
                    continue;
                }

                var changed = false;
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                for (var i = 0; i < bindings.Length; i++)
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, bindings[i]);
                    var bindingChanged = false;

                    for (var k = 0; k < keyframes.Length; k++)
                    {
                        if (keyframes[k].value is not Sprite sourceSprite)
                        {
                            continue;
                        }

                        var mapped = RemapSprite(sourceSprite);
                        if (mapped == null || mapped == sourceSprite)
                        {
                            continue;
                        }

                        keyframes[k].value = mapped;
                        bindingChanged = true;
                    }

                    if (!bindingChanged)
                    {
                        continue;
                    }

                    AnimationUtility.SetObjectReferenceCurve(clip, bindings[i], keyframes);
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(clip);
                }
            }
        }

        private static void RemapScriptableObjects()
        {
            var assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });
            foreach (var guid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scriptable = AssetDatabase.LoadMainAssetAtPath(path) as ScriptableObject;
                if (scriptable == null)
                {
                    continue;
                }

                var serialized = new SerializedObject(scriptable);
                if (RemapSerializedObject(serialized))
                {
                    EditorUtility.SetDirty(scriptable);
                }
            }
        }

        private static bool RemapHierarchy(GameObject root)
        {
            var changed = false;
            var components = root.GetComponentsInChildren<Component>(true);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                var serialized = new SerializedObject(components[i]);
                changed |= RemapSerializedObject(serialized);
            }

            return changed;
        }

        private static bool RemapSerializedObject(SerializedObject serialized)
        {
            var changed = false;
            var iterator = serialized.GetIterator();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;

                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                if (iterator.objectReferenceValue is not Sprite sprite)
                {
                    continue;
                }

                var mapped = RemapSprite(sprite);
                if (mapped == null || mapped == sprite)
                {
                    continue;
                }

                iterator.objectReferenceValue = mapped;
                changed = true;
            }

            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static Sprite RemapSprite(Sprite source)
        {
            if (source == null)
            {
                return null;
            }

            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return source;
            }

            if (sourcePath.Equals(OldPlayerSheetPath, StringComparison.OrdinalIgnoreCase))
            {
                return MapPlayerSprite(source.name) ?? source;
            }

            if (sourcePath.Equals(OldEnemySheetPath, StringComparison.OrdinalIgnoreCase))
            {
                return MapEnemySprite(source.name) ?? source;
            }

            if (sourcePath.Equals(OldButtonPath, StringComparison.OrdinalIgnoreCase))
            {
                return buttonSprite ?? source;
            }

            if (sourcePath.Equals(OldBackgroundPath, StringComparison.OrdinalIgnoreCase))
            {
                return menuBackgroundSprite ?? source;
            }

            if (sourcePath.Equals(OldLoadingBarPath, StringComparison.OrdinalIgnoreCase))
            {
                if (source.name.IndexOf("Empty", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return loadingBarEmptySprite ?? source;
                }

                return loadingBarFillSprite ?? source;
            }

            if (sourcePath.Equals(OldTilesheetPath, StringComparison.OrdinalIgnoreCase))
            {
                return MapTileSprite(source.name) ?? source;
            }

            if (sourcePath.StartsWith("Assets/karsiori/TileMap/Backgrounds/", StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Path.GetFileName(sourcePath);
                if (fileName != null && backgroundMap.TryGetValue(fileName, out var mappedBackground) && mappedBackground != null)
                {
                    return mappedBackground;
                }

                return source;
            }

            if (sourcePath.StartsWith("Assets/karsiori/TileMap/Decorations/", StringComparison.OrdinalIgnoreCase))
            {
                return coinSprite ?? source;
            }

            return source;
        }

        private static Sprite MapPlayerSprite(string oldName)
        {
            var frame = ExtractTrailingNumber(oldName);
            if (frame < 0)
            {
                return playerIdleSprites.FirstOrDefault();
            }

            if (frame <= 11)
            {
                return ResolveFrame(playerIdleSprites, frame);
            }

            if (frame <= 19)
            {
                return ResolveFrame(playerRunSprites, frame - 12);
            }

            if (frame <= 30)
            {
                return ResolveFrame(playerHitSprites, frame - 20);
            }

            if (frame <= 33)
            {
                return ResolveFrame(playerJumpSprites, 0);
            }

            if (frame <= 37)
            {
                return ResolveFrame(playerFallSprites, 0);
            }

            if (frame <= 40)
            {
                return ResolveFrame(playerWallJumpSprites, frame - 38);
            }

            return ResolveFrame(playerHitSprites, playerHitSprites.Count - 1);
        }

        private static Sprite MapEnemySprite(string oldName)
        {
            var frame = ExtractTrailingNumber(oldName);
            if (frame < 0)
            {
                return enemyIdleSprites.FirstOrDefault();
            }

            if (frame <= 3)
            {
                return ResolveFrame(enemyIdleSprites, frame);
            }

            if (frame <= 8)
            {
                return ResolveFrame(enemyWalkSprites, frame - 4);
            }

            if (frame <= 16)
            {
                return ResolveFrame(enemyAttackSprites, frame - 9);
            }

            return ResolveFrame(enemyHitSprites, frame - 17);
        }

        private static Sprite MapTileSprite(string oldName)
        {
            if (tileSprites.Count == 0)
            {
                return null;
            }

            var index = ExtractTrailingNumber(oldName);
            if (index < 0)
            {
                index = 0;
            }

            index %= tileSprites.Count;
            return tileSprites[index];
        }

        private static Sprite ResolveFrame(IReadOnlyList<Sprite> frames, int index)
        {
            if (frames == null || frames.Count == 0)
            {
                return null;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= frames.Count)
            {
                index = frames.Count - 1;
            }

            return frames[index];
        }

        private static List<Sprite> LoadSpritesSorted(string path)
        {
            var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                .OfType<Sprite>()
                .ToList();

            if (sprites.Count == 0)
            {
                var single = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (single != null)
                {
                    sprites.Add(single);
                }
            }

            sprites.Sort((left, right) =>
            {
                var leftNum = ExtractTrailingNumber(left.name);
                var rightNum = ExtractTrailingNumber(right.name);
                var cmp = leftNum.CompareTo(rightNum);
                return cmp != 0 ? cmp : string.CompareOrdinal(left.name, right.name);
            });

            return sprites;
        }

        private static Sprite LoadFirstSprite(string path)
        {
            var sprites = LoadSpritesSorted(path);
            return sprites.Count > 0 ? sprites[0] : null;
        }

        private static int ExtractTrailingNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return -1;
            }

            var match = TrailingNumberRegex.Match(value);
            if (!match.Success)
            {
                return -1;
            }

            return int.TryParse(match.Groups[1].Value, out var result) ? result : -1;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(folderName))
            {
                return;
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
