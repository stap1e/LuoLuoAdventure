using System.Collections.Generic;

namespace LuoLuoTrip
{
    public class FactionConsequenceApplier
    {
        private readonly FactionReputationService _reputationService;

        public FactionConsequenceApplier(FactionReputationService reputationService)
        {
            _reputationService = reputationService;
        }

        public void Apply(IEnumerable<FactionStandingDelta> deltas)
        {
            _reputationService.ApplyDeltas(deltas);
        }

        public void ApplyConsequence(MissionConsequence consequence)
        {
            if (consequence == null) return;
            _reputationService.ApplyDeltas(consequence.FactionDeltas);
        }
    }
}
