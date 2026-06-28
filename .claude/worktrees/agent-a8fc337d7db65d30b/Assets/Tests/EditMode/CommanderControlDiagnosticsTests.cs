using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderControlDiagnosticsTests
    {
        [Test]
        public void RuntimeState_ExposesControlAttemptDiagnostics()
        {
            var state = new CommanderControlRuntimeState
            {
                LastControlAttemptTime = 12.5f,
                LastControlResult = ControlPermissionResult.DeniedResult("No controllable target nearby"),
                LastControlRejectReason = "No controllable target nearby",
                LastSelectedTargetName = "None",
                LastInputRoute = "NoTarget",
                LastSuggestion = "Press Tab/Q to select target or move closer to a low-rank unit."
            };

            Assert.That(state.LastControlAttemptTime, Is.EqualTo(12.5f));
            Assert.That(state.LastControlResult.Mode, Is.EqualTo(ControlMode.Denied));
            Assert.That(state.LastControlRejectReason, Is.EqualTo("No controllable target nearby"));
            Assert.That(state.LastSelectedTargetName, Is.EqualTo("None"));
            Assert.That(state.LastInputRoute, Is.EqualTo("NoTarget"));
            Assert.That(state.LastSuggestion, Does.Contain("Tab/Q"));
        }

        [Test]
        public void CharacterControlInfo_IncludesExplicitLeaderFlag()
        {
            var data = CharacterData.Create("leader", "Leader", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            data.IsHeroOrLeader = true;

            var info = CharacterControlInfo.FromCharacterData(data);

            Assert.That(info.IsHeroOrLeader, Is.True);
        }

        [Test]
        public void HighRankDenialReason_IsActionable()
        {
            var service = new ControlPermissionService();
            var commander = CommanderProfile.CreateDefault();
            var leader = CharacterData.Create("city_lord", "CityLord", SubFactionId.MotorIronRiders, CharacterRole.CityLord);
            var request = new ControlPermissionRequest
            {
                Commander = commander,
                Target = CharacterControlInfo.FromCharacterData(leader),
                CurrentControlledUnitCount = 0,
                FactionTrust = 80
            };

            var result = service.Evaluate(request);

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.Reason, Is.Not.Empty);
        }
    }
}
