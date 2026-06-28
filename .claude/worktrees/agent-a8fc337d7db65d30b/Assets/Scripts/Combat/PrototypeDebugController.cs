using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Combat
{
    public class PrototypeDebugController : MonoBehaviour
    {
        [SerializeField] private bool _debugUIVisible = true;
        [SerializeField] private Vector3 _cityGateTeleportPosition = new Vector3(50f, 0.5f, -4f);

        private readonly List<(Combatant combatant, Vector3 position)> _startSnapshots = new List<(Combatant, Vector3)>();
        private bool _warnedMissingPlayerForCityGateTeleport;

        public bool DebugUIVisible => _debugUIVisible;

        private void Start()
        {
            RefreshSnapshots();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _debugUIVisible = !_debugUIVisible;
                foreach (var hud in FindObjectsOfType<CombatDebugHUD>())
                    hud.enabled = _debugUIVisible;
            }

            if (Input.GetKeyDown(KeyCode.F2))
                RevivePlayer();

            if (Input.GetKeyDown(KeyCode.F3))
                ResetPlayerPosition();

            if (Input.GetKeyDown(KeyCode.F4))
                ResetEncounterEnemies();

            if (Input.GetKeyDown(KeyCode.F8))
                TeleportPlayerToCityGateDisputeArea();
        }

        public void RefreshSnapshots()
        {
            _startSnapshots.Clear();
            foreach (var c in FindObjectsOfType<Combatant>())
                _startSnapshots.Add((c, c.transform.position));
        }

        public void RevivePlayer()
        {
            var controller = FindActivePlayerController();
            if (controller == null)
            {
                Debug.LogWarning("[PROTOTYPE DEBUG] F2 revive failed: no CombatController found");
                return;
            }

            var combatant = controller.GetComponent<Combatant>();
            if (combatant == null || combatant.Stats.maxHealth <= 0f)
            {
                Debug.LogWarning("[PROTOTYPE DEBUG] F2 revive failed: Combatant missing or invalid stats");
                return;
            }

            combatant.RestoreRuntimeState(combatant.Stats.maxHealth, combatant.Stats.maxStamina, combatant.Stats.maxPoise);
            if (combatant.CharacterEntity?.Data != null)
                combatant.CharacterEntity.Data.IsAlive = true;
            controller.SetInputEnabled(true);
            Debug.Log($"[PROTOTYPE DEBUG] F2 revive: {controller.name} HP restored, input enabled");
        }

        public void ResetPlayerPosition()
        {
            var controller = FindActivePlayerController();
            if (controller == null) return;
            for (int i = 0; i < _startSnapshots.Count; i++)
            {
                if (_startSnapshots[i].combatant == null) continue;
                if (_startSnapshots[i].combatant.GetComponent<CombatController>() != controller) continue;
                controller.transform.position = _startSnapshots[i].position;
                Debug.Log($"[PROTOTYPE DEBUG] F3 reset position: {controller.name}");
                return;
            }
            Debug.LogWarning("[PROTOTYPE DEBUG] F3 reset position failed: no start snapshot");
        }

        public void ResetEncounterEnemies()
        {
            int reset = 0;
            foreach (var ai in FindObjectsOfType<SimpleCombatAI>())
            {
                var c = ai.GetComponent<Combatant>();
                if (c == null || c.Stats.maxHealth <= 0f) continue;
                c.RestoreRuntimeState(c.Stats.maxHealth, c.Stats.maxStamina, c.Stats.maxPoise);
                if (c.CharacterEntity?.Data != null)
                    c.CharacterEntity.Data.IsAlive = true;
                ai.ForcedAttackTarget = null;
                ai.FollowTarget = null;
                ai.HoldPosition = null;
                reset++;
            }

            foreach (var encounter in FindObjectsOfType<EncounterRuntime>())
                encounter.DespawnDeadUnits();

            Debug.Log($"[PROTOTYPE DEBUG] F4 reset encounter enemies: {reset}");
        }

        public void TeleportPlayerToCityGateDisputeArea()
        {
            var controller = FindActivePlayerController();
            if (controller == null)
            {
                if (!_warnedMissingPlayerForCityGateTeleport)
                {
                    Debug.LogWarning("[DEBUG TRIGGER] F8 CityGate teleport failed: player missing");
                    _warnedMissingPlayerForCityGateTeleport = true;
                }
                return;
            }

            var destination = ResolveCityGateTeleportPosition();
            var motor = controller.GetComponent<CharacterMovementMotor>();
            if (motor != null)
            {
                motor.SetGroundY(destination.y);
                motor.TeleportTo(destination);
            }
            else
            {
                controller.transform.position = destination;
            }

            var ai = controller.GetComponent<SimpleCombatAI>();
            if (ai != null)
            {
                ai.ForcedAttackTarget = null;
                ai.FollowTarget = null;
                ai.HoldPosition = null;
                if (ai.NavController != null)
                    ai.NavController.ClearNavigation();
            }

            Debug.Log($"[DEBUG TRIGGER] F8: Teleported {controller.name} to CityGateDispute area at {destination}");
        }

        private Vector3 ResolveCityGateTeleportPosition()
        {
            var trigger = GameObject.Find("CityGateDisputeTrigger");
            if (trigger != null)
                return trigger.transform.position + new Vector3(0f, 0.5f, -4f);
            return _cityGateTeleportPosition;
        }

        private static CombatController FindActivePlayerController()
        {
            CombatController fallback = null;
            foreach (var ctrl in FindObjectsOfType<CombatController>())
            {
                if (fallback == null) fallback = ctrl;
                if (ctrl.IsInputEnabled) return ctrl;
            }
            return fallback;
        }
    }
}
