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
        private MissionPhase _phase;
        private bool _visible;
        private bool _showFinal;

        public void UpdateDisplay(MissionRuntimeState state, ConvoyObjective convoy, EnergyNodeObjective energyNode, MissionPhase phase)
        {
            _state = state;
            _convoy = convoy;
            _energyNode = energyNode;
            _phase = phase;
            _visible = state != null;
            _showFinal = false;
        }

        public void ShowFinalResult(MissionRuntimeState state, MissionPhase phase)
        {
            _state = state;
            _phase = phase;
            _showFinal = true;
            _visible = true;
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

            var phaseColor = _phase == MissionPhase.Completed ? Color.green
                : _phase == MissionPhase.Failed ? Color.red
                : _phase == MissionPhase.Resolving ? Color.yellow
                : Color.white;

            GUI.Box(new Rect(x - 4, y - 4, _width + 8, _showFinal ? 100 : 160), "");

            GUI.color = phaseColor;
            GUI.Label(new Rect(x, y, _width, 20), $"=== Mission: {_state.MissionId} [{_phase}] ===");
            GUI.color = Color.white;
            y += 22;

            if (_showFinal)
            {
                GUI.Label(new Rect(x, y, _width, 20), $"Outcome: {_state.Outcome}");
                y += 20;
                GUI.Label(new Rect(x, y, _width, 20), $"Casualties: Mecha {_state.MechaCasualties} / Beast {_state.BeastCasualties}");
                return;
            }

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
                var sharePct = _energyNode.IsSharedByPlayer ? 1f : 0f;
                GUI.Label(new Rect(x, y, _width, 18), $"Energy Node Capture: {capPct:P0} | Shared: {sharePct:P0}");
                y += 18;
            }
        }
    }
}
