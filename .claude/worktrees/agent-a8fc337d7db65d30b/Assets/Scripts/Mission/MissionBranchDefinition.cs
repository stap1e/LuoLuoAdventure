using System;
using UnityEngine;

namespace LuoLuoTrip
{
    [CreateAssetMenu(fileName = "MissionBranch", menuName = "LuoLuoTrip/Mission Branch Definition")]
    public class MissionBranchDefinition : ScriptableObject
    {
        public string BranchId;
        public MissionOutcomeType RequiredOutcome;
        public string SourceMissionId;
        public string Description;

        [Header("Difficulty Modifiers")]
        public float BeastHostilityMultiplier = 1f;
        public float MechaSupportMultiplier = 1f;
        public float InitialHostilityOffset;
        public bool CeasefireActive;
        public bool MechaCaptainTacticalOnly;
        public bool LowTrustMode;

        [Header("Timing")]
        public float DefenseTimerSeconds = 60f;
        public float CeasefireTimerSeconds = 90f;
        public float EvacuationTimerSeconds = 45f;

        [Header("Spawn")]
        public int BeastWaveCount = 3;
        public int BeastPerWave = 3;
        public float BeastSpawnInterval = 10f;

        public static MissionBranchDefinition FromModifier(MissionModifier modifier)
        {
            var def = CreateInstance<MissionBranchDefinition>();
            def.BranchId = modifier.ModifierId;
            def.RequiredOutcome = modifier.SourceOutcome;
            def.SourceMissionId = modifier.SourceMissionId;
            def.Description = modifier.Description;
            def.BeastHostilityMultiplier = modifier.BeastHostilityMultiplier;
            def.MechaSupportMultiplier = modifier.MechaSupportMultiplier;
            def.InitialHostilityOffset = modifier.InitialHostilityOffset;
            def.CeasefireActive = modifier.CeasefireActive;
            def.MechaCaptainTacticalOnly = modifier.MechaCaptainTacticalOnly;
            def.LowTrustMode = modifier.LowTrustMode;

            if (def.CeasefireActive)
                def.DefenseTimerSeconds = def.CeasefireTimerSeconds;
            if (def.LowTrustMode)
                def.DefenseTimerSeconds = def.EvacuationTimerSeconds;

            return def;
        }

        public MissionModifier ToModifier()
        {
            return new MissionModifier
            {
                ModifierId = BranchId,
                SourceMissionId = SourceMissionId,
                SourceOutcome = RequiredOutcome,
                Description = Description,
                BeastHostilityMultiplier = BeastHostilityMultiplier,
                MechaSupportMultiplier = MechaSupportMultiplier,
                InitialHostilityOffset = InitialHostilityOffset,
                CeasefireActive = CeasefireActive,
                MechaCaptainTacticalOnly = MechaCaptainTacticalOnly,
                LowTrustMode = LowTrustMode
            };
        }
    }
}
