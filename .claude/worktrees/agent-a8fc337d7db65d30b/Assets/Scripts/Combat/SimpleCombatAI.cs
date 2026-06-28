using System;
using LuoLuoTrip.AI;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Feedback;
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
        [SerializeField] private float _stopDistance = 1.5f;

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
        [SerializeField] private float _attackWindupDelay = 0.4f;
        [SerializeField] private bool _showAttackIndicator = true;

        [Header("Behavior Profile")]
        [SerializeField] private AIBehaviorProfileSO behaviorProfile;
        [SerializeField] private Transform homePositionSource;
        [SerializeField] private Transform protectedTarget;

        private Combatant _self;
        private CharacterEntity _entity;
        private CharacterMovementMotor _motor;
        private Combatant _target;
        private Vector3 _spawnPoint;
        private float _attackTimer;
        private float _thinkTimer;
        private float _targetRefreshTimer;
        private float _attackIntervalOffset;
        private bool _isWindingUp;
        private bool _showWindupMarker;
        private AICombatNavigationController _navController;
        private Vector3 _homePosition;
        private bool _profileDecisionLogged;

        public Func<Combatant[]> CombatantQuery { get; set; }
        public Combatant CurrentTarget => _target;
        public Transform FollowTarget { get; set; }
        public Vector3? HoldPosition { get; set; }
        private Combatant _forcedAttackTarget;
        public Combatant ForcedAttackTarget
        {
            get
            {
                if (_forcedAttackTarget == null) return null;
                if (!_forcedAttackTarget.IsAlive || _forcedAttackTarget.CharacterEntity?.Data?.IsAlive == false)
                {
                    _forcedAttackTarget = null;
                    return null;
                }
                return _forcedAttackTarget;
            }
            set => _forcedAttackTarget = value;
        }
        public Transform DefendTarget { get; private set; }
        public Vector3? DefendPosition { get; private set; }
        public float DefendRadius { get; private set; }
        public float DefendLeashRadius { get; private set; }
        public string CommanderCommandStatus { get; private set; }
        public AIBehaviorProfileSO BehaviorProfile
        {
            get => behaviorProfile;
            set
            {
                behaviorProfile = value;
                RefreshBehaviorDiagnostics("Profile assigned");
            }
        }
        public Transform HomePositionSource { get => homePositionSource; set => homePositionSource = value; }
        public Transform ProtectedTarget { get => protectedTarget; set => protectedTarget = value; }
        public string CurrentBehaviorLabel { get; private set; } = "Default AI";
        public string LastProfileDecision { get; private set; } = "Default AI behavior";
        public float EffectiveDefendRadius => behaviorProfile != null ? behaviorProfile.EffectiveDefendRadius(DefendRadius > 0f ? DefendRadius : 5f) : DefendRadius;
        public float EffectiveMaxChaseDistance => behaviorProfile != null ? behaviorProfile.EffectiveMaxChaseDistance : 0f;
        public bool RespondsToTacticalCommand => behaviorProfile == null || behaviorProfile.respondsToTacticalCommand;
        public bool RespondsToDefendObjective => behaviorProfile == null || behaviorProfile.respondsToDefendObjective;
        public bool RespondsToFocusFire => behaviorProfile == null || behaviorProfile.respondsToFocusFire;
        public bool CanInitiateCombat => behaviorProfile == null || behaviorProfile.canInitiateCombat;
        public bool IsWindingUp => _isWindingUp;
        public AICombatNavigationController NavController => _navController;
        public float StopDistance => EffectiveStopDistance;
        public float EffectiveStopDistance
        {
            get
            {
                EnsureReferences();
                var attackRange = _self != null ? _self.Stats.attackRange : CombatStats.CreateDefault().attackRange;
                return _stopDistance > 0f ? _stopDistance : Mathf.Max(0.5f, attackRange * 0.8f);
            }
        }

        private void EnsureReferences()
        {
            if (_self == null) _self = GetComponent<Combatant>();
            if (_entity == null) _entity = GetComponent<CharacterEntity>();
            if (_motor == null) _motor = GetComponent<CharacterMovementMotor>();
            if (_motor == null) _motor = gameObject.AddComponent<CharacterMovementMotor>();
            if (_navController == null) _navController = GetComponent<AICombatNavigationController>();
        }

        public void ApplyTuning(CombatTuningConfigSO config)
        {
            if (config == null) return;
            _attackWindupDelay = config.aiAttackWindupDelay;
            _chaseSpeed = config.aiChaseSpeed;
            _stopDistance = config.aiStopDistance;
            _self?.ApplyTuning(config);
        }

        public void SetDefendObjective(Transform target, float radius)
        {
            if (behaviorProfile != null && !behaviorProfile.respondsToDefendObjective)
            {
                CommanderCommandStatus = $"{behaviorProfile.DisplayLabel} ignores DefendObjective";
                SetProfileDecision(CommanderCommandStatus);
                Debug.Log($"[AICommand] {name} ignored DefendObjective (profile={behaviorProfile.DisplayLabel})");
                return;
            }

            DefendTarget = target;
            DefendPosition = target != null ? target.position : (Vector3?)null;
            DefendRadius = Mathf.Max(1f, behaviorProfile != null ? behaviorProfile.EffectiveDefendRadius(radius) : radius);
            DefendLeashRadius = DefendRadius + Mathf.Max(EffectiveStopDistance, 2f);
            FollowTarget = null;
            HoldPosition = null;
            CommanderCommandStatus = target != null ? $"Defending {target.name}" : "Defend objective missing";
            Debug.Log($"[AICommand] Defending objective: {(target != null ? target.name : "None")}");
        }

        public void ClearDefendObjective()
        {
            DefendTarget = null;
            DefendPosition = null;
            DefendRadius = 0f;
            DefendLeashRadius = 0f;
            if (CommanderCommandStatus != null && CommanderCommandStatus.StartsWith("Defending"))
                CommanderCommandStatus = null;
        }

        public void SetFocusFireTarget(Combatant target)
        {
            if (target != null && behaviorProfile != null && (!behaviorProfile.respondsToFocusFire || !behaviorProfile.canInitiateCombat))
            {
                CommanderCommandStatus = $"{behaviorProfile.DisplayLabel} ignores FocusFire";
                SetProfileDecision(CommanderCommandStatus);
                Debug.Log($"[AICommand] {name} ignored FocusFire (profile={behaviorProfile.DisplayLabel})");
                return;
            }

            ForcedAttackTarget = target;
            CommanderCommandStatus = target != null ? $"FocusFire target: {target.name}" : null;
            if (target != null)
                Debug.Log($"[AICommand] FocusFire target: {target.name}");
        }

        public void ClearCommanderCommands()
        {
            FollowTarget = null;
            HoldPosition = null;
            ForcedAttackTarget = null;
            ClearDefendObjective();
            CommanderCommandStatus = null;
            if (_navController != null)
                _navController.ClearNavigation();
        }

        private void Awake()
        {
            EnsureReferences();
            _spawnPoint = transform.position;
            _homePosition = homePositionSource != null ? homePositionSource.position : transform.position;
            RefreshBehaviorDiagnostics("Initialized");
            _attackIntervalOffset = UnityEngine.Random.Range(-_attackIntervalVariance, _attackIntervalVariance);
            CombatantQuery = CombatantQuery ?? (() => FindObjectsOfType<Combatant>());

            _navController = GetComponent<AICombatNavigationController>();
            if (_navController == null)
                _navController = gameObject.AddComponent<AICombatNavigationController>();

            _self.OnStateChanged += HandleCombatantStateChanged;
        }

        private void OnDestroy()
        {
            if (_self != null)
                _self.OnStateChanged -= HandleCombatantStateChanged;
        }

        private void HandleCombatantStateChanged(CombatState newState)
        {
            if (newState == CombatState.AttackRecovery || newState == CombatState.Idle ||
                newState == CombatState.Staggered || newState == CombatState.Dead)
            {
                _showWindupMarker = false;
                UpdateWindupMarker(false);
            }
        }

        private void Start()
        {
            var tuning = CombatTuningConfigSO.LoadOrDefault();
            ApplyTuning(tuning);

            var audio = AudioFeedbackService.Instance;
            if (audio != null)
                audio.SetThrottle(AudioEventId.AIWindupWarning, 0.5f);
        }

        private void Update()
        {
            if (!_self.IsAlive) return;

            _navController.Tick(Time.deltaTime);

            if (FollowTarget != null)
            {
                ExecuteFollow();
                return;
            }

            if (HoldPosition.HasValue)
            {
                ExecuteHoldPosition();
                return;
            }

            if (DefendTarget != null || DefendPosition.HasValue)
            {
                ExecuteDefendObjective();
                return;
            }

            TickTimers();
            if (_thinkTimer > 0f) return;
            _thinkTimer = _thinkInterval;

            if (ForcedAttackTarget != null && ForcedAttackTarget.IsAlive && CanInitiateCombat)
            {
                _target = ForcedAttackTarget;
                SetProfileDecision("Focus fire target");
            }
            else
            {
                ForcedAttackTarget = null;
                if (!CanInitiateCombat)
                    ExecuteNonCombatantBehavior();
                RefreshTargetIfNeeded();
            }

            if (_target == null)
            {
                IdlePatrol();
                return;
            }

            if (IsBeyondProfileChaseLimit(_target.transform.position))
            {
                SetProfileDecision("Returning home: chase limit reached");
                _target = null;
                _navController.MoveToPosition(_homePosition);
                return;
            }

            var distance = Vector3.Distance(transform.position, _target.transform.position);
            if (distance > _detectRange * _disengageDistanceMultiplier)
            {
                _target = null;
                _navController.ClearNavigation();
                return;
            }

            ExecuteIntent(ChooseIntent(distance), distance);
        }

        private void ExecuteFollow()
        {
            if (FollowTarget == null) return;

            var direction = FollowTarget.position - transform.position;
            direction.y = 0f;
            var dist = direction.magnitude;

            if (dist > Mathf.Max(EffectiveStopDistance, 2.5f))
            {
                _navController.FollowTarget(FollowTarget);
            }
            else
            {
                _navController.StopNavigation();
            }
        }

        private void ExecuteHoldPosition()
        {
            if (!HoldPosition.HasValue) return;

            var offset = HoldPosition.Value - transform.position;
            offset.y = 0f;
            if (offset.magnitude > 1f)
                _navController.MoveToPosition(HoldPosition.Value);
            else
                _navController.StopNavigation();
        }

        private void ExecuteDefendObjective()
        {
            if (DefendTarget != null)
                DefendPosition = DefendTarget.position;

            if (!DefendPosition.HasValue)
            {
                ClearDefendObjective();
                return;
            }

            TickTimers();
            if (_thinkTimer > 0f) return;
            _thinkTimer = _thinkInterval;

            var anchor = DefendPosition.Value;
            var anchorOffset = anchor - transform.position;
            anchorOffset.y = 0f;

            if (ForcedAttackTarget != null && ForcedAttackTarget.IsAlive && IsInsideDefendLeash(ForcedAttackTarget.transform.position))
                _target = ForcedAttackTarget;
            else
            {
                ForcedAttackTarget = null;
                _target = SelectBestHostileTargetNear(anchor, Mathf.Max(EffectiveDefendRadius, _detectRange), true);
            }

            if (_target != null && _target.IsAlive)
            {
                if (IsBeyondProfileChaseLimit(_target.transform.position) || !IsInsideDefendLeash(_target.transform.position))
                {
                    _target = null;
                    _navController.MoveToPosition(anchor);
                    CommanderCommandStatus = "Returning to defend objective";
                    return;
                }

                var distance = Vector3.Distance(transform.position, _target.transform.position);
                CommanderCommandStatus = "Engaging threat";
                ExecuteIntent(ChooseIntent(distance), distance);
                return;
            }

            if (anchorOffset.magnitude > Mathf.Max(1f, EffectiveDefendRadius * 0.5f))
            {
                CommanderCommandStatus = "Moving to defend objective";
                _navController.MoveToPosition(anchor);
            }
            else
            {
                CommanderCommandStatus = "Holding defend objective";
                _navController.StopNavigation();
            }
        }

        private bool IsInsideDefendLeash(Vector3 position)
        {
            if (!DefendPosition.HasValue) return false;
            var offset = position - DefendPosition.Value;
            offset.y = 0f;
            return offset.magnitude <= Mathf.Max(EffectiveDefendRadius, DefendLeashRadius);
        }

        private Combatant SelectBestHostileTargetNear(Vector3 anchor, float radius, bool defendAnchor = false)
        {
            Combatant bestTarget = null;
            var bestScore = float.MinValue;
            foreach (var other in CombatantQuery())
            {
                if (defendAnchor)
                {
                    if (!IsValidHostileForDefend(other, anchor, radius)) continue;
                }
                else if (!IsValidHostile(other)) continue;

                var anchorOffset = other.transform.position - anchor;
                anchorOffset.y = 0f;
                if (anchorOffset.magnitude > radius) continue;
                var score = ScoreTarget(other);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = other;
                }
            }
            return bestTarget;
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
                _navController.MoveToPosition(_spawnPoint);
        }

        private CombatIntent ChooseIntent(float distance)
        {
            if (_self.State != CombatState.Idle)
                return CombatIntent.Recover;

            if (IsLowOnStamina())
                return distance <= _self.Stats.attackRange ? CombatIntent.Recover : CombatIntent.Reposition;

            if (IsCriticalHealth() && (_target.State == CombatState.Attacking || _target.State == CombatState.AttackWindup) && distance <= _self.Stats.attackRange + _repositionDistance)
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
                    _navController.ChaseTarget(_target.transform, EffectiveStopDistance);
                    break;
                case CombatIntent.Attack:
                    _navController.StopNavigation();
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
            if (_motor == null)
                _motor = GetComponent<CharacterMovementMotor>();
            if (_motor != null)
                _motor.MoveDirection(direction, speed, Time.deltaTime);
            else
                transform.position += direction * (speed * Time.deltaTime);
        }

        private void TryAttack()
        {
            if (_target == null || _attackTimer > 0f || !CanInitiateCombat) return;

            if (_isWindingUp)
            {
                _isWindingUp = false;
                // Marker stays visible through Combatant AttackWindup+Attacking;
                // cleared by HandleCombatantStateChanged on AttackRecovery/Idle.
                _self.TryLightAttack(_target);
                _attackTimer = GetAttackInterval();
                return;
            }

            if (_self.State == CombatState.AttackWindup || _self.State == CombatState.Attacking || _self.State == CombatState.AttackRecovery) return;

            _isWindingUp = true;
            _showWindupMarker = true;
            _attackTimer = _attackWindupDelay;
            AudioFeedbackService.Play(AudioEventId.AIWindupWarning, transform.position);
            UpdateWindupMarker(true);
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
            var hostile = _entity.IsHostileTo(other.CharacterEntity);
            if (!hostile && !(behaviorProfile != null && behaviorProfile.canAttackNeutral && IsProfilePreferredTarget(other))) return false;

            var distance = Vector3.Distance(transform.position, other.transform.position);
            return distance <= EffectiveDetectRange && !IsBeyondProfileChaseLimit(other.transform.position);
        }

        private bool IsValidHostileForDefend(Combatant other, Vector3 anchor, float anchorRadius)
        {
            if (other == null || other == _self || !other.IsAlive) return false;
            if (_entity == null || other.CharacterEntity == null) return false;
            var hostile = _entity.IsHostileTo(other.CharacterEntity);
            if (!hostile && !(behaviorProfile != null && behaviorProfile.canAttackNeutral && IsProfilePreferredTarget(other))) return false;

            var anchorOffset = other.transform.position - anchor;
            anchorOffset.y = 0f;
            return anchorOffset.magnitude <= Mathf.Max(anchorRadius, DefendLeashRadius);
        }

        private float ScoreTarget(Combatant other)
        {
            var distance = Vector3.Distance(transform.position, other.transform.position);
            var distanceScore = Mathf.Clamp01(1f - distance / _detectRange) * 5f;
            var healthScore = Mathf.Clamp01(1f - other.CurrentHealth / Mathf.Max(1f, other.Stats.maxHealth)) * _focusLowHealthWeight;
            var staggeredScore = other.State == CombatState.Staggered ? _focusStaggeredWeight : 0f;
            var attackingScore = other.State == CombatState.Attacking || other.State == CombatState.AttackWindup ? _focusAttackingWeight : 0f;
            var rangeScore = Mathf.Clamp01((_self.Stats.attackRange - distance + 1f) / Mathf.Max(1f, _self.Stats.attackRange + 1f)) * _focusRangeWeight;

            var baseScore = distanceScore + healthScore + staggeredScore + attackingScore + rangeScore;

            if (behaviorProfile == null)
                return baseScore;

            var score = AITargetSelectionUtility.ScoreTarget(
                _self,
                other,
                behaviorProfile,
                null,
                protectedTarget,
                transform.position,
                EffectiveDetectRange,
                _entity != null && other.CharacterEntity != null && _entity.IsHostileTo(other.CharacterEntity),
                AITargetSelectionUtility.IsObjectiveLike(other),
                IsProtectedTarget(other),
                IsNeutralTarget(other));

            if (score.IsValid)
            {
                SetProfileDecision(score.Reason);
                return baseScore + score.Score;
            }

            return float.MinValue;
        }

        private float EffectiveDetectRange => behaviorProfile != null ? behaviorProfile.EffectiveChaseRadius(_detectRange) : _detectRange;

        private bool IsProtectedTarget(Combatant other)
        {
            if (other == null) return false;
            return protectedTarget != null && other.transform == protectedTarget
                || AITargetSelectionUtility.IsObjectiveLike(other);
        }

        private bool IsNeutralTarget(Combatant other)
        {
            if (other == null || _entity == null || other.CharacterEntity == null) return false;
            return !_entity.IsHostileTo(other.CharacterEntity) && _entity.Data != null && other.CharacterEntity.Data != null
                && _entity.Data.Faction != other.CharacterEntity.Data.Faction;
        }

        private bool IsProfilePreferredTarget(Combatant other)
        {
            return IsProtectedTarget(other) || AITargetSelectionUtility.IsNegotiatorLike(other);
        }

        private bool IsBeyondProfileChaseLimit(Vector3 targetPosition)
        {
            if (behaviorProfile == null || behaviorProfile.EffectiveMaxChaseDistance <= 0f) return false;
            var origin = homePositionSource != null ? homePositionSource.position : _homePosition;
            var offset = targetPosition - origin;
            offset.y = 0f;
            return offset.magnitude > behaviorProfile.EffectiveMaxChaseDistance;
        }

        private void ExecuteNonCombatantBehavior()
        {
            if (behaviorProfile == null) return;

            if (behaviorProfile.canRetreat && _self != null && _self.CurrentHealth <= _self.Stats.maxHealth * behaviorProfile.retreatHealthRatio)
            {
                var retreatDirection = transform.position - (protectedTarget != null ? protectedTarget.position : _homePosition);
                retreatDirection.y = 0f;
                if (retreatDirection.sqrMagnitude <= 0.01f) retreatDirection = -transform.forward;
                _navController.MoveToPosition(transform.position + retreatDirection.normalized * Mathf.Max(2f, EffectiveStopDistance));
                SetProfileDecision("Retreating");
            }
            else if (behaviorProfile.holdPositionWhenNoTarget)
            {
                _navController.StopNavigation();
                SetProfileDecision("Holding");
            }
        }

        private void RefreshBehaviorDiagnostics(string decision)
        {
            CurrentBehaviorLabel = behaviorProfile != null ? behaviorProfile.DisplayLabel : "Default AI";
            SetProfileDecision(decision);
        }

        private void SetProfileDecision(string decision)
        {
            LastProfileDecision = string.IsNullOrEmpty(decision) ? (behaviorProfile != null ? behaviorProfile.DisplayLabel : "Default AI behavior") : decision;
            if (behaviorProfile != null && !_profileDecisionLogged)
            {
                Debug.Log($"[AIProfile] {name}: {behaviorProfile.DisplayLabel}");
                _profileDecisionLogged = true;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showAttackIndicator || !Application.isPlaying) return;

            if (_showWindupMarker || _self.State == CombatState.AttackWindup || _self.State == CombatState.Attacking)
            {
                Gizmos.color = _self.State == CombatState.Attacking ? Color.red : new Color(1f, 0.5f, 0f);
                Gizmos.DrawWireSphere(transform.position, _self.Stats.attackRange);
            }
        }

        private void UpdateWindupMarker(bool active)
        {
            var service = WorldMarkerService.Instance;
            if (service == null) return;
            if (active)
                service.AttachMarker(gameObject, WorldMarkerType.AIWindupWarning);
            else
                service.DetachMarker(gameObject);
        }

        private void OnDisable()
        {
            _showWindupMarker = false;
            UpdateWindupMarker(false);
            if (_navController != null)
                _navController.ClearNavigation();
        }

        private void OnGUI()
        {
            if (!_showAttackIndicator || !_showWindupMarker) return;

            var cam = Camera.main;
            if (cam == null) return;
            var screenPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.2f);
            if (screenPos.z <= 0f) return;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            var rect = new Rect(screenPos.x - 20f, Screen.height - screenPos.y - 12f, 40f, 24f);
            GUI.Label(rect, "!", style);
        }
    }
}
