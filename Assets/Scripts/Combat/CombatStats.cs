using System;
using System.Collections.Generic;

namespace LuoLuoTrip.Combat
{
    public enum CombatState
    {
        Idle,
        Attacking,
        Dodging,
        Staggered,
        Dead
    }

    /// <summary>战斗属性快照，由角色等级 + 阵营配置计算得出</summary>
    [Serializable]
    public struct CombatStats
    {
        public float maxHealth;
        public float maxStamina;
        public float maxPoise;
        public float attackPower;
        public float defense;
        public float attackRange;
        public float attackCooldown;
        public float poiseDamage;
        public float dodgeStaminaCost;
        public float poiseRecoveryPerSecond;
        public float staminaRecoveryPerSecond;

        public static CombatStats CreateDefault() => new CombatStats
        {
            maxHealth = 100f,
            maxStamina = 100f,
            maxPoise = 50f,
            attackPower = 10f,
            defense = 5f,
            attackRange = 2.5f,
            attackCooldown = 0.8f,
            poiseDamage = 15f,
            dodgeStaminaCost = 25f,
            poiseRecoveryPerSecond = 20f,
            staminaRecoveryPerSecond = 25f
        };
    }

    /// <summary>根据 CharacterData 与阵营加成计算战斗属性</summary>
    public static class CombatStatsCalculator
    {
        public static CombatStats Calculate(CharacterData character)
        {
            if (character == null) return CombatStats.CreateDefault();

            var mods = SubFactionRegistry.GetCombatModifiers(character.Faction);
            var roleWeight = CharacterLevelSystem.GetPowerWeight(character.Role);
            var level = Math.Max(1, character.Level);

            var baseHealth = 80f + level * 12f * roleWeight;
            var baseStamina = 70f + level * 3f;
            var basePoise = 30f + level * 4f * roleWeight;
            var baseAttack = 8f + level * 1.8f * roleWeight;
            var baseDefense = 4f + level * 0.6f * roleWeight;

            return new CombatStats
            {
                maxHealth = baseHealth * mods.healthMultiplier,
                maxStamina = baseStamina * mods.staminaMultiplier,
                maxPoise = basePoise * mods.poiseMultiplier,
                attackPower = baseAttack * mods.attackMultiplier,
                defense = baseDefense * mods.defenseMultiplier,
                attackRange = 2.2f + roleWeight * 0.3f,
                attackCooldown = Math.Max(0.35f, 1.1f - roleWeight * 0.15f),
                poiseDamage = 10f + level * 0.5f * mods.attackMultiplier,
                dodgeStaminaCost = 20f + level * 0.1f,
                poiseRecoveryPerSecond = 15f + level * 0.2f,
                staminaRecoveryPerSecond = 20f + level * 0.15f
            };
        }
    }
}
