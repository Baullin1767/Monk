using UnityEngine;
using Monk.Core;

namespace Monk.Presentation
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem landParticles;
        [SerializeField] private float damageFlashDuration = 0.08f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.45f, 0.45f, 1f);

        private Coroutine flashCoroutine;

        public void SetFacing(FacingDirection direction)
        {
            spriteRenderer.flipX = direction == FacingDirection.Left;
        }

        public void PlayDamageFlash()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        public void PlayLandEffect()
        {
            if (landParticles != null)
            {
                landParticles.Play();
            }
        }

        private void OnDisable()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }

        private System.Collections.IEnumerator DamageFlashRoutine()
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = Color.white;
            flashCoroutine = null;
        }
    }
}
