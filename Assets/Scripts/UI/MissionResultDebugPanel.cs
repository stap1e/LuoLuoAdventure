using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class MissionResultDebugPanel : MonoBehaviour
    {
        [SerializeField] private Vector2 _position = new Vector2(10, 350);
        [SerializeField] private int _width = 500;

        private MissionConsequence _lastConsequence;
        private float _displayTime;
        private const float DisplayDuration = 10f;

        public void ShowConsequence(MissionConsequence consequence)
        {
            _lastConsequence = consequence;
            _displayTime = Time.time;
        }

        private void OnGUI()
        {
            if (_lastConsequence == null) return;
            if (Time.time - _displayTime > DisplayDuration) return;

            var x = _position.x;
            var y = _position.y;

            GUI.Box(new Rect(x - 5, y - 5, _width + 10, 120), "");
            GUI.Label(new Rect(x, y, _width, 20), "=== Mission Result ===");
            y += 22;
            GUI.Label(new Rect(x, y, _width, 20), $"Outcome: {_lastConsequence.Outcome}");
            y += 18;
            GUI.Label(new Rect(x, y, _width, 20), $"Commander XP: +{_lastConsequence.CommanderExperienceDelta}");
            y += 18;

            if (_lastConsequence.FactionDeltas != null)
            {
                foreach (var delta in _lastConsequence.FactionDeltas)
                {
                    if (delta.TrustDelta != 0 || delta.HostilityDelta != 0)
                    {
                        GUI.Label(new Rect(x, y, _width, 18),
                            $"{delta.FactionId}: Trust{delta.TrustDelta:+#;-#;0} Hostility{delta.HostilityDelta:+#;-#;0}");
                        y += 16;
                    }
                }
            }

            y += 4;
            GUI.Label(new Rect(x, y, _width, 20), _lastConsequence.SummaryText);
        }
    }
}
