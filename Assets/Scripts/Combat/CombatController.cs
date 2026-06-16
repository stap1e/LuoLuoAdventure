using UnityEngine;

namespace LuoLuoTrip.Combat
{
    [RequireComponent(typeof(Combatant))]
    public class CombatController : MonoBehaviour
    {
        [SerializeField] private float _lockOnRange = 15f;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode _dodgeKey = KeyCode.Space;
        [SerializeField] private KeyCode _lockToggleKey = KeyCode.Q;
        [SerializeField] private KeyCode _lockSwitchKey = KeyCode.Tab;
        [SerializeField] private bool _debugInput;

        private Combatant _self;
        private CharacterEntity _entity;
        private CharacterMovementMotor _motor;
        private Combatant _lockTarget;
        private Camera _camera;
        private bool _inputEnabled = true;
        private bool _moveSpeedWarned;
        private float _debugLogTimer;
        private Vector3 _lastPosition;

        public bool IsInputEnabled => _inputEnabled;
        public float MoveSpeed => _moveSpeed;
        public bool IsExternallyControlled { get; set; }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
        }

        private void Awake()
        {
            _self = GetComponent<Combatant>();
            _entity = GetComponent<CharacterEntity>();
            _motor = GetComponent<CharacterMovementMotor>();
            if (_motor == null)
                _motor = gameObject.AddComponent<CharacterMovementMotor>();
            _camera = Camera.main;
            _lastPosition = transform.position;
        }

        private void Update()
        {
            if (!_self.IsAlive) return;

            if (!_inputEnabled) return;

            HandleLockOn();
            HandleMovement();
            HandleCombatInput();
        }

        public Vector2 ReadMoveInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h == 0f && v == 0f)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            }

            return new Vector2(h, v);
        }

        public void ApplyMoveInput(Vector2 input)
        {
            if (_self.State != CombatState.Idle) return;

            var dir = new Vector3(input.x, 0f, input.y);
            if (dir.sqrMagnitude < 0.01f) return;

            var speed = _moveSpeed;
            if (speed <= 0f)
            {
                speed = 6f;
                if (!_moveSpeedWarned)
                {
                    Debug.LogWarning("[CombatController] Move speed is 0, using fallback 6f");
                    _moveSpeedWarned = true;
                }
            }

            var camForward = _camera != null ? _camera.transform.forward : Vector3.forward;
            var camRight = _camera != null ? _camera.transform.right : Vector3.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            var move = (camForward * dir.z + camRight * dir.x).normalized;
            if (_motor == null)
                _motor = GetComponent<CharacterMovementMotor>();
            if (_motor != null)
                _motor.MoveDirection(move, speed, Time.deltaTime);
            else
                transform.position += move * (speed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(move);
        }

        private void HandleMovement()
        {
            var input = ReadMoveInput();
            ApplyMoveInput(input);
            DebugLogInput(input);
        }

        private void HandleLockOn()
        {
            if (Input.GetKeyDown(_lockToggleKey))
            {
                _lockTarget = _lockTarget == null ? FindNearestHostile() : null;
            }

            if (Input.GetKeyDown(_lockSwitchKey) && _lockTarget != null)
                _lockTarget = FindNextHostile(_lockTarget);
        }

        private void HandleCombatInput()
        {
            if (Input.GetKeyDown(_attackKey))
            {
                var target = _lockTarget != null && _lockTarget.IsAlive ? _lockTarget : FindNearestHostile();
                _self.TryLightAttack(target);
            }

            if (Input.GetKeyDown(_dodgeKey))
            {
                var input = ReadMoveInput();
                var dir = input.sqrMagnitude > 0.01f ? new Vector3(input.x, 0f, input.y) : -transform.forward;
                _self.TryDodge(dir);
            }
        }

        private void DebugLogInput(Vector2 input)
        {
            if (!_debugInput) return;

            _debugLogTimer -= Time.deltaTime;
            if (_debugLogTimer > 0f) return;
            _debugLogTimer = 1f;

            var posDelta = transform.position - _lastPosition;
            _lastPosition = transform.position;

            Debug.Log($"[CombatController] input=({input.x:F1},{input.y:F1}) speed={_moveSpeed} " +
                      $"inputEnabled={_inputEnabled} state={_self.State} " +
                      $"rb={GetComponent<Rigidbody>() != null} posDelta={posDelta.magnitude:F3}");
        }

        private Combatant FindNearestHostile()
        {
            Combatant nearest = null;
            var bestDist = _lockOnRange;

            foreach (var other in FindObjectsOfType<Combatant>())
            {
                if (other == _self || !other.IsAlive) continue;
                if (!IsHostile(other)) continue;

                var dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = other;
                }
            }
            return nearest;
        }

        private Combatant FindNextHostile(Combatant current)
        {
            Combatant next = null;
            var bestAngle = float.MaxValue;
            var forward = (current.transform.position - transform.position).normalized;

            foreach (var other in FindObjectsOfType<Combatant>())
            {
                if (other == _self || other == current || !other.IsAlive) continue;
                if (!IsHostile(other)) continue;

                var toTarget = (other.transform.position - transform.position).normalized;
                var angle = Vector3.Angle(forward, toTarget);
                if (angle > 0f && angle < bestAngle)
                {
                    bestAngle = angle;
                    next = other;
                }
            }
            return next ?? FindNearestHostile();
        }

        private bool IsHostile(Combatant other)
        {
            if (_entity == null || other.CharacterEntity == null) return true;
            return _entity.IsHostileTo(other.CharacterEntity);
        }

        private void OnDrawGizmos()
        {
            if (_lockTarget == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, _lockTarget.transform.position + Vector3.up);
        }
    }
}
