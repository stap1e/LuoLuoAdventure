using System;

namespace LuoLuoTrip
{
    [Serializable]
    public struct FactionStanding
    {
        public SubFactionId FactionId;
        public int Trust;
        public int Respect;
        public int Fear;
        public int Hostility;
        public int ResourcePressure;
        public int WarExhaustion;

        public const int MinValue = -100;
        public const int MaxValue = 100;

        public FactionStanding(SubFactionId factionId)
        {
            FactionId = factionId;
            Trust = 0;
            Respect = 0;
            Fear = 0;
            Hostility = 0;
            ResourcePressure = 0;
            WarExhaustion = 0;
        }

        public void Clamp()
        {
            Trust = Math.Clamp(Trust, MinValue, MaxValue);
            Respect = Math.Clamp(Respect, MinValue, MaxValue);
            Fear = Math.Clamp(Fear, MinValue, MaxValue);
            Hostility = Math.Clamp(Hostility, MinValue, MaxValue);
            ResourcePressure = Math.Clamp(ResourcePressure, MinValue, MaxValue);
            WarExhaustion = Math.Clamp(WarExhaustion, MinValue, MaxValue);
        }

        public void ApplyDelta(FactionStandingDelta delta)
        {
            Trust += delta.TrustDelta;
            Respect += delta.RespectDelta;
            Fear += delta.FearDelta;
            Hostility += delta.HostilityDelta;
            ResourcePressure += delta.ResourcePressureDelta;
            WarExhaustion += delta.WarExhaustionDelta;
            Clamp();
        }
    }
}
