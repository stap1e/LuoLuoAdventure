using LuoLuoTrip.UI;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderManualUsabilityTests
    {
        [Test]
        public void NoTargetE_DiagnosticIsReadable()
        {
            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "None",
                LastControlRejectReason = "No controllable target nearby",
                LastInputRoute = "NoTarget",
                LastSuggestion = "Press Tab/Q to select target or move closer to a low-rank unit."
            };

            var descriptors = CommanderActionPresenter.BuildDescriptors(state);

            Assert.That(descriptors[0].DenialReason, Does.Contain("No target"));
            Assert.That(descriptors[0].Suggestion, Does.Contain("Tab/Q"));
            Assert.That(state.LastInputRoute, Is.EqualTo("NoTarget"));
        }

        [Test]
        public void LowRankControlHint_SaysAllowed()
        {
            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "MechaGateGuard",
                LastSelectedTargetRank = 1,
                LastSelectedTargetTrust = 40,
                LastDirectControlAllowed = true,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastInputRoute = "SelectedTarget"
            };

            var text = string.Join(" | ", CommanderActionPresenter.BuildDescriptors(state).ConvertAll(CommanderActionPresenter.BuildStatusLine));

            Assert.That(text, Does.Contain("DirectControl: Allowed"));
            Assert.That(text, Does.Contain("TacticalCommand: Allowed"));
            Assert.That(text, Does.Contain("SyncAssist: Allowed"));
        }

        [Test]
        public void HighRankControlHint_SaysDeniedWithSuggestion()
        {
            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "MechaHardliner",
                LastSelectedTargetRank = 2,
                LastSelectedTargetAllowDirectControl = false,
                LastDirectControlAllowed = false,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastControlRejectReason = "Direct control disabled",
                LastSuggestion = "Try Tactical Command or select a lower-rank unit.",
                LastInputRoute = "SelectedTarget"
            };

            var descriptors = CommanderActionPresenter.BuildDescriptors(state);
            var direct = descriptors.Find(d => d.ActionType == CommanderActionType.DirectControl);

            Assert.That(direct.StatusText, Is.EqualTo("Denied"));
            Assert.That(direct.DenialReason, Does.Contain("Direct control disabled"));
            Assert.That(direct.Suggestion, Does.Contain("Tactical Command"));
        }

        [Test]
        public void LastInputRouteText_IsReadable()
        {
            var state = new CommanderControlRuntimeState { LastInputRoute = "SelectedTarget -> DirectControl" };

            Assert.That(state.LastInputRoute, Does.Contain("SelectedTarget"));
            Assert.That(state.LastInputRoute, Does.Contain("DirectControl"));
        }
    }
}
