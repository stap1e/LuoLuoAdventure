using LuoLuoTrip.UI;
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

        [Test]
        public void CityGateBalancedMediationSummary_IsReadable()
        {
            var summary = MissionResultSummaryPanel.BuildOutcomeSummary(MissionOutcomeType.BalancedMediation);

            Assert.That(summary, Does.Contain("hostility").IgnoreCase);
            Assert.That(summary, Does.Contain("extremists").IgnoreCase);
        }

        [Test]
        public void MechaSuppressionSummary_IsReadable()
        {
            var summary = MissionResultSummaryPanel.BuildOutcomeSummary(MissionOutcomeType.MechaSuppression);

            Assert.That(summary, Does.Contain("Mecha"));
            Assert.That(summary, Does.Contain("Beast"));
        }

        [TestCase(MissionOutcomeType.MechaVictory)]
        [TestCase(MissionOutcomeType.BeastVictory)]
        [TestCase(MissionOutcomeType.BalancedResolution)]
        [TestCase(MissionOutcomeType.PartialSuccess)]
        [TestCase(MissionOutcomeType.Failed)]
        public void LegacyOutcomes_HaveReadableSummaries(MissionOutcomeType outcome)
        {
            var summary = MissionResultSummaryPanel.BuildOutcomeSummary(outcome);

            Assert.That(summary, Is.Not.Empty);
            Assert.That(summary, Is.Not.EqualTo("No consequence data"));
        }

        [Test]
        public void UnknownOutcomeFallback_IsSafe()
        {
            var summary = MissionResultSummaryPanel.BuildOutcomeSummary((MissionOutcomeType)999);

            Assert.That(summary, Is.EqualTo("No consequence data"));
        }
    }
}
