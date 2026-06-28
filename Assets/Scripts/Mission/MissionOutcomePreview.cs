using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionOutcomePreview
    {
        public string missionId;
        public string missionDisplayName;
        public MissionOutcomeType likelyOutcome;
        public string confidenceLabel;
        public string outcomeSummary;
        public string consequenceSummary;
        public int commanderXpPreview;
        public string nextMissionHint;
        public string previousOutcomeEffect;
        public List<MissionOutcomeRisk> risks = new List<MissionOutcomeRisk>();
        public List<MissionConsequencePreview> consequences = new List<MissionConsequencePreview>();
        public bool isFailureLikely;
        public bool isBalancedLikely;
        public bool hasCriticalRisk;

        public static MissionOutcomePreview Unavailable(string missionId, string reason)
        {
            return new MissionOutcomePreview
            {
                missionId = missionId ?? string.Empty,
                missionDisplayName = MissionOutcomeTextLibrary.DisplayMissionName(missionId),
                likelyOutcome = MissionOutcomeType.PartialSuccess,
                confidenceLabel = "Unavailable",
                outcomeSummary = reason ?? "No active preview data.",
                consequenceSummary = "No consequence preview.",
                nextMissionHint = MissionOutcomeTextLibrary.BuildNextHint(missionId),
                previousOutcomeEffect = "No previous outcome modifier."
            };
        }
    }
}
