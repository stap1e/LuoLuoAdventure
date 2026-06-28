using System.Linq;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionOutcomePreviewServiceTests
    {
        [Test]
        public void MissingContext_NoActiveMission_IsSafe()
        {
            var service = new MissionOutcomePreviewService();

            var preview = service.BuildPreview(null, null);

            Assert.That(preview, Is.Not.Null);
            Assert.That(preview.confidenceLabel, Is.EqualTo("Unavailable"));
            Assert.That(preview.outcomeSummary, Does.Contain("No active mission preview"));
        }

        [Test]
        public void CityGateBalancedState_PreviewsBalancedMediation()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, true, 0, 0);

            Assert.That(preview.likelyOutcome, Is.EqualTo(MissionOutcomeType.BalancedMediation));
            Assert.That(preview.isBalancedLikely, Is.True);
            Assert.That(preview.commanderXpPreview, Is.EqualTo(350));
            Assert.That(preview.consequences.Count, Is.GreaterThan(0));
        }

        [Test]
        public void CityGateCoreDestroyed_PreviewsFailedEscalation()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(false, true, true, 0, 0);

            Assert.That(preview.likelyOutcome, Is.EqualTo(MissionOutcomeType.FailedEscalation));
            Assert.That(preview.isFailureLikely, Is.True);
            Assert.That(preview.hasCriticalRisk, Is.True);
            Assert.That(preview.risks.Any(r => r.riskId == "citygate_core_destroyed"), Is.True);
        }

        [Test]
        public void CityGateNegotiatorDeadRaidersDefeated_PreviewsMechaSuppression()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, false, true, 0, 0);

            Assert.That(preview.likelyOutcome, Is.EqualTo(MissionOutcomeType.MechaSuppression));
            Assert.That(preview.risks.Any(r => r.riskId == "citygate_negotiator_dead"), Is.True);
        }

        [Test]
        public void CityGateCasualtiesHigh_PreviewsPartialContainmentOrSuppression()
        {
            var partial = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, true, 3, 4);
            var failedHigh = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, true, 5, 5);

            Assert.That(partial.likelyOutcome, Is.EqualTo(MissionOutcomeType.PartialContainment));
            Assert.That(failedHigh.likelyOutcome, Is.EqualTo(MissionOutcomeType.MechaSuppression));
            Assert.That(failedHigh.risks.Any(r => r.riskId == "citygate_escalation"), Is.True);
        }

        [Test]
        public void ConsequencePreview_IncludesFactionDeltasAndXp()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, true, 0, 0);

            Assert.That(preview.commanderXpPreview, Is.GreaterThan(0));
            Assert.That(preview.consequences.Count, Is.GreaterThan(0));
            Assert.That(preview.consequences.Any(c => !string.IsNullOrEmpty(c.displayText)), Is.True);
        }

        [Test]
        public void Preview_DoesNotMutateMissionChainState()
        {
            var context = new LuoLuoTripGameContext();
            context.MissionChainService.RecordMissionResult(DemoFlowManager.ConvoyMissionId, MissionOutcomeType.BalancedResolution, 300);
            var beforeCount = context.MissionChainService.State.CompletedMissions.Count;
            var beforeXp = context.CommanderProfile.Experience;

            var preview = new MissionOutcomePreviewService().BuildPreview(DemoFlowManager.BorderMissionId, context);

            Assert.That(preview, Is.Not.Null);
            Assert.That(context.MissionChainService.State.CompletedMissions.Count, Is.EqualTo(beforeCount));
            Assert.That(context.CommanderProfile.Experience, Is.EqualTo(beforeXp));
        }
    }
}
