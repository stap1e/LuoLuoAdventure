using System.Collections;
using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CityGateControlDenialSmokeTests
    {
        [UnityTest]
        public IEnumerator HighRankCityLord_DeniesDirectControl()
        {
            var commander = CommanderProfile.CreateDefault(); // Level 1
            var cityLordData = CharacterData.Create("city_lord_test", "CityLord", SubFactionId.MotorIronRiders, CharacterRole.CityLord);
            var target = CharacterControlInfo.FromCharacterData(cityLordData);

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 30,
                FactionHostility = 0
            };

            var service = new ControlPermissionService();
            var result = service.Evaluate(request);

            Assert.That(result.IsAllowed, Is.False, "CityLord must deny DirectControl at level 1");
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied));
            Assert.That(result.Reason, Is.Not.Empty);

            yield return null;
        }

        [UnityTest]
        public IEnumerator HighRankWarKing_DeniesDirectControl()
        {
            var commander = CommanderProfile.CreateDefault();
            var warKingData = CharacterData.Create("war_king_test", "WarKing", SubFactionId.BeastIronClaw, CharacterRole.WarKing);
            var target = CharacterControlInfo.FromCharacterData(warKingData);

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = true,
                CurrentControlledUnitCount = 0,
                FactionTrust = 0,
                FactionHostility = 0
            };

            var service = new ControlPermissionService();
            var result = service.Evaluate(request);

            Assert.That(result.IsAllowed, Is.False, "WarKing must deny DirectControl at level 1");
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied));

            yield return null;
        }

        [UnityTest]
        public IEnumerator LowRankMinion_AllowsDirectControl()
        {
            var commander = CommanderProfile.CreateDefault();
            var minionData = CharacterData.Create("minion_test", "Minion", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            var target = CharacterControlInfo.FromCharacterData(minionData);

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 35,
                FactionHostility = 0
            };

            var service = new ControlPermissionService();
            var result = service.Evaluate(request);

            Assert.That(result.Mode, Is.EqualTo(ControlMode.DirectControl),
                "Low-rank minion with sufficient trust should allow DirectControl");
            Assert.That(result.IsAllowed, Is.True);

            yield return null;
        }

        [UnityTest]
        public IEnumerator MechaCaptain_AllowsTacticalCommand_NotDirectControl()
        {
            var commander = CommanderProfile.CreateDefault();
            var captainData = CharacterData.Create("captain_test", "MechaCaptain", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            captainData.CommandRank = 2;
            captainData.RequiredCommanderLevel = 5;
            captainData.AllowDirectControl = false;
            captainData.AllowTacticalCommand = true;
            var target = CharacterControlInfo.FromCharacterData(captainData);

            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 10,
                FactionHostility = 0
            };

            var service = new ControlPermissionService();
            var result = service.Evaluate(request);

            Assert.That(result.Mode, Is.Not.EqualTo(ControlMode.DirectControl),
                "MechaCaptain with AllowDirectControl=false must not grant DirectControl");
            yield return null;
        }
    }
}
