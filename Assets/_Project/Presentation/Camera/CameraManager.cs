using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace Monk.Presentation
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera gameplayCamera;
        [SerializeField] private CinemachineFollow followComponent;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 1.5f, -10f);

        private Coroutine shakeRoutine;

        private void Awake()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = GetComponent<CinemachineCamera>();
            }

            if (followComponent == null)
            {
                followComponent = GetComponent<CinemachineFollow>();
            }

            if (followComponent != null)
            {
                followComponent.FollowOffset = followOffset;
            }
        }

        public void SetFollowTarget(Transform target)
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = GetComponent<CinemachineCamera>();
            }

            if (followComponent == null)
            {
                followComponent = GetComponent<CinemachineFollow>();
            }

            if (gameplayCamera == null || target == null)
            {
                return;
            }

            gameplayCamera.Follow = target;
            if (followComponent != null)
            {
                followComponent.FollowOffset = followOffset;
            }
        }

        public void Shake(float intensity, float duration)
        {
            if (followComponent == null)
            {
                return;
            }

            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var offsetX = Random.Range(-intensity, intensity);
                var offsetY = Random.Range(-intensity, intensity);
                followComponent.FollowOffset = followOffset + new Vector3(offsetX, offsetY, 0f);
                yield return null;
            }

            followComponent.FollowOffset = followOffset;
            shakeRoutine = null;
        }
    }
}
