using System;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionHistoryEntry
    {
        public string MissionId;
        public MissionOutcomeType Outcome;
        public int CommanderExperienceDelta;
        public bool SharedEnergy;
        public bool ConvoyDestroyed;
        public bool BeastRaidDefeated;
        public int SequenceIndex;
    }
}
