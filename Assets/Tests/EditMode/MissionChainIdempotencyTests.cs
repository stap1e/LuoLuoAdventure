using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    /// <summary>
    /// Verifies MissionChainService treats RecordMissionResult as idempotent
    /// per missionId unless an explicit allowDuplicate=true reset is requested.
    /// Prevents double mission outcome writes that previously could happen if
    /// a mission script invoked the chain twice or a debug trigger leaked into
    /// the production mission flow.
    /// </summary>
    public class MissionChainIdempotencyTests
    {
        [Test]
        public void RecordMissionResult_DuplicateMissionId_DoesNotAppendSecondEntry()
        {
            var state = new MissionChainState();
            var service = new MissionChainService(state);

            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);
            int afterFirst = state.CompletedMissions.Count;

            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Warning,
                new System.Text.RegularExpressions.Regex("Skip duplicate mission outcome"));
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);

            Assert.That(state.CompletedMissions.Count, Is.EqualTo(afterFirst),
                "Repeated mission outcome must not be appended");
        }

        [Test]
        public void RecordMissionResult_AllowDuplicateTrue_AppendsAgain()
        {
            var state = new MissionChainState();
            var service = new MissionChainService(state);

            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100,
                allowDuplicate: true);

            Assert.That(state.CompletedMissions.Count, Is.EqualTo(2),
                "allowDuplicate=true must let debug-reset paths append a second entry");
        }

        [Test]
        public void RecordMissionResult_DifferentMissionIds_BothAppend()
        {
            var state = new MissionChainState();
            var service = new MissionChainService(state);

            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);
            service.RecordMissionResult("border_retaliation", MissionOutcomeType.BalancedResolution, 50);

            Assert.That(state.CompletedMissions.Count, Is.EqualTo(2));
        }

        [Test]
        public void RecordMissionResult_FirstCallStillUnlocksNextMission()
        {
            var state = new MissionChainState();
            var service = new MissionChainService(state);

            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);
            Assert.That(service.IsUnlocked("border_retaliation"), Is.True);
        }
    }
}
