using LuoLuoTrip.UI;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionReadableTextTests
    {
        [TestCase(DemoFlowManager.ConvoyMissionId, "Mission 1")]
        [TestCase(DemoFlowManager.BorderMissionId, "Mission 2")]
        [TestCase(DemoFlowManager.CityGateMissionId, "Mission 3")]
        public void MissionObjectiveHud_DisplayMissionName_IsReadableForAllThreeMissions(string missionId, string expectedPrefix)
        {
            var name = MissionObjectiveHud.DisplayMissionName(missionId);

            Assert.That(name, Does.Contain(expectedPrefix));
            Assert.That(name, Does.Not.Contain("null"));
        }

        [TestCase(MissionOutcomeType.MechaVictory)]
        [TestCase(MissionOutcomeType.BeastVictory)]
        [TestCase(MissionOutcomeType.BalancedResolution)]
        [TestCase(MissionOutcomeType.Failed)]
        [TestCase(MissionOutcomeType.PartialSuccess)]
        [TestCase(MissionOutcomeType.BalancedMediation)]
        [TestCase(MissionOutcomeType.MechaSuppression)]
        [TestCase(MissionOutcomeType.BeastNegotiation)]
        [TestCase(MissionOutcomeType.FailedEscalation)]
        [TestCase(MissionOutcomeType.PartialContainment)]
        public void ResultSummaryText_IsReadableForAllOutcomes(MissionOutcomeType outcome)
        {
            var summary = MissionResultSummaryPanel.BuildOutcomeSummary(outcome);

            Assert.That(summary, Is.Not.Null.And.Not.Empty);
            Assert.That(summary, Does.Not.Contain("null"));
            Assert.That(summary, Is.Not.EqualTo("No consequence data"));
        }

        [Test]
        public void MissingConsequenceFallback_IsSafe()
        {
            var consequence = MissionConsequence.Empty(MissionOutcomeType.Failed);
            var fallback = string.IsNullOrEmpty(consequence.SummaryText)
                ? MissionResultSummaryPanel.BuildOutcomeSummary(consequence.Outcome)
                : consequence.SummaryText;

            Assert.That(fallback, Is.Not.Null.And.Not.Empty);
            Assert.That(fallback, Does.Not.Contain("null"));
        }
    }
}
