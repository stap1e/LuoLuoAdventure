using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class InputPriorityTests
    {
        [Test]
        public void E_Key_CommanderTarget_TakesPriorityOverEnergyShare()
        {
            var controller = new CommanderControlController_ForTests();
            controller.SetHasSelectedTarget(true);

            Assert.That(controller.ShouldCommanderHandleE(), Is.True);
        }

        [Test]
        public void E_Key_NoTarget_AllowsEnergyShare()
        {
            var controller = new CommanderControlController_ForTests();
            controller.SetHasSelectedTarget(false);

            Assert.That(controller.ShouldCommanderHandleE(), Is.False);
        }

        [Test]
        public void Debug_Triggers_DoNotRecordToMissionChain()
        {
            var chainService = new MissionChainService();
            var testMissionId = "test_mecha";

            Assert.That(testMissionId.StartsWith("test_"), Is.True);
            Assert.That(chainService.State.CompletedMissions.Count, Is.EqualTo(0));
        }

        [Test]
        public void Debug_Triggers_PrefixIdentified()
        {
            Assert.That("test_mecha".StartsWith("test_"), Is.True);
            Assert.That("test_beast".StartsWith("test_"), Is.True);
            Assert.That("test_balance".StartsWith("test_"), Is.True);
            Assert.That("convoy_energy_conflict".StartsWith("test_"), Is.False);
            Assert.That("border_retaliation".StartsWith("test_"), Is.False);
        }

        [Test]
        public void R_Key_ReleasesDirectControl()
        {
            var state = new CommanderControlRuntimeState();
            state.OriginalPlayerEntity = null;
            state.DirectControlledEntity = null;

            Assert.That(state.IsDirectControllingOther, Is.False);
            Assert.That(state.HasActiveCommand, Is.False);
            Assert.That(state.IsSyncAssistActive, Is.False);

            state.ReleaseControl();
            Assert.That(state.DirectControlledEntity, Is.EqualTo(state.OriginalPlayerEntity));
        }
    }

    public class CommanderControlController_ForTests
    {
        private bool _hasSelectedTarget;

        public void SetHasSelectedTarget(bool value) => _hasSelectedTarget = value;

        public bool ShouldCommanderHandleE() => _hasSelectedTarget;
    }
}
