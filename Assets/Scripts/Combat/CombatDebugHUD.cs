using UnityEngine;

namespace LuoLuoTrip.Combat
{
    /// <summary>战斗 HUD 调试显示（原型用）</summary>
    public class CombatDebugHUD : MonoBehaviour
    {
        [SerializeField] private Combatant _target;
        [SerializeField] private bool _autoFindPlayer = true;

        private Combatant _player;

        private void Start()
        {
            if (_autoFindPlayer)
            {
                foreach (var c in FindObjectsByType<Combatant>(FindObjectsSortMode.None))
                {
                    if (c.GetComponent<CombatController>() != null)
                    {
                        _player = c;
                        break;
                    }
                }
            }
        }

        private void OnGUI()
        {
            var combatant = _target != null ? _target : _player;
            if (combatant == null || !combatant.IsAlive) return;

            var stats = combatant.Stats;
            var y = 10f;
            DrawBar(10, y, 200, 16, combatant.CurrentHealth / stats.maxHealth, Color.red, "HP");
            y += 22;
            DrawBar(10, y, 200, 12, combatant.CurrentStamina / stats.maxStamina, Color.yellow, "ST");
            y += 18;
            DrawBar(10, y, 200, 12, combatant.CurrentPoise / stats.maxPoise, Color.cyan, "Poise");
            y += 24;
            GUI.Label(new Rect(10, y, 400, 20), $"State: {combatant.State}");
        }

        private static void DrawBar(float x, float y, float w, float h, float ratio, Color color, string label)
        {
            GUI.color = Color.black;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.color = color;
            GUI.Box(new Rect(x + 1, y + 1, (w - 2) * Mathf.Clamp01(ratio), h - 2), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 4, y, w, h), label);
        }
    }
}
