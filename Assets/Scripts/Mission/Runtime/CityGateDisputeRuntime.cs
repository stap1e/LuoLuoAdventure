using System.Collections.Generic;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Feedback;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>
    /// Mission 3 — CityGateDispute vertical slice.
    ///
    /// Phases: NotStarted → Tension → Skirmish → MediationWindow → Resolved/Failed
    ///
    /// Objectives:
    ///   1. Protect CityGateCore (neutral objective object with HP)
    ///   2. Keep Mecha casualties below threshold
    ///   3. Keep BeastNegotiator alive
    ///   4. Defeat/repel BeastRaiders
    ///
    /// Outcomes:
    ///   BalancedMediation, MechaSuppression, BeastNegotiation, FailedEscalation, PartialContainment
    ///
    /// Uses EncounterRuntime for BeastRaider wave spawning and snapshot persistence.
    /// </summary>
    public class CityGateDisputeRuntime : MonoBehaviour
    {
        [SerializeField] private MissionTriggerZone _triggerZone;
        [SerializeField] private MissionObjectiveHud _objectiveHud;
        [SerializeField] private Combatant _cityGateCore;
        [SerializeField] private CharacterEntity _beastNegotiator;
        [SerializeField] private float _skirmishDelay = 10f;
        [SerializeField] private float _mediationWindowDuration = 45f;
        [SerializeField] private int _maxMechaCasualtiesForBalanced = 2;
        [SerializeField] private int _maxBeastCasualtiesForBalanced = 4;
        [SerializeField] private int _maxTotalCasualtiesForPartial = 8;

        private MissionService _missionService;
        private MissionChainService _chainService;
        private MissionRuntimeState _missionState;
        private MissionModifier _modifier;
        private EncounterRuntime _encounter;
        private MissionAreaRuntime _areaRuntime;
        private MissionPhase _phase = MissionPhase.Inactive;
        private float _phaseTimer;
        private int _mechaCasualties;
        private int _beastCasualties;
        private bool _completionGuard;
        private bool _beastRaidersDefeated;
        private bool _negotiatorSurvived = true;
        private bool _coreSurvived = true;

        public MissionPhase Phase => _phase;
        public EncounterRuntime Encounter => _encounter;
        public MissionAreaRuntime AreaRuntime => _areaRuntime;
        public Combatant CityGateCore => _cityGateCore;
        public CharacterEntity BeastNegotiator => _beastNegotiator;
        public bool BeastRaidersDefeated => _beastRaidersDefeated;
        public bool NegotiatorSurvived => _negotiatorSurvived;
        public bool CoreSurvived => _coreSurvived;
        public int MechaCasualties => _mechaCasualties;
        public int BeastCasualties => _beastCasualties;

        private void Start()
        {
            var context = GameBootstrap.Context;
            if (context == null) return;
            _missionService = context.MissionService;
            _chainService = context.MissionChainService;
            _modifier = _chainService?.BuildMissionModifiers("city_gate_dispute") ?? new MissionModifier();
            ApplyModifierToHostility();

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
            _areaRuntime.SetRetreatTime(15f);
        }

        private void Update()
        {
            if (_missionService == null) return;

            if (_phase == MissionPhase.Inactive)
            {
                if (_triggerZone != null && _triggerZone.MissionStarted)
                    StartDispute();
                return;
            }

            if (_phase != MissionPhase.Active && _phase != MissionPhase.Tension) return;

            _areaRuntime.Tick(Time.deltaTime);
            if (_encounter != null)
                _encounter.TickWaves(Time.deltaTime);
            TrackCasualties();
            CheckCoreStatus();
            CheckNegotiatorStatus();

            switch (_phase)
            {
                case MissionPhase.Tension: UpdateTension(); break;
                case MissionPhase.Active: UpdateActive(); break;
            }

            if (_objectiveHud != null)
            {
                _objectiveHud.SetAreaRuntime(_areaRuntime);
                _objectiveHud.UpdateDisplay(_missionState, null, null, _phase);
            }
        }

        private void StartDispute()
        {
            _missionState = _missionService.StartMission("city_gate_dispute");
            _phase = MissionPhase.Tension;
            _phaseTimer = 0f;
            _completionGuard = false;
            _mechaCasualties = 0;
            _beastCasualties = 0;
            _beastRaidersDefeated = false;
            _negotiatorSurvived = true;
            _coreSurvived = true;

            _encounter.Initialize(new EncounterDefinition
            {
                encounterId = "city_gate_dispute",
                displayName = "City Gate Dispute",
                attackerFaction = SubFactionId.BeastIronClaw,
                defenderFaction = SubFactionId.MotorIronRiders
            });
            _encounter.RegisterUnitsBySubFaction(SubFactionId.BeastIronClaw);
            _encounter.RegisterUnitsBySubFaction(SubFactionId.MotorIronRiders);
            if (_modifier != null)
                _encounter.ApplyMissionModifier(_modifier);

            ConfigureDynamicWaves();
            _encounter.StartEncounter();
            _areaRuntime.Activate("city_gate_dispute");

            _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "protect_core", Description = "Protect the City Gate Core", RequiredProgress = 1 });
            _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "protect_negotiator", Description = "Keep the Beast Negotiator alive", RequiredProgress = 1 });
            _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "defeat_raiders", Description = "Defeat or repel Beast Raiders", RequiredProgress = 1 });

            AttachObjectiveMarker();
            Debug.Log("[CityGateDispute] Phase: Tension — both sides face off at the city gate");
        }

        private void UpdateTension()
        {
            _phaseTimer += Time.deltaTime;
            if (_phaseTimer >= _skirmishDelay)
            {
                _phase = MissionPhase.Active;
                _phaseTimer = 0f;
                Debug.Log("[CityGateDispute] Phase: Skirmish — extremists engage");
            }
        }

        private void UpdateActive()
        {
            _phaseTimer += Time.deltaTime;

            CheckRaidersDefeated();

            if (!_coreSurvived)
            {
                CompleteWithOutcome(MissionOutcomeType.FailedEscalation, "CoreDestroyed");
                return;
            }

            if (!_negotiatorSurvived && _beastRaidersDefeated)
            {
                CompleteWithOutcome(MissionOutcomeType.MechaSuppression, "NegotiatorDeadRaidersDefeated");
                return;
            }

            if (_beastRaidersDefeated && _negotiatorSurvived)
            {
                if (_mechaCasualties <= _maxMechaCasualtiesForBalanced
                    && _beastCasualties <= _maxBeastCasualtiesForBalanced)
                {
                    CompleteWithOutcome(MissionOutcomeType.BalancedMediation, "BalancedMediationSuccess");
                }
                else if (_mechaCasualties + _beastCasualties <= _maxTotalCasualtiesForPartial)
                {
                    CompleteWithOutcome(MissionOutcomeType.PartialContainment, "PartialContainment");
                }
                else
                {
                    CompleteWithOutcome(MissionOutcomeType.MechaSuppression, "HighCasualtiesRaidersDefeated");
                }
                return;
            }

            if (_phaseTimer >= _mediationWindowDuration)
            {
                if (_negotiatorSurvived && _coreSurvived && _beastCasualties <= _maxBeastCasualtiesForBalanced)
                    CompleteWithOutcome(MissionOutcomeType.BeastNegotiation, "NegotiatorMediated");
                else
                    CompleteWithOutcome(MissionOutcomeType.PartialContainment, "TimerExpired");
            }
        }

        private void CheckCoreStatus()
        {
            if (_cityGateCore == null || !_cityGateCore.IsAlive)
                _coreSurvived = false;
        }

        private void CheckNegotiatorStatus()
        {
            if (_beastNegotiator == null || _beastNegotiator.Data == null || !_beastNegotiator.Data.IsAlive)
                _negotiatorSurvived = false;
        }

        private void CheckRaidersDefeated()
        {
            if (_beastRaidersDefeated) return;
            if (_encounter != null && _encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw))
                _beastRaidersDefeated = true;
        }

        private void TrackCasualties()
        {
            if (_encounter != null && _encounter.Units.Count > 0)
            {
                _mechaCasualties = _encounter.CountCasualties(MainRace.MotorTribe);
                _beastCasualties = _encounter.CountCasualties(MainRace.BeastTribe);
            }
        }

        private void ConfigureDynamicWaves()
        {
            var waves = new List<EncounterWave>();
            var beastMult = _encounter?.GetFactionMultiplier(SubFactionId.BeastIronClaw) ?? 1f;
            var beastCount = Mathf.RoundToInt(2 * beastMult);

            waves.Add(new EncounterWave
            {
                waveId = "citygate_beast_raid_1",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = beastCount,
                delaySeconds = 12f,
                initialBehavior = SpawnBehavior.Chase,
                isHostile = true
            });
            waves.Add(new EncounterWave
            {
                waveId = "citygate_beast_raid_2",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = beastCount + 1,
                delaySeconds = 30f,
                initialBehavior = SpawnBehavior.Chase,
                isHostile = true
            });

            if (_encounter != null)
                _encounter.SetWaves(waves);

            Debug.Log($"[CityGateDispute] Configured {waves.Count} BeastRaider waves (beastCount={beastCount})");
        }

        private void ApplyModifierToHostility()
        {
            if (_modifier != null && _modifier.InitialHostilityOffset != 0f && GameBootstrap.Context != null)
            {
                var rep = GameBootstrap.Context.ReputationService;
                if (rep != null)
                    foreach (SubFactionId faction in System.Enum.GetValues(typeof(SubFactionId)))
                        rep.ApplyDelta(FactionStandingDelta.Create(faction, hostility: (int)_modifier.InitialHostilityOffset));
            }
        }

        private void CompleteWithOutcome(MissionOutcomeType outcome, string resultTag)
        {
            if (_completionGuard) return;
            _phase = MissionPhase.Resolving;
            _completionGuard = true;
            if (_missionState == null) return;

            _missionState.Outcome = outcome;
            _missionState.MechaCasualties = _mechaCasualties;
            _missionState.BeastCasualties = _beastCasualties;

            foreach (var obj in _missionState.Objectives)
            {
                if (obj.ObjectiveId == "protect_core")
                    obj.IsCompleted = _coreSurvived;
                else if (obj.ObjectiveId == "protect_negotiator")
                    obj.IsCompleted = _negotiatorSurvived;
                else if (obj.ObjectiveId == "defeat_raiders")
                    obj.IsCompleted = _beastRaidersDefeated;
            }

            var consequence = _missionService.CompleteMissionWithOutcome(outcome);
            if (_chainService != null)
                _chainService.RecordMissionResult("city_gate_dispute", outcome,
                    consequence?.CommanderExperienceDelta ?? 0,
                    beastRaidDefeated: _beastRaidersDefeated);

            _phase = outcome == MissionOutcomeType.FailedEscalation ? MissionPhase.Failed : MissionPhase.Completed;
            if (_triggerZone != null) _triggerZone.MarkCompleted();
            _areaRuntime.MarkComplete();
            if (_encounter != null) _encounter.CompleteEncounter(outcome.ToString());
            if (_objectiveHud != null) _objectiveHud.ShowFinalResult(_missionState, _phase);
            DetachObjectiveMarker();

            if (_phase == MissionPhase.Failed) AudioFeedbackService.PlayUI(AudioEventId.MissionFailed);
            else AudioFeedbackService.PlayUI(AudioEventId.MissionComplete);

            Debug.Log($"[CityGateDispute] Complete: {outcome} ({resultTag}), MechaCasualties={_mechaCasualties}, BeastCasualties={_beastCasualties}, XP: +{consequence?.CommanderExperienceDelta ?? 0}");
        }

        private void AttachObjectiveMarker()
        {
            var service = WorldMarkerService.Instance;
            if (service == null || _triggerZone == null) return;
            service.AttachMarker(_triggerZone.gameObject, WorldMarkerType.MissionObjective, "[CITY GATE]");
        }

        private void DetachObjectiveMarker()
        {
            var service = WorldMarkerService.Instance;
            if (service == null || _triggerZone == null) return;
            service.DetachMarker(_triggerZone.gameObject);
        }

        /// <summary>
        /// Static outcome resolver for EditMode tests. Does not require a live scene.
        /// Returns the expected MissionOutcomeType for the given conditions.
        /// </summary>
        public static MissionOutcomeType ResolveOutcome(
            bool coreSurvived, bool negotiatorSurvived, bool raidersDefeated,
            int mechaCasualties, int beastCasualties,
            int maxMechaForBalanced, int maxBeastForBalanced, int maxTotalForPartial)
        {
            if (!coreSurvived)
                return MissionOutcomeType.FailedEscalation;

            if (!negotiatorSurvived && raidersDefeated)
                return MissionOutcomeType.MechaSuppression;

            if (raidersDefeated && negotiatorSurvived)
            {
                if (mechaCasualties <= maxMechaForBalanced && beastCasualties <= maxBeastForBalanced)
                    return MissionOutcomeType.BalancedMediation;
                if (mechaCasualties + beastCasualties <= maxTotalForPartial)
                    return MissionOutcomeType.PartialContainment;
                return MissionOutcomeType.MechaSuppression;
            }

            if (negotiatorSurvived && coreSurvived && beastCasualties <= maxBeastForBalanced)
                return MissionOutcomeType.BeastNegotiation;

            return MissionOutcomeType.PartialContainment;
        }
    }
}
