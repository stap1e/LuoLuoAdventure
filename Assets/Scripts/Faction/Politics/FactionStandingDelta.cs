using System;

namespace LuoLuoTrip
{
    [Serializable]
    public struct FactionStandingDelta
    {
        public SubFactionId FactionId;
        public int TrustDelta;
        public int RespectDelta;
        public int FearDelta;
        public int HostilityDelta;
        public int ResourcePressureDelta;
        public int WarExhaustionDelta;

        public static FactionStandingDelta Create(SubFactionId factionId,
            int trust = 0, int respect = 0, int fear = 0,
            int hostility = 0, int resourcePressure = 0, int warExhaustion = 0)
        {
            return new FactionStandingDelta
            {
                FactionId = factionId,
                TrustDelta = trust,
                RespectDelta = respect,
                FearDelta = fear,
                HostilityDelta = hostility,
                ResourcePressureDelta = resourcePressure,
                WarExhaustionDelta = warExhaustion
            };
        }
    }
}
