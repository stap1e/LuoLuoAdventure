using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class DynamicHostilityFlowTests
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
        public IEnumerator MissionFlow_Integration()
        {
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var commander = CommanderProfile.CreateDefault();
            var missionService = new MissionService(reputationService, commander);

            var state = missionService.StartMission("test_convoy");
            state.ProtectedConvoy = true;
            state.Objectives.Add(new MissionObjective
            {
                ObjectiveId = "protect",
                Description = "Protect convoy",
                IsCompleted = true,
                Progress = 1,
                RequiredProgress = 1
            });

            var consequence = missionService.CompleteMissionWithOutcome(MissionOutcomeType.MechaVictory);

            Assert.That(consequence, Is.Not.Null);
            Assert.That(consequence.Outcome, Is.EqualTo(MissionOutcomeType.MechaVictory));
            Assert.That(commander.Experience, Is.GreaterThan(0));

            var mechaTrust = reputationService.GetTrust(SubFactionId.MotorIronRiders);
            var beastHostility = reputationService.GetHostility(SubFactionId.BeastIronClaw);
            Assert.That(mechaTrust, Is.GreaterThan(0));
            Assert.That(beastHostility, Is.GreaterThan(0));

            yield return null;
        }
    }
}
