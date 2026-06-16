using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionChainRegressionTests
    {
        [Test]
        public void ConvoyCompletion_UnlocksBorderRetaliation()
        {
            var chainService = new MissionChainService();

            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);

            Assert.That(chainService.IsUnlocked("border_retaliation"), Is.True);
        }

        [Test]
        public void ConvoyNotCompleted_BorderRemainsLocked()
        {
            var chainService = new MissionChainService();

            Assert.That(chainService.IsUnlocked("border_retaliation"), Is.False);
        }

        [Test]
        public void MechaVictory_GeneratesBeastRetaliationModifier()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_beast_retaliation"));
            Assert.That(modifier.BeastHostilityMultiplier, Is.GreaterThan(1f));
        }

        [Test]
        public void BeastVictory_GeneratesMechaDistrustModifier()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.BeastVictory, 80);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_mecha_distrust"));
            Assert.That(modifier.MechaSupportMultiplier, Is.LessThan(1f));
            Assert.That(modifier.MechaCaptainTacticalOnly, Is.True);
        }

        [Test]
        public void BalancedResolution_GeneratesCeasefireModifier()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.BalancedResolution, 120);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_ceasefire"));
            Assert.That(modifier.CeasefireActive, Is.True);
        }

        [Test]
        public void Failed_GeneratesLowTrustModifier()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.Failed, 10);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_low_trust"));
            Assert.That(modifier.LowTrustMode, Is.True);
        }

        [Test]
        public void PartialSuccess_GeneratesLowTrustModifier()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.PartialSuccess, 30);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");

            Assert.That(modifier.ModifierId, Is.EqualTo("border_low_trust"));
            Assert.That(modifier.LowTrustMode, Is.True);
        }

        [Test]
        public void ChainState_PreservesHistory()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100, sharedEnergy: true);

            var state = chainService.State;
            Assert.That(state.CompletedMissions.Count, Is.EqualTo(1));
            Assert.That(state.CompletedMissions[0].SharedEnergy, Is.True);
            Assert.That(state.CompletedMissions[0].Outcome, Is.EqualTo(MissionOutcomeType.MechaVictory));
        }

        [Test]
        public void ChainState_SnapshotPreservesData()
        {
            var chainService = new MissionChainService();
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.BeastVictory, 80);

            var snapshot = chainService.GetSnapshot();

            Assert.That(snapshot.CompletedMissions.Count, Is.EqualTo(1));
            Assert.That(snapshot.CompletedMissions[0].Outcome, Is.EqualTo(MissionOutcomeType.BeastVictory));
            Assert.That(snapshot.IsUnlocked("border_retaliation"), Is.True);
        }
    }
}
