using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class CommanderControlHintPanel : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;

        private CommanderControlRuntimeState _runtimeState;
        private ControlPermissionResult _lastResult;
        private CharacterEntity _playerEntity;

        public void SetRuntimeState(CommanderControlRuntimeState state) => _runtimeState = state;
        public void SetLastControlResult(ControlPermissionResult result) => _lastResult = result;
        public void SetPlayerEntity(CharacterEntity entity) => _playerEntity = entity;

        private void OnGUI()
        {
            if (!_visible) return;
            if (_runtimeState == null) return;

            var layout = DebugUILayout.ControlHint;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            var hints = BuildHints();
            if (hints.Count == 0) return;

            GUI.Box(new Rect(x - 4, y - 4, width + 8, Mathf.Max(layout.height, hints.Count * 18 + 8)), "");
            foreach (var hint in hints)
            {
                GUI.Label(new Rect(x, y, width, 18), hint);
                y += 18;
            }
        }

        private static string AllowedText(bool allowed) => allowed ? "Allowed" : "Denied";

        private List<string> BuildHints()
        {
            var hints = new List<string>();

            if (_runtimeState.IsDirectControllingOther)
            {
                hints.Add("[R] Release control");
                hints.Add("[WASD] Move unit | [LMB] Attack | [Space] Dodge");
            }
            else if (_runtimeState.HasActiveCommand)
            {
                hints.Add("[R] Cancel command");
                hints.Add("[G] DefendObjective | [F] FocusFire");
                hints.Add($"Command: {_runtimeState.TacticalCommand.StatusText}");
                if (_runtimeState.ActiveCommand == CommanderCommandType.FocusFire)
                    hints.Add($"Responders: {_runtimeState.TacticalCommand.ResponderCount} | Duration: {_runtimeState.TacticalCommand.RemainingDuration(Time.time):F1}s");
            }
            else if (_runtimeState.IsSyncAssistActive)
            {
                hints.Add("[R] Stop SyncAssist | Shared damage bonus active");
            }
            else if (_runtimeState.SelectedTarget != null)
            {
                hints.Add($"Target: {_runtimeState.LastSelectedTargetName} Rank {_runtimeState.LastSelectedTargetRank} Trust {_runtimeState.LastSelectedTargetTrust}");
                var ai = _runtimeState.SelectedTarget.GetComponent<Combat.SimpleCombatAI>();
                if (ai != null)
                    hints.Add($"AI: {ai.CurrentBehaviorLabel} | {ai.LastProfileDecision}");
                else
                    hints.Add("AI: Default AI");
                var descriptors = CommanderActionPresenter.BuildDescriptors(_runtimeState, _lastResult);
                hints.Add(string.Join(" | ", descriptors.ConvertAll(CommanderActionPresenter.BuildStatusLine)));
                hints.Add("[G] DefendObjective | [F] FocusFire");
                if (_lastResult.IsAllowed)
                {
                    var modeHint = _lastResult.Mode switch
                    {
                        ControlMode.DirectControl => "[E] Take direct control",
                        ControlMode.TacticalCommand => "[E] Issue tactical command",
                        ControlMode.SyncAssist => "[E] Enter sync assist",
                        _ => "[E] Interact"
                    };
                    hints.Add(modeHint);
                }
                else
                {
                    hints.Add($"[E] Cannot control: {_lastResult.Reason}");
                }
                if (!string.IsNullOrEmpty(_runtimeState.LastSuggestion))
                    hints.Add($"Suggestion: {_runtimeState.LastSuggestion}");
                if (!string.IsNullOrEmpty(_runtimeState.LastInputRoute))
                    hints.Add($"LastInputRoute: {_runtimeState.LastInputRoute}");
                hints.Add("[Tab] Switch target");
            }
            else
            {
                hints.Add("[E] Auto-select nearby low-rank unit");
                hints.Add("[Tab] Select target in range");
                if (!string.IsNullOrEmpty(_runtimeState.LastControlRejectReason))
                    hints.Add($"Reason: {_runtimeState.LastControlRejectReason}");
                if (!string.IsNullOrEmpty(_runtimeState.LastSuggestion))
                    hints.Add($"Suggestion: {_runtimeState.LastSuggestion}");
                if (!string.IsNullOrEmpty(_runtimeState.LastInputRoute))
                    hints.Add($"LastInputRoute: {_runtimeState.LastInputRoute}");
            }

            return hints;
        }
    }
}
