using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateCommanderControlTests
    {
        private ControlPermissionService _service;
        private CommanderProfile _commander;

        [SetUp]
        public void SetUp()
        {
            _service = new ControlPermissionService();
            _commander = CommanderProfile.CreateDefault();
        }

        private CharacterControlInfo CreateTarget(int commandRank, int requiredLevel, bool isHeroOrLeader,
            bool allowDirect, bool allowTactical, int trust = 30)
        {
            return new CharacterControlInfo
            {
                CharacterId = "test",
                Faction = SubFactionId.MotorIronRiders,
                Race = MainRace.MotorTribe,
                Role = isHeroOrLeader ? CharacterRole.CityLord : CharacterRole.Minion,
                CommandRank = commandRank,
                RequiredCommanderLevel = requiredLevel,
                TrustToPlayer = trust,
                IsHeroOrLeader = isHeroOrLeader,
                AllowDirectControl = allowDirect,
                AllowTacticalCommand = allowTactical
            };
        }

        [Test]
        public void CityLord_DeniesDirectControl()
        {
            var target = CreateTarget(commandRank: 4, requiredLevel: 35, isHeroOrLeader: true,
                allowDirect: false, allowTactical: true, trust: 40);
            var request = new ControlPermissionRequest
            {
                Commander = _commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 40
            };
            var result = _service.Evaluate(request);
            Assert.That(result.Mode, Is.Not.EqualTo(ControlMode.DirectControl),
                "CityLord must deny DirectControl");
        }

        [Test]
        public void WarKing_DeniesDirectControl_AndTacticalCommand()
        {
            var target = CreateTarget(commandRank: 5, requiredLevel: 45, isHeroOrLeader: true,
                allowDirect: false, allowTactical: false, trust: 40);
            var request = new ControlPermissionRequest
            {
                Commander = _commander,
                Target = target,
                IsCrossRaceControl = true,
                CurrentControlledUnitCount = 0,
                FactionTrust = 40
            };
            var result = _service.Evaluate(request);
            Assert.That(result.Mode, Is.EqualTo(ControlMode.Denied),
                "WarKing must deny both DirectControl and TacticalCommand");
        }

        [Test]
        public void LowRankMecha_AllowsDirectControl()
        {
            var target = CreateTarget(commandRank: 1, requiredLevel: 1, isHeroOrLeader: false,
                allowDirect: true, allowTactical: true, trust: 35);
            var request = new ControlPermissionRequest
            {
                Commander = _commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 35
            };
            var result = _service.Evaluate(request);
            Assert.That(result.Mode, Is.EqualTo(ControlMode.DirectControl),
                "Low-rank Mecha unit with sufficient trust must allow DirectControl");
        }

        [Test]
        public void MechaCaptain_Rank2_DeniesDirect_AllowsTactical()
        {
            var target = CreateTarget(commandRank: 2, requiredLevel: 5, isHeroOrLeader: false,
                allowDirect: false, allowTactical: true, trust: 10);
            var request = new ControlPermissionRequest
            {
                Commander = _commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 10
            };
            var result = _service.Evaluate(request);
            Assert.That(result.Mode, Is.Not.EqualTo(ControlMode.DirectControl),
                "MechaCaptain with AllowDirectControl=false must deny DirectControl");
        }

        [Test]
        public void HighRankUnit_DenialReason_IsClear()
        {
            var target = CreateTarget(commandRank: 5, requiredLevel: 45, isHeroOrLeader: true,
                allowDirect: false, allowTactical: false, trust: 0);
            var request = new ControlPermissionRequest
            {
                Commander = _commander,
                Target = target,
                IsCrossRaceControl = false,
                CurrentControlledUnitCount = 0,
                FactionTrust = 0
            };
            var result = _service.Evaluate(request);
            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.Reason, Is.Not.Empty, "Denial reason must be non-empty");
        }
    }
}
