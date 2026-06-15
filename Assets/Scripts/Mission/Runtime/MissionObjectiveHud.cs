using UnityEngine;

namespace LuoLuoTrip
{
    public class MissionObjectiveHud : MonoBehaviour
    {
        [SerializeField] private Vector2 _position = new Vector2(10, 10);
        [SerializeField] private int _width = 350;

        private MissionRuntimeState _state;
        private ConvoyObjective _convoy;
        private EnergyNodeObjective _energyNode;
        private bool _visible;

        public void UpdateDisplay(MissionRuntimeState state, ConvoyObjective convoy, EnergyNodeObjective energyNode)
        {
            _state = state;
            _convoy = convoy;
            _energyNode = energyNode;
            _visible = state != null;
        }

        public void Hide()
        {
            _visible = false;
        }

        private void OnGUI()
        {
            if (!_visible || _state == null) return;

            var x = _position.x;
            var y = _position.y;

            GUI.Box(new Rect(x - 4, y - 4, _width + 8, 120), "");
            GUI.Label(new Rect(x, y, _width, 20), $"=== Mission: {_state.MissionId} ===");
            y += 22;

            foreach (var obj in _state.Objectives)
            {
                var status = obj.IsCompleted ? "[DONE]" : obj.IsFailed ? "[FAIL]" : $"[{obj.Progress}/{obj.RequiredProgress}]";
                GUI.Label(new Rect(x, y, _width, 18), $"{obj.Description}: {status}");
                y += 18;
            }

            if (_convoy != null)
            {
                GUI.Label(new Rect(x, y, _width, 18), $"Convoy HP: {_convoy.HealthRatio:P0}");
                y += 18;
            }

            if (_energyNode != null)
            {
                var capPct = _energyNode.BeastCaptureTime > 0
                    ? _energyNode.BeastCaptureProgress / _energyNode.BeastCaptureTime
                    : 0f;
                GUI.Label(new Rect(x, y, _width, 18), $"Energy Node Capture: {capPct:P0}");
                y += 18;
            }
        }
    }
}
