using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DemoFlowManagerTests
    {
        [Test]
        public void NullChain_ReturnsMissionOneHint()
        {
            Assert.That(DemoFlowManager.ResolveStep(null), Is.EqualTo(DemoFlowState.ConvoyAvailable));

            var managerGo = new UnityEngine.GameObject("DemoFlowTest");
            try
            {
                var manager = managerGo.AddComponent<DemoFlowManager>();
                manager.RefreshFromMissionChain((MissionChainState)null);

                Assert.That(manager.GetNextMissionId(), Is.EqualTo(DemoFlowManager.ConvoyMissionId));
                Assert.That(manager.CurrentObjectiveHint, Does.Contain("convoy").IgnoreCase);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(managerGo);
            }
        }

        [Test]
        public void AfterConvoyComplete_ReturnsBorderRetaliation()
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.ConvoyMissionId, Outcome = MissionOutcomeType.BalancedResolution });

            Assert.That(DemoFlowManager.ResolveStep(state), Is.EqualTo(DemoFlowState.BorderRetaliationAvailable));
        }

        [Test]
        public void AfterBorderComplete_ReturnsCityGate()
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.ConvoyMissionId, Outcome = MissionOutcomeType.BalancedResolution });
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.BorderMissionId, Outcome = MissionOutcomeType.BalancedResolution });

            Assert.That(DemoFlowManager.ResolveStep(state), Is.EqualTo(DemoFlowState.CityGateAvailable));
        }

        [Test]
        public void AfterAllComplete_ReturnsAllMissionsComplete()
        {
            var state = new MissionChainState();
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.ConvoyMissionId, Outcome = MissionOutcomeType.BalancedResolution });
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.BorderMissionId, Outcome = MissionOutcomeType.BalancedResolution });
            state.CompletedMissions.Add(new MissionHistoryEntry { MissionId = DemoFlowManager.CityGateMissionId, Outcome = MissionOutcomeType.BalancedMediation });

            Assert.That(DemoFlowManager.ResolveStep(state), Is.EqualTo(DemoFlowState.AllMissionsComplete));
        }
    }
}
