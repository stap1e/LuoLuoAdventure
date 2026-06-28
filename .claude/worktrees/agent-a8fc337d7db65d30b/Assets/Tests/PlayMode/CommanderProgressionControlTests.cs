using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderProgressionControlTests
    {
        [UnityTest]
        public IEnumerator Level1_CannotControlRank3()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "deputy",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.CityLord,
                CommandRank = 3,
                RequiredCommanderLevel = 10,
                TrustToPlayer = 50,
                IsHeroOrLeader = true,
                AllowDirectControl = false,
                AllowTacticalCommand = false
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
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied));

            yield return null;
        }

        [UnityTest]
        public IEnumerator EarnXP_LevelUp_ControlsHigherRank()
        {
            var commander = CommanderProfile.CreateDefault();
            var initialRank = commander.MaxDirectControlRank;

            while (commander.CommanderLevel < 10)
            {
                var xpNeeded = CommanderLevelSystem.ExperienceForLevel(commander.CommanderLevel + 1);
                commander.AddExperience(xpNeeded);
            }

            Assert.That(commander.MaxDirectControlRank, Is.GreaterThan(initialRank));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Rank2Unit_Lv1_TacticalCommand()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "captain_001",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = CharacterRole.Minion,
                CommandRank = 2,
                RequiredCommanderLevel = 5,
                TrustToPlayer = 30,
                IsHeroOrLeader = false,
                AllowDirectControl = false,
                AllowTacticalCommand = true
            };

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 30,
                FactionHostility = 0
            };

            var result = service.Evaluate(request);
            Assert.That(result.Mode, Is.Not.EqualTo(ControlMode.DirectControl));

            yield return null;
        }
    }
}
