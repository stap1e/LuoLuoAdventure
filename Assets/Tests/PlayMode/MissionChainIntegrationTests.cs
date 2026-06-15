using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class MissionChainIntegrationTests
    {
        [UnityTest]
        public IEnumerator CompleteConvoyEnergy_UnlocksBorderRetaliation()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);

            Assert.That(service.IsUnlocked("border_retaliation"), Is.True);

            yield return null;
        }

        [UnityTest]
        public IEnumerator BuildModifiers_AfterMechaVictory_ReturnsBeastRetaliation()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 200);

            var modifier = service.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.ModifierId, Is.EqualTo("border_beast_retaliation"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator MissionChainState_SavesAndRestores()
        {
            var service = new MissionChainService();
            service.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BalancedResolution, 300);

            var snapshot = service.GetSnapshot();
            Assert.That(snapshot.CompletedMissions.Count, Is.EqualTo(1));
            Assert.That(snapshot.UnlockedMissionIds, Does.Contain("border_retaliation"));

            yield return null;
        }
    }
}
