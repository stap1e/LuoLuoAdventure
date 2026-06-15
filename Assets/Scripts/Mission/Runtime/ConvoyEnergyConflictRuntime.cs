using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    public class ConvoyEnergyConflictRuntime : MonoBehaviour
    {
        [SerializeField] private ConvoyObjective _convoy;
        [SerializeField] private EnergyNodeObjective _energyNode;
        [SerializeField] private MissionTriggerZone _triggerZone;
        [SerializeField] private MissionObjectiveHud _objectiveHud;
        [SerializeField] private float _abandonDistance = 20f;
        [SerializeField] private float _abandonTime = 10f;

        private MissionService _missionService;
        private MissionRuntimeState _missionState;
        private int _mechaCasualties;
        private int _beastCasualties;
        private float _abandonTimer;
        private bool _isActive;
        private List<CharacterEntity> _beastEntities = new List<CharacterEntity>();

        public bool IsActive => _isActive;

        private void Start()
        {
            var context = GameBootstrap.Context;
            if (context != null)
                _missionService = context.MissionService;
        }

        private void Update()
        {
            if (_missionService == null) return;

            if (!_isActive)
            {
                if (_triggerZone != null && _triggerZone.MissionStarted)
                    StartConflict();
                return;
            }

            UpdateBeastCapture();
            UpdatePlayerInteract();
            TrackCasualties();
            CheckAbandon();

            if (_objectiveHud != null)
                _objectiveHud.UpdateDisplay(_missionState, _convoy, _energyNode);

            CheckCompletion();
        }

        private void StartConflict()
        {
            _missionState = _missionService.StartMission("convoy_energy_conflict");
            _missionState.Objectives.Add(new MissionObjective
            {
                ObjectiveId = "protect_convoy",
                Description = "Protect the convoy",
                RequiredProgress = 1
            });
            _missionState.Objectives.Add(new MissionObjective
            {
                ObjectiveId = "stop_beast_raid",
                Description = "Stop the beast raid",
                RequiredProgress = 1
            });
            _missionState.Objectives.Add(new MissionObjective
            {
                ObjectiveId = "share_energy",
                Description = "Share energy at node",
                RequiredProgress = 1
            });

            _mechaCasualties = 0;
            _beastCasualties = 0;
            _abandonTimer = 0f;
            _isActive = true;

            RefreshBeastEntities();
        }

        private void RefreshBeastEntities()
        {
            _beastEntities.Clear();
            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (entity.Data != null && entity.Data.IsAlive && GameConstants.IsBeastSubFaction(entity.Data.Faction))
                    _beastEntities.Add(entity);
            }
        }

        private void UpdateBeastCapture()
        {
            if (_energyNode == null || _energyNode.IsCapturedByBeast) return;

            var beastCount = 0;
            foreach (var beast in _beastEntities)
            {
                if (beast.Data == null || !beast.Data.IsAlive) continue;
                if (Vector3.Distance(beast.transform.position, _energyNode.transform.position) <= _energyNode.CaptureRadius)
                    beastCount++;
            }

            if (beastCount > 0)
                _energyNode.TickBeastCapture(Time.deltaTime, beastCount);
        }

        private void UpdatePlayerInteract()
        {
            if (_energyNode == null || _energyNode.IsSharedByPlayer) return;

            var player = FindPlayerEntity();
            if (player == null) return;

            var inRange = Vector3.Distance(player.transform.position, _energyNode.transform.position) <= _energyNode.CaptureRadius;
            var commanderCtrl = player.GetComponent<CommanderControlController>();
            var hasSelectedTarget = commanderCtrl != null && commanderCtrl.HasSelectedTarget();
            if (inRange && Input.GetKey(KeyCode.E) && !hasSelectedTarget)
                _energyNode.TickPlayerInteract(Time.deltaTime, true);
        }

        private void TrackCasualties()
        {
            _mechaCasualties = 0;
            _beastCasualties = 0;

            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (entity.Data == null || entity.Data.IsAlive) continue;
                if (GameConstants.IsMotorSubFaction(entity.Data.Faction))
                    _mechaCasualties++;
                else if (GameConstants.IsBeastSubFaction(entity.Data.Faction))
                    _beastCasualties++;
            }
        }

        private void CheckAbandon()
        {
            var player = FindPlayerEntity();
            if (player == null) return;

            if (_triggerZone != null && !_triggerZone.IsPlayerInZone())
            {
                _abandonTimer += Time.deltaTime;
                if (_abandonTimer >= _abandonTime)
                {
                    _missionState.PlayerRetreated = true;
                    CompleteConflict();
                }
            }
            else
            {
                _abandonTimer = 0f;
            }
        }

        private void CheckCompletion()
        {
            if (_convoy != null && _convoy.IsDestroyed)
            {
                _missionState.EscalatedConflict = true;
                CompleteWithOutcome(MissionOutcomeType.BeastVictory);
                return;
            }

            if (_energyNode != null && _energyNode.IsCapturedByBeast)
            {
                CompleteWithOutcome(MissionOutcomeType.BeastVictory);
                return;
            }

            if (_energyNode != null && _energyNode.IsSharedByPlayer)
            {
                var totalCasualties = _mechaCasualties + _beastCasualties;
                if (totalCasualties <= 1)
                    CompleteWithOutcome(MissionOutcomeType.BalancedResolution);
                else
                    CompleteWithOutcome(MissionOutcomeType.PartialSuccess);
                return;
            }

            bool allBeastsDead = true;
            foreach (var beast in _beastEntities)
            {
                if (beast.Data != null && beast.Data.IsAlive)
                {
                    allBeastsDead = false;
                    break;
                }
            }

            if (allBeastsDead && _beastEntities.Count > 0)
            {
                _missionState.ProtectedConvoy = true;
                CompleteWithOutcome(MissionOutcomeType.MechaVictory);
                return;
            }
        }

        private void CompleteWithOutcome(MissionOutcomeType outcome)
        {
            if (_missionState == null) return;
            _missionState.MechaCasualties = _mechaCasualties;
            _missionState.BeastCasualties = _beastCasualties;
            _missionState.Outcome = outcome;
            CompleteConflict();
        }

        private void CompleteConflict()
        {
            if (_missionState == null) return;

            _missionState.MechaCasualties = _mechaCasualties;
            _missionState.BeastCasualties = _beastCasualties;

            var consequence = _missionService.CompleteMissionWithOutcome(_missionState.Outcome);

            _isActive = false;

            if (_triggerZone != null)
                _triggerZone.MarkCompleted();

            if (_objectiveHud != null)
                _objectiveHud.Hide();

            Debug.Log($"[Mission] ConvoyEnergyConflict complete: {_missionState.Outcome}, XP: +{consequence?.CommanderExperienceDelta ?? 0}");
        }

        private CharacterEntity FindPlayerEntity()
        {
            foreach (var ctrl in FindObjectsOfType<Combat.CombatController>())
            {
                return ctrl.GetComponent<CharacterEntity>();
            }
            return null;
        }
    }
}
