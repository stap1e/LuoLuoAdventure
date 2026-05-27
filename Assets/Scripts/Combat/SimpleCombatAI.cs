using System;
using UnityEngine;

namespace LuoLuoTrip.Combat
{
    [RequireComponent(typeof(Combatant))]
    public class SimpleCombatAI : MonoBehaviour
    {
        private enum CombatIntent
        {
            Chase,
            Attack,
            Recover,
            Reposition
        }

        [Header("Awareness")]
        [SerializeField] private float _detectRange = 12f;
        [SerializeField] private float _targetRefreshInterval = 0.75f;
        [SerializeField] private float _disengageDistanceMultiplier = 1.5f;

        [Header("Movement")]
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _patrolRadius = 5f;
        [SerializeField] private float _repositionDistance = 1.5f;

        [Header("Decision")]
        [SerializeField] private float _thinkInterval = 0.25f;
        [SerializeField] private float _lowStaminaThreshold = 0.3f;
        [SerializeField] private float _criticalHealthThreshold = 0.25f;
        [SerializeField] private float _focusLowHealthWeight = 3f;
        [SerializeField] private float _focusStaggeredWeight = 2f;
        [SerializeField] private float _focusAttackingWeight = 1.5f;
        [SerializeField] private float _focusRangeWeight = 1f;

        [Header("Attack Rhythm")]
        [SerializeField] private float _attackIntervalVariance = 0.2f;
        [SerializeField] private float _lowStaminaAttackDelayMultiplier = 1.5f;

        private Combatant _self;
        private CharacterEntity _entity;
        private Combatant _target;
        private Vector3 _spawnPoint;
        private float _attackTimer;
        private float _thinkTimer;
        private float _targetRefreshTimer;
        private float _attackIntervalOffset;

        internal Func<Combatant[]> CombatantQuery { get; set; }
        internal Combatant CurrentTarget => _target;

        private void Awake()
        {
            _self = GetComponent<Combatant>();
            _entity = GetComponent<CharacterEntity>();
            _spawnPoint = transform.position;
            _attackIntervalOffset = UnityEngine.Random.Range(-_attackIntervalVariance, _attackIntervalVariance);
            CombatantQuery = CombatantQuery ?? (() => FindObjectsOfType<Combatant>());
        }

        private void Update()
        {
            if (!_self.IsAlive) return;

            TickTimers();
            if (_thinkTimer > 0f) return;
            _thinkTimer = _thinkInterval;

            RefreshTargetIfNeeded();
            if (_target == null)
            {
                IdlePatrol();
                return;
            }

            var distance = Vector3.Distance(transform.position, _target.transform.position);
            if (distance > _detectRange * _disengageDistanceMultiplier)
            {
                _target = null;
                return;
            }

            ExecuteIntent(ChooseIntent(distance), distance);
        }

        private void TickTimers()
        {
            _thinkTimer -= Time.deltaTime;
            _targetRefreshTimer -= Time.deltaTime;
            _attackTimer -= Time.deltaTime;
        }

        private void RefreshTargetIfNeeded()
        {
            if (_target != null && _target.IsAlive && _targetRefreshTimer > 0f) return;

            _target = SelectBestHostileTarget();
            _targetRefreshTimer = _targetRefreshInterval;
        }

        private void IdlePatrol()
        {
            if (_self.State != CombatState.Idle) return;

            var offset = _spawnPoint - transform.position;
            offset.y = 0f;
            if (offset.magnitude > _patrolRadius)
                MoveTowards(offset.normalized, _chaseSpeed * 0.5f);
        }

        private CombatIntent ChooseIntent(float distance)
        {
            if (_self.State != CombatState.Idle)
                return CombatIntent.Recover;

            if (IsLowOnStamina())
                return distance <= _self.Stats.attackRange ? CombatIntent.Recover : CombatIntent.Reposition;

            if (IsCriticalHealth() && _target.State == CombatState.Attacking && distance <= _self.Stats.attackRange + _repositionDistance)
                return CombatIntent.Reposition;

            if (distance <= _self.Stats.attackRange)
                return CombatIntent.Attack;

            return CombatIntent.Chase;
        }

        private void ExecuteIntent(CombatIntent intent, float distance)
        {
            switch (intent)
            {
                case CombatIntent.Chase:
                    FaceTarget();
                    MoveTowardsTarget();
                    break;
                case CombatIntent.Attack:
                    FaceTarget();
                    TryAttack();
                    break;
                case CombatIntent.Reposition:
                    FaceTarget();
                    Reposition(distance);
                    break;
                case CombatIntent.Recover:
                    FaceTarget();
                    break;
            }
        }

        private void FaceTarget()
        {
            if (_target == null) return;

            var direction = _target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        private void MoveTowardsTarget()
        {
            if (_target == null) return;

            var direction = _target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.01f) return;

            MoveTowards(direction.normalized, _chaseSpeed);
        }

        private void Reposition(float distance)
        {
            if (_target == null) return;

            var direction = transform.position - _target.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.01f)
                direction = -transform.forward;

            if (distance < _self.Stats.attackRange + _repositionDistance)
                MoveTowards(direction.normalized, _chaseSpeed * 0.8f);
        }

        private void MoveTowards(Vector3 direction, float speed)
        {
            transform.position += direction * (speed * Time.deltaTime);
        }

        private void TryAttack()
        {
            if (_target == null || _attackTimer > 0f) return;

            if (_self.TryLightAttack(_target))
                _attackTimer = GetAttackInterval();
        }

        private float GetAttackInterval()
        {
            var interval = Mathf.Max(0.2f, _self.Stats.attackCooldown + _attackIntervalOffset);
            if (IsLowOnStamina())
                interval *= _lowStaminaAttackDelayMultiplier;
            return interval;
        }

        private bool IsLowOnStamina() =>
            _self.CurrentStamina <= _self.Stats.maxStamina * _lowStaminaThreshold;

        private bool IsCriticalHealth() =>
            _self.CurrentHealth <= _self.Stats.maxHealth * _criticalHealthThreshold;

        private Combatant SelectBestHostileTarget()
        {
            Combatant bestTarget = null;
            var bestScore = float.MinValue;

            foreach (var other in CombatantQuery())
            {
                if (!IsValidHostile(other)) continue;

                var score = ScoreTarget(other);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = other;
                }
            }

            return bestTarget;
        }

        private bool IsValidHostile(Combatant other)
        {
            if (other == null || other == _self || !other.IsAlive) return false;
            if (_entity == null || other.CharacterEntity == null) return false;
            if (!_entity.IsHostileTo(other.CharacterEntity)) return false;

            var distance = Vector3.Distance(transform.position, other.transform.position);
            return distance <= _detectRange;
        }

        private float ScoreTarget(Combatant other)
        {
            var distance = Vector3.Distance(transform.position, other.transform.position);
            var distanceScore = Mathf.Clamp01(1f - distance / _detectRange) * 5f;
            var healthScore = Mathf.Clamp01(1f - other.CurrentHealth / Mathf.Max(1f, other.Stats.maxHealth)) * _focusLowHealthWeight;
            var staggeredScore = other.State == CombatState.Staggered ? _focusStaggeredWeight : 0f;
            var attackingScore = other.State == CombatState.Attacking ? _focusAttackingWeight : 0f;
            var rangeScore = Mathf.Clamp01((_self.Stats.attackRange - distance + 1f) / Mathf.Max(1f, _self.Stats.attackRange + 1f)) * _focusRangeWeight;

            return distanceScore + healthScore + staggeredScore + attackingScore + rangeScore;
        }
    }
}
