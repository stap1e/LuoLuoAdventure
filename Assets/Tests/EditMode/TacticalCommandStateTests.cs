using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class TacticalCommandStateTests
    {
        private CharacterEntity CreateTargetEntity()
        {
            var go = new GameObject("Target");
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData("target", "Target", SubFactionId.MotorIronRiders, CharacterRole.Minion, 5));
            return entity;
        }

        [Test]
        public void SetCommand_UpdatesState()
        {
            var target = CreateTargetEntity();
            try
            {
                var state = new TacticalCommandState();
                state.SetCommand(CommanderCommandType.FollowPlayer, target, 1f);

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.FollowPlayer));
                Assert.That(state.IsActive, Is.True);
                Assert.That(state.IssueTime, Is.EqualTo(1f));
            }
            finally { Object.DestroyImmediate(target.gameObject); }
        }

        [Test]
        public void Clear_ResetsState()
        {
            var target = CreateTargetEntity();
            try
            {
                var state = new TacticalCommandState();
                state.SetCommand(CommanderCommandType.AttackCurrentTarget, target, 2f);
                state.Clear();

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.None));
                Assert.That(state.IsActive, Is.False);
                Assert.That(state.StatusText, Is.EqualTo("No active command"));
            }
            finally { Object.DestroyImmediate(target.gameObject); }
        }

        [Test]
        public void UpdateStatusText_ShowsCommandAndTarget()
        {
            var target = CreateTargetEntity();
            try
            {
                var state = new TacticalCommandState();
                state.SetCommand(CommanderCommandType.HoldPosition, target, 0f);

                Assert.That(state.StatusText, Does.Contain("HoldPosition"));
            }
            finally { Object.DestroyImmediate(target.gameObject); }
        }
    }
}
