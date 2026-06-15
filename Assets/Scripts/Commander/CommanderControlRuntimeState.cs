using System;

namespace LuoLuoTrip
{
    public class CommanderControlRuntimeState
    {
        public CharacterEntity OriginalPlayerEntity;
        public CharacterEntity DirectControlledEntity;
        public CharacterEntity SelectedTarget;
        public CommanderCommandType ActiveCommand;
        public CharacterEntity CommandTarget;
        public ControlPermissionResult LastControlResult;
        public bool IsSyncAssistActive;
        public float SyncAssistRemainingTime;
        public float SyncAssistDamageBonus = 0.25f;
        public float SyncAssistDamageReduction = 0.15f;

        public bool IsDirectControllingOther => DirectControlledEntity != null
            && DirectControlledEntity != OriginalPlayerEntity;

        public bool HasActiveCommand => ActiveCommand != CommanderCommandType.None && CommandTarget != null;

        public void Tick(float deltaTime)
        {
            if (IsSyncAssistActive)
            {
                SyncAssistRemainingTime -= deltaTime;
                if (SyncAssistRemainingTime <= 0f)
                {
                    IsSyncAssistActive = false;
                    SyncAssistRemainingTime = 0f;
                }
            }
        }

        public void ActivateSyncAssist(float duration)
        {
            IsSyncAssistActive = true;
            SyncAssistRemainingTime = duration;
        }

        public void SetDirectControl(CharacterEntity entity)
        {
            DirectControlledEntity = entity;
        }

        public void ReleaseControl()
        {
            DirectControlledEntity = OriginalPlayerEntity;
            ActiveCommand = CommanderCommandType.None;
            CommandTarget = null;
            IsSyncAssistActive = false;
            SyncAssistRemainingTime = 0f;
        }

        public void SetCommand(CommanderCommandType command, CharacterEntity target)
        {
            ActiveCommand = command;
            CommandTarget = target;
        }

        public void ClearCommand()
        {
            ActiveCommand = CommanderCommandType.None;
            CommandTarget = null;
        }
    }
}
