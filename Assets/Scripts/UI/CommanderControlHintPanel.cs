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

            GUI.Box(new Rect(x - 4, y - 4, width + 8, hints.Count * 18 + 8), "");
            foreach (var hint in hints)
            {
                GUI.Label(new Rect(x, y, width, 18), hint);
                y += 18;
            }
        }

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
                hints.Add("[F] Toggle Follow/HoldPosition");
            }
            else if (_runtimeState.IsSyncAssistActive)
            {
                hints.Add("[R] Stop SyncAssist | Shared damage bonus active");
            }
            else if (_runtimeState.SelectedTarget != null)
            {
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
                hints.Add("[Tab] Switch target");
            }
            else
            {
                hints.Add("[Tab] Select target in range");
            }

            return hints;
        }
    }
}
