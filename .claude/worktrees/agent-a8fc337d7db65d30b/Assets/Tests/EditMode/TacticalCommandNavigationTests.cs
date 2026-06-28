using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class TacticalCommandNavigationTests
    {
        [Test]
        public void SetCommand_FollowPlayer_UpdatesStatusText()
        {
            var state = new TacticalCommandState();
            state.SetCommand(CommanderCommandType.FollowPlayer, null, 0f);
            Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.FollowPlayer));
        }

        [Test]
        public void Clear_ResetsStatusText()
        {
            var state = new TacticalCommandState();
            state.SetCommand(CommanderCommandType.FollowPlayer, null, 0f);
            state.Clear();
            Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.None));
            Assert.That(state.StatusText, Is.EqualTo("No active command"));
        }

        [Test]
        public void IsActive_TrueWhenCommandAndTargetSet()
        {
            var go = new UnityEngine.GameObject("Target");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("t1", "Test", SubFactionId.MotorIronRiders, CharacterRole.Common));
                var state = new TacticalCommandState();
                state.SetCommand(CommanderCommandType.HoldPosition, entity, 1f);
                Assert.That(state.IsActive, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
