using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.AI
{
    public static class AITargetSelectionUtility
    {
        public static AITargetScore ScoreTarget(
            Combatant self,
            Combatant candidate,
            AIBehaviorProfileSO profile,
            Combatant forcedTarget,
            Transform protectedTarget,
            Vector3 origin,
            float radius,
            bool isHostile,
            bool isObjectiveTarget = false,
            bool isProtectedTarget = false,
            bool isNeutralTarget = false)
        {
            if (self == null) return AITargetScore.Invalid("Missing self");
            if (candidate == null || candidate == self) return AITargetScore.Invalid("Missing target");
            if (!candidate.IsAlive) return AITargetScore.Invalid("Target dead");

            if (forcedTarget != null)
                return candidate == forcedTarget && forcedTarget.IsAlive
                    ? AITargetScore.Valid(candidate, 10000f, "Focus fire target")
                    : AITargetScore.Invalid("Not forced target");

            if (profile != null)
            {
                if (!profile.canInitiateCombat)
                    return AITargetScore.Invalid("Profile cannot initiate combat");
                if (!isHostile && !(profile.canAttackNeutral && isNeutralTarget))
                    return AITargetScore.Invalid("Target not hostile");
            }
            else if (!isHostile)
            {
                return AITargetScore.Invalid("Target not hostile");
            }

            var distance = Vector3.Distance(origin, candidate.transform.position);
            var effectiveRadius = radius > 0f ? radius : 12f;
            if (distance > effectiveRadius)
                return AITargetScore.Invalid("Target outside radius");

            var distanceScore = Mathf.Clamp01(1f - distance / Mathf.Max(0.1f, effectiveRadius)) * 5f;
            var healthScore = Mathf.Clamp01(1f - candidate.CurrentHealth / Mathf.Max(1f, candidate.Stats.maxHealth)) * 2f;
            var score = distanceScore + healthScore;
            var reason = "Hostile target";

            if (isHostile && (profile == null || profile.prefersEnemyUnits))
                score += 2f;

            if (profile != null)
            {
                score *= Mathf.Max(0.1f, profile.threatPriorityBias);

                if (profile.prefersObjectiveTargets && isObjectiveTarget)
                {
                    score += 8f;
                    reason = "Objective target";
                }

                if (profile.prefersProtectedTargets && (isProtectedTarget || protectedTarget != null && candidate.transform == protectedTarget))
                {
                    score += 10f;
                    reason = "Protected target";
                }

                if (profile.profileType == AIBehaviorProfileType.Hardliner && IsNegotiatorLike(candidate))
                {
                    score += 7f;
                    reason = "Negotiator target";
                }

                if (profile.canAttackNeutral && isNeutralTarget)
                {
                    score += 4f;
                    reason = "Escalation target";
                }

                if (profile.profileType == AIBehaviorProfileType.DefensiveGuard && isHostile)
                {
                    score += 3f;
                    reason = "Defensive threat";
                }
            }

            return AITargetScore.Valid(candidate, score, reason);
        }

        public static bool IsObjectiveLike(Combatant target)
        {
            if (target == null) return false;
            var entity = target.CharacterEntity;
            var name = target.name;
            var display = entity != null && entity.Data != null ? entity.Data.DisplayName : string.Empty;
            return ContainsObjectiveKeyword(name) || ContainsObjectiveKeyword(display);
        }

        public static bool IsNegotiatorLike(Combatant target)
        {
            if (target == null) return false;
            var entity = target.CharacterEntity;
            var name = target.name;
            var display = entity != null && entity.Data != null ? entity.Data.DisplayName : string.Empty;
            return ContainsIgnoreCase(name, "Negotiator") || ContainsIgnoreCase(display, "Negotiator");
        }

        public static bool ContainsObjectiveKeyword(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return ContainsIgnoreCase(value, "Objective")
                || ContainsIgnoreCase(value, "Core")
                || ContainsIgnoreCase(value, "Convoy")
                || ContainsIgnoreCase(value, "Energy")
                || ContainsIgnoreCase(value, "Protected")
                || ContainsIgnoreCase(value, "Negotiator");
        }

        private static bool ContainsIgnoreCase(string value, string token)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
