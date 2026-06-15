using System;

namespace LuoLuoTrip
{
    public static class SyncRateCalculator
    {
        public const int TrustSyncBonusDivisor = 200;

        public static float Calculate(CommanderProfile commander, CharacterControlInfo target, bool isCrossRace, int effectiveTrust)
        {
            var syncRate = commander.BaseSyncRate;

            if (target.CommandRank > commander.MaxDirectControlRank)
                syncRate -= (target.CommandRank - commander.MaxDirectControlRank) * 0.15f;

            syncRate += effectiveTrust / (float)TrustSyncBonusDivisor;

            if (isCrossRace)
                syncRate -= ControlPermissionService.CrossRaceSyncRatePenalty / 100f;

            if (target.Role == CharacterRole.WarKing || target.Role == CharacterRole.CityLord)
                syncRate -= 0.25f;

            return Math.Clamp(syncRate, 0f, 1f);
        }
    }
}
