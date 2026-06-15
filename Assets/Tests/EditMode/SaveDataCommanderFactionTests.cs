using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class SaveDataCommanderFactionTests
    {
        [Test]
        public void GameSaveData_NewFields_HaveDefaults()
        {
            var save = new GameSaveData();

            Assert.That(save.commander, Is.Not.Null);
            Assert.That(save.commander.commanderLevel, Is.EqualTo(1));
            Assert.That(save.commander.commandCapacity, Is.GreaterThan(0));
            Assert.That(save.factionPolitics, Is.Not.Null);
            Assert.That(save.completedMissions, Is.Not.Null);
        }

        [Test]
        public void CommanderProfile_WriteAndRestore()
        {
            var original = CommanderProfile.CreateDefault();
            original.AddExperience(500);
            original.MechaTrust = 40;
            original.BeastTrust = -20;
            original.BalanceScore = 10;

            var entry = new CommanderSaveEntry
            {
                commanderLevel = original.CommanderLevel,
                experience = original.Experience,
                commandCapacity = original.CommandCapacity,
                maxDirectControlRank = original.MaxDirectControlRank,
                maxTacticalCommandRank = original.MaxTacticalCommandRank,
                baseSyncRate = original.BaseSyncRate,
                mechaTrust = original.MechaTrust,
                beastTrust = original.BeastTrust,
                balanceScore = original.BalanceScore
            };

            var restored = CommanderProfile.CreateDefault();
            restored.CommanderLevel = entry.commanderLevel;
            restored.Experience = entry.experience;
            restored.CommandCapacity = entry.commandCapacity;
            restored.MaxDirectControlRank = entry.maxDirectControlRank;
            restored.MaxTacticalCommandRank = entry.maxTacticalCommandRank;
            restored.BaseSyncRate = entry.baseSyncRate;
            restored.MechaTrust = entry.mechaTrust;
            restored.BeastTrust = entry.beastTrust;
            restored.BalanceScore = entry.balanceScore;

            Assert.That(restored.CommanderLevel, Is.EqualTo(original.CommanderLevel));
            Assert.That(restored.Experience, Is.EqualTo(original.Experience));
            Assert.That(restored.MechaTrust, Is.EqualTo(original.MechaTrust));
            Assert.That(restored.BeastTrust, Is.EqualTo(original.BeastTrust));
            Assert.That(restored.BalanceScore, Is.EqualTo(original.BalanceScore));
        }

        [Test]
        public void FactionPoliticsState_WriteAndRestore()
        {
            var state = new FactionPoliticsState();
            state.InitializeAll();
            state.ApplyDelta(FactionStandingDelta.Create(SubFactionId.MotorIronRiders, trust: 30, hostility: -10));
            state.ApplyDelta(FactionStandingDelta.Create(SubFactionId.BeastIronClaw, trust: -20, hostility: 25));

            var snapshot = state.CreateSnapshot();

            var restored = new FactionPoliticsState();
            restored.RestoreFromSnapshot(snapshot);

            Assert.That(restored.GetStanding(SubFactionId.MotorIronRiders).Trust,
                Is.EqualTo(state.GetStanding(SubFactionId.MotorIronRiders).Trust));
            Assert.That(restored.GetStanding(SubFactionId.BeastIronClaw).Hostility,
                Is.EqualTo(state.GetStanding(SubFactionId.BeastIronClaw).Hostility));
        }

        [Test]
        public void OldSaveVersion_DoesNotCrash()
        {
            var save = new GameSaveData { version = 1 };

            Assert.That(save.commander, Is.Not.Null);
            Assert.That(save.factionPolitics, Is.Not.Null);
            Assert.DoesNotThrow(() =>
            {
                var level = save.commander.commanderLevel;
                var trust = save.commander.mechaTrust;
            });
        }
    }
}
