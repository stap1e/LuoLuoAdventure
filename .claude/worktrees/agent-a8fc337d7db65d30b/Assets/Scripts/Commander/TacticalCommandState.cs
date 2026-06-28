using System;
using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip
{
    [Serializable]
    public class TacticalCommandState
    {
        public CommanderCommandType CommandType = CommanderCommandType.None;
        public CharacterEntity Target;
        public CharacterEntity DefendTarget;
        public Vector3 DefendPosition;
        public float DefendRadius;
        public Combatant FocusTarget;
        public float Duration;
        public float ExpiresAtTime;
        public int ResponderCount;
        public bool IssuedByCommander;
        public float IssueTime;
        public string StatusText;

        public bool IsActive => CommandType != CommanderCommandType.None && Target != null;

        public void SetCommand(CommanderCommandType type, CharacterEntity target, float time)
        {
            CommandType = type;
            Target = target;
            IssueTime = time;
            IssuedByCommander = true;
            UpdateStatusText();
        }

        public void SetDefendObjective(CharacterEntity ally, CharacterEntity objective, float radius, float time)
        {
            CommandType = CommanderCommandType.DefendObjective;
            Target = ally;
            DefendTarget = objective;
            DefendPosition = objective != null ? objective.transform.position : Vector3.zero;
            DefendRadius = radius;
            IssueTime = time;
            Duration = 0f;
            ExpiresAtTime = 0f;
            ResponderCount = ally != null ? 1 : 0;
            IssuedByCommander = true;
            UpdateStatusText();
        }

        public void SetFocusFire(CharacterEntity issuerOrSelected, Combatant focusTarget, float duration, int responders, float time)
        {
            CommandType = CommanderCommandType.FocusFire;
            Target = issuerOrSelected;
            FocusTarget = focusTarget;
            Duration = duration;
            ExpiresAtTime = duration > 0f ? time + duration : 0f;
            ResponderCount = responders;
            IssueTime = time;
            IssuedByCommander = true;
            UpdateStatusText();
        }

        public bool IsExpired(float now)
        {
            return Duration > 0f && ExpiresAtTime > 0f && now >= ExpiresAtTime;
        }

        public float RemainingDuration(float now)
        {
            if (Duration <= 0f || ExpiresAtTime <= 0f) return 0f;
            return Mathf.Max(0f, ExpiresAtTime - now);
        }

        public void Clear()
        {
            CommandType = CommanderCommandType.None;
            Target = null;
            DefendTarget = null;
            DefendPosition = Vector3.zero;
            DefendRadius = 0f;
            FocusTarget = null;
            Duration = 0f;
            ExpiresAtTime = 0f;
            ResponderCount = 0;
            IssuedByCommander = false;
            IssueTime = 0f;
            StatusText = "No active command";
        }

        public void UpdateStatusText()
        {
            if (!IsActive && CommandType != CommanderCommandType.FocusFire)
            {
                StatusText = "No active command";
                return;
            }

            switch (CommandType)
            {
                case CommanderCommandType.DefendObjective:
                    var objectiveName = DefendTarget != null && DefendTarget.Data != null
                        ? DefendTarget.Data.DisplayName
                        : DefendTarget != null ? DefendTarget.name : "Objective";
                    StatusText = $"DefendObjective -> {objectiveName}";
                    break;
                case CommanderCommandType.FocusFire:
                    var focusName = FocusTarget != null && FocusTarget.CharacterEntity != null && FocusTarget.CharacterEntity.Data != null
                        ? FocusTarget.CharacterEntity.Data.DisplayName
                        : FocusTarget != null ? FocusTarget.name : "Target";
                    StatusText = $"FocusFire -> {focusName} (Responders: {ResponderCount})";
                    break;
                default:
                    var targetName = Target != null && Target.Data != null ? Target.Data.DisplayName : "Unknown";
                    StatusText = $"{CommandType} -> {targetName}";
                    break;
            }
        }
    }
}
