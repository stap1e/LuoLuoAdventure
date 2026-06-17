using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Combat
{
    /// <summary>
    /// Lightweight prototype-only debug controller.
    /// F1: toggle combat debug UI
    /// F2: reset player and all enemies HP/stamina/poise
    /// F3: reset positions to scene-start snapshots
    /// F4: reset all enemy states (revive, full HP, clear AI target)
    /// F6: toggle attack range gizmos/debug display
    /// </summary>
    public class CombatPrototypeDebugController : MonoBehaviour
    {
        [SerializeField] private bool _debugUIVisible = true;
        [SerializeField] private bool _attackGizmosEnabled = true;

        private readonly List<(Combatant combatant, Vector3 position)> _startSnapshots =
            new List<(Combatant, Vector3)>();
        private bool _warnedNoPlayer;

        public bool DebugUIVisible => _debugUIVisible;
        public bool AttackGizmosEnabled => _attackGizmosEnabled;

        private void Start()
        {
            foreach (var c in FindObjectsOfType<Combatant>())
                _startSnapshots.Add((c, c.transform.position));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _debugUIVisible = !_debugUIVisible;
                ToggleDebugUI(_debugUIVisible);
            }

            if (Input.GetKeyDown(KeyCode.F2))
                ResetAllHP();

            if (Input.GetKeyDown(KeyCode.F3))
                ResetPositions();

            if (Input.GetKeyDown(KeyCode.F4))
                ResetEnemyStates();

            if (Input.GetKeyDown(KeyCode.F6))
            {
                _attackGizmosEnabled = !_attackGizmosEnabled;
                ToggleAttackGizmos(_attackGizmosEnabled);
            }
        }

        private void ToggleDebugUI(bool visible)
        {
            foreach (var hud in FindObjectsOfType<CombatDebugHUD>())
                hud.enabled = visible;
        }

        private void ToggleAttackGizmos(bool enabled)
        {
            foreach (var c in FindObjectsOfType<Combatant>())
                c.ShowAttackDebug = enabled;
        }

        private static void ResetAllHP()
        {
            foreach (var c in FindObjectsOfType<Combatant>())
            {
                if (c.Stats.maxHealth <= 0f) continue;
                c.RestoreRuntimeState(c.Stats.maxHealth, c.Stats.maxStamina, c.Stats.maxPoise);
                if (c.CharacterEntity?.Data != null)
                    c.CharacterEntity.Data.IsAlive = true;
            }
            Debug.Log("[CombatDebug] F2: Reset all HP/Stamina/Poise to full");
        }

        private void ResetPositions()
        {
            foreach (var snap in _startSnapshots)
            {
                if (snap.combatant == null) continue;
                snap.combatant.transform.position = snap.position;
            }
            Debug.Log("[CombatDebug] F3: Reset positions to scene-start");
        }

        private static void ResetEnemyStates()
        {
            foreach (var ai in FindObjectsOfType<SimpleCombatAI>())
            {
                var c = ai.GetComponent<Combatant>();
                if (c == null || c.Stats.maxHealth <= 0f) continue;
                c.RestoreRuntimeState(c.Stats.maxHealth, c.Stats.maxStamina, c.Stats.maxPoise);
                if (c.CharacterEntity?.Data != null)
                    c.CharacterEntity.Data.IsAlive = true;
            }
            Debug.Log("[CombatDebug] F4: Reset all enemy states (revive + full HP)");
        }

        private void OnGUI()
        {
            if (!_debugUIVisible) return;
            var y = Screen.height - 120f;
            var rect = new Rect(10f, y, 400f, 110f);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            GUI.Label(new Rect(16f, y + 6f, 390f, 20f), "[Combat Debug] F1:UI  F2:HP  F3:Pos  F4:Enemy  F6:Gizmos", style);
            GUI.Label(new Rect(16f, y + 26f, 390f, 20f), $"Gizmos: {(_attackGizmosEnabled ? "ON" : "OFF")} | UI: {(_debugUIVisible ? "ON" : "OFF")}", style);

            var player = FindPlayer();
            if (player != null)
            {
                GUI.Label(new Rect(16f, y + 46f, 390f, 20f),
                    $"Player HP: {(int)player.CurrentHealth}/{(int)player.Stats.maxHealth}  State: {player.State}", style);
            }
            else if (!_warnedNoPlayer)
            {
                GUI.Label(new Rect(16f, y + 46f, 390f, 20f), "Player not found", style);
            }

            int enemyCount = 0;
            int aliveEnemies = 0;
            foreach (var c in FindObjectsOfType<Combatant>())
            {
                if (c.GetComponent<CombatController>() != null) continue;
                enemyCount++;
                if (c.IsAlive) aliveEnemies++;
            }
            GUI.Label(new Rect(16f, y + 66f, 390f, 20f), $"Enemies: {aliveEnemies}/{enemyCount} alive", style);
            GUI.color = prev;
        }

        private static Combatant FindPlayer()
        {
            foreach (var c in FindObjectsOfType<Combatant>())
                if (c.GetComponent<CombatController>() != null) return c;
            return null;
        }
    }
}
