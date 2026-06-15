using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class MissionGameplayFlowTests
    {
        [UnityTest]
        public IEnumerator ConvoyEnergyConflict_CompleteMechaVictory_ChangesFactionStanding()
        {
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var commander = CommanderProfile.CreateDefault();
            var missionService = new MissionService(reputationService, commander);

            var state = missionService.StartMission("convoy_energy_conflict");
            state.ProtectedConvoy = true;
            state.MechaCasualties = 0;
            state.BeastCasualties = 2;

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

        [UnityTest]
        public IEnumerator ConvoyEnergyConflict_BalancedResolution_LowersHostility()
        {
            var reputationService = new FactionReputationService();
            reputationService.InitializeDefaultPolitics();

            var commander = CommanderProfile.CreateDefault();
            var missionService = new MissionService(reputationService, commander);

            var state = missionService.StartMission("convoy_energy_conflict");
            state.SharedResources = true;
            state.ProtectedConvoy = true;

            var consequence = missionService.CompleteMissionWithOutcome(MissionOutcomeType.BalancedResolution);

            Assert.That(consequence, Is.Not.Null);
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(300));

            foreach (var delta in consequence.FactionDeltas)
            {
                Assert.That(delta.HostilityDelta, Is.LessThanOrEqualTo(0));
            }

            yield return null;
        }
    }
}
