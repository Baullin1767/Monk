using UnityEngine;

namespace Monk.Presentation
{
    public class ParallaxBackgroundController : MonoBehaviour
    {
        [System.Serializable]
        private struct ParallaxLayer
        {
            public Transform transform;
            [Range(0f, 1f)] public float xMultiplier;
            [Range(0f, 1f)] public float yMultiplier;
            public bool endlessX;
        }

        [SerializeField] private Transform targetCamera;
        [SerializeField] private bool endlessByDefault = true;
        [SerializeField] private ParallaxLayer[] layers;

        private struct LayerRuntime
        {
            public Transform center;
            public Transform left;
            public Transform right;
            public float width;
            public float xMultiplier;
            public float yMultiplier;
            public bool endlessX;
        }

        private Vector3[] initialLayerPositions;
        private LayerRuntime[] runtimeLayers;
        private Vector3 initialCameraPosition;

        private void Awake()
        {
            if (targetCamera == null && Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }

            if (targetCamera == null || layers == null)
            {
                return;
            }

            initialCameraPosition = targetCamera.position;
            initialLayerPositions = new Vector3[layers.Length];
            runtimeLayers = new LayerRuntime[layers.Length];

            for (var i = 0; i < layers.Length; i++)
            {
                var center = layers[i].transform;
                if (center == null)
                {
                    continue;
                }

                initialLayerPositions[i] = center.position;
                runtimeLayers[i] = CreateRuntimeLayer(center, layers[i]);
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || layers == null || initialLayerPositions == null || runtimeLayers == null)
            {
                return;
            }

            var cameraDelta = targetCamera.position - initialCameraPosition;
            for (var i = 0; i < layers.Length; i++)
            {
                if (runtimeLayers[i].center == null)
                {
                    continue;
                }

                var start = initialLayerPositions[i];
                var position = new Vector3(
                    start.x + (cameraDelta.x * runtimeLayers[i].xMultiplier),
                    start.y + (cameraDelta.y * runtimeLayers[i].yMultiplier),
                    start.z);

                runtimeLayers[i].center.position = position;

                if (!runtimeLayers[i].endlessX || runtimeLayers[i].width <= 0f)
                {
                    continue;
                }

                var centerPosition = runtimeLayers[i].center.position;
                while (targetCamera.position.x - centerPosition.x > runtimeLayers[i].width)
                {
                    centerPosition.x += runtimeLayers[i].width;
                }

                while (targetCamera.position.x - centerPosition.x < -runtimeLayers[i].width)
                {
                    centerPosition.x -= runtimeLayers[i].width;
                }

                runtimeLayers[i].center.position = centerPosition;
                if (runtimeLayers[i].left != null)
                {
                    runtimeLayers[i].left.position = new Vector3(
                        centerPosition.x - runtimeLayers[i].width,
                        centerPosition.y,
                        centerPosition.z);
                }

                if (runtimeLayers[i].right != null)
                {
                    runtimeLayers[i].right.position = new Vector3(
                        centerPosition.x + runtimeLayers[i].width,
                        centerPosition.y,
                        centerPosition.z);
                }
            }
        }

        private LayerRuntime CreateRuntimeLayer(Transform center, ParallaxLayer layer)
        {
            var runtime = new LayerRuntime
            {
                center = center,
                xMultiplier = layer.xMultiplier,
                yMultiplier = layer.yMultiplier,
                endlessX = layer.endlessX || endlessByDefault
            };

            if (!runtime.endlessX)
            {
                return runtime;
            }

            var renderer = center.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null)
            {
                runtime.endlessX = false;
                return runtime;
            }

            runtime.width = renderer.bounds.size.x;
            if (runtime.width <= 0.01f)
            {
                runtime.endlessX = false;
                return runtime;
            }

            runtime.left = Instantiate(center.gameObject, center.parent).transform;
            runtime.left.name = $"{center.name}_Left";
            runtime.right = Instantiate(center.gameObject, center.parent).transform;
            runtime.right.name = $"{center.name}_Right";

            runtime.left.position = new Vector3(center.position.x - runtime.width, center.position.y, center.position.z);
            runtime.right.position = new Vector3(center.position.x + runtime.width, center.position.y, center.position.z);

            return runtime;
        }
    }
}
