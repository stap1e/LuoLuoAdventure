using System;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionModifier
    {
        public string ModifierId;
        public string SourceMissionId;
        public MissionOutcomeType SourceOutcome;
        public float BeastHostilityMultiplier = 1f;
        public float MechaSupportMultiplier = 1f;
        public float InitialHostilityOffset;
        public bool MechaCaptainTacticalOnly;
        public bool CeasefireActive;
        public bool LowTrustMode;
        public string Description;
    }
}
