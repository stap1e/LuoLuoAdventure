using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderActionPresenterTests
    {
        [Test]
        public void LowRankTarget_ShowsDirectControlAllowed()
        {
            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "Low-Rank Ally",
                LastDirectControlAllowed = true,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastSuggestion = "Press E to control."
            };

            var descriptors = CommanderActionPresenter.BuildDescriptors(state);

            Assert.That(descriptors.Find(d => d.ActionType == CommanderActionType.DirectControl).IsAllowed, Is.True);
        }

        [Test]
        public void LeaderTarget_ShowsDirectDeniedAndTacticalSuggestion()
        {
            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "CityLord",
                LastControlRejectReason = "Leader unit",
                LastDirectControlAllowed = false,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastSuggestion = "Try Tactical Command or select a lower-rank unit."
            };

            var descriptors = CommanderActionPresenter.BuildDescriptors(state);
            var direct = descriptors.Find(d => d.ActionType == CommanderActionType.DirectControl);
            var tactical = descriptors.Find(d => d.ActionType == CommanderActionType.TacticalCommand);

            Assert.That(direct.IsAllowed, Is.False);
            Assert.That(direct.DenialReason, Does.Contain("Leader"));
            Assert.That(tactical.IsAllowed, Is.True);
            Assert.That(direct.Suggestion, Does.Contain("Tactical"));
        }

        [Test]
        public void NoTarget_ShowsSelectTargetSuggestion()
        {
            var descriptors = CommanderActionPresenter.BuildDescriptors(new CommanderControlRuntimeState());
            var direct = descriptors.Find(d => d.ActionType == CommanderActionType.DirectControl);

            Assert.That(direct.IsAllowed, Is.False);
            Assert.That(direct.Suggestion, Does.Contain("select target").IgnoreCase);
        }

        [Test]
        public void SyncAssistDescriptor_IsNullSafe()
        {
            var descriptors = CommanderActionPresenter.BuildDescriptors(null);

            Assert.That(descriptors, Has.Count.EqualTo(5));
            Assert.That(descriptors.Exists(d => d.ActionType == CommanderActionType.SyncAssist), Is.True);
            Assert.That(descriptors.Exists(d => d.ActionType == CommanderActionType.DefendObjective), Is.True);
            Assert.That(descriptors.Exists(d => d.ActionType == CommanderActionType.FocusFire), Is.True);
        }
    }
}
