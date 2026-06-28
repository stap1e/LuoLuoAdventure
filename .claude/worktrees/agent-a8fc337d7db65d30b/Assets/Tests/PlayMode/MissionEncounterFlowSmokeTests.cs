using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class MissionEncounterFlowSmokeTests
    {
        [UnityTest]
        public IEnumerator EncounterRuntime_InitializedInMission()
        {
            var go = new GameObject("EncounterTest");
            try
            {
                var encounter = go.AddComponent<EncounterRuntime>();
                encounter.Initialize(new EncounterDefinition
                {
                    encounterId = "test",
                    attackerFaction = SubFactionId.BeastIronClaw,
                    defenderFaction = SubFactionId.MotorIronRiders
                });
                yield return null;
                Assert.That(encounter.Definition, Is.Not.Null);
                Assert.That(encounter.Definition.encounterId, Is.EqualTo("test"));
            }
            finally
            {
                Object.Destroy(go);
            }
        }

        [UnityTest]
        public IEnumerator MissionAreaRuntime_ActivateAndTick()
        {
            var go = new GameObject("AreaTest");
            try
            {
                var area = go.AddComponent<MissionAreaRuntime>();
                area.Activate("test_mission");
                yield return null;
                Assert.That(area.IsActive, Is.True);
                area.MarkComplete();
                Assert.That(area.IsComplete, Is.True);
            }
            finally
            {
                Object.Destroy(go);
            }
        }
    }
}
