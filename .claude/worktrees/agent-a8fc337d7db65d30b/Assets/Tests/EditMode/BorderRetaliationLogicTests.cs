using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class BorderRetaliationLogicTests
    {
        [Test]
        public void MechaVictoryModifier_BeastRetaliationBranch()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_beast_retaliation"));
            Assert.That(modifier.BeastHostilityMultiplier, Is.GreaterThan(1f));
        }

        [Test]
        public void BeastVictoryModifier_MechaDistrustBranch()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BeastVictory, 200);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_mecha_distrust"));
            Assert.That(modifier.MechaCaptainTacticalOnly, Is.True);
        }

        [Test]
        public void BalancedResolutionModifier_CeasefireBranch()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BalancedResolution, 300);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_ceasefire"));
            Assert.That(modifier.CeasefireActive, Is.True);
        }

        [Test]
        public void FailedModifier_LowTrustBranch()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.Failed, 30);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_low_trust"));
            Assert.That(modifier.LowTrustMode, Is.True);
        }
    }
}
