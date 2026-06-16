using System;
using LuoLuoTrip.Audio;
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
        [SerializeField] private float _attackRecovery = 0.3f;
        [SerializeField] private float _dodgeInvulnerableDuration = 0.3f;

        private CharacterEntity _entity;
        private CharacterMovementMotor _motor;
        private float _currentHealth;
        private float _currentStamina;
        private float _currentPoise;
        private float _stateTimer;
        private float _attackCooldownTimer;
        private float _dodgeInvulnerableTimer;
        private Vector3 _dodgeDirection;
        private Collider _hitCollider;
        private Combatant _pendingAttackTarget;
        private bool _activeHitConsumed;
        private float _attackMultiplier = 1f;
        private float _lastHitDamage;
        private bool _showAttackDebug;

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
        public bool IsInvulnerable => _state == CombatState.Dodging || _dodgeInvulnerableTimer > 0f;
        public bool AutoTickEnabled { get; set; } = true;
        public float SyncAssistAttackBonus { get; set; }
        public float SyncAssistDefenseBonus { get; set; }
        public float AttackRecovery => _attackRecovery;
        public float AttackWindup => _attackWindup;
        public float AttackActive => _attackActive;
        public float StateTimer => _stateTimer;
        public Combatant PendingAttackTarget => _pendingAttackTarget;
        public bool IsAttackActiveWindow => _state == CombatState.Attacking;
        public float LastHitDamage => _lastHitDamage;
        public bool ShowAttackDebug { get => _showAttackDebug; set => _showAttackDebug = value; }

        public void ApplyTuning(CombatTuningConfigSO config)
        {
            if (config == null) return;
            _attackWindup = config.playerAttackWindup;
            _attackActive = config.playerAttackActive;
            _attackRecovery = config.playerAttackRecovery;
            _dodgeDuration = config.dodgeDuration;
            _dodgeDistance = config.dodgeDistance;
            _dodgeInvulnerableDuration = config.dodgeInvulnerableDuration;
            _staggerDuration = config.staggerDuration;
        }

        private void Awake()
        {
            _entity = GetComponent<CharacterEntity>();
            _motor = GetComponent<CharacterMovementMotor>();
            _hitCollider = GetComponentInChildren<Collider>();
            InitializeFromCharacter();
        }

        private void Start()
        {
            var tuning = CombatTuningConfigSO.LoadOrDefault();
            ApplyTuning(tuning);
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
            RestoreRuntimeState(_stats.maxHealth, _stats.maxStamina, _stats.maxPoise);
        }

        public void InitializeForTests(CombatStats stats, float? health = null, float? stamina = null, float? poise = null)
        {
            _stats = stats;
            RestoreRuntimeState(
                health ?? stats.maxHealth,
                stamina ?? stats.maxStamina,
                poise ?? stats.maxPoise);
        }

        public void RestoreRuntimeState(float health, float stamina, float poise)
        {
            _currentHealth = health >= 0 ? health : _stats.maxHealth;
            _currentStamina = stamina >= 0 ? stamina : _stats.maxStamina;
            _currentPoise = poise >= 0 ? poise : _stats.maxPoise;
            _attackCooldownTimer = 0f;
            _stateTimer = 0f;
            _dodgeInvulnerableTimer = 0f;

            if (_currentHealth <= 0f)
                SetState(CombatState.Dead);
            else
                SetState(CombatState.Idle);
        }

        public void Tick(float deltaTime)
        {
            if (!IsAlive || deltaTime <= 0f) return;

            RecoverResources(deltaTime);
            UpdateStateTimer(deltaTime);
            _attackCooldownTimer = Math.Max(0f, _attackCooldownTimer - deltaTime);
            _dodgeInvulnerableTimer = Math.Max(0f, _dodgeInvulnerableTimer - deltaTime);
        }

        private void Update()
        {
            if (!AutoTickEnabled) return;
            Tick(Time.deltaTime);
        }

        public bool TryLightAttack(Combatant target)
        {
            if (!CanAct() || _attackCooldownTimer > 0f) return false;
            if (_currentStamina < 10f) return false;

            _currentStamina -= 10f;
            SetState(CombatState.AttackWindup);
            _stateTimer = _attackWindup;
            _attackCooldownTimer = _stats.attackCooldown + _attackWindup + _attackActive + _attackRecovery;

            // Cache target + reset hit dedup; resolve damage later when entering active window.
            _pendingAttackTarget = target;
            _activeHitConsumed = false;
            _attackMultiplier = 1f;

            AudioFeedbackService.Play(AudioEventId.AttackStart, transform.position);
            return true;
        }

        /// <summary>
        /// Resolve a single hit on the cached target during the Attacking active window.
        /// Idempotent: calling twice in the same attack does nothing the second time.
        /// </summary>
        private void ResolveActiveHitOnce()
        {
            if (_activeHitConsumed) return;
            _activeHitConsumed = true;

            var target = _pendingAttackTarget;
            if (target == null || !target.IsAlive)
            {
                if (_showAttackDebug)
                    Debug.Log($"[Combatant] {name} attack MISS (no live target)");
                OnHitLanded?.Invoke(new CombatHitEvent { Attacker = this, Defender = target, Result = default });
                return;
            }

            if (!IsInRange(target))
            {
                if (_showAttackDebug)
                    Debug.Log($"[Combatant] {name} attack MISS (out of range)");
                OnHitLanded?.Invoke(new CombatHitEvent { Attacker = this, Defender = target, Result = default });
                return;
            }

            var result = DamageCalculator.Calculate(this, target, _attackMultiplier);
            _lastHitDamage = result.finalDamage;
            var hitEvent = new CombatHitEvent
            {
                Attacker = this,
                Defender = target,
                Result = result
            };
            target.NotifyHitReceived(hitEvent);
            OnHitLanded?.Invoke(hitEvent);
            AudioFeedbackService.Play(AudioEventId.Hit, target.transform.position);

            if (_showAttackDebug)
                Debug.Log($"[Combatant] {name} HIT {target.name} dmg={result.finalDamage:F1} fatal={result.wasFatal}");
        }

        public bool TryDodge(Vector3 direction)
        {
            if (!CanAct()) return false;
            if (_currentStamina < _stats.dodgeStaminaCost) return false;

            _currentStamina -= _stats.dodgeStaminaCost;
            _dodgeDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
            SetState(CombatState.Dodging);
            _stateTimer = _dodgeDuration;
            _dodgeInvulnerableTimer = _dodgeInvulnerableDuration;
            AudioFeedbackService.Play(AudioEventId.Dodge, transform.position);
            return true;
        }

        public void NotifyHitReceived(CombatHitEvent hitEvent)
        {
            OnHitReceived?.Invoke(hitEvent);
        }

        public void AnimEvent_OnAttackActive()
        {
            if (_state == CombatState.AttackWindup || _state == CombatState.Attacking)
                ResolveActiveHitOnce();
        }

        public void AnimEvent_OnAttackEnd()
        {
            if (_state == CombatState.Attacking)
                SetState(CombatState.AttackRecovery);
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
            AudioFeedbackService.Play(AudioEventId.Stagger, transform.position);
        }

        private void Die()
        {
            _currentHealth = 0f;
            _pendingAttackTarget = null;
            SetState(CombatState.Dead);
            if (_entity?.Data != null)
                _entity.Data.IsAlive = false;
            OnDeath?.Invoke(this);
        }

        private void RecoverResources(float deltaTime)
        {
            if (_state == CombatState.Attacking || _state == CombatState.AttackWindup || _state == CombatState.AttackRecovery) return;

            _currentStamina = Math.Min(_stats.maxStamina,
                _currentStamina + _stats.staminaRecoveryPerSecond * deltaTime);

            if (_state != CombatState.Staggered)
            {
                _currentPoise = Math.Min(_stats.maxPoise,
                    _currentPoise + _stats.poiseRecoveryPerSecond * deltaTime);
            }
        }

        private void UpdateStateTimer(float deltaTime)
        {
            if (_stateTimer <= 0f) return;

            _stateTimer -= deltaTime;

            if (_state == CombatState.Dodging)
            {
                var dodgeSpeed = _dodgeDistance / Mathf.Max(_dodgeDuration, 0.0001f);
                if (_motor == null)
                    _motor = GetComponent<CharacterMovementMotor>();
                if (_motor != null)
                    _motor.Dodge(_dodgeDirection, dodgeSpeed, deltaTime);
                else
                {
                    var delta = _dodgeDirection * (dodgeSpeed * deltaTime);
                    delta.y = 0f;
                    transform.position += delta;
                }
            }

            if (_stateTimer <= 0f && _state != CombatState.Dead)
            {
                if (_state == CombatState.AttackWindup)
                {
                    SetState(CombatState.Attacking);
                    _stateTimer = _attackActive;
                    ResolveActiveHitOnce();
                    return;
                }

                if (_state == CombatState.Attacking)
                {
                    SetState(CombatState.AttackRecovery);
                    _stateTimer = _attackRecovery;
                    _pendingAttackTarget = null;
                    return;
                }

                if (_state == CombatState.AttackRecovery)
                {
                    SetState(CombatState.Idle);
                    return;
                }

                SetState(CombatState.Idle);
            }
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
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * Mathf.Max(0.5f, _stats.attackRange));
        }

        private void OnDrawGizmos()
        {
            if (!_showAttackDebug || !Application.isPlaying) return;
            switch (_state)
            {
                case CombatState.AttackWindup:
                    Gizmos.color = new Color(1f, 0.7f, 0f, 0.6f);
                    Gizmos.DrawWireSphere(transform.position, _stats.attackRange);
                    break;
                case CombatState.Attacking:
                    Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
                    Gizmos.DrawWireSphere(transform.position, _stats.attackRange);
                    break;
                case CombatState.AttackRecovery:
                    Gizmos.color = new Color(0.4f, 0.4f, 1f, 0.5f);
                    Gizmos.DrawWireSphere(transform.position, _stats.attackRange);
                    break;
            }
        }
    }
}
