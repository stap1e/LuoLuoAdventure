using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class BorderRetaliationBranchRegressionTests
    {
        [TestCase(MissionOutcomeType.MechaVictory, "border_beast_retaliation")]
        [TestCase(MissionOutcomeType.BeastVictory, "border_mecha_distrust")]
        [TestCase(MissionOutcomeType.BalancedResolution, "border_ceasefire")]
        [TestCase(MissionOutcomeType.Failed, "border_low_trust")]
        [TestCase(MissionOutcomeType.PartialSuccess, "border_low_trust")]
        public void EachOutcome_GeneratesCorrectBranchId(MissionOutcomeType outcome, string expectedBranchId)
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict", outcome, 50);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo(expectedBranchId));
        }

        [Test]
        public void BeastRetaliation_HasIncreasedBeastHostility()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.BeastHostilityMultiplier, Is.GreaterThan(1f));
            Assert.That(modifier.SourceOutcome, Is.EqualTo(MissionOutcomeType.MechaVictory));
        }

        [Test]
        public void MechaDistrust_RestrictsCaptainToTactical()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.BeastVictory, 80);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.MechaCaptainTacticalOnly, Is.True);
            Assert.That(modifier.MechaSupportMultiplier, Is.LessThan(1f));
        }

        [Test]
        public void Ceasefire_HasNegativeHostilityOffset()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.BalancedResolution, 120);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.CeasefireActive, Is.True);
            Assert.That(modifier.InitialHostilityOffset, Is.LessThan(0f));
        }

        [Test]
        public void LowTrust_HasPositiveHostilityOffset()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.Failed, 10);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.LowTrustMode, Is.True);
            Assert.That(modifier.InitialHostilityOffset, Is.GreaterThan(0f));
        }

        [Test]
        public void NoConvoyCompletion_ReturnsDefaultModifier()
        {
            var chainService = new MissionChainService();

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_retaliation_default"));
            Assert.That(modifier.BeastHostilityMultiplier, Is.EqualTo(1f));
            Assert.That(modifier.CeasefireActive, Is.False);
            Assert.That(modifier.LowTrustMode, Is.False);
        }
    }
}
