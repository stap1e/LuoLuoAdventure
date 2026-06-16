using LuoLuoTrip.AI;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Feedback;
using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip
{
    public class CommanderControlController : MonoBehaviour
    {
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private KeyCode _releaseKey = KeyCode.R;
        [SerializeField] private float _syncAssistDuration = 3f;
        private CommanderTargetSelector _targetSelector;
        private CommanderControlRuntimeState _state;
        private CombatController _playerCombatController;
        private SimpleCombatAI _playerAI;
        private ControlPermissionService _permissionService;
        private CommanderDebugHud _debugHud;
        private CommanderControlHintPanel _hintPanel;
        private CameraFollowController _cameraFollow;
        private CharacterEntity _lastSelectedTarget;
        private CharacterEntity _lastControlledMarkerEntity;

        public CommanderControlRuntimeState State => _state;
        public CommanderTargetSelector TargetSelector => _targetSelector;

        public void SetHintPanel(CommanderControlHintPanel panel) => _hintPanel = panel;

        private void Awake()
        {
            _targetSelector = GetComponent<CommanderTargetSelector>();
            if (_targetSelector == null)
                _targetSelector = gameObject.AddComponent<CommanderTargetSelector>();

            _playerCombatController = GetComponent<CombatController>();
            _playerAI = GetComponent<SimpleCombatAI>();
        }

        private void Start()
        {
            _state = new CommanderControlRuntimeState
            {
                OriginalPlayerEntity = GetComponent<CharacterEntity>()
            };
            _state.DirectControlledEntity = _state.OriginalPlayerEntity;

            _permissionService = new ControlPermissionService();

            var tuning = CombatTuningConfigSO.LoadOrDefault();
            _syncAssistDuration = tuning.syncAssistDuration;

            _cameraFollow = FindObjectOfType<CameraFollowController>();

            var context = GameBootstrap.Context;
            if (context != null)
            {
                _debugHud = FindObjectOfType<CommanderDebugHud>();
                SetupDynamicHostility(context);
            }

            UpdateCameraTarget();
        }

        private void Update()
        {
            if (_state == null) return;
            _state.Tick(Time.deltaTime);

            _state.SelectedTarget = _targetSelector.CurrentTarget;

            HandleTacticalCommandExecution();

            UpdatePredictedControl();

            if (Input.GetKeyDown(_interactKey))
                TryInteract();

            if (Input.GetKeyDown(_releaseKey))
                ReleaseControl();

            UpdateSelectionMarker();
            UpdateControlledMarker();
            UpdateDebugHud();
        }

        private void TryInteract()
        {
            if (_state.IsSyncAssistActive)
            {
                if (_debugHud != null)
                    _debugHud.SetLastControlResult(ControlPermissionResult.DeniedResult("SyncAssist active, wait"));
                return;
            }

            var target = _targetSelector.CurrentTarget;
            if (target == null || target.Data == null)
            {
                if (_debugHud != null)
                    _debugHud.SetLastControlResult(ControlPermissionResult.DeniedResult("No target selected (Tab)"));
                return;
            }

            var context = GameBootstrap.Context;
            if (context == null) return;

            var request = BuildRequest(context, target);
            var result = _permissionService.Evaluate(request);
            _state.LastControlResult = result;

            switch (result.Mode)
            {
                case ControlMode.DirectControl:
                    ApplyDirectControl(target);
                    AudioFeedbackService.PlayUI(AudioEventId.DirectControlSuccess);
                    break;
                case ControlMode.TacticalCommand:
                    ApplyTacticalCommand(target);
                    AudioFeedbackService.PlayUI(AudioEventId.TacticalCommandIssued);
                    break;
                case ControlMode.SyncAssist:
                    ApplySyncAssist(target);
                    AudioFeedbackService.PlayUI(AudioEventId.SyncAssistActive);
                    break;
                case ControlMode.Denied:
                default:
                    AudioFeedbackService.PlayUI(AudioEventId.DeniedControl);
                    break;
            }
        }

        public bool HasSelectedTarget()
        {
            return _targetSelector != null && _targetSelector.CurrentTarget != null;
        }

        private ControlPermissionRequest BuildRequest(LuoLuoTripGameContext context, CharacterEntity target)
        {
            var commander = context.CommanderProfile;
            var targetInfo = CharacterControlInfo.FromCharacterData(target.Data);
            var playerFaction = _state.OriginalPlayerEntity.Data.Faction;
            var isCrossRace = target.Data.Race != _state.OriginalPlayerEntity.Data.Race;
            var factionTrust = context.ReputationService.GetTrust(target.Data.Faction);
            var factionHostility = context.ReputationService.GetHostility(target.Data.Faction);

            return new ControlPermissionRequest
            {
                Commander = commander,
                Target = targetInfo,
                IsCrossRaceControl = isCrossRace,
                CurrentControlledUnitCount = _state.IsDirectControllingOther ? 1 : 0,
                FactionTrust = factionTrust,
                FactionHostility = factionHostility
            };
        }

        private void ApplyDirectControl(CharacterEntity target)
        {
            var prevEntity = _state.DirectControlledEntity;

            if (prevEntity != null && prevEntity != _state.OriginalPlayerEntity)
            {
                var prevCombat = prevEntity.GetComponent<CombatController>();
                if (prevCombat != null) prevCombat.SetInputEnabled(false);

                var prevAI = prevEntity.GetComponent<SimpleCombatAI>();
                if (prevAI != null)
                {
                    prevAI.FollowTarget = null;
                    prevAI.HoldPosition = null;
                    prevAI.ForcedAttackTarget = null;
                    prevAI.enabled = true;
                    if (prevAI.NavController != null)
                        prevAI.NavController.ClearNavigation();
                }
            }

            var targetCombat = target.GetComponent<CombatController>();
            if (targetCombat == null)
                targetCombat = target.gameObject.AddComponent<CombatController>();
            targetCombat.SetInputEnabled(true);

            var targetAI = target.GetComponent<SimpleCombatAI>();
            if (targetAI != null)
            {
                targetAI.enabled = false;
                if (targetAI.NavController != null)
                    targetAI.NavController.ClearNavigation();
            }

            if (_playerCombatController != null)
                _playerCombatController.SetInputEnabled(false);

            _state.SetDirectControl(target);

            if (_state.HasActiveCommand)
                _state.ClearCommand();

            UpdateCameraTarget();
        }

        private void ApplyTacticalCommand(CharacterEntity target)
        {
            _state.SetCommand(CommanderCommandType.FollowPlayer, target);
        }

        private void ApplySyncAssist(CharacterEntity target)
        {
            _state.ActivateSyncAssist(_syncAssistDuration);
            _state.ApplySyncAssistBuff(target);
        }

        private void ReleaseControl()
        {
            if (!_state.IsDirectControllingOther && !_state.HasActiveCommand && !_state.IsSyncAssistActive)
                return;

            if (_state.DirectControlledEntity != null && _state.DirectControlledEntity != _state.OriginalPlayerEntity)
            {
                var prevCombat = _state.DirectControlledEntity.GetComponent<CombatController>();
                if (prevCombat != null) prevCombat.SetInputEnabled(false);

                var prevAI = _state.DirectControlledEntity.GetComponent<SimpleCombatAI>();
                if (prevAI != null)
                {
                    prevAI.FollowTarget = null;
                    prevAI.HoldPosition = null;
                    prevAI.ForcedAttackTarget = null;
                    prevAI.enabled = true;
                    if (prevAI.NavController != null)
                        prevAI.NavController.ClearNavigation();
                }
            }

            if (_state.HasActiveCommand && _state.CommandTarget != null)
            {
                var cmdAI = _state.CommandTarget.GetComponent<SimpleCombatAI>();
                if (cmdAI != null)
                {
                    cmdAI.FollowTarget = null;
                    cmdAI.HoldPosition = null;
                    cmdAI.ForcedAttackTarget = null;
                    if (cmdAI.NavController != null)
                        cmdAI.NavController.ClearNavigation();
                }
            }

            _state.ReleaseControl();

            if (_playerCombatController != null)
                _playerCombatController.SetInputEnabled(true);

            if (_playerAI != null)
                _playerAI.enabled = false;

            UpdateCameraTarget();
        }

        private void HandleTacticalCommandExecution()
        {
            if (!_state.HasActiveCommand || _state.CommandTarget == null)
            {
                if (_state.HasActiveCommand && _state.CommandTarget == null)
                    _state.ClearCommand();
                return;
            }

            var target = _state.CommandTarget;
            if (!target.Data.IsAlive)
            {
                ClearTacticalCommandOnTarget(target);
                _state.ClearCommand();
                return;
            }

            var ai = target.GetComponent<SimpleCombatAI>();
            if (ai == null) return;

            switch (_state.ActiveCommand)
            {
                case CommanderCommandType.FollowPlayer:
                    ai.FollowTarget = _state.OriginalPlayerEntity?.transform;
                    ai.HoldPosition = null;
                    ai.ForcedAttackTarget = null;
                    break;
                case CommanderCommandType.HoldPosition:
                    ai.FollowTarget = null;
                    ai.HoldPosition = target.transform.position;
                    ai.ForcedAttackTarget = null;
                    break;
                case CommanderCommandType.AttackCurrentTarget:
                    ai.FollowTarget = null;
                    ai.HoldPosition = null;
                    var playerCombatant = _state.OriginalPlayerEntity?.GetComponent<Combatant>();
                    ai.ForcedAttackTarget = playerCombatant != null && playerCombatant.IsAlive
                        ? playerCombatant
                        : ai.CurrentTarget;
                    if (ai.ForcedAttackTarget != null && !ai.ForcedAttackTarget.IsAlive)
                        ai.ForcedAttackTarget = null;
                    break;
            }

            _state.TacticalCommand.UpdateStatusText();
        }

        private void ClearTacticalCommandOnTarget(CharacterEntity target)
        {
            var ai = target.GetComponent<SimpleCombatAI>();
            if (ai == null) return;
            ai.FollowTarget = null;
            ai.HoldPosition = null;
            ai.ForcedAttackTarget = null;
        }

        private void UpdateCameraTarget()
        {
            if (_cameraFollow == null) return;
            var entity = _state?.DirectControlledEntity;
            if (entity != null)
                _cameraFollow.SetTarget(entity.transform);
        }

        private void UpdatePredictedControl()
        {
            var target = _targetSelector.CurrentTarget;
            if (target == null || target.Data == null) return;

            var context = GameBootstrap.Context;
            if (context == null) return;

            var request = BuildRequest(context, target);
            var predicted = _permissionService.Evaluate(request);
            _state.LastControlResult = predicted;
        }

        private void UpdateDebugHud()
        {
            if (_debugHud != null)
            {
                _debugHud.SetProfile(GameBootstrap.Context?.CommanderProfile);
                _debugHud.SetRuntimeState(_state);
            }

            if (_hintPanel != null)
            {
                _hintPanel.SetRuntimeState(_state);
                _hintPanel.SetLastControlResult(_state?.LastControlResult ?? default);
                _hintPanel.SetPlayerEntity(_state?.OriginalPlayerEntity);
            }
        }

        private void UpdateSelectionMarker()
        {
            var service = WorldMarkerService.Instance;
            if (service == null) return;

            var current = _targetSelector?.CurrentTarget;
            if (current == _lastSelectedTarget) return;

            if (_lastSelectedTarget != null && _lastSelectedTarget.gameObject != null)
                service.DetachMarker(_lastSelectedTarget.gameObject);

            if (current != null)
                service.AttachMarker(current.gameObject, WorldMarkerType.SelectedCommanderTarget);

            _lastSelectedTarget = current;
        }

        private void UpdateControlledMarker()
        {
            var service = WorldMarkerService.Instance;
            if (service == null || _state == null) return;

            CharacterEntity controlled = null;
            if (_state.IsDirectControllingOther)
                controlled = _state.DirectControlledEntity;

            if (controlled == _lastControlledMarkerEntity) return;

            if (_lastControlledMarkerEntity != null && _lastControlledMarkerEntity.gameObject != null)
                service.DetachMarker(_lastControlledMarkerEntity.gameObject);

            if (controlled != null && controlled != _state.OriginalPlayerEntity)
                service.AttachMarker(controlled.gameObject, WorldMarkerType.ControlledUnit);

            _lastControlledMarkerEntity = controlled;
        }

        private void SetupDynamicHostility(LuoLuoTripGameContext context)
        {
            var dynamicService = new DynamicFactionHostilityService(
                context.ReputationService,
                context.RelationshipService);

            CharacterEntity.HostilityResolver = (sourceFaction, targetFaction) =>
            {
                if (dynamicService.IsHostileToPlayer(sourceFaction) || dynamicService.IsHostileToPlayer(targetFaction))
                    return true;
                return context.RelationshipService.Matrix.IsHostile(sourceFaction, targetFaction);
            };
        }

        private void OnDestroy()
        {
            CharacterEntity.HostilityResolver = null;

            var service = WorldMarkerService.Instance;
            if (service != null)
            {
                if (_lastSelectedTarget != null && _lastSelectedTarget.gameObject != null)
                    service.DetachMarker(_lastSelectedTarget.gameObject);
                if (_lastControlledMarkerEntity != null && _lastControlledMarkerEntity.gameObject != null)
                    service.DetachMarker(_lastControlledMarkerEntity.gameObject);
            }
        }
    }
}
