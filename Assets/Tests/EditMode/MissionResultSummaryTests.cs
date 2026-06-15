using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionResultSummaryTests
    {
        [Test]
        public void MissionConsequence_ContainsOutcomeAndXP()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "test",
                ProtectedConvoy = true,
                Outcome = MissionOutcomeType.MechaVictory
            };

            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.Outcome, Is.EqualTo(MissionOutcomeType.MechaVictory));
            Assert.That(consequence.CommanderExperienceDelta, Is.GreaterThan(0));
        }

        [Test]
        public void MissionConsequence_ContainsFactionDeltas()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "test",
                Outcome = MissionOutcomeType.MechaVictory
            };

            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.FactionDeltas, Is.Not.Null);
            Assert.That(consequence.FactionDeltas.Count, Is.GreaterThan(0));
        }

        [Test]
        public void MissionConsequence_ContainsSummaryText()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "test",
                Outcome = MissionOutcomeType.BalancedResolution
            };

            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.SummaryText, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ChainService_UnlocksNextMission_AfterRecord()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);

            Assert.That(service.IsUnlocked("border_retaliation"), Is.True);
        }
    }
}
