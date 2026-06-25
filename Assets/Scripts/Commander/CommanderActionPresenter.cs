using System.Collections.Generic;

namespace LuoLuoTrip
{
    public static class CommanderActionPresenter
    {
        public static List<CommanderActionDescriptor> BuildDescriptors(CommanderControlRuntimeState state)
        {
            return BuildDescriptors(state, state?.LastControlResult ?? default);
        }

        public static List<CommanderActionDescriptor> BuildDescriptors(CommanderControlRuntimeState state, ControlPermissionResult lastResult)
        {
            var targetName = GetTargetName(state);
            var noTarget = state == null || string.IsNullOrEmpty(targetName) || targetName == "None";
            var stateReason = state != null ? state.LastControlRejectReason : string.Empty;
            var stateSuggestion = state != null ? state.LastSuggestion : string.Empty;
            var reason = noTarget ? "No target selected" : FirstNonEmpty(stateReason, lastResult.Reason, "Not available");
            var suggestion = noTarget
                ? "Press Tab/Q to select target or move closer to a low-rank unit."
                : FirstNonEmpty(stateSuggestion, BuildFallbackSuggestion(state));
            var directAllowed = state != null && state.LastDirectControlAllowed;
            var tacticalAllowed = state != null && state.LastTacticalCommandAllowed;
            var syncAllowed = state != null && state.LastSyncAssistAllowed;
            var defendAllowed = state != null && state.LastDefendObjectiveAllowed;
            var focusAllowed = state != null && state.LastFocusFireAllowed;

            return new List<CommanderActionDescriptor>
            {
                new CommanderActionDescriptor(CommanderActionType.DirectControl, "DirectControl",
                    directAllowed,
                    directAllowed ? string.Empty : BuildDenialReason(CommanderActionType.DirectControl, noTarget, reason, directAllowed, tacticalAllowed, syncAllowed),
                    BuildSuggestionForAction(CommanderActionType.DirectControl, noTarget, suggestion, directAllowed, tacticalAllowed, syncAllowed, defendAllowed, focusAllowed),
                    targetName),
                new CommanderActionDescriptor(CommanderActionType.TacticalCommand, "TacticalCommand",
                    tacticalAllowed,
                    tacticalAllowed ? string.Empty : BuildDenialReason(CommanderActionType.TacticalCommand, noTarget, reason, directAllowed, tacticalAllowed, syncAllowed),
                    BuildSuggestionForAction(CommanderActionType.TacticalCommand, noTarget, suggestion, directAllowed, tacticalAllowed, syncAllowed, defendAllowed, focusAllowed),
                    targetName),
                new CommanderActionDescriptor(CommanderActionType.SyncAssist, "SyncAssist",
                    syncAllowed,
                    syncAllowed ? string.Empty : BuildDenialReason(CommanderActionType.SyncAssist, noTarget, reason, directAllowed, tacticalAllowed, syncAllowed),
                    BuildSuggestionForAction(CommanderActionType.SyncAssist, noTarget, suggestion, directAllowed, tacticalAllowed, syncAllowed, defendAllowed, focusAllowed),
                    targetName),
                new CommanderActionDescriptor(CommanderActionType.DefendObjective, "DefendObjective",
                    defendAllowed,
                    defendAllowed ? string.Empty : FirstNonEmpty(state?.LastDefendObjectiveReason, noTarget ? "No ally selected" : "No objective selected"),
                    defendAllowed ? "Press G to defend objective." : "Select a low-rank ally and press G to defend an objective.",
                    FirstNonEmpty(state?.LastObjectiveTargetName, targetName)),
                new CommanderActionDescriptor(CommanderActionType.FocusFire, "FocusFire",
                    focusAllowed,
                    focusAllowed ? string.Empty : FirstNonEmpty(state?.LastFocusFireReason, noTarget ? "No hostile target selected" : "No nearby responders"),
                    focusAllowed ? "Press F to order nearby allies to focus fire." : "Select a hostile target near allies and press F to focus fire.",
                    FirstNonEmpty(state?.LastFocusTargetName, targetName))
            };
        }

        public static CommanderActionDescriptor GetRecommendedAction(CommanderControlRuntimeState state)
        {
            var descriptors = BuildDescriptors(state);
            foreach (var descriptor in descriptors)
            {
                if (descriptor.IsAllowed)
                    return descriptor;
            }

            return descriptors.Count > 0 ? descriptors[0] : default;
        }

        public static string BuildStatusLine(CommanderActionDescriptor descriptor)
        {
            return $"{descriptor.DisplayName}: {descriptor.StatusText}";
        }

        private static string GetTargetName(CommanderControlRuntimeState state)
        {
            if (state == null)
                return "None";
            if (!string.IsNullOrEmpty(state.LastSelectedTargetName))
                return state.LastSelectedTargetName;
            if (state.SelectedTarget != null && state.SelectedTarget.Data != null)
                return state.SelectedTarget.Data.DisplayName;
            return "None";
        }

        private static string BuildFallbackSuggestion(CommanderControlRuntimeState state)
        {
            if (state == null)
                return "Press Tab/Q to select target or move closer to a low-rank unit.";
            if (state.LastFocusFireAllowed)
                return "Press F to order nearby allies to focus fire.";
            if (state.LastDefendObjectiveAllowed)
                return "Press G to defend the objective.";
            if (!state.LastDirectControlAllowed && state.LastTacticalCommandAllowed)
                return "Try Tactical Command or select a lower-rank unit.";
            if (!state.LastDirectControlAllowed && state.LastSyncAssistAllowed)
                return "Try Sync Assist instead.";
            if (state.LastDirectControlAllowed)
                return "Press E to control.";
            return "Move closer, improve trust, or select a lower-rank unit.";
        }

        private static string BuildDenialReason(CommanderActionType actionType, bool noTarget, string sharedReason,
            bool directAllowed, bool tacticalAllowed, bool syncAllowed)
        {
            if (noTarget)
                return "No target selected";
            if (actionType == CommanderActionType.TacticalCommand && directAllowed)
                return "DirectControl is the preferred action";
            if (actionType == CommanderActionType.SyncAssist && (directAllowed || tacticalAllowed))
                return tacticalAllowed ? "TacticalCommand is available" : "DirectControl is available";
            return FirstNonEmpty(sharedReason, "Not available");
        }

        private static string BuildSuggestionForAction(CommanderActionType actionType, bool noTarget, string sharedSuggestion,
            bool directAllowed, bool tacticalAllowed, bool syncAllowed, bool defendAllowed, bool focusAllowed)
        {
            if (noTarget)
                return "Press Tab/Q to select target or move closer to a low-rank unit.";
            if (actionType == CommanderActionType.DirectControl && !directAllowed && focusAllowed)
                return "Press F to order nearby allies to focus fire.";
            if (actionType == CommanderActionType.DirectControl && !directAllowed && defendAllowed)
                return "Press G to defend an objective.";
            if (actionType == CommanderActionType.DirectControl && !directAllowed && tacticalAllowed)
                return "Try Tactical Command or select a lower-rank unit.";
            if (actionType == CommanderActionType.DirectControl && !directAllowed && syncAllowed)
                return "Try Sync Assist instead.";
            if (actionType == CommanderActionType.TacticalCommand && tacticalAllowed)
                return "Press E to issue a tactical command.";
            if (actionType == CommanderActionType.SyncAssist && syncAllowed)
                return "Press E to enter Sync Assist.";
            return FirstNonEmpty(sharedSuggestion, "Move closer, improve trust, or select a lower-rank unit.");
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return string.Empty;
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }
    }
}
