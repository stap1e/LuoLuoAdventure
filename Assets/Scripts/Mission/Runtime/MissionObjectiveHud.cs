using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip
{
    public class MissionObjectiveHud : MonoBehaviour
    {
        private MissionRuntimeState _state;
        private ConvoyObjective _convoy;
        private EnergyNodeObjective _energyNode;
        private MissionPhase _phase;
        private bool _visible;
        private bool _showFinal;
        private MissionAreaRuntime _areaRuntime;

        public void UpdateDisplay(MissionRuntimeState state, ConvoyObjective convoy, EnergyNodeObjective energyNode, MissionPhase phase)
        {
            _state = state;
            _convoy = convoy;
            _energyNode = energyNode;
            _phase = phase;
            _visible = state != null;
            _showFinal = false;
        }

        public void SetAreaRuntime(MissionAreaRuntime area)
        {
            _areaRuntime = area;
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

            var layout = DebugUILayout.MissionObjective;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            var phaseColor = _phase == MissionPhase.Completed ? Color.green
                : _phase == MissionPhase.Failed ? Color.red
                : _phase == MissionPhase.Resolving ? Color.yellow
                : Color.white;

            GUI.Box(new Rect(x - 4, y - 4, width + 8, _showFinal ? 100 : 200), "");

            GUI.color = phaseColor;
            GUI.Label(new Rect(x, y, width, 20), $"=== Mission: {_state.MissionId} [{_phase}] ===");
            GUI.color = Color.white;
            y += 22;

            if (_showFinal)
            {
                GUI.Label(new Rect(x, y, width, 20), $"Outcome: {_state.Outcome}");
                y += 20;
                GUI.Label(new Rect(x, y, width, 20), $"Casualties: Mecha {_state.MechaCasualties} / Beast {_state.BeastCasualties}");
                return;
            }

            if (_areaRuntime != null && _areaRuntime.IsActive)
            {
                var areaColor = _areaRuntime.IsPlayerInside ? Color.green : Color.red;
                GUI.color = areaColor;
                var areaStatus = _areaRuntime.IsPlayerInside ? "INSIDE" : "OUTSIDE";
                GUI.Label(new Rect(x, y, width, 18), $"Area: {areaStatus}");
                GUI.color = Color.white;
                y += 18;

                if (!_areaRuntime.IsPlayerInside && _areaRuntime.Retreat != null)
                {
                    var retreatProgress = _areaRuntime.Retreat.Progress;
                    if (retreatProgress > 0f)
                    {
                        GUI.color = Color.red;
                        GUI.Label(new Rect(x, y, width, 18), $"Retreat in: {_areaRuntime.Retreat.RetreatTime - _areaRuntime.Retreat.CurrentTimer:F1}s");
                        GUI.color = Color.white;
                        y += 18;
                    }
                }
            }

            foreach (var obj in _state.Objectives)
            {
                var status = obj.IsCompleted ? "[DONE]" : obj.IsFailed ? "[FAIL]" : $"[{obj.Progress}/{obj.RequiredProgress}]";
                GUI.Label(new Rect(x, y, width, 18), $"{obj.Description}: {status}");
                y += 18;
            }

            if (_convoy != null)
            {
                GUI.Label(new Rect(x, y, width, 18), $"Convoy HP: {_convoy.HealthRatio:P0}");
                y += 18;
            }

            if (_energyNode != null)
            {
                var capPct = _energyNode.BeastCaptureTime > 0
                    ? _energyNode.BeastCaptureProgress / _energyNode.BeastCaptureTime
                    : 0f;
                var sharePct = _energyNode.IsSharedByPlayer ? 1f : 0f;
                GUI.Label(new Rect(x, y, width, 18), $"Energy Node Capture: {capPct:P0} | Shared: {sharePct:P0}");
                y += 18;
            }
        }
    }
}
