using System.Collections.Generic;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Feedback;
using UnityEngine;

namespace LuoLuoTrip
{
    public class BorderRetaliationRuntime : MonoBehaviour
    {
        [SerializeField] private MissionTriggerZone _triggerZone;
        [SerializeField] private MissionObjectiveHud _objectiveHud;
        [SerializeField] private float _defenseDuration = 60f;
        [SerializeField] private float _abandonTime = 15f;

        private MissionService _missionService;
        private MissionChainService _chainService;
        private MissionRuntimeState _missionState;
        private MissionModifier _modifier;
        private EncounterRuntime _encounter;
        private MissionAreaRuntime _areaRuntime;
        private MissionPhase _phase = MissionPhase.Inactive;
        private float _defenseTimer;
        private float _abandonTimer;
        private bool _completionGuard;
        private int _mechaCasualties;
        private int _beastCasualties;
        private List<CharacterEntity> _enemyEntities = new List<CharacterEntity>();

        public MissionPhase Phase => _phase;
        public MissionModifier CurrentModifier => _modifier;
        public EncounterRuntime Encounter => _encounter;
        public MissionAreaRuntime AreaRuntime => _areaRuntime;

        private void Start()
        {
            var context = GameBootstrap.Context;
            if (context == null) return;
            _missionService = context.MissionService;
            _chainService = context.MissionChainService;
            _modifier = _chainService?.BuildMissionModifiers("border_retaliation") ?? new MissionModifier();
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
            _areaRuntime.SetRetreatTime(_abandonTime);
        }

        private void Update()
        {
            if (_missionService == null) return;

            if (_phase == MissionPhase.Inactive)
            {
                if (_triggerZone != null && _triggerZone.MissionStarted)
                    StartRetaliation();
                return;
            }

            if (_phase != MissionPhase.Active) return;

            _areaRuntime.Tick(Time.deltaTime);
            TrackCasualties();
            CheckAbandon();

            switch (_modifier.ModifierId)
            {
                case "border_beast_retaliation": UpdateDefenseMission(); break;
                case "border_mecha_distrust": UpdateRecaptureMission(); break;
                case "border_ceasefire": UpdateCeasefireMission(); break;
                case "border_low_trust": UpdateEvacuationMission(); break;
                default: UpdateDefenseMission(); break;
            }

            if (_objectiveHud != null)
            {
                _objectiveHud.SetAreaRuntime(_areaRuntime);
                _objectiveHud.UpdateDisplay(_missionState, null, null, _phase);
            }
        }

        private void StartRetaliation()
        {
            _missionState = _missionService.StartMission("border_retaliation");
            _phase = MissionPhase.Active;
            _completionGuard = false;
            _mechaCasualties = 0;
            _beastCasualties = 0;
            _defenseTimer = 0f;
            _abandonTimer = 0f;

            RefreshEnemyEntities();

            _encounter.Initialize(new EncounterDefinition { encounterId = "border_retaliation", displayName = "Border Retaliation", attackerFaction = SubFactionId.BeastIronClaw, defenderFaction = SubFactionId.MotorIronRiders });
            _encounter.RegisterUnitsBySubFaction(SubFactionId.BeastIronClaw);
            _encounter.RegisterUnitsBySubFaction(SubFactionId.MotorIronRiders);
            if (_modifier != null)
                _encounter.ApplyMissionModifier(_modifier);

            _areaRuntime.Activate("border_retaliation");
            AttachObjectiveMarker();

            switch (_modifier.ModifierId)
            {
                case "border_beast_retaliation":
                    _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "defend_outpost", Description = "Defend the outpost (60s)", RequiredProgress = 1 });
                    _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "repel_raid", Description = "Repel the beast raid", RequiredProgress = 1 });
                    break;
                case "border_mecha_distrust":
                    _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "recapture", Description = "Recapture the resource point", RequiredProgress = 1 });
                    break;
                case "border_ceasefire":
                    _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "stabilize", Description = "Prevent ceasefire breakdown", RequiredProgress = 1 });
                    break;
                default:
                    _missionState.Objectives.Add(new MissionObjective { ObjectiveId = "evacuate", Description = "Complete evacuation", RequiredProgress = 1 });
                    break;
            }
        }

        private void UpdateDefenseMission()
        {
            _defenseTimer += Time.deltaTime;
            bool allBeastsDead = AreAllEnemiesDefeated();
            if (allBeastsDead && _enemyEntities.Count > 0)
                CompleteWithOutcome(MissionOutcomeType.MechaVictory, "MechaDefenseSuccess");
            else if (_defenseTimer >= _defenseDuration)
                CompleteWithOutcome(MissionOutcomeType.MechaVictory, "MechaDefenseSuccess");
            else if (_mechaCasualties > 2)
                CompleteWithOutcome(MissionOutcomeType.PartialSuccess, "PartialSuccess");
        }

        private void UpdateRecaptureMission()
        {
            bool allBeastsDead = AreAllEnemiesDefeated();
            if (allBeastsDead && _enemyEntities.Count > 0)
                CompleteWithOutcome(MissionOutcomeType.BalancedResolution, "RestoreTrust");
            else if (_mechaCasualties > 1)
                CompleteWithOutcome(MissionOutcomeType.PartialSuccess, "MechaDistrustIncreases");
        }

        private void UpdateCeasefireMission()
        {
            var total = _mechaCasualties + _beastCasualties;
            if (total <= 1 && _defenseTimer >= 30f)
                CompleteWithOutcome(MissionOutcomeType.BalancedResolution, "CeasefireStabilized");
            else if (total > 3)
                CompleteWithOutcome(MissionOutcomeType.BeastVictory, "CeasefireBroken");
            _defenseTimer += Time.deltaTime;
        }

        private void UpdateEvacuationMission()
        {
            var player = FindPlayerEntity();
            if (player != null && _triggerZone != null && !_triggerZone.IsPlayerInZone())
            {
                _defenseTimer += Time.deltaTime;
                if (_defenseTimer >= 15f)
                    CompleteWithOutcome(MissionOutcomeType.PartialSuccess, "RecoverReputation");
            }
            else
                _defenseTimer = 0f;
            if (_mechaCasualties >= 3)
                CompleteWithOutcome(MissionOutcomeType.Failed, "CommanderAuthorityDamaged");
        }

        private bool AreAllEnemiesDefeated()
        {
            if (_encounter != null && _encounter.Units.Count > 0)
                return _encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw);
            foreach (var beast in _enemyEntities)
                if (beast.Data != null && beast.Data.IsAlive) return false;
            return _enemyEntities.Count > 0;
        }

        private void ApplyModifierToHostility()
        {
            if (_modifier.InitialHostilityOffset != 0f && GameBootstrap.Context != null)
            {
                var rep = GameBootstrap.Context.ReputationService;
                if (rep != null)
                    foreach (SubFactionId faction in System.Enum.GetValues(typeof(SubFactionId)))
                        rep.ApplyDelta(FactionStandingDelta.Create(faction, hostility: (int)_modifier.InitialHostilityOffset));
            }
        }

        private void RefreshEnemyEntities()
        {
            _enemyEntities.Clear();
            if (CharacterRuntimeRegistry.Count > 0)
            {
                var beasts = CharacterRuntimeRegistry.QueryBySubFaction(SubFactionId.BeastIronClaw);
                _enemyEntities.AddRange(beasts);
            }
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
                var all = CharacterRuntimeRegistry.Count > 0 ? CharacterRuntimeRegistry.AllCharacters : new List<CharacterEntity>();
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
                    CompleteWithOutcome(MissionOutcomeType.Failed, "PlayerRetreated");
                }
                return;
            }
            var player = FindPlayerEntity();
            if (player == null || _triggerZone == null) return;
            if (!_triggerZone.IsPlayerInZone())
            {
                _abandonTimer += Time.deltaTime;
                if (_abandonTimer >= _abandonTime)
                {
                    _missionState.PlayerRetreated = true;
                    CompleteWithOutcome(MissionOutcomeType.Failed, "PlayerRetreated");
                }
            }
            else
                _abandonTimer = 0f;
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
            var consequence = _missionService.CompleteMissionWithOutcome(outcome);
            if (_chainService != null)
                _chainService.RecordMissionResult("border_retaliation", outcome, consequence?.CommanderExperienceDelta ?? 0, convoyDestroyed: false, beastRaidDefeated: outcome == MissionOutcomeType.MechaVictory);
            _phase = outcome == MissionOutcomeType.Failed ? MissionPhase.Failed : MissionPhase.Completed;
            if (_triggerZone != null) _triggerZone.MarkCompleted();
            _areaRuntime.MarkComplete();
            if (_objectiveHud != null) _objectiveHud.ShowFinalResult(_missionState, _phase);
            DetachObjectiveMarker();
            if (_phase == MissionPhase.Failed) AudioFeedbackService.PlayUI(AudioEventId.MissionFailed);
            else AudioFeedbackService.PlayUI(AudioEventId.MissionComplete);
            Debug.Log($"[Mission] BorderRetaliation complete: {outcome} ({resultTag}), XP: +{consequence?.CommanderExperienceDelta ?? 0}");
        }

        private void AttachObjectiveMarker()
        {
            var service = WorldMarkerService.Instance;
            if (service == null || _triggerZone == null) return;
            service.AttachMarker(_triggerZone.gameObject, WorldMarkerType.MissionObjective, "[BORDER]");
        }

        private void DetachObjectiveMarker()
        {
            var service = WorldMarkerService.Instance;
            if (service == null || _triggerZone == null) return;
            service.DetachMarker(_triggerZone.gameObject);
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
