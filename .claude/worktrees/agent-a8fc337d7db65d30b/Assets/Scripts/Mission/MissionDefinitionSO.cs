using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    [CreateAssetMenu(fileName = "MissionDefinition", menuName = "LuoLuoTrip/Mission Definition")]
    public class MissionDefinitionSO : ScriptableObject
    {
        public string MissionId;
        public string DisplayName;
        [TextArea(2, 6)] public string Description;
        public int RecommendedCommanderLevel = 1;
        public List<MissionObjective> DefaultObjectives = new List<MissionObjective>();
        public List<MissionOutcomeConsequenceMapping> OutcomeConsequences = new List<MissionOutcomeConsequenceMapping>();

        public MissionRuntimeState CreateRuntimeState()
        {
            var state = new MissionRuntimeState
            {
                MissionId = MissionId
            };

            foreach (var obj in DefaultObjectives)
            {
                state.Objectives.Add(new MissionObjective
                {
                    ObjectiveId = obj.ObjectiveId,
                    Description = obj.Description,
                    RequiredProgress = obj.RequiredProgress,
                    Progress = 0,
                    IsCompleted = false,
                    IsFailed = false
                });
            }

            return state;
        }
    }

    [Serializable]
    public class MissionOutcomeConsequenceMapping
    {
        public MissionOutcomeType Outcome;
        public int CommanderExperienceDelta;
        public List<FactionStandingDelta> FactionDeltas = new List<FactionStandingDelta>();
        public string SummaryText;
    }
}
