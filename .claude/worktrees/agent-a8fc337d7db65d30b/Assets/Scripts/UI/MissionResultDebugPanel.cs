using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class MissionResultDebugPanel : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;

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
            if (!_visible) return;
            if (_lastConsequence == null) return;
            if (Time.time - _displayTime > DisplayDuration) return;

            var layout = DebugUILayout.MissionResultDebug;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            GUI.Box(new Rect(x - 5, y - 5, width + 10, 120), "");
            GUI.Label(new Rect(x, y, width, 20), "=== Mission Result ===");
            y += 22;
            GUI.Label(new Rect(x, y, width, 20), $"Outcome: {_lastConsequence.Outcome}");
            y += 18;
            GUI.Label(new Rect(x, y, width, 20), $"Commander XP: +{_lastConsequence.CommanderExperienceDelta}");
            y += 18;

            if (_lastConsequence.FactionDeltas != null)
            {
                foreach (var delta in _lastConsequence.FactionDeltas)
                {
                    if (delta.TrustDelta != 0 || delta.HostilityDelta != 0)
                    {
                        GUI.Label(new Rect(x, y, width, 18),
                            $"{delta.FactionId}: Trust{delta.TrustDelta:+#;-#;0} Hostility{delta.HostilityDelta:+#;-#;0}");
                        y += 16;
                    }
                }
            }

            y += 4;
            GUI.Label(new Rect(x, y, width, 20), _lastConsequence.SummaryText);
        }
    }
}
