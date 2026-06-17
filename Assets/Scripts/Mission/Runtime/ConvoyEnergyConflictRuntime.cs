using System.Collections.Generic;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Feedback;
using UnityEngine;

namespace LuoLuoTrip
{
    public enum MissionPhase
    {
        Inactive,
        Active,
        Resolving,
        Completed,
        Failed
    }

    public class ConvoyEnergyConflictRuntime : MonoBehaviour
    {
        [SerializeField] private ConvoyObjective _convoy;
        [SerializeField] private EnergyNodeObjective _energyNode;
        [SerializeField] private MissionTriggerZone _triggerZone;
        [SerializeField] private MissionObjectiveHud _objectiveHud;
        [SerializeField] private float _abandonTime = 10f;

        private MissionService _missionService;
        private MissionChainService _chainService;
        private MissionRuntimeState _missionState;
        private EncounterRuntime _encounter;
        private MissionAreaRuntime _areaRuntime;
        private int _mechaCasualties;
        private int _beastCasualties;
        private float _abandonTimer;
        private bool _completionGuard;
        private List<CharacterEntity> _beastEntities = new List<CharacterEntity>();

        public MissionPhase Phase { get; private set; } = MissionPhase.Inactive;
        public bool IsActive => Phase == MissionPhase.Active;
        public MissionOutcomeType? LastOutcome { get; private set; }
        public EncounterRuntime Encounter => _encounter;
        public MissionAreaRuntime AreaRuntime => _areaRuntime;

        private void Start()
        {
            var context = GameBootstrap.Context;
            if (context != null)
            {
                _missionService = context.MissionService;
                _chainService = context.MissionChainService;
            }

            _encounter = GetComponent<EncounterRuntime>();
            if (_encounter == null)
            {
                var go = new GameObject("EncounterRuntime");
                go.transform.SetParent(transform, false);
                _encounter = go.AddComponent<EncounterRuntime>();
            }

            _areaRuntime = GetComponent<MissionAreaRuntime>();
            if (_areaRuntime == null)
            {
                var go = new GameObject("MissionAreaRuntime");
                go.transform.SetParent(transform, false);
                _areaRuntime = go.AddComponent<MissionAreaRuntime>();
            }

            if (_triggerZone != null)
                _areaRuntime.ConfigureBoundary(_triggerZone);
            _areaRuntime.SetRetreatTime(_abandonTime);
        }

        private void Update()
        {
            if (_missionService == null) return;

            if (Phase == MissionPhase.Inactive)
            {
                if (_triggerZone != null && _triggerZone.MissionStarted)
                    StartConflict();
                return;
            }

            if (Phase != MissionPhase.Active) return;

            _areaRuntime.Tick(Time.deltaTime);

            UpdateBeastCapture();
            UpdatePlayerInteract();
            TrackCasualties();
            CheckAbandon();

            if (_objectiveHud != null)
            {
                _objectiveHud.SetAreaRuntime(_areaRuntime);
                _objectiveHud.UpdateDisplay(_missionState, _convoy, _energyNode, Phase);
            }

            CheckCompletion();
        }

        private void StartConflict()
        {
            _missionState = _missionService.StartMission("convoy_energy_conflict");
            _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "protect_convoy", Description = "Protect the convoy", RequiredProgress = 1 });
            _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "stop_beast_raid", Description = "Stop the beast raid", RequiredProgress = 1 });
            _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "share_energy", Description = "Share energy at node", RequiredProgress = 1 });

            _mechaCasualties = 0;
            _beastCasualties = 0;
            _abandonTimer = 0f;
            _completionGuard = false;
            Phase = MissionPhase.Active;

            RefreshBeastEntities();

            _encounter.Initialize(new EncounterDefinition { encounterId = "convoy_energy_conflict", displayName = "Convoy Energy Conflict", attackerFaction = SubFactionId.BeastIronClaw, defenderFaction = SubFactionId.MotorIronRiders });
            _encounter.RegisterUnitsBySubFaction(SubFactionId.BeastIronClaw);
            _encounter.RegisterUnitsBySubFaction(SubFactionId.MotorIronRiders);
            _encounter.StartEncounter();

            _areaRuntime.Activate("convoy_energy_conflict");
            AttachObjectiveMarkers();
        }

        private void AttachObjectiveMarkers()
        {
            var service = WorldMarkerService.Instance;
            if (service == null) return;
            if (_convoy != null)
                service.AttachMarker(_convoy.gameObject, WorldMarkerType.MissionObjective, "[CONVOY]");
            if (_energyNode != null)
                service.AttachMarker(_energyNode.gameObject, WorldMarkerType.Interactable, "[E] SHARE");
        }

        private void DetachObjectiveMarkers()
        {
            var service = WorldMarkerService.Instance;
            if (service == null) return;
            if (_convoy != null)
                service.DetachMarker(_convoy.gameObject);
            if (_energyNode != null)
                service.DetachMarker(_energyNode.gameObject);
        }

        private void RefreshBeastEntities()
        {
            _beastEntities.Clear();
            if (_encounter != null && _encounter.Units.Count > 0)
            {
                var alive = _encounter.GetAliveUnits(SubFactionId.BeastIronClaw);
                foreach (var handle in alive)
                    _beastEntities.Add(handle.Entity);
            }
            else
            {
                var all = CharacterRuntimeRegistry.Count > 0
                    ? CharacterRuntimeRegistry.QueryBySubFaction(SubFactionId.BeastIronClaw)
                    : new List<CharacterEntity>();
                if (all.Count == 0)
                {
                    foreach (var entity in FindObjectsOfType<CharacterEntity>())
                        if (entity.Data != null && entity.Data.IsAlive && GameConstants.IsBeastSubFaction(entity.Data.Faction))
                            _beastEntities.Add(entity);
                }
                else
                    _beastEntities.AddRange(all);
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
            if (_encounter != null && _encounter.Units.Count > 0)
            {
                _mechaCasualties = _encounter.CountCasualties(MainRace.MotorTribe);
                _beastCasualties = _encounter.CountCasualties(MainRace.BeastTribe);
            }
            else
            {
                _mechaCasualties = 0;
                _beastCasualties = 0;
                var all = CharacterRuntimeRegistry.Count > 0 ? CharacterRuntimeRegistry.AllCharacters : new List<CharacterEntity>(FindObjectsOfType<CharacterEntity>());
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].Data == null || all[i].Data.IsAlive) continue;
                    if (GameConstants.IsMotorSubFaction(all[i].Data.Faction)) _mechaCasualties++;
                    else if (GameConstants.IsBeastSubFaction(all[i].Data.Faction)) _beastCasualties++;
                }
            }
        }

        private void CheckAbandon()
        {
            if (_areaRuntime != null && _areaRuntime.IsActive)
            {
                if (_areaRuntime.ShouldTriggerRetreat())
                {
                    _missionState.PlayerRetreated = true;
                    CompleteWithOutcome(MissionOutcomeType.Failed);
                }
                return;
            }
            var player = FindPlayerEntity();
            if (player == null) return;
            if (_triggerZone != null && !_triggerZone.IsPlayerInZone())
            {
                _abandonTimer += Time.deltaTime;
                if (_abandonTimer >= _abandonTime)
                {
                    _missionState.PlayerRetreated = true;
                    CompleteWithOutcome(MissionOutcomeType.Failed);
                }
            }
            else
                _abandonTimer = 0f;
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
                var total = _mechaCasualties + _beastCasualties;
                CompleteWithOutcome(total <= 1 ? MissionOutcomeType.BalancedResolution : MissionOutcomeType.PartialSuccess);
                return;
            }
            bool allBeastsDead = _encounter != null && _encounter.Units.Count > 0
                ? _encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw)
                : AreAllBeastsDeadLegacy();
            if (allBeastsDead && _beastEntities.Count > 0)
            {
                _missionState.ProtectedConvoy = true;
                CompleteWithOutcome(MissionOutcomeType.MechaVictory);
            }
        }

        private bool AreAllBeastsDeadLegacy()
        {
            foreach (var beast in _beastEntities)
                if (beast.Data != null && beast.Data.IsAlive) return false;
            return true;
        }

        private void CompleteWithOutcome(MissionOutcomeType outcome)
        {
            if (_completionGuard) return;
            Phase = MissionPhase.Resolving;
            LastOutcome = outcome;
            if (_missionState == null) return;
            _missionState.MechaCasualties = _mechaCasualties;
            _missionState.BeastCasualties = _beastCasualties;
            _missionState.Outcome = outcome;
            CompleteConflict();
        }

        private void CompleteConflict()
        {
            if (_completionGuard) return;
            _completionGuard = true;
            if (_missionState == null) return;
            _missionState.MechaCasualties = _mechaCasualties;
            _missionState.BeastCasualties = _beastCasualties;
            var consequence = _missionService.CompleteMissionWithOutcome(_missionState.Outcome);
            if (_chainService != null)
                _chainService.RecordMissionResult("convoy_energy_conflict", _missionState.Outcome, consequence?.CommanderExperienceDelta ?? 0, sharedEnergy: _energyNode?.IsSharedByPlayer ?? false, convoyDestroyed: _convoy?.IsDestroyed ?? false, beastRaidDefeated: _missionState.Outcome == MissionOutcomeType.MechaVictory);
            Phase = _missionState.Outcome == MissionOutcomeType.Failed ? MissionPhase.Failed : MissionPhase.Completed;
            if (_triggerZone != null) _triggerZone.MarkCompleted();
            _areaRuntime.MarkComplete();
            if (_encounter != null) _encounter.CompleteEncounter(_missionState.Outcome.ToString());
            if (_objectiveHud != null) _objectiveHud.ShowFinalResult(_missionState, Phase);
            DetachObjectiveMarkers();
            if (Phase == MissionPhase.Failed) AudioFeedbackService.PlayUI(AudioEventId.MissionFailed);
            else AudioFeedbackService.PlayUI(AudioEventId.MissionComplete);
            Debug.Log($"[Mission] ConvoyEnergyConflict complete: {_missionState.Outcome}, Phase: {Phase}, XP: +{consequence?.CommanderExperienceDelta ?? 0}");
        }

        private CharacterEntity FindPlayerEntity()
        {
            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.AllCharacters;
                for (int i = 0; i < all.Count; i++)
                    if (all[i] != null && all[i].GetComponent<Combat.CombatController>() != null) return all[i];
            }
            foreach (var ctrl in FindObjectsOfType<Combat.CombatController>())
                return ctrl.GetComponent<CharacterEntity>();
            return null;
        }
    }
}
