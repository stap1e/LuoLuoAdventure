using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class TacticalCommandStateTests
    {
        [Test]
        public void SetCommand_UpdatesState()
        {
            var state = new TacticalCommandState();
            state.SetCommand(CommanderCommandType.FollowPlayer, null, 1f);

            Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.FollowPlayer));
            Assert.That(state.IsActive, Is.True);
            Assert.That(state.IssueTime, Is.EqualTo(1f));
        }

        [Test]
        public void Clear_ResetsState()
        {
            var state = new TacticalCommandState();
            state.SetCommand(CommanderCommandType.AttackCurrentTarget, null, 2f);
            state.Clear();

            Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.None));
            Assert.That(state.IsActive, Is.False);
            Assert.That(state.StatusText, Is.EqualTo("No active command"));
        }

        [Test]
        public void UpdateStatusText_ShowsCommandAndTarget()
        {
            var state = new TacticalCommandState();
            state.SetCommand(CommanderCommandType.HoldPosition, null, 0f);

            Assert.That(state.StatusText, Does.Contain("HoldPosition"));
        }
    }
}
