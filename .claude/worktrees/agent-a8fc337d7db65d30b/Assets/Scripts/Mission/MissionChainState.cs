using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionChainState
    {
        public List<MissionHistoryEntry> CompletedMissions = new List<MissionHistoryEntry>();
        public string ActiveMissionId;
        public List<string> UnlockedMissionIds = new List<string>();
        public int NextSequenceIndex;

        public MissionOutcomeType? GetLastOutcome(string missionId)
        {
            for (int i = CompletedMissions.Count - 1; i >= 0; i--)
            {
                if (CompletedMissions[i].MissionId == missionId)
                    return CompletedMissions[i].Outcome;
            }
            return null;
        }

        public bool HasCompleted(string missionId)
        {
            foreach (var entry in CompletedMissions)
            {
                if (entry.MissionId == missionId)
                    return true;
            }
            return false;
        }

        public MissionHistoryEntry GetLastEntry(string missionId)
        {
            for (int i = CompletedMissions.Count - 1; i >= 0; i--)
            {
                if (CompletedMissions[i].MissionId == missionId)
                    return CompletedMissions[i];
            }
            return null;
        }

        public bool IsUnlocked(string missionId)
        {
            return UnlockedMissionIds.Contains(missionId);
        }
    }
}
