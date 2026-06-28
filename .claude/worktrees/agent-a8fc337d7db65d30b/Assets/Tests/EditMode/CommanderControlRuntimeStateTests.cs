using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderControlRuntimeStateTests
    {
        [Test]
        public void DirectControl_SetsControlledEntity()
        {
            var state = new CommanderControlRuntimeState();
            var fakeOriginal = new CommanderControlRuntimeState();
            state.OriginalPlayerEntity = null;
            state.DirectControlledEntity = null;

            state.SetDirectControl(null);
            Assert.That(state.DirectControlledEntity, Is.Null);
        }

        [Test]
        public void ReleaseControl_ReturnsToOriginal()
        {
            var state = new CommanderControlRuntimeState
            {
                OriginalPlayerEntity = null,
                DirectControlledEntity = null
            };
            state.SetDirectControl(null);
            state.ReleaseControl();
            Assert.That(state.DirectControlledEntity, Is.EqualTo(state.OriginalPlayerEntity));
        }

        [Test]
        public void TacticalCommand_DoesNotChangeDirectControlled()
        {
            var state = new CommanderControlRuntimeState
            {
                OriginalPlayerEntity = null,
                DirectControlledEntity = null
            };

            state.SetCommand(CommanderCommandType.FollowPlayer, null);
            Assert.That(state.ActiveCommand, Is.EqualTo(CommanderCommandType.FollowPlayer));
            Assert.That(state.DirectControlledEntity, Is.EqualTo(state.OriginalPlayerEntity));
        }

        [Test]
        public void SyncAssist_HasDuration_AndEnds()
        {
            var state = new CommanderControlRuntimeState();
            state.ActivateSyncAssist(3f);

            Assert.That(state.IsSyncAssistActive, Is.True);
            Assert.That(state.SyncAssistRemainingTime, Is.GreaterThan(0f));

            state.Tick(2f);
            Assert.That(state.IsSyncAssistActive, Is.True);

            state.Tick(1.5f);
            Assert.That(state.IsSyncAssistActive, Is.False);
            Assert.That(state.SyncAssistRemainingTime, Is.EqualTo(0f));
        }

        [Test]
        public void ClearCommand_ResetsCommandState()
        {
            var state = new CommanderControlRuntimeState();
            state.SetCommand(CommanderCommandType.AttackCurrentTarget, null);
            state.ClearCommand();

            Assert.That(state.ActiveCommand, Is.EqualTo(CommanderCommandType.None));
            Assert.That(state.CommandTarget, Is.Null);
            Assert.That(state.HasActiveCommand, Is.False);
        }

        [Test]
        public void IsDirectControllingOther_WhenSameAsOriginal_IsFalse()
        {
            var state = new CommanderControlRuntimeState();
            Assert.That(state.IsDirectControllingOther, Is.False);
        }

        [Test]
        public void SyncAssistDamageBonus_HasDefault()
        {
            var state = new CommanderControlRuntimeState();
            Assert.That(state.SyncAssistDamageBonus, Is.GreaterThan(0f));
            Assert.That(state.SyncAssistDamageReduction, Is.GreaterThan(0f));
        }
    }
}
