using System;
using UnityEngine;

namespace LuoLuoTrip
{
    [Serializable]
    public class EncounterDefinition
    {
        public string encounterId;
        public string displayName;
        public SubFactionId attackerFaction;
        public SubFactionId defenderFaction;
        public float BeastHostilityMultiplier = 1f;
        public float MechaSupportMultiplier = 1f;
        public float InitialHostilityOffset;
        public int RaidUnitCount;
        public float DefenseTimer;
    }
}
