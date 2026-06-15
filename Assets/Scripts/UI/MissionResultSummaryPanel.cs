using System;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class MissionResultSummaryPanel : MonoBehaviour
    {
        [SerializeField] private Vector2 _position = new Vector2(200, 50);
        [SerializeField] private int _width = 500;
        [SerializeField] private float _displayDuration = 15f;

        private MissionConsequence _consequence;
        private CommanderProfile _profileBefore;
        private CommanderProfile _profileAfter;
        private string _missionName;
        private string _unlockedMission;
        private MissionModifier _modifier;
        private float _displayTimer;
        private bool _visible;

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
            _visible = true;
        }

        public void Dismiss()
        {
            _visible = false;
        }

        private void Update()
        {
            if (!_visible) return;
            _displayTimer -= Time.deltaTime;
            if (_displayTimer <= 0f || Input.GetKeyDown(KeyCode.Space))
                _visible = false;
        }

        private void OnGUI()
        {
            if (!_visible || _consequence == null) return;

            var x = _position.x;
            var y = _position.y;
            var height = 320;

            GUI.Box(new Rect(x - 4, y - 4, _width + 8, height), "");
            GUI.Label(new Rect(x, y, _width, 22), $"=== Mission Result: {_missionName} ===");
            y += 24;

            GUI.color = Color.yellow;
            GUI.Label(new Rect(x, y, _width, 18), $"Outcome: {_consequence.Outcome}");
            GUI.color = Color.white;
            y += 20;

            GUI.Label(new Rect(x, y, _width, 18), $"Commander XP: +{_consequence.CommanderExperienceDelta}");
            y += 18;

            if (_profileBefore != null && _profileAfter != null)
            {
                var levelBefore = _profileBefore.CommanderLevel;
                var levelAfter = _profileAfter.CommanderLevel;
                GUI.Label(new Rect(x, y, _width, 18), $"Level: {levelBefore} -> {levelAfter}");
                y += 18;
                if (levelAfter > levelBefore)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(x, y, _width, 18), "LEVEL UP!");
                    GUI.color = Color.white;
                    y += 18;
                }
            }

            y += 6;
            GUI.Label(new Rect(x, y, _width, 18), "Faction Changes:");
            y += 18;

            if (_consequence.FactionDeltas != null)
            {
                foreach (var delta in _consequence.FactionDeltas)
                {
                    var line = $"  {delta.FactionId}: Trust{delta.TrustDelta:+#;-#;0} Hostility{delta.HostilityDelta:+#;-#;0} Respect{delta.RespectDelta:+#;-#;0}";
                    GUI.Label(new Rect(x, y, _width, 16), line);
                    y += 16;
                }
            }

            y += 6;
            if (!string.IsNullOrEmpty(_unlockedMission))
            {
                GUI.color = Color.cyan;
                GUI.Label(new Rect(x, y, _width, 18), $"Next mission unlocked: {_unlockedMission}");
                GUI.color = Color.white;
                y += 18;
            }

            if (_modifier != null && !string.IsNullOrEmpty(_modifier.Description))
            {
                GUI.Label(new Rect(x, y, _width, 18), $"Modifier: {_modifier.Description}");
                y += 18;
            }

            GUI.Label(new Rect(x, y, _width, 18), _consequence.SummaryText ?? "");
            y += 22;

            var remaining = Mathf.CeilToInt(Mathf.Max(0, _displayTimer));
            GUI.Label(new Rect(x, y, _width, 16), $"[Space to dismiss] ({remaining}s)");
        }
    }
}
