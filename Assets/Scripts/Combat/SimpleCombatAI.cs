using UnityEngine;

namespace LuoLuoTrip.Combat
{
    /// <summary>简易敌方 AI：巡逻、发现敌对目标后追击并攻击</summary>
    [RequireComponent(typeof(Combatant))]
    public class SimpleCombatAI : MonoBehaviour
    {
        [SerializeField] private float _detectRange = 12f;
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _attackInterval = 1.5f;
        [SerializeField] private float _patrolRadius = 5f;

        private Combatant _self;
        private CharacterEntity _entity;
        private Combatant _target;
        private Vector3 _spawnPoint;
        private float _attackTimer;
        private float _thinkTimer;

        private void Awake()
        {
            _self = GetComponent<Combatant>();
            _entity = GetComponent<CharacterEntity>();
            _spawnPoint = transform.position;
        }

        private void Update()
        {
            if (!_self.IsAlive) return;

            _thinkTimer -= Time.deltaTime;
            if (_thinkTimer > 0f) return;
            _thinkTimer = 0.25f;

            if (_target == null || !_target.IsAlive)
                _target = FindNearestHostile();

            if (_target == null)
            {
                IdlePatrol();
                return;
            }

            var dist = Vector3.Distance(transform.position, _target.transform.position);
            if (dist > _detectRange * 1.5f)
            {
                _target = null;
                return;
            }

            ChaseAndAttack(dist);
        }

        private void IdlePatrol()
        {
            if (_self.State != CombatState.Idle) return;

            var offset = _spawnPoint - transform.position;
            offset.y = 0f;
            if (offset.magnitude > _patrolRadius)
            {
                var dir = offset.normalized;
                transform.position -= dir * (_chaseSpeed * 0.5f * Time.deltaTime);
            }
        }

        private void ChaseAndAttack(float distance)
        {
            if (_self.State != CombatState.Idle) return;

            var dir = (_target.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);

            if (distance > _self.Stats.attackRange)
            {
                transform.position += dir.normalized * (_chaseSpeed * Time.deltaTime);
                return;
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _self.TryLightAttack(_target);
                _attackTimer = _attackInterval;
            }
        }

        private Combatant FindNearestHostile()
        {
            Combatant nearest = null;
            var best = _detectRange;

            foreach (var other in FindObjectsByType<Combatant>(FindObjectsSortMode.None))
            {
                if (other == _self || !other.IsAlive) continue;
                if (_entity != null && other.CharacterEntity != null && !_entity.IsHostileTo(other.CharacterEntity))
                    continue;

                var dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist < best)
                {
                    best = dist;
                    nearest = other;
                }
            }
            return nearest;
        }
    }
}
