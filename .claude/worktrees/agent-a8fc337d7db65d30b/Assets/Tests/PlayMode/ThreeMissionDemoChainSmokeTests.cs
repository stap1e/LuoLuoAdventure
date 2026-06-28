using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class ThreeMissionDemoChainSmokeTests
    {
        [UnityTest]
        public IEnumerator DemoFlowAdvancesThroughThreeMissionChain_WithoutDuplicateRecords()
        {
            var chain = new MissionChainService(new MissionChainState());
            var flow = DemoFlowManager.ResolveStep(chain.State);
            Assert.That(flow, Is.EqualTo(DemoFlowState.ConvoyAvailable));

            chain.RecordMissionResult(DemoFlowManager.ConvoyMissionId, MissionOutcomeType.BalancedResolution, 300);
            Assert.That(DemoFlowManager.ResolveStep(chain.State), Is.EqualTo(DemoFlowState.BorderRetaliationAvailable));

            chain.RecordMissionResult(DemoFlowManager.BorderMissionId, MissionOutcomeType.BalancedResolution, 300);
            Assert.That(DemoFlowManager.ResolveStep(chain.State), Is.EqualTo(DemoFlowState.CityGateAvailable));

            chain.RecordMissionResult(DemoFlowManager.CityGateMissionId, MissionOutcomeType.BalancedMediation, 350);
            Assert.That(DemoFlowManager.ResolveStep(chain.State), Is.EqualTo(DemoFlowState.AllMissionsComplete));

            var countBeforeDuplicate = chain.State.CompletedMissions.Count;
            chain.RecordMissionResult(DemoFlowManager.CityGateMissionId, MissionOutcomeType.MechaSuppression, 250);
            Assert.That(chain.State.CompletedMissions.Count, Is.EqualTo(countBeforeDuplicate));

            yield return null;
        }
    }
}
