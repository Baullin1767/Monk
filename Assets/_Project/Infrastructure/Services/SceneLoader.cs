using System;
using UnityEngine;

namespace Monk.Infrastructure
{
    public class SceneLoader : MonoBehaviour
    {
        public event Action OnSceneLoadStarted;
        public event Action OnSceneLoadCompleted;

        public void LoadScene(string sceneName)
        {
        }

        public void ReloadCurrentScene()
        {
        }
    }
}
