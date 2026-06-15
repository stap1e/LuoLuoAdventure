using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderControlIntegrationTests
    {
        [UnityTest]
        public IEnumerator Lv1_Player_DirectControl_Rank1_Success()
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

            yield return null;
        }

        [UnityTest]
        public IEnumerator Lv1_Player_Cannot_Control_Rank5_WarKing()
        {
            var commander = CommanderProfile.CreateDefault();
            var service = new ControlPermissionService();

            var target = new CharacterControlInfo
            {
                CharacterId = "war_king_001",
                Faction = SubFactionId.BeastIronClaw,
                Race = MainRace.BeastTribe,
                Role = CharacterRole.WarKing,
                CommandRank = 5,
                RequiredCommanderLevel = 45,
                TrustToPlayer = 80,
                IsHeroOrLeader = true,
                AllowDirectControl = false,
                AllowTacticalCommand = false
            };

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = true,
                CurrentControlledUnitCount = 0,
                FactionTrust = 80,
                FactionHostility = 0
            };

            var result = service.Evaluate(request);
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied));
            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.Reason, Is.Not.Empty);

            yield return null;
        }
    }
}
