using System.Collections;
using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    public class AppearingTargetHitPointsVisualizer : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private float _visibleDuration = 1f;
        [SerializeField] private float _showAnimationDuration = 0.4f;
        [SerializeField] private float _hidingAnimationDuration = 0.5f;
        [SerializeField] private float _movingUpSpeed = 0.1f;
        [SerializeField] private float _movingDownSpeed = 0.15f;

        private void OnEnable()
        {
            StartCoroutine(HitPointsTextLifeCoroutine());
        }

        private IEnumerator HitPointsTextLifeCoroutine()
        {
            float showTimer = 0f; 
            while (showTimer < _showAnimationDuration)
            {
                showTimer += Time.deltaTime;
                float visiblePercentage = showTimer / _showAnimationDuration;
                _canvasGroup.alpha = visiblePercentage;
                transform.position += Vector3.up * _movingUpSpeed * Time.deltaTime;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(_visibleDuration);

            float timer = 0f;
            Vector3 originalScale = transform.localScale;
            while (timer < _hidingAnimationDuration)
            {
                timer += Time.deltaTime;
                float visiblePercentage = 1.0f - timer / _hidingAnimationDuration;
                _canvasGroup.alpha = visiblePercentage;
                transform.position -= Vector3.up * _movingDownSpeed * Time.deltaTime;
                transform.localScale = originalScale * visiblePercentage;
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            Destroy(gameObject);
        }
    }
}
