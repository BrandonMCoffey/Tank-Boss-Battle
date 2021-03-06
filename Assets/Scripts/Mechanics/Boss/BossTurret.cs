using Mechanics.Player_Systems;
using UnityEngine;
using UnityEngine.Events;
using Utility.CustomFloats;

namespace Mechanics.Boss
{
    public class BossTurret : MonoBehaviour
    {
        public UnityEvent<Vector2> OnAimTurret = new UnityEvent<Vector2>();
        public UnityEvent OnShoot = new UnityEvent();

        [SerializeField] private float _accuracy = 3;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector2 _fireTime = new Vector2(4, 6);

        private bool _lockTurret = true;
        private float _fireTimer;
        private bool _canShoot;
        private float _fireTimeMultiplier = 1;

        private void Start()
        {
            _fireTimer = RandomFloat.MinMax(_fireTime);
            OnAimTurret.Invoke(AimDownwards());
            if (_playerTransform == null) {
                _playerTransform = FindObjectOfType<PlayerTank>().transform;
            }
        }

        private void Update()
        {
            if (_lockTurret) return;
            SetAimPosition();
        }

        public void SetLockTurret(bool active)
        {
            _lockTurret = active;
        }

        public void Escalate()
        {
            _fireTimeMultiplier /= 2;
        }

        public void SetCanShoot(bool active)
        {
            _canShoot = active;
            OnAimTurret.Invoke(AimDownwards());
        }

        private void SetAimPosition()
        {
            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0) {
                _fireTimer = RandomFloat.MinMax(_fireTime) * _fireTimeMultiplier;
                if (_canShoot) {
                    OnAimTurret.Invoke(NewAimPosition());
                    OnShoot.Invoke();
                }
            }
        }

        private Vector2 NewAimPosition()
        {
            Vector3 playerPos = _playerTransform.position;
            Vector2 accuratePos = new Vector2(playerPos.x, playerPos.z);

            Vector2 offset = Random.insideUnitCircle * _accuracy * _fireTimeMultiplier;

            return accuratePos + offset;
        }

        private Vector2 AimDownwards()
        {
            return new Vector2(transform.position.x, -10);
        }
    }
}