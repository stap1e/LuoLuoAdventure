namespace LuoLuoTrip
{
    public struct CommanderActionDescriptor
    {
        public CommanderActionType ActionType;
        public string DisplayName;
        public bool IsAllowed;
        public string DenialReason;
        public string Suggestion;
        public string TargetName;

        public string StatusText => IsAllowed ? "Allowed" : "Denied";

        public CommanderActionDescriptor(CommanderActionType actionType, string displayName, bool isAllowed,
            string denialReason, string suggestion, string targetName)
        {
            ActionType = actionType;
            DisplayName = displayName;
            IsAllowed = isAllowed;
            DenialReason = denialReason;
            Suggestion = suggestion;
            TargetName = targetName;
        }
    }
}
