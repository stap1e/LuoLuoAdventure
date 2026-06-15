using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    [Serializable]
    public class FactionPoliticsState
    {
        private readonly Dictionary<SubFactionId, FactionStanding> _standings = new Dictionary<SubFactionId, FactionStanding>();

        public IReadOnlyDictionary<SubFactionId, FactionStanding> Standings => _standings;

        public void InitializeAll()
        {
            _standings.Clear();
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                var standing = new FactionStanding(id);
                if (GameConstants.IsMotorSubFaction(id))
                {
                    standing.Trust = 30;
                    standing.Hostility = -20;
                }
                else
                {
                    standing.Trust = -10;
                    standing.Hostility = 20;
                }
                standing.Clamp();
                _standings[id] = standing;
            }
        }

        public FactionStanding GetStanding(SubFactionId factionId)
        {
            if (_standings.TryGetValue(factionId, out var standing))
                return standing;
            return new FactionStanding(factionId);
        }

        public void SetStanding(SubFactionId factionId, FactionStanding standing)
        {
            standing.FactionId = factionId;
            standing.Clamp();
            _standings[factionId] = standing;
        }

        public void ApplyDelta(FactionStandingDelta delta)
        {
            if (!_standings.ContainsKey(delta.FactionId))
                _standings[delta.FactionId] = new FactionStanding(delta.FactionId);

            var standing = _standings[delta.FactionId];
            standing.ApplyDelta(delta);
            _standings[delta.FactionId] = standing;
        }

        public void ApplyDeltas(IEnumerable<FactionStandingDelta> deltas)
        {
            foreach (var delta in deltas)
                ApplyDelta(delta);
        }

        public FactionPoliticsSnapshot CreateSnapshot()
        {
            return new FactionPoliticsSnapshot(_standings);
        }

        public void RestoreFromSnapshot(FactionPoliticsSnapshot snapshot)
        {
            _standings.Clear();
            foreach (var entry in snapshot.Entries)
                _standings[entry.FactionId] = entry;
        }
    }

    [Serializable]
    public struct FactionPoliticsEntry
    {
        public SubFactionId FactionId;
        public int Trust;
        public int Respect;
        public int Fear;
        public int Hostility;
        public int ResourcePressure;
        public int WarExhaustion;
    }

    [Serializable]
    public class FactionPoliticsSnapshot
    {
        public List<FactionPoliticsEntry> Entries = new List<FactionPoliticsEntry>();

        public FactionPoliticsSnapshot() { }

        internal FactionPoliticsSnapshot(Dictionary<SubFactionId, FactionStanding> source)
        {
            foreach (var pair in source)
            {
                Entries.Add(new FactionPoliticsEntry
                {
                    FactionId = pair.Value.FactionId,
                    Trust = pair.Value.Trust,
                    Respect = pair.Value.Respect,
                    Fear = pair.Value.Fear,
                    Hostility = pair.Value.Hostility,
                    ResourcePressure = pair.Value.ResourcePressure,
                    WarExhaustion = pair.Value.WarExhaustion
                });
            }
        }
    }
}
