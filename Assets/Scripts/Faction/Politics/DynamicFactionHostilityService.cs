using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    public class DynamicFactionHostilityService
    {
        private readonly FactionReputationService _reputationService;
        private readonly FactionRelationshipService _relationshipService;

        public DynamicFactionHostilityService(
            FactionReputationService reputationService,
            FactionRelationshipService relationshipService)
        {
            _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
            _relationshipService = relationshipService ?? throw new ArgumentNullException(nameof(relationshipService));
        }

        public bool IsHostileToPlayer(SubFactionId factionId)
        {
            return _reputationService.IsFactionHostileToPlayer(factionId);
        }

        public bool IsHostileBetweenFactions(SubFactionId a, SubFactionId b)
        {
            if (_reputationService.IsFactionHostileToPlayer(a) || _reputationService.IsFactionHostileToPlayer(b))
                return true;
            return _relationshipService.Matrix.IsHostile(a, b);
        }

        public bool ShouldAttackPlayer(SubFactionId factionId)
        {
            var standing = _reputationService.GetStanding(factionId);
            return standing.Hostility >= 40 || standing.Trust <= -50;
        }
    }
}
