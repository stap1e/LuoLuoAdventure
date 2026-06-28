using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class MissionChainSummaryPanel : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;

        private MissionChainService _chainService;
        private CommanderProfile _profile;

        public void SetChainService(MissionChainService service) => _chainService = service;
        public void SetProfile(CommanderProfile profile) => _profile = profile;

        private void OnGUI()
        {
            if (!_visible) return;
            if (_chainService == null) return;

            var layout = DebugUILayout.MissionChainSummary;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            GUI.Label(new Rect(x, y, width, 20), "=== Mission Chain ===");
            y += 20;

            var state = _chainService.State;
            if (state.CompletedMissions.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, 18), "No missions completed yet.");
                return;
            }

            foreach (var entry in state.CompletedMissions)
            {
                var color = entry.Outcome == MissionOutcomeType.Failed ? Color.red : Color.green;
                GUI.color = color;
                GUI.Label(new Rect(x, y, width, 18),
                    $"#{entry.SequenceIndex} {entry.MissionId}: {entry.Outcome} (+{entry.CommanderExperienceDelta} XP)");
                GUI.color = Color.white;
                y += 18;

                if (entry.SharedEnergy)
                {
                    GUI.Label(new Rect(x + 20, y, width - 20, 16), "  SharedEnergy");
                    y += 16;
                }
                if (entry.ConvoyDestroyed)
                {
                    GUI.Label(new Rect(x + 20, y, width - 20, 16), "  ConvoyDestroyed");
                    y += 16;
                }
                if (entry.BeastRaidDefeated)
                {
                    GUI.Label(new Rect(x + 20, y, width - 20, 16), "  BeastRaidDefeated");
                    y += 16;
                }
            }

            y += 8;
            GUI.Label(new Rect(x, y, width, 18), "Unlocked:");
            y += 18;
            foreach (var id in state.UnlockedMissionIds)
            {
                var completed = state.HasCompleted(id);
                GUI.color = completed ? Color.gray : Color.yellow;
                GUI.Label(new Rect(x + 20, y, width - 20, 16),
                    $"{id}{(completed ? " (done)" : "")}");
                GUI.color = Color.white;
                y += 16;
            }

            if (_profile != null)
            {
                y += 8;
                GUI.Label(new Rect(x, y, width, 18),
                    $"Commander Lv.{_profile.CommanderLevel} | XP: {_profile.Experience}");
            }
        }
    }
}
