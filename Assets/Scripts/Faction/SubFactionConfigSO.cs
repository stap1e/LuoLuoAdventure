using System;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>阵营战斗加成系数，可在 Inspector 中按阵营微调</summary>
    [Serializable]
    public struct SubFactionCombatModifiers
    {
        [Min(0.1f)] public float healthMultiplier;
        [Min(0.1f)] public float attackMultiplier;
        [Min(0.1f)] public float defenseMultiplier;
        [Min(0.1f)] public float poiseMultiplier;
        [Min(0.1f)] public float staminaMultiplier;

        public static SubFactionCombatModifiers Default => new()
        {
            healthMultiplier = 1f,
            attackMultiplier = 1f,
            defenseMultiplier = 1f,
            poiseMultiplier = 1f,
            staminaMultiplier = 1f
        };
    }

    /// <summary>单个子阵营 ScriptableObject 配置</summary>
    [CreateAssetMenu(fileName = "SubFactionConfig", menuName = "LuoLuoTrip/Sub Faction Config")]
    public class SubFactionConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public SubFactionId factionId;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public Color themeColor = Color.white;

        [Header("Territory")]
        public string homeRegionName;

        [Header("Leader")]
        public string leaderTitleSuffix = "城主";

        [Header("Combat Modifiers")]
        public SubFactionCombatModifiers combatModifiers = SubFactionCombatModifiers.Default;

        public MainRace Race => GameConstants.GetMainRace(factionId);

        public SubFactionDefinition ToDefinition() =>
            new(factionId, displayName, description);

        public static SubFactionCombatModifiers GetDefaultCombatModifiers(SubFactionId id)
        {
            switch (id)
            {
                case SubFactionId.MotorIronRiders:
                    return new SubFactionCombatModifiers { healthMultiplier = 1.2f, attackMultiplier = 0.9f, defenseMultiplier = 1.3f, poiseMultiplier = 1.4f, staminaMultiplier = 0.85f };
                case SubFactionId.MotorStormGang:
                    return new SubFactionCombatModifiers { healthMultiplier = 0.9f, attackMultiplier = 1.15f, defenseMultiplier = 0.85f, poiseMultiplier = 0.8f, staminaMultiplier = 1.2f };
                case SubFactionId.MotorDustDevils:
                    return SubFactionCombatModifiers.Default;
                case SubFactionId.MotorNightRunners:
                    return new SubFactionCombatModifiers { healthMultiplier = 0.85f, attackMultiplier = 1.2f, defenseMultiplier = 0.75f, poiseMultiplier = 0.7f, staminaMultiplier = 1.25f };
                case SubFactionId.MotorSteelWolves:
                    return new SubFactionCombatModifiers { healthMultiplier = 1.05f, attackMultiplier = 1.05f, defenseMultiplier = 1.1f, poiseMultiplier = 1.1f, staminaMultiplier = 1f };
                case SubFactionId.MotorFlameCarts:
                    return new SubFactionCombatModifiers { healthMultiplier = 0.95f, attackMultiplier = 1.25f, defenseMultiplier = 0.9f, poiseMultiplier = 0.85f, staminaMultiplier = 0.95f };
                case SubFactionId.BeastIronClaw:
                    return new SubFactionCombatModifiers { healthMultiplier = 1.15f, attackMultiplier = 1.2f, defenseMultiplier = 1f, poiseMultiplier = 1.3f, staminaMultiplier = 0.9f };
                case SubFactionId.BeastShadowFang:
                    return new SubFactionCombatModifiers { healthMultiplier = 0.9f, attackMultiplier = 1.15f, defenseMultiplier = 0.8f, poiseMultiplier = 0.75f, staminaMultiplier = 1.15f };
                case SubFactionId.BeastThunderHide:
                    return new SubFactionCombatModifiers { healthMultiplier = 1.35f, attackMultiplier = 1.05f, defenseMultiplier = 1.15f, poiseMultiplier = 1.5f, staminaMultiplier = 0.8f };
                default:
                    return SubFactionCombatModifiers.Default;
            }
        }

        public void SetDefaultsFromId()
        {
            leaderTitleSuffix = GameConstants.IsBeastSubFaction(factionId) ? "战王" : "城主";
            combatModifiers = GetDefaultCombatModifiers(factionId);

            switch (factionId)
            {
                case SubFactionId.MotorIronRiders:
                    displayName = "铁骑团"; description = "机车族重装先锋"; themeColor = new Color(0.55f, 0.55f, 0.6f);
                    break;
                case SubFactionId.MotorStormGang:
                    displayName = "风暴帮"; description = "机车族高速突击"; themeColor = new Color(0.3f, 0.6f, 0.95f);
                    break;
                case SubFactionId.MotorDustDevils:
                    displayName = "尘魔帮"; description = "机车族沙漠游骑"; themeColor = new Color(0.85f, 0.65f, 0.35f);
                    break;
                case SubFactionId.MotorNightRunners:
                    displayName = "夜行者"; description = "机车族暗夜斥候"; themeColor = new Color(0.35f, 0.25f, 0.55f);
                    break;
                case SubFactionId.MotorSteelWolves:
                    displayName = "钢狼团"; description = "机车族机械狼骑"; themeColor = new Color(0.5f, 0.52f, 0.58f);
                    break;
                case SubFactionId.MotorFlameCarts:
                    displayName = "烈焰车帮"; description = "机车族火焰改装"; themeColor = new Color(0.95f, 0.4f, 0.15f);
                    break;
                case SubFactionId.BeastIronClaw:
                    displayName = "铁爪部"; description = "猛兽族利爪战团"; themeColor = new Color(0.7f, 0.25f, 0.2f);
                    break;
                case SubFactionId.BeastShadowFang:
                    displayName = "影牙部"; description = "猛兽族暗影猎手"; themeColor = new Color(0.25f, 0.3f, 0.35f);
                    break;
                case SubFactionId.BeastThunderHide:
                    displayName = "雷皮部"; description = "猛兽族雷霆巨兽"; themeColor = new Color(0.55f, 0.45f, 0.85f);
                    break;
            }
        }
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                SetDefaultsFromId();
        }
    }
}
