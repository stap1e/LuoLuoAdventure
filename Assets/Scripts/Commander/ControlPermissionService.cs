using System;

namespace LuoLuoTrip
{
    public class ControlPermissionService
    {
        public const int MinTrustForSyncAssist = -20;
        public const int MinTrustForTacticalCommand = 0;
        public const int MinTrustForDirectControl = 30;
        public const int CrossRaceTrustPenalty = 20;
        public const int CrossRaceSyncRatePenalty = 15;

        public ControlPermissionResult Evaluate(ControlPermissionRequest request)
        {
            if (request == null || request.Commander == null)
                return ControlPermissionResult.DeniedResult("Invalid request");

            var commander = request.Commander;
            var target = request.Target;

            if (commander.CommandCapacity <= request.CurrentControlledUnitCount)
                return ControlPermissionResult.DeniedResult("Command capacity exceeded");

            if (target.IsHeroOrLeader && commander.CommanderLevel < 35)
                return ControlPermissionResult.DeniedResult("Cannot control hero/leader at this level");

            var effectiveTrust = request.FactionTrust - (request.IsCrossRaceControl ? CrossRaceTrustPenalty : 0);

            if (target.AllowDirectControl && commander.MaxDirectControlRank >= target.CommandRank
                && commander.CommanderLevel >= target.RequiredCommanderLevel
                && effectiveTrust >= MinTrustForDirectControl)
            {
                var syncRate = SyncRateCalculator.Calculate(commander, target, request.IsCrossRaceControl, effectiveTrust);
                return ControlPermissionResult.DirectControlResult(syncRate, "Direct control granted");
            }

            if (target.AllowTacticalCommand && commander.MaxTacticalCommandRank >= target.CommandRank
                && commander.CommanderLevel >= target.RequiredCommanderLevel - 5
                && effectiveTrust >= MinTrustForTacticalCommand)
            {
                var syncRate = SyncRateCalculator.Calculate(commander, target, request.IsCrossRaceControl, effectiveTrust);
                return ControlPermissionResult.TacticalCommandResult(syncRate, "Tactical command granted");
            }

            if (effectiveTrust >= MinTrustForSyncAssist && commander.CommanderLevel >= 35
                || (effectiveTrust >= MinTrustForSyncAssist && commander.CommanderLevel >= target.RequiredCommanderLevel - 10))
            {
                var syncRate = SyncRateCalculator.Calculate(commander, target, request.IsCrossRaceControl, effectiveTrust);
                if (syncRate > 0.05f)
                    return ControlPermissionResult.SyncAssistResult(syncRate, "Sync assist available");
            }

            return ControlPermissionResult.DeniedResult("Insufficient level, trust, or capacity");
        }
    }
}
