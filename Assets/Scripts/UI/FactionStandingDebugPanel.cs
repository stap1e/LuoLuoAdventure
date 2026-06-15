using System;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class FactionStandingDebugPanel : MonoBehaviour
    {
        [SerializeField] private Vector2 _position = new Vector2(300, 150);
        [SerializeField] private int _width = 360;

        private FactionReputationService _service;

        public void SetService(FactionReputationService service) => _service = service;

        private void OnGUI()
        {
            if (_service == null) return;

            var x = _position.x;
            var y = _position.y;

            GUI.Label(new Rect(x, y, _width, 20), "=== Faction Standing ===");
            y += 22;

            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                var standing = _service.GetStanding(id);
                var name = id.ToString();
                if (name.Length > 16) name = name.Substring(0, 16);
                GUI.Label(new Rect(x, y, _width, 18),
                    $"{name}: T={standing.Trust} R={standing.Respect} H={standing.Hostility} WE={standing.WarExhaustion}");
                y += 18;
            }
        }
    }
}
