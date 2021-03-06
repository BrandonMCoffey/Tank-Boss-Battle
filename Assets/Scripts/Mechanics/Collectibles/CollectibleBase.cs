using Audio;
using Interfaces;
using UnityEngine;

namespace Mechanics.Collectibles
{
    [RequireComponent(typeof(Collider))]
    public abstract class CollectibleBase : MonoBehaviour, IDamageable
    {
        [Header("Feedback")]
        [SerializeField] private ParticleSystem _collectParticles = null;
        [SerializeField] private SfxReference _collectSfx = new SfxReference();
        [Header("Movement")]
        [SerializeField] private Transform _transformToRotate = null;
        [SerializeField] private float _rotationSpeed = 1;

        protected Collider Collider { get; private set; }
        protected float RotationSpeed => _rotationSpeed;

        private void Awake()
        {
            Collider = GetComponent<Collider>();
            Collider.isTrigger = true;
            // Ensure collect particles don't play on awake or self destruct
            if (_collectParticles != null && _collectParticles.gameObject.activeInHierarchy) {
                _collectParticles.gameObject.SetActive(false);
            }
            if (_transformToRotate == null) _transformToRotate = transform;
        }

        private void FixedUpdate()
        {
            Movement(_transformToRotate);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (OnCollect(other.gameObject)) {
                Feedback();
                DisableObject();
            }
        }

        protected abstract bool OnCollect(GameObject other);

        protected virtual void Feedback()
        {
            if (_collectParticles != null) {
                Instantiate(_collectParticles, transform.position, Quaternion.identity).gameObject.SetActive(true);
            }
            _collectSfx.Play();
        }

        protected virtual void DisableObject()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Movement(Transform obj)
        {
            Quaternion turnOffset = Quaternion.Euler(0, _rotationSpeed, 0);
            obj.rotation *= turnOffset;
        }

        public bool OnDamageVolume(int damage)
        {
            Destroy(gameObject);
            return true;
        }

        public void OnTankImpact(int damageTaken)
        {
        }

        public void OnBombDealDamage(int damageTaken)
        {
        }

        public bool OnBulletImpact(int damageTaken, Vector3 forward)
        {
            return false;
        }

        public void OnKill()
        {
        }
    }
}