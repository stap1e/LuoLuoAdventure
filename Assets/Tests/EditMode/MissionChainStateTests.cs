using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionChainStateTests
    {
        [Test]
        public void RecordMissionResult_StoresEntry()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);

            Assert.That(service.HasCompleted("convoy_energy_conflict"), Is.True);
            Assert.That(service.GetLastOutcome("convoy_energy_conflict"), Is.EqualTo(MissionOutcomeType.MechaVictory));
        }

        [Test]
        public void GetLastOutcome_ReturnsNullForUnknown()
        {
            var service = new MissionChainService();
            Assert.That(service.GetLastOutcome("nonexistent"), Is.Null);
        }

        [Test]
        public void UnlockNextMission_UnlocksBorderRetaliation()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);

            Assert.That(service.IsUnlocked("border_retaliation"), Is.True);
        }

        [Test]
        public void ConvoyEnergyConflict_UnlockedByDefault()
        {
            var service = new MissionChainService();
            Assert.That(service.IsUnlocked("convoy_energy_conflict"), Is.True);
        }

        [Test]
        public void BuildModifiers_MechaVictory_ProducesBeastRetaliation()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);

            var modifier = service.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.BeastHostilityMultiplier, Is.GreaterThan(1f));
            Assert.That(modifier.ModifierId, Is.EqualTo("border_beast_retaliation"));
        }

        [Test]
        public void BuildModifiers_BeastVictory_ProducesMechaDistrust()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BeastVictory, 200);

            var modifier = service.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.MechaSupportMultiplier, Is.LessThan(1f));
            Assert.That(modifier.MechaCaptainTacticalOnly, Is.True);
        }

        [Test]
        public void BuildModifiers_BalancedResolution_ProducesCeasefire()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BalancedResolution, 300);

            var modifier = service.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.CeasefireActive, Is.True);
            Assert.That(modifier.InitialHostilityOffset, Is.LessThan(0f));
        }

        [Test]
        public void BuildModifiers_Failed_ProducesLowTrust()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.Failed, 30);

            var modifier = service.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.LowTrustMode, Is.True);
        }

        [Test]
        public void MultipleMissions_RecordedInSequence()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);
            service.RecordMissionResult("border_retaliation", MissionOutcomeType.MechaVictory, 250);

            Assert.That(service.State.CompletedMissions.Count, Is.EqualTo(2));
        }
    }
}
