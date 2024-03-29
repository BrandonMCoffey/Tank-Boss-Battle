﻿using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceController : MonoBehaviour
    {
        private Transform _parent;
        private AudioSource _source;
        private float _originalVolume;

        public bool Claimed { get; set; } = false;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (_source.outputAudioMixerGroup == null) {
                _source.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;
            }
        }

        private void LateUpdate()
        {
            if (!Claimed) return;
            if (_source.isPlaying == false) {
                Stop();
            } else if (_parent != null) {
                transform.position = _parent.position;
            }
        }

        public void Reset()
        {
            transform.localPosition = Vector3.zero;
            _parent = null;
            Claimed = false;
        }

        public void SetSourceProperties(AudioClip clip, float volume, float pitch, bool loop, float spacialBlend)
        {
            _source.clip = clip;
            _source.volume = volume;
            _originalVolume = volume;
            _source.pitch = pitch;
            _source.loop = loop;
            _source.spatialBlend = spacialBlend;
        }

        public void SetCustomVolume(float volume)
        {
            if (volume < 0) return;
            _source.volume = _originalVolume * volume;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetParent(Transform parent)
        {
            _parent = parent;
        }

        public void Play()
        {
            _source.Play();
        }

        public void Stop()
        {
            _source.Stop();
            Reset();
            AudioManager.Instance.ReturnController(this);
        }
    }
}