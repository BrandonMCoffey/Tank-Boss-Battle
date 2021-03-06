using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility.CustomFloats;

namespace UI
{
    [RequireComponent(typeof(Slider))]
    public class VariableSlider : MonoBehaviour
    {
        [SerializeField] private FloatVariable _variable = null;
        [SerializeField] private bool _animateOnStart = true;
        [SerializeField] private float _offset = 2;
        [SerializeField] private float _timeToAnimate = 4;

        private Slider _slider;
        private float _max = 25;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            if (_variable == null) return;
            _variable.OnValueChanged += UpdateSlider;
        }

        private void OnDisable()
        {
            if (_variable == null) return;
            _variable.OnValueChanged -= UpdateSlider;
        }

        public void StartAnimation()
        {
            if (_animateOnStart) {
                StartCoroutine(AnimateSlider());
            }
        }

        public void UpdateSlider(float newValue)
        {
            if (newValue > _max) {
                _max = newValue;
            }
            if (_animateOnStart) return;
            _slider.value = newValue / _max;
        }

        public void CancelAnimation()
        {
            StopAllCoroutines();
            AnimateSlider(1);
            _animateOnStart = false;
        }

        private void AnimateSlider(float delta)
        {
            _slider.value = delta;
        }

        private IEnumerator AnimateSlider()
        {
            yield return new WaitForSecondsRealtime(_offset);
            for (float t = 0; t < _timeToAnimate; t += Time.deltaTime) {
                float delta = t / _timeToAnimate;
                AnimateSlider(delta);
                yield return null;
            }
            _animateOnStart = false;
        }
    }
}