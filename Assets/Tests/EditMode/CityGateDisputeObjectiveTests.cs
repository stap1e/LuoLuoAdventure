using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateDisputeObjectiveTests
    {
        [Test]
        public void ResolveOutcome_CoreDestroyed_ReturnsFailedEscalation()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: false, negotiatorSurvived: true, raidersDefeated: false,
                mechaCasualties: 0, beastCasualties: 0,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.FailedEscalation));
        }

        [Test]
        public void ResolveOutcome_BalancedConditions_ReturnsBalancedMediation()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true, negotiatorSurvived: true, raidersDefeated: true,
                mechaCasualties: 1, beastCasualties: 3,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.BalancedMediation));
        }

        [Test]
        public void ResolveOutcome_NegotiatorDead_RaidersDefeated_ReturnsMechaSuppression()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true, negotiatorSurvived: false, raidersDefeated: true,
                mechaCasualties: 1, beastCasualties: 3,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.MechaSuppression));
        }

        [Test]
        public void ResolveOutcome_HighCasualties_RaidersDefeated_ReturnsMechaSuppression()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true, negotiatorSurvived: true, raidersDefeated: true,
                mechaCasualties: 5, beastCasualties: 5,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.MechaSuppression));
        }

        [Test]
        public void ResolveOutcome_PartialCasualties_ReturnsPartialContainment()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true, negotiatorSurvived: true, raidersDefeated: true,
                mechaCasualties: 3, beastCasualties: 4,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.PartialContainment));
        }

        [Test]
        public void ResolveOutcome_NegotiatorAlive_RaidersNotDefeated_LowBeastCasualties_ReturnsBeastNegotiation()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true, negotiatorSurvived: true, raidersDefeated: false,
                mechaCasualties: 1, beastCasualties: 2,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.BeastNegotiation));
        }

        [Test]
        public void ResolveOutcome_NegotiatorAlive_RaidersNotDefeated_HighBeastCasualties_ReturnsPartialContainment()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true, negotiatorSurvived: true, raidersDefeated: false,
                mechaCasualties: 1, beastCasualties: 6,
                maxMechaForBalanced: 2, maxBeastForBalanced: 4, maxTotalForPartial: 8);
            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.PartialContainment));
        }
    }
}
