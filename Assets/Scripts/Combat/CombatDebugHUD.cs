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

            if (combatant == null) return;

            var stats = combatant.Stats;
            if (!combatant.IsAlive)
            {
                var deadRect = new Rect(Screen.width / 2f - 180f, Screen.height / 2f - 50f, 360f, 100f);
                var prev = GUI.color;
                GUI.color = new Color(0.4f, 0f, 0f, 0.75f);
                GUI.DrawTexture(deadRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                var hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(new Rect(deadRect.x, deadRect.y + 6f, deadRect.width, 34f), "PLAYER DEAD", titleStyle);
                GUI.Label(new Rect(deadRect.x, deadRect.y + 46f, deadRect.width, 20f), "Input disabled: player is dead", hintStyle);
                GUI.Label(new Rect(deadRect.x, deadRect.y + 68f, deadRect.width, 20f), "Press F2 to revive in prototype", hintStyle);
                GUI.color = prev;
            }
            var y = 10f;
            DrawBar(10, y, 200, 16, combatant.CurrentHealth / stats.maxHealth, Color.red, "HP");
            y += 22;
            DrawBar(10, y, 200, 12, combatant.CurrentStamina / stats.maxStamina, Color.yellow, "ST");
            y += 18;
            DrawBar(10, y, 200, 12, combatant.CurrentPoise / stats.maxPoise, Color.cyan, "Poise");
            y += 24;

            var stateColor = StateColor(combatant.State);
            var prevColor = GUI.color;
            GUI.color = stateColor;
            GUI.Label(new Rect(10, y, 400, 20), $"State: {combatant.State}");
            GUI.color = prevColor;
            y += 20;

            if (combatant.State == CombatState.Attacking)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(10, y, 400, 20), "ATTACK ACTIVE");
                GUI.color = prevColor;
                y += 20;
            }

            if (combatant.State == CombatState.Staggered)
            {
                GUI.color = Color.magenta;
                GUI.Label(new Rect(10, y, 400, 20), "STAGGERED");
                GUI.color = prevColor;
                y += 20;
            }

            GUI.Label(new Rect(10, y, 400, 20),
                $"AtkRange: {stats.attackRange:F1}  Dmg: {stats.attackPower:F0}  Last: {combatant.LastHitDamage:F0}");
            y += 20;

            if (_playerCombat != null)
            {
                GUI.Label(new Rect(10, y, 520, 20),
                    $"Input: {(_playerCombat.IsInputEnabled ? "ON" : "OFF")} | Speed: {_playerCombat.MoveSpeed:F1}");
                y += 20;
                GUI.Label(new Rect(10, y, 640, 20),
                    $"Attack: {_playerCombat.LastAttackResult}  Reason: {_playerCombat.LastAttackRejectReason}");
                y += 20;
                var dist = _playerCombat.LastAttackDistance >= 0f ? _playerCombat.LastAttackDistance.ToString("F1") : "-";
                var range = _playerCombat.LastAttackRange >= 0f ? _playerCombat.LastAttackRange.ToString("F1") : "-";
                GUI.Label(new Rect(10, y, 640, 20),
                    $"Target: {_playerCombat.LastAttackTargetName}  Distance: {dist}/{range}  State: {_playerCombat.LastAttackState}");
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
