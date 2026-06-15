using System;
using LuoLuoTrip.Combat;
using UnityEngine;

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

        private CharacterEntity _syncAssistTarget;
        private float _originalAttackBonus;
        private float _originalDefenseBonus;

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
                    DeactivateSyncAssist();
                }
            }
        }

        public void ActivateSyncAssist(float duration)
        {
            IsSyncAssistActive = true;
            SyncAssistRemainingTime = duration;
        }

        public void ApplySyncAssistBuff(CharacterEntity target)
        {
            if (target == null || _syncAssistTarget == target) return;

            RemoveSyncAssistBuff();

            _syncAssistTarget = target;
            var combatant = target.GetComponent<Combatant>();
            if (combatant != null)
            {
                _originalAttackBonus = combatant.SyncAssistAttackBonus;
                _originalDefenseBonus = combatant.SyncAssistDefenseBonus;
                combatant.SyncAssistAttackBonus = SyncAssistDamageBonus;
                combatant.SyncAssistDefenseBonus = SyncAssistDamageReduction;
            }
        }

        public void RemoveSyncAssistBuff()
        {
            if (_syncAssistTarget == null) return;

            var combatant = _syncAssistTarget.GetComponent<Combatant>();
            if (combatant != null)
            {
                combatant.SyncAssistAttackBonus = _originalAttackBonus;
                combatant.SyncAssistDefenseBonus = _originalDefenseBonus;
            }
            _syncAssistTarget = null;
        }

        private void DeactivateSyncAssist()
        {
            IsSyncAssistActive = false;
            SyncAssistRemainingTime = 0f;
            RemoveSyncAssistBuff();
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
            if (IsSyncAssistActive)
                DeactivateSyncAssist();
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
