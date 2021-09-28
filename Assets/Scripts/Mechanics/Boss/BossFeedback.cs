using System.Collections;
using UnityEngine;

namespace Mechanics.Boss
{
    public class BossFeedback : MonoBehaviour
    {
        [SerializeField] private float _screenShakeIntensity = 0.5f;
        [SerializeField] private float _screenShakeDuration = 0.5f;

        private Transform _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main.transform;
        }

        public void ShakeScreen()
        {
            StartCoroutine(ShakeCamera());
        }

        public void EscalationFeedback()
        {
        }

        public void MidpointFeedback()
        {
        }

        public void KillSequenceFeedback()
        {
        }

        private IEnumerator ShakeCamera()
        {
            Vector3 originalPosition = _mainCamera.localPosition;
            for (float t = 0; t < _screenShakeDuration; t += Time.deltaTime) {
                float delta = 1 - Mathf.Abs(1f - 2 * t / _screenShakeDuration);
                _mainCamera.localPosition = originalPosition + Random.insideUnitSphere * _screenShakeIntensity * delta;
                yield return null;
            }
        }
    }
}