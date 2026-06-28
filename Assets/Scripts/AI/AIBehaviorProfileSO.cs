using System;
using UnityEngine;

namespace LuoLuoTrip.AI
{
    [CreateAssetMenu(menuName = "LuoLuoTrip/AI/Behavior Profile", fileName = "AIBehaviorProfile")]
    public class AIBehaviorProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string profileId = "default_ai";
        public string displayName = "Default AI";
        public AIBehaviorProfileType profileType = AIBehaviorProfileType.DefensiveGuard;

        [Header("Engagement")]
        public float chaseRadius = 12f;
        public float attackRadiusMultiplier = 1f;
        public float defendRadius = 5f;
        public float maxChaseDistanceFromHome = 0f;

        [Header("Target Preference")]
        public bool prefersObjectiveTargets;
        public bool prefersEnemyUnits = true;
        public bool prefersProtectedTargets;
        public bool canInitiateCombat = true;
        public bool canAttackNeutral;

        [Header("Retreat")]
        public bool canRetreat;
        [Range(0f, 1f)] public float retreatHealthRatio = 0.25f;

        [Header("Commander Response")]
        public bool respondsToTacticalCommand = true;
        public bool respondsToDefendObjective = true;
        public bool respondsToFocusFire = true;

        [Header("Boundaries")]
        public bool respectsMissionBoundaries = true;
        public float threatPriorityBias = 1f;
        public bool holdPositionWhenNoTarget;

        [Header("Behavior Tuning")]
        public float objectivePressureWeight = 8f;
        public float protectedTargetPressureWeight = 10f;
        public float neutralTargetPressureWeight = 4f;
        public float hostileUnitWeight = 2f;
        public float homeDistancePenalty = 0f;
        public float retreatDistance = 4f;
        public float retreatTriggerRadius = 5f;
        public float guardLeashRadius = 0f;
        public float guardReturnSpeedMultiplier = 1.25f;
        public float hardlinerEscalationBias = 0f;
        public float negotiatorThreatAvoidanceBias = 0f;
        public float maxEngageDuration = 0f;
        public float decisionRefreshInterval = 0f;

        [TextArea(2, 5)] public string debugDescription;

        public string DisplayLabel => string.IsNullOrEmpty(displayName) ? profileType.ToString() : displayName;
        public bool IsNonCombatant => !canInitiateCombat && !canAttackNeutral;

        public float EffectiveChaseRadius(float fallback) => chaseRadius > 0f ? chaseRadius : fallback;
        public float EffectiveDefendRadius(float fallback) => defendRadius > 0f ? defendRadius : fallback;
        public float EffectiveAttackRange(float fallback) => Mathf.Max(0.1f, fallback * Mathf.Max(0.1f, attackRadiusMultiplier));
        public float EffectiveMaxChaseDistance => Mathf.Max(0f, maxChaseDistanceFromHome);
        public float EffectiveDecisionRefreshInterval(float fallback) => decisionRefreshInterval > 0f ? decisionRefreshInterval : fallback;
        public float EffectiveGuardLeashRadius(float fallback) => guardLeashRadius > 0f ? guardLeashRadius : fallback;
        public float EffectiveRetreatDistance(float fallback) => retreatDistance > 0f ? retreatDistance : fallback;

        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                error = "profileId is required";
                return false;
            }

            if (chaseRadius < 0f)
            {
                error = "chaseRadius must be >= 0";
                return false;
            }

            if (attackRadiusMultiplier <= 0f)
            {
                error = "attackRadiusMultiplier must be > 0";
                return false;
            }

            if (defendRadius < 0f)
            {
                error = "defendRadius must be >= 0";
                return false;
            }

            if (maxChaseDistanceFromHome < 0f)
            {
                error = "maxChaseDistanceFromHome must be >= 0";
                return false;
            }

            if (retreatHealthRatio < 0f || retreatHealthRatio > 1f)
            {
                error = "retreatHealthRatio must be between 0 and 1";
                return false;
            }

            if (objectivePressureWeight < 0f || protectedTargetPressureWeight < 0f ||
                neutralTargetPressureWeight < 0f || hostileUnitWeight < 0f)
            {
                error = "target pressure weights must be >= 0";
                return false;
            }

            if (homeDistancePenalty < 0f || retreatDistance < 0f || retreatTriggerRadius < 0f || guardLeashRadius < 0f)
            {
                error = "distance tuning fields must be >= 0";
                return false;
            }

            if (guardReturnSpeedMultiplier <= 0f)
            {
                error = "guardReturnSpeedMultiplier must be > 0";
                return false;
            }

            if (hardlinerEscalationBias < 0f || negotiatorThreatAvoidanceBias < 0f ||
                maxEngageDuration < 0f || decisionRefreshInterval < 0f)
            {
                error = "bias and timing fields must be >= 0";
                return false;
            }

            if (profileType == AIBehaviorProfileType.Negotiator && canInitiateCombat)
            {
                error = "Negotiator cannot initiate combat";
                return false;
            }

            if (profileType == AIBehaviorProfileType.NeutralCivilian && (canInitiateCombat || canAttackNeutral))
            {
                error = "NeutralCivilian cannot initiate combat or attack neutral targets";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static void ConfigureDefaults(AIBehaviorProfileSO profile, AIBehaviorProfileType type)
        {
            if (profile == null) return;
            profile.profileType = type;
            profile.profileId = ToProfileId(type);
            profile.displayName = ToDisplayName(type);
            profile.chaseRadius = 12f;
            profile.attackRadiusMultiplier = 1f;
            profile.defendRadius = 5f;
            profile.maxChaseDistanceFromHome = 0f;
            profile.prefersObjectiveTargets = false;
            profile.prefersEnemyUnits = true;
            profile.prefersProtectedTargets = false;
            profile.canInitiateCombat = true;
            profile.canAttackNeutral = false;
            profile.canRetreat = false;
            profile.retreatHealthRatio = 0.25f;
            profile.respondsToTacticalCommand = true;
            profile.respondsToDefendObjective = true;
            profile.respondsToFocusFire = true;
            profile.respectsMissionBoundaries = true;
            profile.threatPriorityBias = 1f;
            profile.holdPositionWhenNoTarget = false;
            profile.objectivePressureWeight = 8f;
            profile.protectedTargetPressureWeight = 10f;
            profile.neutralTargetPressureWeight = 4f;
            profile.hostileUnitWeight = 2f;
            profile.homeDistancePenalty = 0f;
            profile.retreatDistance = 4f;
            profile.retreatTriggerRadius = 5f;
            profile.guardLeashRadius = 0f;
            profile.guardReturnSpeedMultiplier = 1.25f;
            profile.hardlinerEscalationBias = 0f;
            profile.negotiatorThreatAvoidanceBias = 0f;
            profile.maxEngageDuration = 0f;
            profile.decisionRefreshInterval = 0f;

            switch (type)
            {
                case AIBehaviorProfileType.AggressiveRaider:
                    profile.chaseRadius = 16f;
                    profile.defendRadius = 3f;
                    profile.maxChaseDistanceFromHome = 18f;
                    profile.prefersObjectiveTargets = true;
                    profile.prefersProtectedTargets = true;
                    profile.respondsToDefendObjective = false;
                    profile.respondsToFocusFire = false;
                    profile.threatPriorityBias = 1.35f;
                    profile.objectivePressureWeight = 14f;
                    profile.protectedTargetPressureWeight = 13f;
                    profile.hostileUnitWeight = 2f;
                    profile.homeDistancePenalty = 0.15f;
                    profile.maxEngageDuration = 12f;
                    profile.decisionRefreshInterval = 0.45f;
                    profile.debugDescription = "Aggressive raider that pressures objectives and protected targets.";
                    break;
                case AIBehaviorProfileType.DefensiveGuard:
                    profile.chaseRadius = 10f;
                    profile.defendRadius = 6f;
                    profile.maxChaseDistanceFromHome = 7f;
                    profile.holdPositionWhenNoTarget = true;
                    profile.respondsToDefendObjective = true;
                    profile.threatPriorityBias = 1.15f;
                    profile.hostileUnitWeight = 5f;
                    profile.homeDistancePenalty = 0.35f;
                    profile.guardLeashRadius = 8f;
                    profile.guardReturnSpeedMultiplier = 1.4f;
                    profile.decisionRefreshInterval = 0.6f;
                    profile.debugDescription = "Defensive guard that holds near protected points and avoids over-chasing.";
                    break;
                case AIBehaviorProfileType.Negotiator:
                    profile.chaseRadius = 8f;
                    profile.defendRadius = 4f;
                    profile.maxChaseDistanceFromHome = 4f;
                    profile.prefersEnemyUnits = false;
                    profile.canInitiateCombat = false;
                    profile.canRetreat = true;
                    profile.retreatHealthRatio = 0.75f;
                    profile.respondsToTacticalCommand = false;
                    profile.respondsToDefendObjective = false;
                    profile.respondsToFocusFire = false;
                    profile.holdPositionWhenNoTarget = true;
                    profile.retreatDistance = 6f;
                    profile.retreatTriggerRadius = 7f;
                    profile.negotiatorThreatAvoidanceBias = 1.5f;
                    profile.decisionRefreshInterval = 0.4f;
                    profile.debugDescription = "Non-combatant negotiator that should be protected and retreats under threat.";
                    break;
                case AIBehaviorProfileType.Hardliner:
                    profile.chaseRadius = 14f;
                    profile.defendRadius = 4f;
                    profile.maxChaseDistanceFromHome = 12f;
                    profile.prefersProtectedTargets = true;
                    profile.canAttackNeutral = true;
                    profile.respondsToDefendObjective = false;
                    profile.threatPriorityBias = 1.25f;
                    profile.protectedTargetPressureWeight = 12f;
                    profile.neutralTargetPressureWeight = 8f;
                    profile.hardlinerEscalationBias = 4f;
                    profile.homeDistancePenalty = 0.2f;
                    profile.maxEngageDuration = 10f;
                    profile.decisionRefreshInterval = 0.55f;
                    profile.debugDescription = "Escalation-risk hardliner that can pressure protected or neutral targets.";
                    break;
                case AIBehaviorProfileType.CommanderUnit:
                    profile.chaseRadius = 13f;
                    profile.defendRadius = 6f;
                    profile.maxChaseDistanceFromHome = 10f;
                    profile.respondsToTacticalCommand = true;
                    profile.respondsToDefendObjective = true;
                    profile.respondsToFocusFire = true;
                    profile.threatPriorityBias = 1.2f;
                    profile.hostileUnitWeight = 4f;
                    profile.homeDistancePenalty = 0.2f;
                    profile.guardLeashRadius = 9f;
                    profile.maxEngageDuration = 14f;
                    profile.decisionRefreshInterval = 0.55f;
                    profile.debugDescription = "High-rank commander unit that can receive tactical commands but keeps DirectControl rules.";
                    break;
                case AIBehaviorProfileType.NeutralCivilian:
                    profile.chaseRadius = 6f;
                    profile.defendRadius = 3f;
                    profile.maxChaseDistanceFromHome = 3f;
                    profile.prefersEnemyUnits = false;
                    profile.canInitiateCombat = false;
                    profile.canAttackNeutral = false;
                    profile.canRetreat = true;
                    profile.retreatHealthRatio = 0.9f;
                    profile.respondsToTacticalCommand = false;
                    profile.respondsToDefendObjective = false;
                    profile.respondsToFocusFire = false;
                    profile.holdPositionWhenNoTarget = true;
                    profile.retreatDistance = 5f;
                    profile.retreatTriggerRadius = 6f;
                    profile.negotiatorThreatAvoidanceBias = 1f;
                    profile.debugDescription = "Neutral civilian fallback for later non-combatant units.";
                    break;
            }
        }

        public static string ToProfileId(AIBehaviorProfileType type)
        {
            switch (type)
            {
                case AIBehaviorProfileType.AggressiveRaider: return "aggressive_raider";
                case AIBehaviorProfileType.DefensiveGuard: return "defensive_guard";
                case AIBehaviorProfileType.Negotiator: return "negotiator";
                case AIBehaviorProfileType.Hardliner: return "hardliner";
                case AIBehaviorProfileType.CommanderUnit: return "commander_unit";
                case AIBehaviorProfileType.NeutralCivilian: return "neutral_civilian";
                default: return "default_ai";
            }
        }

        public static string ToDisplayName(AIBehaviorProfileType type)
        {
            switch (type)
            {
                case AIBehaviorProfileType.AggressiveRaider: return "Raider: Aggressive";
                case AIBehaviorProfileType.DefensiveGuard: return "Guard: Defensive";
                case AIBehaviorProfileType.Negotiator: return "Negotiator: Non-combatant";
                case AIBehaviorProfileType.Hardliner: return "Hardliner: Escalation risk";
                case AIBehaviorProfileType.CommanderUnit: return "CommanderUnit: Tactical only";
                case AIBehaviorProfileType.NeutralCivilian: return "Civilian: Neutral";
                default: return "Default AI";
            }
        }

        public static string ResourcePath(AIBehaviorProfileType type) => $"AIProfiles/{type}";

        public static AIBehaviorProfileSO LoadDefault(AIBehaviorProfileType type)
        {
            var profile = Resources.Load<AIBehaviorProfileSO>(ResourcePath(type));
            if (profile != null)
                return profile;

            profile = CreateInstance<AIBehaviorProfileSO>();
            ConfigureDefaults(profile, type);
            return profile;
        }

        public static bool TryParseProfileType(string value, out AIBehaviorProfileType type)
        {
            return Enum.TryParse(value, true, out type);
        }
    }
}
