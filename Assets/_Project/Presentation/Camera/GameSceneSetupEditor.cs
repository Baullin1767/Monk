#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

namespace Monk.Presentation.Editor
{
    public static class GameSceneSetupEditor
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Level 1.unity";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";

        [MenuItem("Tools/Monk/Setup/Setup Level 1 Player + Cinemachine Camera")]
        public static void SetupGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to load scene at '{GameScenePath}'.");
                return;
            }

            var player = EnsurePlayerInstance();
            if (player == null)
            {
                return;
            }

            EnsureInputManager();
            EnsureCameraRig(player.transform);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Selection.activeGameObject = player;

            Debug.Log("Level 1 configured with Player, InputManager, and Cinemachine 2D camera.");
        }

        private static GameObject EnsurePlayerInstance()
        {
            var existing = GameObject.Find("Player");
            if (existing != null)
            {
                return existing;
            }

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"Player prefab not found at '{PlayerPrefabPath}'.");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate player prefab.");
                return null;
            }

            instance.name = "Player";
            instance.transform.position = new Vector3(0f, 0f, 0f);
            return instance;
        }

        private static void EnsureInputManager()
        {
            var inputManagerGo = GameObject.Find("InputManager");
            if (inputManagerGo == null)
            {
                inputManagerGo = new GameObject("InputManager");
            }

            if (inputManagerGo.GetComponent<Monk.Input.InputManager>() == null)
            {
                inputManagerGo.AddComponent<Monk.Input.InputManager>();
            }

            if (inputManagerGo.GetComponent<Monk.Input.DesktopInputProvider>() == null)
            {
                inputManagerGo.AddComponent<Monk.Input.DesktopInputProvider>();
            }

            if (inputManagerGo.GetComponent<Monk.Input.MobileInputProvider>() == null)
            {
                inputManagerGo.AddComponent<Monk.Input.MobileInputProvider>();
            }
        }

        private static void EnsureCameraRig(Transform followTarget)
        {
            var mainCameraGo = GameObject.Find("Main Camera");
            if (mainCameraGo == null)
            {
                mainCameraGo = new GameObject("Main Camera");
            }

            mainCameraGo.tag = "MainCamera";
            mainCameraGo.transform.position = new Vector3(0f, 0f, -10f);
            mainCameraGo.transform.rotation = Quaternion.identity;

            var camera = mainCameraGo.GetComponent<Camera>();
            if (camera == null)
            {
                camera = mainCameraGo.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 5f;

            if (mainCameraGo.GetComponent<AudioListener>() == null)
            {
                mainCameraGo.AddComponent<AudioListener>();
            }

            if (mainCameraGo.GetComponent<CinemachineBrain>() == null)
            {
                mainCameraGo.AddComponent<CinemachineBrain>();
            }

            var cmCameraGo = GameObject.Find("CinemachineCamera2D");
            if (cmCameraGo == null)
            {
                cmCameraGo = new GameObject("CinemachineCamera2D");
            }

            cmCameraGo.transform.position = new Vector3(0f, 1.5f, -10f);
            cmCameraGo.transform.rotation = Quaternion.identity;

            var cmCamera = cmCameraGo.GetComponent<CinemachineCamera>();
            if (cmCamera == null)
            {
                cmCamera = cmCameraGo.AddComponent<CinemachineCamera>();
            }
            var lens = cmCamera.Lens;
            lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            lens.OrthographicSize = 5f;
            cmCamera.Lens = lens;

            var cmFollow = cmCameraGo.GetComponent<CinemachineFollow>();
            if (cmFollow == null)
            {
                cmFollow = cmCameraGo.AddComponent<CinemachineFollow>();
            }

            cmFollow.FollowOffset = new Vector3(0f, 1.5f, -10f);
            cmCamera.Follow = followTarget;

            var cameraManager = cmCameraGo.GetComponent<CameraManager>();
            if (cameraManager == null)
            {
                cameraManager = cmCameraGo.AddComponent<CameraManager>();
            }

            cameraManager.SetFollowTarget(followTarget);
        }
    }
}
#endif
