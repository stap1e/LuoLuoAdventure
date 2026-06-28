using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class DynamicHostilityIntegrationTests
    {
        [UnityTest]
        public IEnumerator MissionConsequence_ChangesDynamicHostility()
        {
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var relationshipService = new FactionRelationshipService();
            var dynamicService = new DynamicFactionHostilityService(reputationService, relationshipService);

            Assert.That(dynamicService.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.False);

            var delta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 50);
            reputationService.ApplyDelta(delta);

            Assert.That(dynamicService.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.True);

            yield return null;
        }

        [UnityTest]
        public IEnumerator BalancedResolution_ReducesHostility()
        {
            // Design (Phase 3): BalancedResolution must reduce mainstream hostility
            // below the IsFactionHostileToPlayer threshold (Hostility >= 40), but
            // does not zero it out. The reduction is larger than the buildup so
            // residual extremism is allowed.
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var relationshipService = new FactionRelationshipService();
            var dynamicService = new DynamicFactionHostilityService(reputationService, relationshipService);

            // BeastIronClaw starts at 20. Mission 1 buildup: +60 -> 80 hostile.
            reputationService.ApplyDelta(FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 60));
            Assert.That(dynamicService.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.True,
                "Setup: faction is hostile before BalancedResolution");

            // BalancedResolution: stronger de-escalation -60 -> 20 (below 40 threshold).
            reputationService.ApplyDelta(FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: -60));
            var finalStanding = reputationService.GetStanding(SubFactionId.BeastIronClaw);

            Assert.That(dynamicService.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.False,
                "BalancedResolution must lower hostility below threshold");
            Assert.That(finalStanding.Hostility, Is.GreaterThan(0),
                "BalancedResolution must NOT zero out hostility (extremists remain)");

            yield return null;
        }

        [UnityTest]
        public IEnumerator MissionFlow_IntegrationWithDynamicHostility()
        {
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var relationshipService = new FactionRelationshipService();
            var commander = CommanderProfile.CreateDefault();
            var missionService = new MissionService(reputationService, commander);

            var state = missionService.StartMission("test_mecha");
            var consequence = missionService.CompleteMissionWithOutcome(MissionOutcomeType.MechaVictory);

            var dynamicService = new DynamicFactionHostilityService(reputationService, relationshipService);
            var beastStanding = reputationService.GetStanding(SubFactionId.BeastIronClaw);

            Assert.That(beastStanding.Hostility, Is.GreaterThan(0));

            yield return null;
        }
    }
}
