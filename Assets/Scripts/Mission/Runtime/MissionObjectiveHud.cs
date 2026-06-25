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
        private DemoFlowManager _demoFlow;

        public void SetDemoFlowManager(DemoFlowManager demoFlow)
        {
            _demoFlow = demoFlow;
        }

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

        public static string DisplayMissionName(string missionId)
        {
            if (missionId == DemoFlowManager.ConvoyMissionId)
                return "Mission 1: Convoy Energy Conflict";
            if (missionId == DemoFlowManager.BorderMissionId)
                return "Mission 2: Border Retaliation";
            if (missionId == DemoFlowManager.CityGateMissionId)
                return "Mission 3: City Gate Dispute";
            return string.IsNullOrEmpty(missionId) ? "No active mission" : missionId;
        }

        public static string[] BuildMissionStartLines(string missionId)
        {
            return missionId switch
            {
                DemoFlowManager.ConvoyMissionId => new[]
                {
                    "Mission Started: Mission 1: Convoy Energy Conflict",
                    "Primary Objective: Protect convoy",
                    "Optional Objective: Share energy and avoid excessive casualties",
                    "Suggested Action: Move to Convoy Mission Area, defend the convoy, then interact with Energy Node.",
                    "Suggested Command: Use G DefendObjective on Convoy or Energy Node."
                },
                DemoFlowManager.BorderMissionId => new[]
                {
                    "Mission Started: Mission 2: Border Retaliation",
                    "Primary Objective: Survive retaliation",
                    "Optional Objective: Defeat raiders and protect allied units",
                    "Suggested Action: Move to Border Retaliation Area and hold the Allied Defense Point.",
                    "Suggested Command: Use G on Allied Defense Point; F to FocusFire raiders."
                },
                DemoFlowManager.CityGateMissionId => new[]
                {
                    "Mission Started: Mission 3: City Gate Dispute",
                    "Primary Objective: Protect CityGateCore",
                    "Optional Objective: Keep BeastNegotiator alive, defeat raiders, keep casualties low",
                    "Suggested Action: Use F8 for demo positioning, then protect core/negotiator and defeat BeastRaiders.",
                    "Suggested Command: Use G on CityGateCore/BeastNegotiator; F on BeastRaider."
                },
                _ => new[]
                {
                    $"Mission Started: {DisplayMissionName(missionId)}",
                    "Primary Objective: Follow current objective checklist",
                    "Optional Objective: Keep casualties low",
                    "Suggested Action: Follow HUD markers and mission prompts.",
                    "Suggested Command: Use G to defend objectives and F to focus fire threats."
                }
            };
        }

        public static string BuildProgressSummary(MissionRuntimeState state, MissionPhase phase)
        {
            if (state == null)
                return "No active mission: follow DemoFlow guidance.";
            var objectiveCount = state.Objectives?.Count ?? 0;
            var protectedStatus = state.ProtectedConvoy ? "protected target alive" : "protected target vulnerable/dead";
            var raiderObjective = state.Objectives?.Find(o => o != null && (o.ObjectiveId == "defeat_raiders" || o.Description.Contains("raider")));
            var raiderStatus = raiderObjective != null && raiderObjective.IsCompleted;
            return $"Phase: {phase} | Objectives: {objectiveCount} | {protectedStatus} | Raiders defeated: {raiderStatus} | Casualties Mecha {state.MechaCasualties} / Beast {state.BeastCasualties}";
        }

        public static string BuildCompletionSummary(MissionRuntimeState state, MissionPhase phase)
        {
            if (state == null)
                return phase == MissionPhase.Failed ? "Mission Failed: no mission state available. Continue or retry from checkpoint." : "Mission Complete: no mission state available.";
            var result = phase == MissionPhase.Failed ? "Mission Failed" : "Mission Complete";
            var next = state.MissionId == DemoFlowManager.ConvoyMissionId ? "Next recommended mission: Border Retaliation"
                : state.MissionId == DemoFlowManager.BorderMissionId ? "Next recommended mission: City Gate Dispute"
                : state.MissionId == DemoFlowManager.CityGateMissionId ? "Next recommended step: Review Border / City stability"
                : "Next recommended step: Continue demo flow";
            return $"{result}: {DisplayMissionName(state.MissionId)} | Outcome: {state.Outcome} | What changed: consequences applied through MissionService | {next}";
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                DrawDemoFlowFallback();
                return;
            }

            if (_state == null)
            {
                DrawDemoFlowFallback();
                return;
            }

            var layout = DebugUILayout.MissionObjective;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            var phaseColor = _phase == MissionPhase.Completed ? Color.green
                : _phase == MissionPhase.Failed ? Color.red
                : _phase == MissionPhase.Resolving ? Color.yellow
                : Color.white;

            GUI.Box(new Rect(x - 4, y - 4, width + 8, _showFinal ? 120 : layout.height), "");

            GUI.color = phaseColor;
            GUI.Label(new Rect(x, y, width, 20), $"=== Mission: {DisplayMissionName(_state.MissionId)} [{_phase}] ===");
            GUI.color = Color.white;
            y += 22;

            var startLines = BuildMissionStartLines(_state.MissionId);
            GUI.Label(new Rect(x, y, width + 160, 18), startLines[1]);
            y += 18;
            GUI.Label(new Rect(x, y, width + 220, 18), startLines[3]);
            y += 18;
            if (startLines.Length > 4)
            {
                GUI.color = Color.cyan;
                GUI.Label(new Rect(x, y, width + 220, 18), startLines[4]);
                GUI.color = Color.white;
                y += 18;
            }
            GUI.Label(new Rect(x, y, width + 220, 18), BuildProgressSummary(_state, _phase));
            y += 20;

            if (_showFinal)
            {
                GUI.Label(new Rect(x, y, width, 20), $"Outcome: {_state.Outcome}");
                y += 20;
                GUI.Label(new Rect(x, y, width, 20), $"Casualties: Mecha {_state.MechaCasualties} / Beast {_state.BeastCasualties}");
                y += 20;
                GUI.Label(new Rect(x, y, width + 220, 20), BuildCompletionSummary(_state, _phase));
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

            if (_state.Objectives != null)
            {
                foreach (var obj in _state.Objectives)
                {
                    if (obj == null) continue;
                    var status = obj.IsCompleted ? "[DONE]" : obj.IsFailed ? "[FAIL]" : $"[{obj.Progress}/{obj.RequiredProgress}]";
                    GUI.Label(new Rect(x, y, width, 18), $"{obj.Description}: {status}");
                    y += 18;
                }
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

        private void DrawDemoFlowFallback()
        {
            if (_demoFlow == null)
                _demoFlow = FindObjectOfType<DemoFlowManager>();
            if (_demoFlow == null)
                return;

            _demoFlow.RefreshFromMissionChain();

            var layout = DebugUILayout.MissionObjective;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            GUI.Box(new Rect(x - 4, y - 4, width + 8, 92), "");
            GUI.color = Color.cyan;
            GUI.Label(new Rect(x, y, width, 20), "=== Next Demo Objective ===");
            GUI.color = Color.white;
            y += 22;
            GUI.Label(new Rect(x, y, width, 18), _demoFlow.CurrentStepDisplayName);
            y += 18;
            GUI.Label(new Rect(x, y, width, 18), $"Go to: {_demoFlow.CurrentWorldTargetName}");
            y += 18;
            GUI.Label(new Rect(x, y, width + 140, 18), _demoFlow.CurrentObjectiveHint);
        }
    }
}
