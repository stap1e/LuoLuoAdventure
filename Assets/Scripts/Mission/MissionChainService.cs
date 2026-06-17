using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    public class MissionChainService
    {
        private readonly MissionChainState _state;
        private static readonly string[] MissionChainOrder = { "convoy_energy_conflict", "border_retaliation" };

        public MissionChainState State => _state;

        public MissionChainService(MissionChainState state = null)
        {
            _state = state ?? new MissionChainState();
            if (_state.UnlockedMissionIds.Count == 0 && _state.CompletedMissions.Count == 0)
                _state.UnlockedMissionIds.Add("convoy_energy_conflict");
        }

        public void RecordMissionResult(string missionId, MissionOutcomeType outcome, int xpDelta,
            bool sharedEnergy = false, bool convoyDestroyed = false, bool beastRaidDefeated = false,
            bool allowDuplicate = false)
        {
            // Mission chain entries are append-only by sequence index. Re-completing
            // the same missionId is normally a bug (mission flow re-entry / double
            // trigger). Block by default unless allowDuplicate is set (used by
            // explicit debug reset paths only).
            if (!allowDuplicate && _state.HasCompleted(missionId))
            {
                UnityEngine.Debug.LogWarning($"[MissionChain] Skip duplicate mission outcome '{missionId}' (already recorded). Pass allowDuplicate=true for debug reset.");
                return;
            }

            var entry = new MissionHistoryEntry
            {
                MissionId = missionId,
                Outcome = outcome,
                CommanderExperienceDelta = xpDelta,
                SharedEnergy = sharedEnergy,
                ConvoyDestroyed = convoyDestroyed,
                BeastRaidDefeated = beastRaidDefeated,
                SequenceIndex = _state.NextSequenceIndex++
            };
            _state.CompletedMissions.Add(entry);

            UnlockNextMission(missionId);
        }

        public MissionOutcomeType? GetLastOutcome(string missionId)
        {
            return _state.GetLastOutcome(missionId);
        }

        public bool HasCompleted(string missionId)
        {
            return _state.HasCompleted(missionId);
        }

        public bool IsUnlocked(string missionId)
        {
            return _state.IsUnlocked(missionId);
        }

        public void UnlockNextMission(string completedMissionId)
        {
            var idx = Array.IndexOf(MissionChainOrder, completedMissionId);
            if (idx < 0 || idx >= MissionChainOrder.Length - 1) return;

            var nextId = MissionChainOrder[idx + 1];
            if (!_state.UnlockedMissionIds.Contains(nextId))
                _state.UnlockedMissionIds.Add(nextId);
        }

        public MissionModifier BuildMissionModifiers(string nextMissionId)
        {
            var modifier = new MissionModifier
            {
                ModifierId = $"{nextMissionId}_default",
                SourceMissionId = "none",
                Description = "Default conditions"
            };

            if (nextMissionId != "border_retaliation") return modifier;

            var convoyOutcome = GetLastOutcome("convoy_energy_conflict");
            if (convoyOutcome == null) return modifier;

            var convoyEntry = _state.GetLastEntry("convoy_energy_conflict");
            modifier.SourceMissionId = "convoy_energy_conflict";
            modifier.SourceOutcome = convoyOutcome.Value;

            switch (convoyOutcome.Value)
            {
                case MissionOutcomeType.MechaVictory:
                    modifier.ModifierId = "border_beast_retaliation";
                    modifier.BeastHostilityMultiplier = 1.5f;
                    modifier.Description = "Beast retaliation after MechaVictory";
                    break;
                case MissionOutcomeType.BeastVictory:
                    modifier.ModifierId = "border_mecha_distrust";
                    modifier.MechaSupportMultiplier = 0.5f;
                    modifier.MechaCaptainTacticalOnly = true;
                    modifier.Description = "Mecha distrust after BeastVictory";
                    break;
                case MissionOutcomeType.BalancedResolution:
                    modifier.ModifierId = "border_ceasefire";
                    modifier.CeasefireActive = true;
                    modifier.InitialHostilityOffset = -15f;
                    modifier.Description = "Ceasefire after BalancedResolution";
                    break;
                case MissionOutcomeType.PartialSuccess:
                case MissionOutcomeType.Failed:
                    modifier.ModifierId = "border_low_trust";
                    modifier.LowTrustMode = true;
                    modifier.InitialHostilityOffset = 10f;
                    modifier.Description = "Low trust after previous failure";
                    break;
            }

            return modifier;
        }

        public MissionChainState GetSnapshot()
        {
            var json = UnityEngine.JsonUtility.ToJson(_state);
            return UnityEngine.JsonUtility.FromJson<MissionChainState>(json);
        }
    }
}
