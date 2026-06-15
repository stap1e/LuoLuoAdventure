using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionConsequence
    {
        public MissionOutcomeType Outcome;
        public int CommanderExperienceDelta;
        public List<FactionStandingDelta> FactionDeltas = new List<FactionStandingDelta>();
        public string SummaryText;

        public static MissionConsequence Empty(MissionOutcomeType outcome)
        {
            return new MissionConsequence
            {
                Outcome = outcome,
                CommanderExperienceDelta = 0,
                SummaryText = ""
            };
        }
    }
}
