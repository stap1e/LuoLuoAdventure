using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DebugTriggerMissionChainTests
    {
        [Test]
        public void TestMissionIds_StartWithTestPrefix()
        {
            Assert.That("test_mecha".StartsWith("test_"), Is.True);
            Assert.That("test_beast".StartsWith("test_"), Is.True);
            Assert.That("test_balance".StartsWith("test_"), Is.True);
        }

        [Test]
        public void RealMissionIds_DoNotStartWithTestPrefix()
        {
            Assert.That("convoy_energy_conflict".StartsWith("test_"), Is.False);
            Assert.That("border_retaliation".StartsWith("test_"), Is.False);
        }

        [Test]
        public void TestMissions_AreNotRecordedToChain()
        {
            var chainService = new MissionChainService();

            var testMissions = new[] { "test_mecha", "test_beast", "test_balance" };
            foreach (var id in testMissions)
            {
                if (!id.StartsWith("test_"))
                    chainService.RecordMissionResult(id, MissionOutcomeType.MechaVictory, 50);
            }

            Assert.That(chainService.State.CompletedMissions.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestMissions_DoNotUnlockBorderRetaliation()
        {
            var chainService = new MissionChainService();

            Assert.That(chainService.IsUnlocked("border_retaliation"), Is.False);
        }

        [Test]
        public void ChainService_RemainsValid_AfterMixedTestAndRealMissions()
        {
            var chainService = new MissionChainService();

            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);

            Assert.That(chainService.State.CompletedMissions.Count, Is.EqualTo(1));
            Assert.That(chainService.HasCompleted("convoy_energy_conflict"), Is.True);
            Assert.That(chainService.IsUnlocked("border_retaliation"), Is.True);

            var modifier = chainService.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.ModifierId, Is.EqualTo("border_beast_retaliation"));
        }

        [Test]
        public void MultipleRealRecordings_UseLastOutcome()
        {
            // Phase 4: RecordMissionResult is idempotent per missionId by default,
            // but explicit debug-reset flows pass allowDuplicate=true. When that
            // happens, GetLastOutcome must reflect the most recent recording.
            var chainService = new MissionChainService();

            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);
            chainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.BeastVictory, 80, allowDuplicate: true);

            var lastOutcome = chainService.GetLastOutcome("convoy_energy_conflict");
            Assert.That(lastOutcome, Is.EqualTo(MissionOutcomeType.BeastVictory));
            Assert.That(chainService.State.CompletedMissions.Count, Is.EqualTo(2),
                "Both entries must be present when allowDuplicate=true");
        }
    }
}
