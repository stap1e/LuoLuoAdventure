using System;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class CommanderDebugHud : MonoBehaviour
    {
        [SerializeField] private Vector2 _position = new Vector2(10, 150);
        [SerializeField] private int _width = 280;

        private CommanderProfile _profile;
        private ControlPermissionResult _lastResult;

        public void SetProfile(CommanderProfile profile) => _profile = profile;
        public void SetLastControlResult(ControlPermissionResult result) => _lastResult = result;

        private void OnGUI()
        {
            if (_profile == null) return;

            var x = _position.x;
            var y = _position.y;

            GUI.Label(new Rect(x, y, _width, 20), $"=== Commander (Lv.{_profile.CommanderLevel}) ===");
            y += 20;
            GUI.Label(new Rect(x, y, _width, 20), $"XP: {_profile.Experience}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Command Capacity: {_profile.CommandCapacity}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Max Direct Control Rank: {_profile.MaxDirectControlRank}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Max Tactical Cmd Rank: {_profile.MaxTacticalCommandRank}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Base Sync Rate: {_profile.BaseSyncRate:P0}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Mecha Trust: {_profile.MechaTrust} | Beast Trust: {_profile.BeastTrust}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Balance Score: {_profile.BalanceScore}");

            if (_lastResult.IsAllowed || _lastResult.Mode != ControlMode.Denied || !string.IsNullOrEmpty(_lastResult.Reason))
            {
                y += 24;
                var color = _lastResult.IsAllowed ? Color.green : Color.red;
                GUI.color = color;
                GUI.Label(new Rect(x, y, _width + 120, 20), $"Control: {_lastResult.Mode} | Sync: {_lastResult.SyncRate:P0}");
                y += 18;
                GUI.Label(new Rect(x, y, _width + 120, 20), $"Reason: {_lastResult.Reason}");
                GUI.color = Color.white;
            }
        }
    }
}
