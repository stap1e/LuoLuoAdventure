using LuoLuoTrip.Save;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class SaveDataMigrationTests
    {
        [Test]
        public void V1Save_DefaultsNewFields()
        {
            var save = new GameSaveData { version = 1 };

            Assert.That(save.commander, Is.Not.Null);
            Assert.That(save.commander.commanderLevel, Is.EqualTo(1));
            Assert.That(save.factionPolitics, Is.Not.Null);
            Assert.That(save.completedMissions, Is.Not.Null);
        }

        [Test]
        public void V2Save_RestoresCommanderProfile()
        {
            var save = new GameSaveData { version = 2 };
            save.commander = new CommanderSaveEntry
            {
                commanderLevel = 5,
                experience = 500,
                commandCapacity = 4,
                maxDirectControlRank = 2,
                maxTacticalCommandRank = 1,
                baseSyncRate = 0.35f,
                mechaTrust = 40,
                beastTrust = -20,
                balanceScore = 10
            };

            var profile = CommanderProfile.CreateDefault();
            profile.CommanderLevel = save.commander.commanderLevel;
            profile.Experience = save.commander.experience;
            profile.MechaTrust = save.commander.mechaTrust;

            Assert.That(profile.CommanderLevel, Is.EqualTo(5));
            Assert.That(profile.Experience, Is.EqualTo(500));
            Assert.That(profile.MechaTrust, Is.EqualTo(40));
        }

        [Test]
        public void V2Save_RestoresFactionPolitics()
        {
            var save = new GameSaveData { version = 2 };
            save.factionPolitics = new FactionPoliticsSnapshot();
            save.factionPolitics.Entries.Add(new FactionPoliticsEntry
            {
                FactionId = SubFactionId.MotorIronRiders,
                Trust = 50,
                Hostility = -10
            });

            var state = new FactionPoliticsState();
            state.RestoreFromSnapshot(save.factionPolitics);

            var standing = state.GetStanding(SubFactionId.MotorIronRiders);
            Assert.That(standing.Trust, Is.EqualTo(50));
            Assert.That(standing.Hostility, Is.EqualTo(-10));
        }

        [Test]
        public void EmptyFactionPolitics_DoesNotCrash()
        {
            var save = new GameSaveData { version = 2 };
            save.factionPolitics = new FactionPoliticsSnapshot();

            var state = new FactionPoliticsState();
            Assert.DoesNotThrow(() => state.RestoreFromSnapshot(save.factionPolitics));
        }
    }
}
