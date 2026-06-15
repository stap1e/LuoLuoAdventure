using LuoLuoTrip.Combat;
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

        public CommanderControlRuntimeState State => _state;
        public CommanderTargetSelector TargetSelector => _targetSelector;

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

            var context = GameBootstrap.Context;
            if (context != null)
            {
                _debugHud = FindObjectOfType<CommanderDebugHud>();
                SetupDynamicHostility(context);
            }
        }

        private void Update()
        {
            if (_state == null) return;
            _state.Tick(Time.deltaTime);

            HandleTacticalCommandExecution();

            UpdatePredictedControl();

            if (Input.GetKeyDown(_interactKey))
                TryInteract();

            if (Input.GetKeyDown(_releaseKey))
                ReleaseControl();

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
                    break;
                case ControlMode.TacticalCommand:
                    ApplyTacticalCommand(target);
                    break;
                case ControlMode.SyncAssist:
                    ApplySyncAssist(target);
                    break;
                case ControlMode.Denied:
                default:
                    break;
            }
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
                if (prevCombat != null) prevCombat.enabled = false;

                var prevAI = prevEntity.GetComponent<SimpleCombatAI>();
                if (prevAI != null) prevAI.enabled = true;
            }

            var targetCombat = target.GetComponent<CombatController>();
            if (targetCombat == null)
                targetCombat = target.gameObject.AddComponent<CombatController>();
            targetCombat.enabled = true;

            var targetAI = target.GetComponent<SimpleCombatAI>();
            if (targetAI != null) targetAI.enabled = false;

            if (_playerCombatController != null)
                _playerCombatController.enabled = false;

            _state.SetDirectControl(target);
        }

        private void ApplyTacticalCommand(CharacterEntity target)
        {
            _state.SetCommand(CommanderCommandType.FollowPlayer, target);
        }

        private void ApplySyncAssist(CharacterEntity target)
        {
            _state.ActivateSyncAssist(_syncAssistDuration);
        }

        private void ReleaseControl()
        {
            if (!_state.IsDirectControllingOther && !_state.HasActiveCommand && !_state.IsSyncAssistActive)
                return;

            if (_state.DirectControlledEntity != null && _state.DirectControlledEntity != _state.OriginalPlayerEntity)
            {
                var prevCombat = _state.DirectControlledEntity.GetComponent<CombatController>();
                if (prevCombat != null) prevCombat.enabled = false;

                var prevAI = _state.DirectControlledEntity.GetComponent<SimpleCombatAI>();
                if (prevAI != null) prevAI.enabled = true;
            }

            _state.ReleaseControl();

            if (_playerCombatController != null)
                _playerCombatController.enabled = true;

            if (_playerAI != null)
                _playerAI.enabled = false;
        }

        private void HandleTacticalCommandExecution()
        {
            if (!_state.HasActiveCommand || _state.CommandTarget == null) return;

            var target = _state.CommandTarget;
            if (!target.Data.IsAlive)
            {
                _state.ClearCommand();
                return;
            }

            var ai = target.GetComponent<SimpleCombatAI>();
            if (ai == null) return;

            switch (_state.ActiveCommand)
            {
                case CommanderCommandType.FollowPlayer:
                    break;
                case CommanderCommandType.HoldPosition:
                    break;
                case CommanderCommandType.AttackCurrentTarget:
                    break;
            }
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
            if (_debugHud == null) return;

            _debugHud.SetProfile(GameBootstrap.Context?.CommanderProfile);
            _debugHud.SetRuntimeState(_state);
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
        }
    }
}
