using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionModifierTests
    {
        [Test]
        public void MechaVictoryModifier_IncreasesBeastHostility()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.BeastHostilityMultiplier, Is.EqualTo(1.5f));
            Assert.That(modifier.SourceOutcome, Is.EqualTo(MissionOutcomeType.MechaVictory));
        }

        [Test]
        public void BeastVictoryModifier_ReducesMechaSupport()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BeastVictory, 200);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.MechaSupportMultiplier, Is.EqualTo(0.5f));
            Assert.That(modifier.MechaCaptainTacticalOnly, Is.True);
        }

        [Test]
        public void BalancedResolutionModifier_EnablesCeasefire()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BalancedResolution, 300);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.CeasefireActive, Is.True);
            Assert.That(modifier.InitialHostilityOffset, Is.EqualTo(-15f));
        }

        [Test]
        public void FailedModifier_EnablesLowTrust()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.Failed, 30);
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.LowTrustMode, Is.True);
            Assert.That(modifier.InitialHostilityOffset, Is.EqualTo(10f));
        }

        [Test]
        public void NoPreviousMission_DefaultModifier()
        {
            var service = new MissionChainService();
            var modifier = service.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.BeastHostilityMultiplier, Is.EqualTo(1f));
            Assert.That(modifier.MechaSupportMultiplier, Is.EqualTo(1f));
            Assert.That(modifier.CeasefireActive, Is.False);
            Assert.That(modifier.LowTrustMode, Is.False);
        }
    }
}
