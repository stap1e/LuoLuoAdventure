using UnityEngine;

namespace LuoLuoTrip
{
    public class CharacterMovementMotor : MonoBehaviour
    {
        [SerializeField] private float _groundY;
        [SerializeField] private bool _lockY = true;
        [SerializeField] private bool _captureGroundOnAwake = true;

        private Rigidbody _rigidbody;
        private bool _hasRigidbody;
        private bool _initialized;

        public float GroundY => _groundY;
        public bool LockY
        {
            get => _lockY;
            set => _lockY = value;
        }

        public Vector3 RootPosition => transform.position;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _hasRigidbody = _rigidbody != null;

            if (_captureGroundOnAwake)
                _groundY = transform.position.y;

            _initialized = true;
        }

        public void SetGroundY(float y)
        {
            _groundY = y;
        }

        public void Move(Vector3 worldDelta)
        {
            EnsureInitialized();

            worldDelta.y = 0f;
            if (worldDelta.sqrMagnitude < 1e-8f) return;

            var target = transform.position + worldDelta;
            ApplyPosition(target);
        }

        public void MoveDirection(Vector3 worldDirection, float speed, float deltaTime)
        {
            if (deltaTime <= 0f || speed <= 0f) return;
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 1e-6f) return;
            Move(worldDirection.normalized * (speed * deltaTime));
        }

        public bool MoveTowards(Vector3 target, float speed, float deltaTime)
        {
            EnsureInitialized();

            if (deltaTime <= 0f) return false;

            var diff = target - transform.position;
            diff.y = 0f;
            var dist = diff.magnitude;
            if (dist < 1e-4f) return true;

            var step = Mathf.Min(speed * deltaTime, dist);
            var next = transform.position + diff.normalized * step;
            ApplyPosition(next);
            return step >= dist - 1e-4f;
        }

        public void Dodge(Vector3 direction, float distancePerSecond, float deltaTime)
        {
            if (deltaTime <= 0f || distancePerSecond <= 0f) return;
            direction.y = 0f;
            if (direction.sqrMagnitude < 1e-6f) return;
            Move(direction.normalized * (distancePerSecond * deltaTime));
        }

        public void ClampToGroundPlane()
        {
            EnsureInitialized();
            if (!_lockY) return;
            var pos = transform.position;
            if (Mathf.Abs(pos.y - _groundY) > 1e-4f)
            {
                pos.y = _groundY;
                ApplyPositionRaw(pos);
            }
        }

        public void TeleportTo(Vector3 worldPosition)
        {
            EnsureInitialized();
            ApplyPosition(worldPosition);
        }

        private void ApplyPosition(Vector3 target)
        {
            if (_lockY)
                target.y = _groundY;
            ApplyPositionRaw(target);
        }

        private void ApplyPositionRaw(Vector3 target)
        {
            if (_hasRigidbody && _rigidbody != null && !_rigidbody.isKinematic)
            {
                _rigidbody.MovePosition(target);
            }
            else if (_hasRigidbody && _rigidbody != null && _rigidbody.isKinematic)
            {
                transform.position = target;
                _rigidbody.position = target;
            }
            else
            {
                transform.position = target;
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _rigidbody = GetComponent<Rigidbody>();
            _hasRigidbody = _rigidbody != null;
            if (_captureGroundOnAwake)
                _groundY = transform.position.y;
            _initialized = true;
        }
    }
}
