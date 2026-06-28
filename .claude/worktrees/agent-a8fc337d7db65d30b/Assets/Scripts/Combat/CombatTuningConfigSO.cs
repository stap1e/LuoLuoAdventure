using System;
using UnityEngine;

namespace LuoLuoTrip.Combat
{
    [CreateAssetMenu(fileName = "CombatTuningConfig", menuName = "LuoLuoTrip/Combat Tuning Config")]
    public class CombatTuningConfigSO : ScriptableObject
    {
        [Header("Player Attack Timing")]
        public float playerAttackWindup = 0.25f;
        public float playerAttackActive = 0.2f;
        public float playerAttackRecovery = 0.3f;

        [Header("Enemy Attack Timing")]
        public float enemyAttackWindup = 0.35f;
        public float enemyAttackActiveDuration = 0.2f;
        public float enemyAttackRecovery = 0.4f;

        [Header("Stat Overrides (0 = use calculated)")]
        public float playerMaxHp = 0f;
        public float enemyMaxHp = 0f;
        public float playerAttackDamage = 0f;
        public float enemyAttackDamage = 0f;
        public float playerAttackRange = 0f;
        public float enemyAttackRange = 0f;

        [Header("Dodge")]
        public float dodgeDuration = 0.35f;
        public float dodgeDistance = 4f;
        public float dodgeInvulnerableDuration = 0.3f;

        [Header("Stagger / Poise")]
        public float staggerDuration = 1.2f;
        public float staggerDamageThreshold = 0f;
        public float poiseBreakThreshold = 0f;

        [Header("AI")]
        public float aiAttackWindupDelay = 0.4f;
        public float aiChaseSpeed = 4f;
        public float aiAttackCooldown = 1.2f;
        public float aiStopDistance = 1.5f;

        [Header("Feedback Durations")]
        public float hitFlashDuration = 0.12f;
        public float damageNumberDuration = 1.0f;

        [Header("Sync Assist")]
        public float syncAssistDuration = 3f;
        public float syncAssistAttackBonus = 0.25f;
        public float syncAssistDefenseBonus = 0.15f;

        private static CombatTuningConfigSO _cachedDefault;

        public static CombatTuningConfigSO Default
        {
            get
            {
                if (_cachedDefault != null) return _cachedDefault;
                _cachedDefault = CreateInstance<CombatTuningConfigSO>();
                return _cachedDefault;
            }
        }

        public static CombatTuningConfigSO LoadOrDefault()
        {
            var config = Resources.Load<CombatTuningConfigSO>("CombatTuningConfig");
            if (config != null) return config;

            Debug.LogWarning("[CombatTuning] CombatTuningConfig not found in Resources, using defaults");
            return Default;
        }

        public void ApplyTo(Combatant combatant)
        {
            if (combatant == null) return;
            combatant.ApplyTuning(this);
        }

        public void ApplyTo(SimpleCombatAI ai)
        {
            if (ai == null) return;
            ai.ApplyTuning(this);
        }

        public bool Validate(out string firstError)
        {
            firstError = null;
            if (playerAttackWindup <= 0f) { firstError = "playerAttackWindup must be > 0"; return false; }
            if (playerAttackActive <= 0f) { firstError = "playerAttackActive must be > 0"; return false; }
            if (playerAttackRecovery <= 0f) { firstError = "playerAttackRecovery must be > 0"; return false; }
            if (enemyAttackWindup <= 0f) { firstError = "enemyAttackWindup must be > 0"; return false; }
            if (enemyAttackActiveDuration <= 0f) { firstError = "enemyAttackActiveDuration must be > 0"; return false; }
            if (enemyAttackRecovery <= 0f) { firstError = "enemyAttackRecovery must be > 0"; return false; }
            if (dodgeDuration <= 0f) { firstError = "dodgeDuration must be > 0"; return false; }
            if (dodgeDistance <= 0f) { firstError = "dodgeDistance must be > 0"; return false; }
            if (staggerDuration <= 0f) { firstError = "staggerDuration must be > 0"; return false; }
            if (aiChaseSpeed <= 0f) { firstError = "aiChaseSpeed must be > 0"; return false; }
            if (aiAttackCooldown <= 0f) { firstError = "aiAttackCooldown must be > 0"; return false; }
            if (hitFlashDuration <= 0f) { firstError = "hitFlashDuration must be > 0"; return false; }
            if (damageNumberDuration <= 0f) { firstError = "damageNumberDuration must be > 0"; return false; }
            return true;
        }
    }
}
