using UnityEngine;

namespace LuoLuoTrip.Combat
{
    /// <summary>玩家战斗控制：锁定、轻攻击、闪避</summary>
    [RequireComponent(typeof(Combatant))]
    public class CombatController : MonoBehaviour
    {
        [SerializeField] private float _lockOnRange = 15f;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode _dodgeKey = KeyCode.Space;
        [SerializeField] private KeyCode _lockToggleKey = KeyCode.Q;
        [SerializeField] private KeyCode _lockSwitchKey = KeyCode.Tab;

        private Combatant _self;
        private CharacterEntity _entity;
        private Combatant _lockTarget;
        private Camera _camera;

        private void Awake()
        {
            _self = GetComponent<Combatant>();
            _entity = GetComponent<CharacterEntity>();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!_self.IsAlive) return;

            HandleLockOn();
            HandleMovement();
            HandleCombatInput();
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

        private void HandleMovement()
        {
            if (_self.State != CombatState.Idle) return;

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude < 0.01f) return;

            var camForward = _camera != null ? _camera.transform.forward : Vector3.forward;
            var camRight = _camera != null ? _camera.transform.right : Vector3.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            var move = (camForward * input.z + camRight * input.x).normalized;
            transform.position += move * (_moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(move);
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
                var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
                var dir = input.sqrMagnitude > 0.01f ? input : -transform.forward;
                _self.TryDodge(dir);
            }
        }

        private Combatant FindNearestHostile()
        {
            Combatant nearest = null;
            var bestDist = _lockOnRange;

            foreach (var other in FindObjectsByType<Combatant>(FindObjectsSortMode.None))
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

            foreach (var other in FindObjectsByType<Combatant>(FindObjectsSortMode.None))
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
