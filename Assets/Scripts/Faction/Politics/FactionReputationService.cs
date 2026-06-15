using System;
using System.Collections.Generic;
using System.Linq;

namespace LuoLuoTrip
{
    public class FactionReputationService
    {
        private readonly FactionPoliticsState _state;

        public FactionPoliticsState State => _state;

        public FactionReputationService()
        {
            _state = new FactionPoliticsState();
        }

        public FactionReputationService(FactionPoliticsState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void InitializeDefaultPolitics()
        {
            _state.InitializeAll();
        }

        public FactionStanding GetStanding(SubFactionId factionId) => _state.GetStanding(factionId);

        public void ApplyDelta(FactionStandingDelta delta) => _state.ApplyDelta(delta);

        public void ApplyDeltas(IEnumerable<FactionStandingDelta> deltas) => _state.ApplyDeltas(deltas);

        public int GetTrust(SubFactionId factionId) => _state.GetStanding(factionId).Trust;

        public int GetHostility(SubFactionId factionId) => _state.GetStanding(factionId).Hostility;

        public bool IsFactionHostileToPlayer(SubFactionId factionId) =>
            _state.GetStanding(factionId).Hostility >= 40;

        public int GetMainRaceTrust(MainRace race)
        {
            var ids = new List<SubFactionId>();
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                if (GameConstants.GetMainRace(id) == race)
                    ids.Add(id);
            }

            if (ids.Count == 0) return 0;

            return (int)ids.Average(id => _state.GetStanding(id).Trust);
        }

        public FactionPoliticsSnapshot SaveSnapshot() => _state.CreateSnapshot();

        public void LoadSnapshot(FactionPoliticsSnapshot snapshot) => _state.RestoreFromSnapshot(snapshot);
    }
}
