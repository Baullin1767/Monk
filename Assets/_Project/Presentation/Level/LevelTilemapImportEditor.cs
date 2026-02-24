#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monk.Presentation.Editor
{
    public static class LevelTilemapImportEditor
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Level 1.unity";
        private const string SampleScenePath = "Assets/karsiori/Scenes/SampleScene - Woods Tileset and Background.unity";
        private const string LevelRootName = "BaseLevel_Tilemap";

        [MenuItem("Tools/Monk/Setup/Import Karsiori Tilemap Level")]
        public static void ImportKarsioriLevel()
        {
            var gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            if (!gameScene.IsValid())
            {
                Debug.LogError($"Failed to open '{GameScenePath}'.");
                return;
            }

            var existingRoot = GameObject.Find(LevelRootName);
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }

            var sampleScene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Additive);
            if (!sampleScene.IsValid())
            {
                Debug.LogError($"Failed to open '{SampleScenePath}'.");
                return;
            }

            var levelRoot = new GameObject(LevelRootName);
            SceneManager.MoveGameObjectToScene(levelRoot, gameScene);

            var sampleRoots = sampleScene.GetRootGameObjects();
            foreach (var root in sampleRoots)
            {
                if (!ShouldImportRoot(root))
                {
                    continue;
                }

                var clone = Object.Instantiate(root);
                clone.name = root.name;
                SceneManager.MoveGameObjectToScene(clone, gameScene);
                clone.transform.SetParent(levelRoot.transform, true);
            }

            EditorSceneManager.CloseScene(sampleScene, true);

            GameSceneSetupEditor.SetupGameScene();
            EditorSceneManager.MarkSceneDirty(gameScene);
            EditorSceneManager.SaveScene(gameScene);

            Debug.Log("Imported Karsiori tilemap level into Level 1.");
        }

        private static bool ShouldImportRoot(GameObject root)
        {
            if (root.GetComponentInChildren<Camera>(true) != null)
            {
                return false;
            }

            if (root.GetComponentInChildren<Unity.Cinemachine.CinemachineBrain>(true) != null)
            {
                return false;
            }

            var nameLower = root.name.ToLowerInvariant();
            if (nameLower.Contains("eventsystem"))
            {
                return false;
            }

            return true;
        }
    }
}
#endif
