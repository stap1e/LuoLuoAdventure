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
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var relationshipService = new FactionRelationshipService();
            var dynamicService = new DynamicFactionHostilityService(reputationService, relationshipService);

            var delta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 60);
            reputationService.ApplyDelta(delta);
            Assert.That(dynamicService.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.True);

            var reduceDelta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: -40);
            reputationService.ApplyDelta(reduceDelta);
            Assert.That(dynamicService.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.False);

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
