using LuoLuoTrip.Save;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class SaveRuntimeVerifierTests
    {
        [Test]
        public void ExportSave_ContainsCommanderData()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(false);

            var save = context.ExportSave("player_1", null, null);
            Assert.That(save, Is.Not.Null);
            Assert.That(save.commander, Is.Not.Null);
            Assert.That(save.commander.commanderLevel, Is.GreaterThan(0));
        }

        [Test]
        public void ExportSave_ContainsFactionPolitics()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(false);

            var save = context.ExportSave("player_1", null, null);
            Assert.That(save.factionPolitics, Is.Not.Null);
        }

        [Test]
        public void ApplySave_RestoresCommanderLevel()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(false);

            var save = context.ExportSave("player_1", null, null);
            save.commander.commanderLevel = 10;
            save.commander.experience = 999;

            context.ApplySave(save);
            Assert.That(context.CommanderProfile.CommanderLevel, Is.EqualTo(10));
            Assert.That(context.CommanderProfile.Experience, Is.EqualTo(999));
        }

        [Test]
        public void OldVersionSave_DoesNotCrash()
        {
            var save = new GameSaveData { version = 1 };
            Assert.DoesNotThrow(() =>
            {
                var commander = save.commander;
                Assert.That(commander.commanderLevel, Is.EqualTo(1));
            });
        }
    }
}
