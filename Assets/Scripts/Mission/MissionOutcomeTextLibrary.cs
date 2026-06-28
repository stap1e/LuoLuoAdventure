using System.Collections.Generic;

namespace LuoLuoTrip
{
    public static class MissionOutcomeTextLibrary
    {
        public static string DisplayMissionName(string missionId)
        {
            return DemoFlowManager.DisplayMissionName(missionId);
        }

        public static string BuildOutcomeSummary(MissionOutcomeType outcome)
        {
            return outcome switch
            {
                MissionOutcomeType.MechaVictory => "Mecha victory improves Motor trust but increases Beast retaliation pressure.",
                MissionOutcomeType.BeastVictory => "Beast victory improves Beast trust but weakens Mecha support.",
                MissionOutcomeType.BalancedResolution => "Balanced resolution lowers mainstream hostility while extremists remain.",
                MissionOutcomeType.PartialSuccess => "Partial success contains the conflict with lingering trust loss.",
                MissionOutcomeType.Failed => "Mission failed; faction confidence drops and hostility may rise.",
                MissionOutcomeType.BalancedMediation => "Mainstream hostility reduced below 40; extremists remain.",
                MissionOutcomeType.MechaSuppression => "Mecha order is restored, but Beast hostility rises sharply.",
                MissionOutcomeType.BeastNegotiation => "Beast negotiation lowers Beast hostility while Mecha support softens.",
                MissionOutcomeType.FailedEscalation => "City gate collapse escalates hostility on both sides.",
                MissionOutcomeType.PartialContainment => "The dispute is contained, but casualties leave both sides wary.",
                _ => "No consequence data"
            };
        }

        public static string BuildNextHint(string missionId)
        {
            return missionId switch
            {
                DemoFlowManager.ConvoyMissionId => "Next: Border Retaliation",
                DemoFlowManager.BorderMissionId => "Next: City Gate Dispute",
                DemoFlowManager.CityGateMissionId => "Next: Review Border / City stability",
                _ => "Next: Continue demo flow"
            };
        }

        public static string FormatConsequenceSummary(MissionConsequence consequence)
        {
            if (consequence == null)
                return "No consequence data";

            if (!string.IsNullOrEmpty(consequence.SummaryText))
                return consequence.SummaryText;

            return BuildOutcomeSummary(consequence.Outcome);
        }

        public static string FormatFactionDelta(FactionStandingDelta delta)
        {
            return $"{delta.FactionId}: Trust{FormatSigned(delta.TrustDelta)} Respect{FormatSigned(delta.RespectDelta)} Hostility{FormatSigned(delta.HostilityDelta)} War{FormatSigned(delta.WarExhaustionDelta)}";
        }

        public static List<string> BuildFactionDeltaLines(IEnumerable<FactionStandingDelta> deltas, int maxLines = 3)
        {
            var lines = new List<string>();
            if (deltas == null)
                return lines;

            foreach (var delta in deltas)
            {
                lines.Add(FormatFactionDelta(delta));
                if (maxLines > 0 && lines.Count >= maxLines)
                    break;
            }
            return lines;
        }

        private static string FormatSigned(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }
    }
}
