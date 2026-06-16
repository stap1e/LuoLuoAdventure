using System;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class MissionResultSummaryPanel : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;
        [SerializeField] private float _displayDuration = 15f;

        private MissionConsequence _consequence;
        private CommanderProfile _profileBefore;
        private CommanderProfile _profileAfter;
        private string _missionName;
        private string _unlockedMission;
        private MissionModifier _modifier;
        private float _displayTimer;
        private bool _showPanel;

        public void ShowSummary(string missionName, MissionConsequence consequence,
            CommanderProfile profileBefore, CommanderProfile profileAfter,
            string unlockedMission = null, MissionModifier modifier = null)
        {
            _missionName = missionName;
            _consequence = consequence;
            _profileBefore = profileBefore;
            _profileAfter = profileAfter;
            _unlockedMission = unlockedMission;
            _modifier = modifier;
            _displayTimer = _displayDuration;
            _showPanel = true;
        }

        public void Dismiss()
        {
            _showPanel = false;
        }

        private void Update()
        {
            if (!_showPanel) return;
            _displayTimer -= Time.deltaTime;
            if (_displayTimer <= 0f || Input.GetKeyDown(KeyCode.Space))
                _showPanel = false;
        }

        private void OnGUI()
        {
            if (!_visible || !_showPanel || _consequence == null) return;

            var layout = DebugUILayout.MissionResultSummary;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;
            var height = 320;

            GUI.Box(new Rect(x - 4, y - 4, width + 8, height), "");
            GUI.Label(new Rect(x, y, width, 22), $"=== Mission Result: {_missionName} ===");
            y += 24;

            GUI.color = Color.yellow;
            GUI.Label(new Rect(x, y, width, 18), $"Outcome: {_consequence.Outcome}");
            GUI.color = Color.white;
            y += 20;

            GUI.Label(new Rect(x, y, width, 18), $"Commander XP: +{_consequence.CommanderExperienceDelta}");
            y += 18;

            if (_profileBefore != null && _profileAfter != null)
            {
                var levelBefore = _profileBefore.CommanderLevel;
                var levelAfter = _profileAfter.CommanderLevel;
                GUI.Label(new Rect(x, y, width, 18), $"Level: {levelBefore} -> {levelAfter}");
                y += 18;
                if (levelAfter > levelBefore)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(x, y, width, 18), "LEVEL UP!");
                    GUI.color = Color.white;
                    y += 18;
                }
            }

            y += 6;
            GUI.Label(new Rect(x, y, width, 18), "Faction Changes:");
            y += 18;

            if (_consequence.FactionDeltas != null)
            {
                foreach (var delta in _consequence.FactionDeltas)
                {
                    var line = $"  {delta.FactionId}: Trust{delta.TrustDelta:+#;-#;0} Hostility{delta.HostilityDelta:+#;-#;0} Respect{delta.RespectDelta:+#;-#;0}";
                    GUI.Label(new Rect(x, y, width, 16), line);
                    y += 16;
                }
            }

            y += 6;
            if (!string.IsNullOrEmpty(_unlockedMission))
            {
                GUI.color = Color.cyan;
                GUI.Label(new Rect(x, y, width, 18), $"Next mission unlocked: {_unlockedMission}");
                GUI.color = Color.white;
                y += 18;
            }

            if (_modifier != null && !string.IsNullOrEmpty(_modifier.Description))
            {
                GUI.Label(new Rect(x, y, width, 18), $"Modifier: {_modifier.Description}");
                y += 18;
            }

            GUI.Label(new Rect(x, y, width, 18), _consequence.SummaryText ?? "");
            y += 22;

            var remaining = Mathf.CeilToInt(Mathf.Max(0, _displayTimer));
            GUI.Label(new Rect(x, y, width, 16), $"[Space to dismiss] ({remaining}s)");
        }
    }
}
