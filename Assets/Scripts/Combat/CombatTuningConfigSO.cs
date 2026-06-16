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

        [Header("Dodge")]
        public float dodgeDuration = 0.35f;
        public float dodgeDistance = 4f;
        public float dodgeInvulnerableDuration = 0.3f;

        [Header("Stagger")]
        public float staggerDuration = 1.2f;

        [Header("AI")]
        public float aiAttackWindupDelay = 0.4f;

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
    }
}
