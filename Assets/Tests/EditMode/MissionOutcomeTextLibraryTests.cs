using System;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionOutcomeTextLibraryTests
    {
        [Test]
        public void AllMissionOutcomeTypes_HaveNonEmptySummary()
        {
            foreach (MissionOutcomeType outcome in Enum.GetValues(typeof(MissionOutcomeType)))
            {
                var summary = MissionOutcomeTextLibrary.BuildOutcomeSummary(outcome);

                Assert.That(summary, Is.Not.Null.And.Not.Empty, outcome.ToString());
                Assert.That(summary, Is.Not.EqualTo("No consequence data"), outcome.ToString());
            }
        }

        [TestCase(MissionOutcomeType.BalancedMediation)]
        [TestCase(MissionOutcomeType.MechaSuppression)]
        [TestCase(MissionOutcomeType.BeastNegotiation)]
        [TestCase(MissionOutcomeType.FailedEscalation)]
        [TestCase(MissionOutcomeType.PartialContainment)]
        public void CityGateOutcomes_HaveReadableSummary(MissionOutcomeType outcome)
        {
            var summary = MissionOutcomeTextLibrary.BuildOutcomeSummary(outcome);

            Assert.That(summary.Length, Is.GreaterThan(20));
            Assert.That(summary, Does.Not.Contain("No consequence"));
        }

        [TestCase(MissionOutcomeType.MechaVictory)]
        [TestCase(MissionOutcomeType.BeastVictory)]
        [TestCase(MissionOutcomeType.BalancedResolution)]
        [TestCase(MissionOutcomeType.PartialSuccess)]
        [TestCase(MissionOutcomeType.Failed)]
        public void LegacyOutcomes_HaveReadableSummary(MissionOutcomeType outcome)
        {
            var summary = MissionOutcomeTextLibrary.BuildOutcomeSummary(outcome);

            Assert.That(summary.Length, Is.GreaterThan(20));
        }

        [Test]
        public void MissingConsequence_IsSafe()
        {
            Assert.That(MissionOutcomeTextLibrary.FormatConsequenceSummary(null), Is.EqualTo("No consequence data"));
        }

        [Test]
        public void ResultPanel_UsesSharedOutcomeFormatter()
        {
            foreach (MissionOutcomeType outcome in Enum.GetValues(typeof(MissionOutcomeType)))
                Assert.That(LuoLuoTrip.UI.MissionResultSummaryPanel.BuildOutcomeSummary(outcome), Is.EqualTo(MissionOutcomeTextLibrary.BuildOutcomeSummary(outcome)));
        }
    }
}
