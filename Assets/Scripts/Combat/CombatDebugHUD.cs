using UnityEngine;

namespace LuoLuoTrip.Combat
{
    public class CombatDebugHUD : MonoBehaviour
    {
        [SerializeField] private Combatant _target;
        [SerializeField] private bool _autoFindPlayer = true;
        [SerializeField] private bool _showAttackDebug = true;

        private Combatant _player;
        private CombatController _playerCombat;

        private void Start()
        {
            if (_autoFindPlayer)
            {
                foreach (var c in FindObjectsOfType<Combatant>())
                {
                    if (c.GetComponent<CombatController>() != null)
                    {
                        _player = c;
                        _playerCombat = c.GetComponent<CombatController>();
                        break;
                    }
                }
            }
            if (_showAttackDebug && _player != null) _player.ShowAttackDebug = true;
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

            var stateColor = StateColor(combatant.State);
            var prev = GUI.color;
            GUI.color = stateColor;
            GUI.Label(new Rect(10, y, 400, 20), $"State: {combatant.State}");
            GUI.color = prev;
            y += 20;

            if (combatant.State == CombatState.Attacking)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(10, y, 400, 20), "ATTACK ACTIVE");
                GUI.color = prev;
                y += 20;
            }

            GUI.Label(new Rect(10, y, 400, 20),
                $"AtkRange: {stats.attackRange:F1}  Dmg: {stats.attackPower:F0}  Last: {combatant.LastHitDamage:F0}");
            y += 20;

            if (_playerCombat != null)
            {
                GUI.Label(new Rect(10, y, 400, 20),
                    $"Input: {(_playerCombat.IsInputEnabled ? "ON" : "OFF")} | Speed: {_playerCombat.MoveSpeed:F1}");
            }
        }

        private static Color StateColor(CombatState s)
        {
            switch (s)
            {
                case CombatState.AttackWindup: return new Color(1f, 0.7f, 0f);
                case CombatState.Attacking: return Color.red;
                case CombatState.AttackRecovery: return new Color(0.5f, 0.5f, 1f);
                case CombatState.Staggered: return Color.magenta;
                case CombatState.Dodging: return Color.green;
                case CombatState.Dead: return Color.gray;
                default: return Color.white;
            }
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
