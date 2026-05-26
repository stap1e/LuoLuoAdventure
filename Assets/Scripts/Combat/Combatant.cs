using System;
using LuoLuoTrip.Combat.Feedback;
using UnityEngine;

namespace LuoLuoTrip.Combat
{
    public struct CombatHitEvent
    {
        public Combatant Attacker;
        public Combatant Defender;
        public DamageResult Result;
    }

    /// <summary>
    /// Soul-like 战斗组件原型：生命/耐力/架势、轻攻击、闪避、受击硬直。
    /// </summary>
    [RequireComponent(typeof(CharacterEntity))]
    public class Combatant : MonoBehaviour
    {
        [Header("Runtime")]
        [SerializeField] private CombatStats _stats;
        [SerializeField] private CombatState _state = CombatState.Idle;

        [Header("Tuning")]
        [SerializeField] private float _dodgeDuration = 0.35f;
        [SerializeField] private float _dodgeDistance = 4f;
        [SerializeField] private float _staggerDuration = 1.2f;
        [SerializeField] private float _attackWindup = 0.25f;
        [SerializeField] private float _attackActive = 0.2f;

        private CharacterEntity _entity;
        private float _currentHealth;
        private float _currentStamina;
        private float _currentPoise;
        private float _stateTimer;
        private float _attackCooldownTimer;
        private Vector3 _dodgeDirection;
        private Collider _hitCollider;

        public event Action<CombatHitEvent> OnHitLanded;
        public event Action<CombatHitEvent> OnHitReceived;
        public event Action<Combatant> OnDeath;
        public event Action<CombatState> OnStateChanged;

        public CharacterEntity CharacterEntity => _entity;
        public CombatStats Stats => _stats;
        public CombatState State => _state;
        public float CurrentHealth => _currentHealth;
        public float CurrentStamina => _currentStamina;
        public float CurrentPoise => _currentPoise;
        public bool IsAlive => _state != CombatState.Dead && _currentHealth > 0f;
        public bool IsInvulnerable => _state == CombatState.Dodging;

        private void Awake()
        {
            _entity = GetComponent<CharacterEntity>();
            _hitCollider = GetComponentInChildren<Collider>();
            InitializeFromCharacter();
        }

        private void Start()
        {
            CombatHitFeedbackHub.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            CombatHitFeedbackHub.Instance?.Unregister(this);
        }

        public void InitializeFromCharacter()
        {
            if (_entity?.Data == null) return;

            _stats = CombatStatsCalculator.Calculate(_entity.Data);
            _currentHealth = _stats.maxHealth;
            _currentStamina = _stats.maxStamina;
            _currentPoise = _stats.maxPoise;
            SetState(CombatState.Idle);
        }

        public void RestoreRuntimeState(float health, float stamina, float poise)
        {
            _currentHealth = health >= 0 ? health : _stats.maxHealth;
            _currentStamina = stamina >= 0 ? stamina : _stats.maxStamina;
            _currentPoise = poise >= 0 ? poise : _stats.maxPoise;

            if (_currentHealth <= 0f)
                SetState(CombatState.Dead);
            else
                SetState(CombatState.Idle);
        }

        private void Update()
        {
            if (!IsAlive) return;

            RecoverResources();
            UpdateStateTimer();
            _attackCooldownTimer = Math.Max(0f, _attackCooldownTimer - Time.deltaTime);
        }

        public bool TryLightAttack(Combatant target)
        {
            if (!CanAct() || _attackCooldownTimer > 0f) return false;
            if (_currentStamina < 10f) return false;

            _currentStamina -= 10f;
            SetState(CombatState.Attacking);
            _stateTimer = _attackWindup + _attackActive;
            _attackCooldownTimer = _stats.attackCooldown;

            if (target != null && IsInRange(target))
            {
                var result = DamageCalculator.Calculate(this, target);
                var hitEvent = new CombatHitEvent
                {
                    Attacker = this,
                    Defender = target,
                    Result = result
                };
                target.NotifyHitReceived(hitEvent);
                OnHitLanded?.Invoke(hitEvent);
            }

            return true;
        }

        public bool TryDodge(Vector3 direction)
        {
            if (!CanAct()) return false;
            if (_currentStamina < _stats.dodgeStaminaCost) return false;

            _currentStamina -= _stats.dodgeStaminaCost;
            _dodgeDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
            SetState(CombatState.Dodging);
            _stateTimer = _dodgeDuration;
            return true;
        }

        public void NotifyHitReceived(CombatHitEvent hitEvent)
        {
            OnHitReceived?.Invoke(hitEvent);
        }

        /// <summary>供 Animator Animation Event 调用：攻击判定帧</summary>
        public void AnimEvent_OnAttackActive() { }

        /// <summary>供 Animator Animation Event 调用：攻击动画结束</summary>
        public void AnimEvent_OnAttackEnd()
        {
            if (_state == CombatState.Attacking)
                SetState(CombatState.Idle);
        }

        public bool ApplyHealthDamage(float amount)
        {
            if (!IsAlive || IsInvulnerable) return false;

            _currentHealth = Math.Max(0f, _currentHealth - amount);
            if (_currentHealth <= 0f)
            {
                Die();
                return true;
            }
            return false;
        }

        public bool ApplyPoiseDamage(float amount)
        {
            if (!IsAlive || IsInvulnerable) return false;

            _currentPoise -= amount;
            if (_currentPoise <= 0f)
            {
                _currentPoise = 0f;
                EnterStagger();
                return true;
            }
            return false;
        }

        private void EnterStagger()
        {
            SetState(CombatState.Staggered);
            _stateTimer = _staggerDuration;
        }

        private void Die()
        {
            _currentHealth = 0f;
            SetState(CombatState.Dead);
            if (_entity?.Data != null)
                _entity.Data.IsAlive = false;
            OnDeath?.Invoke(this);
        }

        private void RecoverResources()
        {
            if (_state == CombatState.Attacking) return;

            _currentStamina = Math.Min(_stats.maxStamina,
                _currentStamina + _stats.staminaRecoveryPerSecond * Time.deltaTime);

            if (_state != CombatState.Staggered)
            {
                _currentPoise = Math.Min(_stats.maxPoise,
                    _currentPoise + _stats.poiseRecoveryPerSecond * Time.deltaTime);
            }
        }

        private void UpdateStateTimer()
        {
            if (_stateTimer <= 0f) return;

            _stateTimer -= Time.deltaTime;

            if (_state == CombatState.Dodging)
                transform.position += _dodgeDirection * (_dodgeDistance / _dodgeDuration * Time.deltaTime);

            if (_stateTimer <= 0f && _state != CombatState.Dead)
                SetState(CombatState.Idle);
        }

        private bool CanAct() =>
            IsAlive && _state is CombatState.Idle;

        private bool IsInRange(Combatant other)
        {
            if (other == null) return false;
            var dist = Vector3.Distance(transform.position, other.transform.position);
            return dist <= _stats.attackRange + 0.5f;
        }

        private void SetState(CombatState newState)
        {
            if (_state == newState) return;
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _stats.attackRange);
        }
    }
}
