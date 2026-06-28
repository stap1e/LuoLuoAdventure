using System;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionConsequencePreview
    {
        public SubFactionId targetFaction;
        public int standingDelta;
        public int hostilityDelta;
        public int supportDelta;
        public string displayText;

        public static MissionConsequencePreview FromDelta(FactionStandingDelta delta)
        {
            var support = delta.TrustDelta + delta.RespectDelta - delta.ResourcePressureDelta - delta.WarExhaustionDelta;
            return new MissionConsequencePreview
            {
                targetFaction = delta.FactionId,
                standingDelta = delta.TrustDelta + delta.RespectDelta,
                hostilityDelta = delta.HostilityDelta,
                supportDelta = support,
                displayText = MissionOutcomeTextLibrary.FormatFactionDelta(delta)
            };
        }
    }
}
