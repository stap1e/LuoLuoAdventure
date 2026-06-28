using LuoLuoTrip.Save;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class SaveLoadRegressionTests
    {
        [Test]
        public void GameSaveData_Defaults_AreNonNull()
        {
            var save = new GameSaveData();

            Assert.That(save.characters, Is.Not.Null);
            Assert.That(save.commander, Is.Not.Null);
            Assert.That(save.factionPolitics, Is.Not.Null);
            Assert.That(save.relationships, Is.Not.Null);
            Assert.That(save.completedMissions, Is.Not.Null);
            Assert.That(save.missionChainState, Is.Not.Null);
        }

        [Test]
        public void GameSaveData_VersionIsCurrent()
        {
            var save = new GameSaveData();
            Assert.That(save.version, Is.EqualTo(SaveConstants.CurrentSaveVersion));
        }

        [Test]
        public void CommanderSaveEntry_Defaults_AreValid()
        {
            var entry = new CommanderSaveEntry();
            Assert.That(entry.commanderLevel, Is.EqualTo(1));
            Assert.That(entry.commandCapacity, Is.GreaterThan(0));
            Assert.That(entry.baseSyncRate, Is.GreaterThan(0f));
        }

        [Test]
        public void MissionChainState_Default_HasConvoyUnlocked()
        {
            var chainService = new MissionChainService();
            Assert.That(chainService.IsUnlocked("convoy_energy_conflict"), Is.True);
            Assert.That(chainService.IsUnlocked("border_retaliation"), Is.False);
        }

        [Test]
        public void ExportSave_IncludesMissionChainState()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(spawnMinionSquads: false);

            var save = context.ExportSave();

            Assert.That(save.missionChainState, Is.Not.Null);
            Assert.That(save.missionChainState.UnlockedMissionIds, Is.Not.Null);
            Assert.That(save.missionChainState.UnlockedMissionIds.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ApplySave_RestoresMissionChainState()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(spawnMinionSquads: false);

            context.MissionChainService.RecordMissionResult("convoy_energy_conflict",
                MissionOutcomeType.MechaVictory, 100);

            var save = context.ExportSave();
            Assert.That(save.missionChainState.CompletedMissions.Count, Is.EqualTo(1));

            var context2 = new LuoLuoTripGameContext();
            context2.InitializeWorld(spawnMinionSquads: false);
            context2.ApplySave(save);

            Assert.That(context2.MissionChainService.State.CompletedMissions.Count, Is.EqualTo(1));
            Assert.That(context2.MissionChainService.HasCompleted("convoy_energy_conflict"), Is.True);
        }
    }
}
