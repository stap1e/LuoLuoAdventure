using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionOutcomePreviewPreviousEffectTests
    {
        [Test]
        public void NoPreviousOutcome_DisplaysNoModifier()
        {
            var text = new MissionOutcomePreviewService()
                .BuildPreviousOutcomeEffectText(DemoFlowManager.BorderMissionId, new MissionChainState());

            Assert.That(text, Is.EqualTo("No previous outcome modifier."));
        }

        [Test]
        public void ConvoyMechaVictory_IncreasesBorderRetaliationText()
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.ConvoyMissionId, Outcome = MissionOutcomeType.MechaVictory });

            var text = new MissionOutcomePreviewService().BuildPreviousOutcomeEffectText(DemoFlowManager.BorderMissionId, state);

            Assert.That(text, Does.Contain("Beast retaliation intensified"));
        }

        [Test]
        public void ConvoyBalancedResolution_ReducesBorderHostilityText()
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.ConvoyMissionId, Outcome = MissionOutcomeType.BalancedResolution });

            var text = new MissionOutcomePreviewService().BuildPreviousOutcomeEffectText(DemoFlowManager.BorderMissionId, state);

            Assert.That(text, Does.Contain("Border hostility reduced"));
        }

        [Test]
        public void BorderBalancedResolution_ReducesCityGateHostilityText()
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.BorderMissionId, Outcome = MissionOutcomeType.BalancedResolution });

            var text = new MissionOutcomePreviewService().BuildPreviousOutcomeEffectText(DemoFlowManager.CityGateMissionId, state);

            Assert.That(text, Does.Contain("CityGate mainstream hostility reduced"));
        }

        [TestCase(MissionOutcomeType.Failed)]
        [TestCase(MissionOutcomeType.PartialSuccess)]
        public void BorderFailedOrPartial_IncreasesCityGateTensionText(MissionOutcomeType outcome)
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.BorderMissionId, Outcome = outcome });

            var text = new MissionOutcomePreviewService().BuildPreviousOutcomeEffectText(DemoFlowManager.CityGateMissionId, state);

            Assert.That(text, Does.Contain("CityGate tension increased"));
        }
    }
}
