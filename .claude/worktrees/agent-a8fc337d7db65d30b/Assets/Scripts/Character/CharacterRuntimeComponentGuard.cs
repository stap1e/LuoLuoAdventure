using System.Collections.Generic;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>
    /// Static helper that ensures every gameplay character has the runtime components
    /// required for root movement, hit detection, and animator safety.
    ///
    /// Use after Instantiate / dynamic spawn to backfill missing pieces. The guard
    /// logs a single warning per missing component type per session so test failures
    /// surface clearly without spamming the console.
    /// </summary>
    public static class CharacterRuntimeComponentGuard
    {
        private static readonly HashSet<string> _warnedKeys = new HashSet<string>();

        public struct GuardResult
        {
            public bool MotorAdded;
            public bool RigidbodyAdded;
            public bool ColliderAdded;
            public bool AnimatorRootMotionDisabled;
            public bool VisualMissing;
            public bool HealthBarAdded;
            public bool HitFlashAdded;
            public bool NavBridgeAdded;
        }

        public static void ResetWarnings()
        {
            _warnedKeys.Clear();
        }

        public static GuardResult Ensure(GameObject go, bool isPlayer = false)
        {
            var result = default(GuardResult);
            if (go == null) return result;

            // 1. CharacterMovementMotor on root (root-only X/Z movement contract)
            if (go.GetComponent<CharacterMovementMotor>() == null)
            {
                go.AddComponent<CharacterMovementMotor>();
                result.MotorAdded = true;
                WarnOnce(go, "missing CharacterMovementMotor", "motor");
            }

            // 2. Rigidbody on root (kinematic, no gravity, freeze rotation only)
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                result.RigidbodyAdded = true;
                WarnOnce(go, "missing Rigidbody (added kinematic, no gravity, freeze rotation)", "rb");
            }
            else
            {
                if ((rb.constraints & (RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ)) != 0)
                    WarnOnce(go, "Rigidbody freezes X/Z position — root cannot move", "rb_xz_frozen");
                if (rb.useGravity && !rb.isKinematic)
                    WarnOnce(go, "Rigidbody non-kinematic with gravity — prototype characters may sink", "rb_gravity");
            }

            // 3. Collider somewhere on the prefab (root or Collision child)
            var anyCollider = go.GetComponentInChildren<Collider>();
            if (anyCollider == null)
            {
                var capsule = go.AddComponent<CapsuleCollider>();
                capsule.height = 2f;
                capsule.radius = 0.5f;
                capsule.center = new Vector3(0f, 1f, 0f);
                result.ColliderAdded = true;
                WarnOnce(go, "missing Collider (added fallback CapsuleCollider on root)", "collider");
            }

            // 4. Animator must not drive root motion
            var animator = go.GetComponent<Animator>();
            if (animator != null && animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
                result.AnimatorRootMotionDisabled = true;
                WarnOnce(go, "Animator.applyRootMotion=true — disabled to preserve gameplay root movement", "anim_rootmotion");
            }

            // 5. Visual child should exist for ProceduralCombatAnimator and animator clip rebinding.
            //    We do NOT auto-create one (prefabs already have it) but log if absent.
            if (go.transform.Find("Visual") == null)
            {
                result.VisualMissing = true;
                WarnOnce(go, "no 'Visual' child — animation clips bound to 'Visual' path will not affect this prefab", "visual_missing");
            }

            return result;
        }

        public static GuardResult EnsureForAI(GameObject go)
        {
            var result = Ensure(go, isPlayer: false);

            if (go.GetComponent<NavigationAgentBridge>() == null)
            {
                go.AddComponent<NavigationAgentBridge>();
                result.NavBridgeAdded = true;
                WarnOnce(go, "missing NavigationAgentBridge for AI unit", "nav_bridge");
            }

            // Combat readability: AI units get health bar + hit flash if missing.
            if (go.GetComponent<CombatantHealthBarPresenter>() == null)
            {
                go.AddComponent<CombatantHealthBarPresenter>();
                result.HealthBarAdded = true;
                WarnOnce(go, "missing CombatantHealthBarPresenter for AI unit", "healthbar");
            }

            if (go.GetComponent<HitFlashFeedback>() == null)
            {
                go.AddComponent<HitFlashFeedback>();
                result.HitFlashAdded = true;
                WarnOnce(go, "missing HitFlashFeedback for AI unit", "hitflash");
            }

            return result;
        }

        public static void EnsureForPlayer(GameObject go)
        {
            Ensure(go, isPlayer: true);
        }

        private static void WarnOnce(GameObject go, string message, string keySuffix)
        {
            var key = $"{(go != null ? go.name : "<null>")}::{keySuffix}";
            if (_warnedKeys.Contains(key)) return;
            _warnedKeys.Add(key);
            Debug.LogWarning($"[CharacterRuntimeComponentGuard] '{(go != null ? go.name : "<null>")}' {message}");
        }
    }
}
