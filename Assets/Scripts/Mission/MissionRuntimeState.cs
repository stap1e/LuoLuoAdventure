using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionRuntimeState
    {
        public string MissionId;
        public List<MissionObjective> Objectives = new List<MissionObjective>();
        public int MechaCasualties;
        public int BeastCasualties;
        public bool ProtectedConvoy;
        public bool SharedResources;
        public bool EscalatedConflict;
        public bool PlayerRetreated;
        public MissionOutcomeType Outcome;

        public bool IsComplete
        {
            get
            {
                foreach (var obj in Objectives)
                {
                    if (!obj.IsCompleted && !obj.IsFailed) return false;
                }
                return Objectives.Count > 0;
            }
        }

        public void DetermineOutcome()
        {
            bool allCompleted = true;
            bool anyFailed = false;
            foreach (var obj in Objectives)
            {
                if (obj.IsFailed) anyFailed = true;
                if (!obj.IsCompleted) allCompleted = false;
            }

            if (PlayerRetreated)
            {
                Outcome = MissionOutcomeType.Failed;
                return;
            }

            if (anyFailed || !allCompleted)
            {
                Outcome = MissionOutcomeType.PartialSuccess;
                return;
            }

            if (ProtectedConvoy && !SharedResources)
                Outcome = MissionOutcomeType.MechaVictory;
            else if (SharedResources && !ProtectedConvoy)
                Outcome = MissionOutcomeType.BeastVictory;
            else if (SharedResources && ProtectedConvoy)
                Outcome = MissionOutcomeType.BalancedResolution;
            else
                Outcome = MissionOutcomeType.MechaVictory;
        }
    }
}
