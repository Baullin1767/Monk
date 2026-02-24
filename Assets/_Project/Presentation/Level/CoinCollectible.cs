using UnityEngine;

namespace Monk.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class CoinCollectible : MonoBehaviour
    {
        [SerializeField] private int coinValue = 1;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobFrequency = 2f;

        private Vector3 basePosition;
        private CoinManager coinManager;
        private bool collected;

        private void Awake()
        {
            basePosition = transform.position;
            coinManager = FindFirstObjectByType<CoinManager>();

            var trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void Update()
        {
            if (collected)
            {
                return;
            }

            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            var bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            var position = basePosition;
            position.y += bobY;
            transform.position = position;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected)
            {
                return;
            }

            if (collision.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            if (coinManager == null)
            {
                coinManager = FindFirstObjectByType<CoinManager>();
            }

            coinManager?.AddCoins(coinValue);
            collected = true;
            Destroy(gameObject);
        }
    }
}
