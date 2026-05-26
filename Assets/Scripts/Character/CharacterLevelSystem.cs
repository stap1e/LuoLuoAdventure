using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    /// <summary>角色等级规则与升级逻辑</summary>
    public static class CharacterLevelSystem
    {
        public const int MaxLevel = 100;

        public static bool CanLevelUp(CharacterData character) =>
            character != null && character.IsAlive && character.Level < MaxLevel;

        public static void LevelUp(CharacterData character, int amount = 1)
        {
            if (character == null || amount <= 0) return;
            character.Level = Math.Min(MaxLevel, character.Level + amount);
        }

        public static void SetLevel(CharacterData character, int level)
        {
            if (character == null) return;
            character.Level = Math.Clamp(level, 1, MaxLevel);
        }

        /// <summary>根据角色身份返回推荐战力权重（后续战斗系统可扩展）</summary>
        public static float GetPowerWeight(CharacterRole role) => role switch
        {
            CharacterRole.WarKing => 3.0f,
            CharacterRole.CityLord => 2.0f,
            CharacterRole.Minion => 0.5f,
            _ => 1.0f
        };

        public static float EstimateCombatPower(CharacterData character)
        {
            if (character == null || !character.IsAlive) return 0f;
            return character.Level * GetPowerWeight(character.Role);
        }
    }
}
