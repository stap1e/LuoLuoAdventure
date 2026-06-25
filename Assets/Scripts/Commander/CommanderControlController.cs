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
        [SerializeField] private KeyCode _defendObjectiveKey = KeyCode.G;
        [SerializeField] private KeyCode _focusFireKey = KeyCode.F;
        [SerializeField] private float _syncAssistDuration = 3f;
        [SerializeField] private float _defendRadius = 5f;
        [SerializeField] private float _focusFireDuration = 8f;
        [SerializeField] private float _focusFireRadius = 14f;
        [SerializeField] private int _focusFireMaxResponders = 3;
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

            _state = new CommanderControlRuntimeState
            {
                OriginalPlayerEntity = GetComponent<CharacterEntity>()
            };
            _state.DirectControlledEntity = _state.OriginalPlayerEntity;
        }

        private void Start()
        {

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

            if (Input.GetKeyDown(_defendObjectiveKey))
                TryIssueDefendObjective();

            if (Input.GetKeyDown(_focusFireKey))
                TryIssueFocusFire();

            if (Input.GetKeyDown(_interactKey))
                TryInteract();

            if (Input.GetKeyDown(_releaseKey))
                ReleaseControl();

            UpdatePredictedCommanderActions();
            UpdateSelectionMarker();
            UpdateControlledMarker();
            UpdateDebugHud();
        }

        public void TryInteract()
        {
            var context = GameBootstrap.Context;
            var target = _targetSelector.CurrentTarget;
            var hadSelectedTarget = target != null;
            if (target == null)
            {
                target = TryAutoAcquireDirectControlTarget(context);
            }

            RecordControlAttempt(context, target, hadSelectedTarget ? "SelectedTarget" : target != null ? "AutoAcquire" : "NoTarget");

            var playerCombatant = _state?.OriginalPlayerEntity != null ? _state.OriginalPlayerEntity.GetComponent<Combatant>() : null;
            if (playerCombatant != null && !playerCombatant.IsAlive)
            {
                RecordDenied(context, target, "PlayerDead", "Player is down; revive or reload before controlling units.");
                return;
            }

            if (_state.IsSyncAssistActive)
            {
                RecordDenied(context, target, "SyncAssist active, wait", "Press R to stop SyncAssist or wait for it to expire.");
                return;
            }

            if (target == null || target.Data == null)
            {
                RecordDenied(context, null, "No controllable target nearby", "Press Tab/Q to select target or move closer to a low-rank unit.");
                return;
            }
            if (!target.Data.IsAlive)
            {
                RecordDenied(context, target, "TargetDead", "Select a living low-rank unit.");
                return;
            }

            if (context == null)
            {
                RecordDenied(context, target, "Missing game context", "Wait for GameBootstrap to initialize.");
                return;
            }

            var request = BuildRequest(context, target);
            var result = _permissionService.Evaluate(request);
            result = NormalizePermissionResult(request, result);
            _state.LastControlResult = result;
            UpdatePermissionDiagnostics(context, target, result, BuildSuggestion(request, result));

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

        public bool TryIssueDefendObjective()
        {
            EnsureRuntimeState();
            var selected = _targetSelector != null ? _targetSelector.CurrentTarget : null;
            var ally = IsCommandableAlly(selected) ? selected : FindNearestCommandableAlly(transform.position, _focusFireRadius);
            var objective = IsObjectiveTarget(selected) ? selected : FindNearestObjective(ally != null ? ally.transform.position : transform.position);
            return TryIssueDefendObjective(ally, objective);
        }

        public bool TryIssueDefendObjective(CharacterEntity ally, CharacterEntity objective)
        {
            EnsureRuntimeState();
            var context = GameBootstrap.Context;
            if (!ValidateDefendObjective(context, ally, objective, out var reason, out var suggestion))
            {
                RecordCommanderActionDenied(context, ally, reason, suggestion, CommanderActionType.DefendObjective);
                Debug.Log($"[CommanderAction] DefendObjective denied: {reason}");
                return false;
            }

            var ai = ally.GetComponent<SimpleCombatAI>();
            if (ai == null)
            {
                RecordCommanderActionDenied(context, ally, "No AI responder", "Select a low-rank ally with SimpleCombatAI.", CommanderActionType.DefendObjective);
                Debug.Log("[CommanderAction] DefendObjective denied: No AI responder");
                return false;
            }

            ai.SetDefendObjective(objective.transform, _defendRadius);
            _state.SetCommand(CommanderCommandType.DefendObjective, ally);
            _state.TacticalCommand.SetDefendObjective(ally, objective, _defendRadius, Time.time);
            _state.LastDefendObjectiveAllowed = true;
            _state.LastDefendObjectiveReason = string.Empty;
            _state.LastObjectiveTargetName = objective.Data != null ? objective.Data.DisplayName : objective.name;
            _state.LastResponderCount = 1;
            _state.LastInputRoute = "DefendObjective";
            _state.LastSuggestion = $"Command: Defend {_state.LastObjectiveTargetName}";
            Debug.Log($"[CommanderAction] DefendObjective issued: {ally.name} -> {objective.name}");
            AudioFeedbackService.PlayUI(AudioEventId.TacticalCommandIssued);
            return true;
        }

        public bool TryIssueFocusFire()
        {
            EnsureRuntimeState();
            var focusTarget = _targetSelector != null ? _targetSelector.CurrentTarget : null;
            return TryIssueFocusFire(focusTarget);
        }

        public bool TryIssueFocusFire(CharacterEntity focusTarget)
        {
            EnsureRuntimeState();
            var context = GameBootstrap.Context;
            if (!ValidateFocusFireTarget(context, focusTarget, out var reason, out var suggestion))
            {
                RecordCommanderActionDenied(context, focusTarget, reason, suggestion, CommanderActionType.FocusFire);
                Debug.Log($"[CommanderAction] FocusFire denied: {reason}");
                return false;
            }

            var combatTarget = focusTarget.GetComponent<Combatant>();
            var responders = FindFocusFireResponders(context, focusTarget.transform.position);
            if (responders.Count == 0)
            {
                RecordCommanderActionDenied(context, focusTarget, "No nearby responders", "Move near commandable allies or select a lower-rank ally first.", CommanderActionType.FocusFire);
                Debug.Log("[CommanderAction] FocusFire denied: No nearby responders");
                return false;
            }

            for (int i = 0; i < responders.Count; i++)
            {
                var ai = responders[i].GetComponent<SimpleCombatAI>();
                if (ai != null)
                    ai.SetFocusFireTarget(combatTarget);
            }

            _state.SetCommand(CommanderCommandType.FocusFire, focusTarget);
            _state.TacticalCommand.SetFocusFire(focusTarget, combatTarget, _focusFireDuration, responders.Count, Time.time);
            _state.LastFocusFireAllowed = true;
            _state.LastFocusFireReason = string.Empty;
            _state.LastFocusTargetName = focusTarget.Data != null ? focusTarget.Data.DisplayName : focusTarget.name;
            _state.LastResponderCount = responders.Count;
            _state.LastInputRoute = "FocusFire";
            _state.LastSuggestion = $"FocusFire: {_state.LastFocusTargetName}";
            Debug.Log($"[CommanderAction] FocusFire issued: {_state.LastFocusTargetName}, responders={responders.Count}");
            AudioFeedbackService.PlayUI(AudioEventId.TacticalCommandIssued);
            return true;
        }

        private void RecordCommanderActionDenied(LuoLuoTripGameContext context, CharacterEntity target, string reason, string suggestion, CommanderActionType action)
        {
            RecordDenied(context, target, reason, suggestion);
            if (action == CommanderActionType.DefendObjective)
            {
                _state.LastDefendObjectiveAllowed = false;
                _state.LastDefendObjectiveReason = reason;
            }
            else if (action == CommanderActionType.FocusFire)
            {
                _state.LastFocusFireAllowed = false;
                _state.LastFocusFireReason = reason;
            }
        }

        private bool ValidateDefendObjective(LuoLuoTripGameContext context, CharacterEntity ally, CharacterEntity objective, out string reason, out string suggestion)
        {
            if (ally == null || ally.Data == null)
            {
                reason = "No ally selected";
                suggestion = "Select a low-rank ally and press G to defend an objective.";
                return false;
            }
            if (objective == null)
            {
                reason = "No objective selected";
                suggestion = "Move near Convoy, Energy Node, Allied Defense Point, CityGateCore, or BeastNegotiator.";
                return false;
            }
            if (!ally.Data.IsAlive)
            {
                reason = "Ally dead";
                suggestion = "Select a living low-rank ally.";
                return false;
            }
            if (objective.Data != null && !objective.Data.IsAlive)
            {
                reason = "Objective dead";
                suggestion = "Select a living objective such as CityGateCore or BeastNegotiator.";
                return false;
            }
            if (!ally.Data.AllowTacticalCommand)
            {
                reason = "Tactical command disabled";
                suggestion = "Select an ally that can receive TacticalCommand.";
                return false;
            }
            if (context != null && !WouldAllowTacticalCommand(BuildRequest(context, ally)))
            {
                reason = "Commander level or trust too low";
                suggestion = "Improve trust/level or select a lower-rank ally.";
                return false;
            }
            reason = string.Empty;
            suggestion = "Press G to defend this objective.";
            return true;
        }

        private bool ValidateFocusFireTarget(LuoLuoTripGameContext context, CharacterEntity focusTarget, out string reason, out string suggestion)
        {
            if (focusTarget == null || focusTarget.Data == null)
            {
                reason = "No hostile target selected";
                suggestion = "Select a BeastRaider, Hardliner, or valid threat and press F.";
                return false;
            }
            var combatTarget = focusTarget.GetComponent<Combatant>();
            if (combatTarget == null || !combatTarget.IsAlive || !focusTarget.Data.IsAlive)
            {
                reason = "Target dead";
                suggestion = "Select a living hostile target.";
                return false;
            }
            if (!IsThreatTarget(context, focusTarget))
            {
                reason = "No hostile target selected";
                suggestion = "Select an enemy or valid threat before pressing F.";
                return false;
            }
            reason = string.Empty;
            suggestion = "Press F to order nearby allies to focus fire.";
            return true;
        }

        private bool IsCommandableAlly(CharacterEntity entity)
        {
            if (entity == null || entity.Data == null || _state?.OriginalPlayerEntity?.Data == null) return false;
            if (!entity.Data.IsAlive || !entity.Data.AllowTacticalCommand) return false;
            return !IsThreatTarget(GameBootstrap.Context, entity);
        }

        private bool IsObjectiveTarget(CharacterEntity entity)
        {
            if (entity == null) return false;
            var name = entity.name;
            var display = entity.Data != null ? entity.Data.DisplayName : string.Empty;
            return name.Contains("Convoy") || name.Contains("Energy") || name.Contains("Objective") || name.Contains("CityGateCore") || name.Contains("BeastNegotiator")
                || display.Contains("Convoy") || display.Contains("Energy") || display.Contains("CityGateCore") || display.Contains("BeastNegotiator");
        }

        private bool IsThreatTarget(LuoLuoTripGameContext context, CharacterEntity entity)
        {
            if (entity == null || entity.Data == null || _state?.OriginalPlayerEntity == null) return false;
            if (_state.OriginalPlayerEntity.IsHostileTo(entity) || entity.IsHostileTo(_state.OriginalPlayerEntity)) return true;
            return GameConstants.IsBeastSubFaction(entity.Data.Faction) && entity.Data.Faction != _state.OriginalPlayerEntity.Data.Faction;
        }

        private CharacterEntity FindNearestCommandableAlly(Vector3 origin, float radius)
        {
            CharacterEntity best = null;
            var bestDistance = float.MaxValue;
            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (!IsCommandableAlly(entity)) continue;
                var distance = Vector3.Distance(origin, entity.transform.position);
                if (distance > radius || distance >= bestDistance) continue;
                best = entity;
                bestDistance = distance;
            }
            return best;
        }

        private CharacterEntity FindNearestObjective(Vector3 origin)
        {
            CharacterEntity best = null;
            var bestDistance = float.MaxValue;
            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (!IsObjectiveTarget(entity)) continue;
                var distance = Vector3.Distance(origin, entity.transform.position);
                if (distance >= bestDistance) continue;
                best = entity;
                bestDistance = distance;
            }
            return best;
        }

        private System.Collections.Generic.List<CharacterEntity> FindFocusFireResponders(LuoLuoTripGameContext context, Vector3 origin)
        {
            var responders = new System.Collections.Generic.List<CharacterEntity>();
            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (responders.Count >= _focusFireMaxResponders) break;
                if (!IsCommandableAlly(entity)) continue;
                if (Vector3.Distance(origin, entity.transform.position) > _focusFireRadius) continue;
                var ai = entity.GetComponent<SimpleCombatAI>();
                if (ai == null) continue;
                if (context != null && !WouldAllowTacticalCommand(BuildRequest(context, entity))) continue;
                responders.Add(entity);
            }
            return responders;
        }

        private ControlPermissionRequest BuildRequest(LuoLuoTripGameContext context, CharacterEntity target)
        {
            var commander = context.CommanderProfile;
            var targetInfo = CharacterControlInfo.FromCharacterData(target.Data);
            var playerFaction = _state.OriginalPlayerEntity.Data.Faction;
            var isCrossRace = target.Data.Race != _state.OriginalPlayerEntity.Data.Race;
            var factionTrust = Mathf.Max(context.ReputationService.GetTrust(target.Data.Faction), target.Data.TrustToPlayer);
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

        private CharacterEntity TryAutoAcquireDirectControlTarget(LuoLuoTripGameContext context)
        {
            if (context == null || _targetSelector == null)
                return null;

            CharacterEntity best = null;
            var bestRank = int.MaxValue;
            var bestDistance = float.MaxValue;
            var candidates = _targetSelector.GetCandidates();
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate.Data == null || !candidate.Data.IsAlive)
                    continue;
                if (candidate.Data.IsHeroOrLeader || candidate.Data.Role == CharacterRole.CityLord || candidate.Data.Role == CharacterRole.WarKing)
                    continue;
                if (!candidate.Data.AllowDirectControl)
                    continue;

                var request = BuildRequest(context, candidate);
                var result = _permissionService.Evaluate(request);
                if (result.Mode != ControlMode.DirectControl)
                    continue;

                var distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (request.Target.CommandRank < bestRank || (request.Target.CommandRank == bestRank && distance < bestDistance))
                {
                    best = candidate;
                    bestRank = request.Target.CommandRank;
                    bestDistance = distance;
                }
            }

            if (best != null)
            {
                _targetSelector.TrySelectTarget(best);
                _state.LastAutoAcquiredTarget = best;
            }

            return best;
        }

        private void RecordControlAttempt(LuoLuoTripGameContext context, CharacterEntity target, string route)
        {
            if (_state == null) return;
            _state.LastControlAttemptTime = Time.time;
            _state.LastInputRoute = route;
            _state.LastAttemptHadSelectedTarget = route == "SelectedTarget";
            PopulateTargetDiagnostics(context, target);
        }

        private void RecordDenied(LuoLuoTripGameContext context, CharacterEntity target, string reason, string suggestion)
        {
            var result = ControlPermissionResult.DeniedResult(reason);
            _state.LastControlResult = result;
            UpdatePermissionDiagnostics(context, target, result, suggestion);
            if (_debugHud != null)
                _debugHud.SetLastControlResult(result);
            AudioFeedbackService.PlayUI(AudioEventId.DeniedControl);
        }

        private void PopulateTargetDiagnostics(LuoLuoTripGameContext context, CharacterEntity target)
        {
            if (_state == null) return;
            var commander = context?.CommanderProfile;
            _state.LastCommanderLevel = commander?.CommanderLevel ?? 0;

            if (target == null || target.Data == null)
            {
                _state.LastSelectedTargetName = "None";
                _state.LastSelectedTargetRank = 0;
                _state.LastSelectedTargetRequiredLevel = 0;
                _state.LastSelectedTargetTrust = 0;
                _state.LastSelectedTargetIsLeader = false;
                _state.LastSelectedTargetAllowDirectControl = false;
                _state.LastSelectedTargetAllowTacticalCommand = false;
                return;
            }

            var data = target.Data;
            _state.LastSelectedTargetName = data.DisplayName;
            _state.LastSelectedTargetRank = data.CommandRank;
            _state.LastSelectedTargetRequiredLevel = data.RequiredCommanderLevel;
            _state.LastSelectedTargetTrust = context != null ? Mathf.Max(context.ReputationService.GetTrust(data.Faction), data.TrustToPlayer) : data.TrustToPlayer;
            _state.LastSelectedTargetIsLeader = data.IsHeroOrLeader || data.Role == CharacterRole.CityLord || data.Role == CharacterRole.WarKing;
            _state.LastSelectedTargetAllowDirectControl = data.AllowDirectControl;
            _state.LastSelectedTargetAllowTacticalCommand = data.AllowTacticalCommand;
        }

        private void UpdatePermissionDiagnostics(LuoLuoTripGameContext context, CharacterEntity target, ControlPermissionResult result, string suggestion)
        {
            PopulateTargetDiagnostics(context, target);
            _state.LastControlResult = result;
            _state.LastControlRejectReason = result.IsAllowed ? string.Empty : result.Reason;
            _state.LastSuggestion = suggestion;

            if (context != null && target != null && target.Data != null)
            {
                var request = BuildRequest(context, target);
                _state.LastDirectControlAllowed = WouldAllowDirectControl(request);
                _state.LastTacticalCommandAllowed = WouldAllowTacticalCommand(request);
                _state.LastSyncAssistAllowed = WouldAllowSyncAssist(request);
            }
            else
            {
                _state.LastDirectControlAllowed = false;
                _state.LastTacticalCommandAllowed = false;
                _state.LastSyncAssistAllowed = false;
            }

            if (_debugHud != null)
                _debugHud.SetLastControlResult(result);
        }

        private ControlPermissionResult NormalizePermissionResult(ControlPermissionRequest request, ControlPermissionResult result)
        {
            if (result.Mode != ControlMode.Denied)
                return result;

            return ControlPermissionResult.DeniedResult(GetDirectControlDenialReason(request));
        }

        private string GetDirectControlDenialReason(ControlPermissionRequest request)
        {
            if (request == null || request.Commander == null)
                return "Invalid request";

            var target = request.Target;
            var commander = request.Commander;
            var effectiveTrust = request.FactionTrust - (request.IsCrossRaceControl ? ControlPermissionService.CrossRaceTrustPenalty : 0);

            if (commander.CommandCapacity <= request.CurrentControlledUnitCount)
                return "Command capacity exceeded";
            if (target.IsHeroOrLeader)
                return "Leader unit";
            if (!target.AllowDirectControl)
                return "Direct control disabled";
            if (commander.MaxDirectControlRank < target.CommandRank)
                return "Rank too high";
            if (commander.CommanderLevel < target.RequiredCommanderLevel)
                return "Commander level too low";
            if (effectiveTrust < ControlPermissionService.MinTrustForDirectControl)
                return "Trust too low";
            return resultReasonFallback;
        }

        private const string resultReasonFallback = "Insufficient level, trust, or capacity";

        private string BuildSuggestion(ControlPermissionRequest request, ControlPermissionResult result)
        {
            if (request == null)
                return "Press Tab/Q to select target or move closer to a low-rank unit.";
            if (result.Mode == ControlMode.DirectControl)
                return "Press E to control.";
            if (result.Mode == ControlMode.TacticalCommand)
                return "DirectControl denied. TacticalCommand available.";
            if (result.Mode == ControlMode.SyncAssist)
                return "SyncAssist available.";
            if (WouldAllowTacticalCommand(request))
                return "Try Tactical Command or select a lower-rank unit.";
            if (WouldAllowSyncAssist(request))
                return "Try Sync Assist instead.";
            if (request.Target.IsHeroOrLeader || request.Target.CommandRank > request.Commander.MaxDirectControlRank)
                return "Try Tactical Command or select a lower-rank unit.";
            return "Move closer, improve trust, or select a lower-rank unit.";
        }

        private bool WouldAllowDirectControl(ControlPermissionRequest request)
        {
            if (request == null || request.Commander == null)
                return false;
            var effectiveTrust = request.FactionTrust - (request.IsCrossRaceControl ? ControlPermissionService.CrossRaceTrustPenalty : 0);
            return request.Commander.CommandCapacity > request.CurrentControlledUnitCount
                && !request.Target.IsHeroOrLeader
                && request.Target.AllowDirectControl
                && request.Commander.MaxDirectControlRank >= request.Target.CommandRank
                && request.Commander.CommanderLevel >= request.Target.RequiredCommanderLevel
                && effectiveTrust >= ControlPermissionService.MinTrustForDirectControl;
        }

        private bool WouldAllowTacticalCommand(ControlPermissionRequest request)
        {
            if (request == null || request.Commander == null)
                return false;
            var effectiveTrust = request.FactionTrust - (request.IsCrossRaceControl ? ControlPermissionService.CrossRaceTrustPenalty : 0);
            return request.Commander.CommandCapacity > request.CurrentControlledUnitCount
                && request.Target.AllowTacticalCommand
                && request.Commander.MaxTacticalCommandRank >= request.Target.CommandRank
                && request.Commander.CommanderLevel >= request.Target.RequiredCommanderLevel - 5
                && effectiveTrust >= ControlPermissionService.MinTrustForTacticalCommand;
        }

        private bool WouldAllowSyncAssist(ControlPermissionRequest request)
        {
            if (request == null || request.Commander == null)
                return false;
            var effectiveTrust = request.FactionTrust - (request.IsCrossRaceControl ? ControlPermissionService.CrossRaceTrustPenalty : 0);
            if (effectiveTrust < ControlPermissionService.MinTrustForSyncAssist)
                return false;
            if (request.Commander.CommanderLevel < 35 && request.Commander.CommanderLevel < request.Target.RequiredCommanderLevel - 10)
                return false;
            return SyncRateCalculator.Calculate(request.Commander, request.Target, request.IsCrossRaceControl, effectiveTrust) > 0.05f;
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
                    prevAI.ClearCommanderCommands();
                    prevAI.enabled = true;
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
            {
                if (_state.ActiveCommand == CommanderCommandType.FocusFire)
                    ClearFocusFireResponders();
                _state.ClearCommand();
            }

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
                    prevAI.ClearCommanderCommands();
                    prevAI.enabled = true;
                }
            }

            if (_state.HasActiveCommand && _state.CommandTarget != null)
            {
                if (_state.ActiveCommand == CommanderCommandType.FocusFire)
                {
                    ClearFocusFireResponders();
                }
                else
                {
                    var cmdAI = _state.CommandTarget.GetComponent<SimpleCombatAI>();
                    if (cmdAI != null)
                        cmdAI.ClearCommanderCommands();
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
                    ai.ClearDefendObjective();
                    break;
                case CommanderCommandType.HoldPosition:
                    ai.FollowTarget = null;
                    ai.HoldPosition = target.transform.position;
                    ai.ForcedAttackTarget = null;
                    ai.ClearDefendObjective();
                    break;
                case CommanderCommandType.AttackCurrentTarget:
                    ai.FollowTarget = null;
                    ai.HoldPosition = null;
                    ai.ClearDefendObjective();
                    var playerCombatant = _state.OriginalPlayerEntity?.GetComponent<Combatant>();
                    ai.ForcedAttackTarget = playerCombatant != null && playerCombatant.IsAlive
                        ? playerCombatant
                        : ai.CurrentTarget;
                    if (ai.ForcedAttackTarget != null && !ai.ForcedAttackTarget.IsAlive)
                        ai.ForcedAttackTarget = null;
                    break;
                case CommanderCommandType.DefendObjective:
                    if (_state.TacticalCommand.DefendTarget == null || !_state.TacticalCommand.DefendTarget.Data.IsAlive)
                    {
                        ClearTacticalCommandOnTarget(target);
                        _state.ClearCommand();
                        return;
                    }
                    ai.SetDefendObjective(_state.TacticalCommand.DefendTarget.transform, _state.TacticalCommand.DefendRadius > 0f ? _state.TacticalCommand.DefendRadius : _defendRadius);
                    break;
                case CommanderCommandType.FocusFire:
                    if (_state.TacticalCommand.IsExpired(Time.time) || _state.TacticalCommand.FocusTarget == null || !_state.TacticalCommand.FocusTarget.IsAlive)
                    {
                        ClearFocusFireResponders();
                        _state.ClearCommand();
                        return;
                    }
                    break;
            }

            _state.TacticalCommand.UpdateStatusText();
        }

        private void ClearTacticalCommandOnTarget(CharacterEntity target)
        {
            var ai = target.GetComponent<SimpleCombatAI>();
            if (ai == null) return;
            ai.ClearCommanderCommands();
        }

        private void ClearFocusFireResponders()
        {
            foreach (var ai in FindObjectsOfType<SimpleCombatAI>())
            {
                if (ai.ForcedAttackTarget == _state.TacticalCommand.FocusTarget)
                    ai.SetFocusFireTarget(null);
            }
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
            var predicted = NormalizePermissionResult(request, _permissionService.Evaluate(request));
            _state.LastControlResult = predicted;
            _state.LastSuggestion = BuildSuggestion(request, predicted);
            _state.LastDirectControlAllowed = WouldAllowDirectControl(request);
            _state.LastTacticalCommandAllowed = WouldAllowTacticalCommand(request);
            _state.LastSyncAssistAllowed = WouldAllowSyncAssist(request);
            PopulateTargetDiagnostics(context, target);
        }

        private void UpdatePredictedCommanderActions()
        {
            if (_state == null) return;

            var selected = _targetSelector != null ? _targetSelector.CurrentTarget : null;
            var ally = IsCommandableAlly(selected) ? selected : FindNearestCommandableAlly(transform.position, _focusFireRadius);
            var objective = IsObjectiveTarget(selected) ? selected : FindNearestObjective(ally != null ? ally.transform.position : transform.position);
            _state.LastDefendObjectiveAllowed = ValidateDefendObjective(GameBootstrap.Context, ally, objective, out var defendReason, out _);
            _state.LastDefendObjectiveReason = _state.LastDefendObjectiveAllowed ? string.Empty : defendReason;
            _state.LastObjectiveTargetName = objective != null
                ? objective.Data != null ? objective.Data.DisplayName : objective.name
                : string.Empty;

            var focusTarget = selected;
            var focusTargetValid = ValidateFocusFireTarget(GameBootstrap.Context, focusTarget, out var focusReason, out _);
            var responderCount = focusTargetValid ? FindFocusFireResponders(GameBootstrap.Context, focusTarget.transform.position).Count : 0;
            _state.LastFocusFireAllowed = focusTargetValid && responderCount > 0;
            _state.LastFocusFireReason = _state.LastFocusFireAllowed ? string.Empty : focusTargetValid ? "No nearby responders" : focusReason;
            _state.LastResponderCount = responderCount;
            _state.LastFocusTargetName = focusTarget != null
                ? focusTarget.Data != null ? focusTarget.Data.DisplayName : focusTarget.name
                : string.Empty;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) return string.Empty;
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
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
