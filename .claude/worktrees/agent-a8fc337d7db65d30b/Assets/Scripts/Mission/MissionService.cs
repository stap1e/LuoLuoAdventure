using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    public class MissionService
    {
        private readonly FactionReputationService _reputationService;
        private readonly CommanderProfile _commander;
        private MissionRuntimeState _activeMission;
        private readonly List<MissionConsequence> _completedConsequences = new List<MissionConsequence>();

        public MissionRuntimeState ActiveMission => _activeMission;
        public IReadOnlyList<MissionConsequence> CompletedConsequences => _completedConsequences;

        public MissionService(FactionReputationService reputationService, CommanderProfile commander)
        {
            _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        public MissionRuntimeState StartMission(MissionDefinitionSO definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (_activeMission != null) throw new InvalidOperationException("A mission is already active");

            _activeMission = definition.CreateRuntimeState();
            return _activeMission;
        }

        public MissionRuntimeState StartMission(string missionId)
        {
            if (_activeMission != null) throw new InvalidOperationException("A mission is already active");

            _activeMission = new MissionRuntimeState { MissionId = missionId };
            return _activeMission;
        }

        public void UpdateObjective(string objectiveId, int progress = 1)
        {
            if (_activeMission == null) return;

            foreach (var obj in _activeMission.Objectives)
            {
                if (obj.ObjectiveId != objectiveId) continue;
                obj.Progress = Math.Min(obj.Progress + progress, obj.RequiredProgress);
                if (obj.Progress >= obj.RequiredProgress)
                    obj.IsCompleted = true;
                break;
            }
        }

        public void FailObjective(string objectiveId)
        {
            if (_activeMission == null) return;

            foreach (var obj in _activeMission.Objectives)
            {
                if (obj.ObjectiveId != objectiveId) continue;
                obj.IsFailed = true;
                break;
            }
        }

        public MissionConsequence CompleteMission()
        {
            if (_activeMission == null) return null;

            _activeMission.DetermineOutcome();
            return ResolveAndApply();
        }

        public MissionConsequence CompleteMissionWithOutcome(MissionOutcomeType outcome)
        {
            if (_activeMission == null) return null;

            _activeMission.Outcome = outcome;
            return ResolveAndApply();
        }

        private MissionConsequence ResolveAndApply()
        {
            var consequence = MissionConsequenceResolver.Resolve(_activeMission);

            _reputationService.ApplyDeltas(consequence.FactionDeltas);

            _commander.AddExperience(consequence.CommanderExperienceDelta);

            _completedConsequences.Add(consequence);
            _activeMission = null;

            return consequence;
        }

        public void SetActiveMissionState(MissionRuntimeState state)
        {
            _activeMission = state;
        }

        public void ClearActiveMission()
        {
            _activeMission = null;
        }
    }
}
