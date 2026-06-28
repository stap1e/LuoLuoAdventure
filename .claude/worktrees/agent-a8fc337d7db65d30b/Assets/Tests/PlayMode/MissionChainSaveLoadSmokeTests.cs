using System.Collections;
using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class MissionChainSaveLoadSmokeTests
    {
        [UnityTest]
        public IEnumerator MissionChainState_SurvivesSaveLoadCycle()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(spawnMinionSquads: false);

            context.MissionChainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);

            var save = context.ExportSave();

            var context2 = new LuoLuoTripGameContext();
            context2.InitializeWorld(spawnMinionSquads: false);
            context2.ApplySave(save);

            Assert.That(context2.MissionChainService.HasCompleted("convoy_energy_conflict"), Is.True);
            Assert.That(context2.MissionChainService.IsUnlocked("border_retaliation"), Is.True);

            var modifier = context2.MissionChainService.BuildMissionModifiers("border_retaliation");
            Assert.That(modifier.ModifierId, Is.EqualTo("border_beast_retaliation"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator CommanderProfile_SurvivesSaveLoadCycle()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(spawnMinionSquads: false);

            context.CommanderProfile.AddExperience(500);

            var save = context.ExportSave();

            var context2 = new LuoLuoTripGameContext();
            context2.InitializeWorld(spawnMinionSquads: false);
            context2.ApplySave(save);

            Assert.That(context2.CommanderProfile.Experience, Is.GreaterThanOrEqualTo(500));

            yield return null;
        }

        [UnityTest]
        public IEnumerator EmptySave_DoesNotBreakContext()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(spawnMinionSquads: false);

            var save = new GameSaveData();
            Assert.DoesNotThrow(() => context.ApplySave(save));

            yield return null;
        }
    }
}
