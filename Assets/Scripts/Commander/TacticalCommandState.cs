using System;
using UnityEngine;

namespace LuoLuoTrip
{
    [Serializable]
    public class TacticalCommandState
    {
        public CommanderCommandType CommandType = CommanderCommandType.None;
        public CharacterEntity Target;
        public float IssueTime;
        public string StatusText;

        public bool IsActive => CommandType != CommanderCommandType.None && Target != null;

        public void SetCommand(CommanderCommandType type, CharacterEntity target, float time)
        {
            CommandType = type;
            Target = target;
            IssueTime = time;
            UpdateStatusText();
        }

        public void Clear()
        {
            CommandType = CommanderCommandType.None;
            Target = null;
            IssueTime = 0f;
            StatusText = "No active command";
        }

        public void UpdateStatusText()
        {
            if (!IsActive)
            {
                StatusText = "No active command";
                return;
            }

            var targetName = Target != null && Target.Data != null ? Target.Data.DisplayName : "Unknown";
            StatusText = $"{CommandType} -> {targetName}";
        }
    }
}
