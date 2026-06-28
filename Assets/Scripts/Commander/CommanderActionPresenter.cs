using System.Collections.Generic;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;

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
            var profileSuggestion = BuildProfileSuggestion(state);
            var noTarget = state == null || string.IsNullOrEmpty(targetName) || targetName == "None";
            var stateReason = state != null ? state.LastControlRejectReason : string.Empty;
            var stateSuggestion = state != null ? state.LastSuggestion : string.Empty;
            var reason = noTarget ? "No target selected" : FirstNonEmpty(stateReason, lastResult.Reason, "Not available");
            var suggestion = noTarget
                ? "Press Tab/Q to select target or move closer to a low-rank unit."
                : FirstNonEmpty(profileSuggestion, stateSuggestion, BuildFallbackSuggestion(state));
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

        public static string BuildProfileSummary(SimpleCombatAI ai)
        {
            if (ai == null)
                return "AI Profile: Default AI";
            return $"AI Profile: {ai.CurrentBehaviorLabel}";
        }

        public static string BuildBehaviorSummary(SimpleCombatAI ai)
        {
            if (ai == null)
                return "Behavior: Default AI";

            var behavior = !string.IsNullOrEmpty(ai.LastProfileDecision) ? ai.LastProfileDecision : ai.CurrentBehaviorLabel;
            return $"Behavior: {behavior}";
        }

        public static string BuildResponseSummary(SimpleCombatAI ai)
        {
            if (ai == null)
                return "Responds: Tactical Yes | Defend Yes | Focus Yes";

            return $"Responds: Tactical {YesNo(ai.RespondsToTacticalCommand)} | Defend {YesNo(ai.RespondsToDefendObjective)} | Focus {YesNo(ai.RespondsToFocusFire)}";
        }

        public static string BuildProfileSuggestion(SimpleCombatAI ai)
        {
            var profile = ai != null ? ai.BehaviorProfile : null;
            if (profile == null) return "Default AI behavior. Select a valid target for commands.";

            switch (profile.profileType)
            {
                case AIBehaviorProfileType.DefensiveGuard:
                    return "Use G to defend objective.";
                case AIBehaviorProfileType.Negotiator:
                case AIBehaviorProfileType.NeutralCivilian:
                    return "Protect this non-combatant.";
                case AIBehaviorProfileType.CommanderUnit:
                    return "High-rank unit: Tactical only.";
                case AIBehaviorProfileType.Hardliner:
                    return "Escalation risk. Use F to suppress if hostile.";
                case AIBehaviorProfileType.AggressiveRaider:
                    return "Use F to focus fire raider.";
                default:
                    return "Use profile-compatible tactical commands.";
            }
        }

        private static string YesNo(bool value) => value ? "Yes" : "No";

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

        private static string BuildProfileSuggestion(CommanderControlRuntimeState state)
        {
            var ai = state?.SelectedTarget != null ? state.SelectedTarget.GetComponent<SimpleCombatAI>() : null;
            var profile = ai != null ? ai.BehaviorProfile : null;
            if (profile == null) return string.Empty;

            switch (profile.profileType)
            {
                case AIBehaviorProfileType.DefensiveGuard:
                    return "This unit is defensive. Use DefendObjective.";
                case AIBehaviorProfileType.Negotiator:
                case AIBehaviorProfileType.NeutralCivilian:
                    return "This unit is non-combatant. Protect it.";
                case AIBehaviorProfileType.CommanderUnit:
                    return "This unit is high-rank. TacticalCommand only.";
                case AIBehaviorProfileType.Hardliner:
                    return "Escalation risk. Consider FocusFire or protect non-combatants.";
                case AIBehaviorProfileType.AggressiveRaider:
                    return "Aggressive raider. Use FocusFire if allies can respond.";
                default:
                    return string.Empty;
            }
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
            if (!string.IsNullOrEmpty(sharedSuggestion) && sharedSuggestion.StartsWith("This unit"))
                return sharedSuggestion;
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
