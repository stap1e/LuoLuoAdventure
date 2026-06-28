using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderAuthorityTests
    {
        [Test]
        public void Lv1_Commander_CanDirectControl_Rank1_Common()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "minion_001",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.Minion,
                CommandRank = 1,
                RequiredCommanderLevel = 1,
                TrustToPlayer = 50,
                IsHeroOrLeader = false,
                AllowDirectControl = true,
                AllowTacticalCommand = true
            };

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 50,
                FactionHostility = 0
            };

            var result = service.Evaluate(request);
            Assert.That(result.Mode, Is.EqualTo(ControlMode.DirectControl));
            Assert.That(result.IsAllowed, Is.True);
        }

        [Test]
        public void Lv1_Commander_CannotDirectControl_HighRank()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "city_lord_001",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.CityLord,
                CommandRank = 4,
                RequiredCommanderLevel = 35,
                TrustToPlayer = 80,
                IsHeroOrLeader = true,
                AllowDirectControl = false,
                AllowTacticalCommand = true
            };

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 80,
                FactionHostility = 0
            };

            var result = service.Evaluate(request);
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied));
            Assert.That(result.IsAllowed, Is.False);
        }

        [Test]
        public void HighTrust_LowLevel_Returns_TacticalCommand_Or_SyncAssist()
        {
            var commander = CommanderProfile.CreateDefault();
            commander.AddExperience(10000);
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "elite_001",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.Common,
                CommandRank = 2,
                RequiredCommanderLevel = 10,
                TrustToPlayer = 60,
                IsHeroOrLeader = false,
                AllowDirectControl = true,
                AllowTacticalCommand = true
            };

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 60,
                FactionHostility = 0
            };

            var result = service.Evaluate(request);
            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.Mode, Is.Not.EqualTo(ControlMode.Denied));
        }

        [Test]
        public void CommandCapacityExceeded_ReturnsDenied()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "minion_001",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.Minion,
                CommandRank = 1,
                RequiredCommanderLevel = 1,
                TrustToPlayer = 50,
                IsHeroOrLeader = false,
                AllowDirectControl = true,
                AllowTacticalCommand = true
            };

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = commander.CommandCapacity,
                FactionTrust = 50,
                FactionHostility = 0
            };

            var result = service.Evaluate(request);
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied));
            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.Reason, Does.Contain("capacity"));
        }

        [Test]
        public void SyncRate_ClampedBetween0And1()
        {
            var commander = CommanderProfile.CreateDefault();
            var target = new CharacterControlInfo
            {
                CharacterId = "minion_001",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.Minion,
                CommandRank = 1,
                RequiredCommanderLevel = 1,
                TrustToPlayer = 50,
                IsHeroOrLeader = false,
                AllowDirectControl = true,
                AllowTacticalCommand = true
            };

            var highTrust = 100;
            var rate = SyncRateCalculator.Calculate(commander, target, false, highTrust);
            Assert.That(rate, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rate, Is.LessThanOrEqualTo(1f));

            var lowTrust = -100;
            var rateLow = SyncRateCalculator.Calculate(commander, target, true, lowTrust);
            Assert.That(rateLow, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rateLow, Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void CommanderProfile_Default_IsLv1()
        {
            var profile = CommanderProfile.CreateDefault();
            Assert.That(profile.CommanderLevel, Is.EqualTo(1));
            Assert.That(profile.CommandCapacity, Is.EqualTo(2));
            Assert.That(profile.MaxDirectControlRank, Is.EqualTo(1));
        }

        [Test]
        public void CommanderProfile_AddExperience_LevelsUp()
        {
            var profile = CommanderProfile.CreateDefault();
            profile.AddExperience(500);
            Assert.That(profile.CommanderLevel, Is.GreaterThan(1));
        }

        [Test]
        public void CommanderLevelSystem_CorrectCapacityAtLv5()
        {
            Assert.That(CommanderLevelSystem.GetCommandCapacity(5), Is.EqualTo(4));
        }

        [Test]
        public void CommanderLevelSystem_CorrectDirectControlRankAtLv10()
        {
            Assert.That(CommanderLevelSystem.GetMaxDirectControlRank(10), Is.EqualTo(2));
        }

        [Test]
        public void CrossRaceControl_HasPenalty()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "beast_001",
                Faction = SubFactionId.BeastIronClaw,
                Race = MainRace.BeastTribe,
                Role = CharacterRole.Minion,
                CommandRank = 1,
                RequiredCommanderLevel = 1,
                TrustToPlayer = 50,
                IsHeroOrLeader = false,
                AllowDirectControl = true,
                AllowTacticalCommand = true
            };

            var sameRaceRate = SyncRateCalculator.Calculate(commander, target, false, 50);
            var crossRaceRate = SyncRateCalculator.Calculate(commander, target, true, 50);
            Assert.That(crossRaceRate, Is.LessThan(sameRaceRate));
        }
    }
}
