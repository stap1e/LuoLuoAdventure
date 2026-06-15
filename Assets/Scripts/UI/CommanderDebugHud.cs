using System;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class CommanderDebugHud : MonoBehaviour
    {
        [SerializeField] private Vector2 _position = new Vector2(10, 150);
        [SerializeField] private int _width = 320;

        private CommanderProfile _profile;
        private ControlPermissionResult _lastResult;
        private CommanderControlRuntimeState _runtimeState;

        public void SetProfile(CommanderProfile profile) => _profile = profile;
        public void SetLastControlResult(ControlPermissionResult result) => _lastResult = result;
        public void SetRuntimeState(CommanderControlRuntimeState state) => _runtimeState = state;

        private void OnGUI()
        {
            if (_profile == null) return;

            var x = _position.x;
            var y = _position.y;

            GUI.Label(new Rect(x, y, _width, 20), $"=== Commander (Lv.{_profile.CommanderLevel}) ===");
            y += 20;
            GUI.Label(new Rect(x, y, _width, 20), $"XP: {_profile.Experience} | Capacity: {_profile.CommandCapacity}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Direct Rank: {_profile.MaxDirectControlRank} | Tact Rank: {_profile.MaxTacticalCommandRank}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Sync: {_profile.BaseSyncRate:P0} | Mecha: {_profile.MechaTrust} | Beast: {_profile.BeastTrust}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Balance: {_profile.BalanceScore}");

            if (_runtimeState != null)
            {
                y += 22;

                if (_runtimeState.SelectedTarget != null && _runtimeState.SelectedTarget.Data != null)
                {
                    var td = _runtimeState.SelectedTarget.Data;
                    GUI.Label(new Rect(x, y, _width, 18), $"Target: {td.DisplayName} [{td.Race}/{td.Faction}]");
                    y += 18;
                    GUI.Label(new Rect(x, y, _width, 18), $"  Role: {td.Role} Rank: {td.CommandRank} ReqLv: {td.RequiredCommanderLevel}");
                    y += 18;
                }
                else
                {
                    GUI.Label(new Rect(x, y, _width, 18), "Target: None (Tab to select)");
                    y += 18;
                }

                if (_lastResult.IsAllowed || !string.IsNullOrEmpty(_lastResult.Reason))
                {
                    var color = _lastResult.IsAllowed ? Color.green : Color.red;
                    GUI.color = color;
                    GUI.Label(new Rect(x, y, _width + 120, 18), $"Control: {_lastResult.Mode} | Sync: {_lastResult.SyncRate:P0}");
                    y += 18;
                    GUI.Label(new Rect(x, y, _width + 120, 18), $"Reason: {_lastResult.Reason}");
                    GUI.color = Color.white;
                    y += 18;
                }

                if (_runtimeState.IsDirectControllingOther && _runtimeState.DirectControlledEntity != null)
                {
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(x, y, _width, 18), $"DirectControl: {_runtimeState.DirectControlledEntity.Data?.DisplayName}");
                    GUI.color = Color.white;
                    y += 18;
                }

                if (_runtimeState.HasActiveCommand)
                {
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(x, y, _width, 18), $"Command: {_runtimeState.TacticalCommand.StatusText}");
                    GUI.color = Color.white;
                    y += 18;
                }

                if (_runtimeState.IsSyncAssistActive)
                {
                    GUI.color = new Color(0.5f, 1f, 1f);
                    GUI.Label(new Rect(x, y, _width, 18), $"SyncAssist: {_runtimeState.SyncAssistRemainingTime:F1}s");
                    GUI.color = Color.white;
                    y += 18;
                }
            }
        }
    }
}
