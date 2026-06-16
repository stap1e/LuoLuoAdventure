using System;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class FactionStandingDebugPanel : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;

        private FactionReputationService _service;

        public void SetService(FactionReputationService service) => _service = service;

        private void OnGUI()
        {
            if (!_visible || _service == null) return;

            var layout = DebugUILayout.FactionStanding;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            GUI.Label(new Rect(x, y, width, 20), "=== Faction Standing ===");
            y += 22;

            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                var standing = _service.GetStanding(id);
                var name = id.ToString();
                if (name.Length > 16) name = name.Substring(0, 16);
                GUI.Label(new Rect(x, y, width, 18),
                    $"{name}: T={standing.Trust} R={standing.Respect} H={standing.Hostility} WE={standing.WarExhaustion}");
                y += 18;
            }
        }
    }
}
