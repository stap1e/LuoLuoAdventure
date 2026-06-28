using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeDemoFlowSmokeTests
    {
        [UnityTest]
        public IEnumerator DemoFlowComponents_QueryInitialNextMission()
        {
            var go = new GameObject("DemoFlowSmoke");
            try
            {
                var manager = go.AddComponent<DemoFlowManager>();
                var hud = go.AddComponent<LuoLuoTrip.UI.DemoFlowHud>();
                hud.SetFlowManager(manager);
                yield return null;

                manager.RefreshFromMissionChain((MissionChainState)null);

                Assert.That(manager.GetNextMissionId(), Is.EqualTo(DemoFlowManager.ConvoyMissionId));
                Assert.That(hud.FlowManager, Is.EqualTo(manager));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
