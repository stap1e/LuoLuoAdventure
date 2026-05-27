using System.Collections.Generic;

namespace LuoLuoTrip
{
    /// <summary>
    /// 子阵营注册表。优先读取 SubFactionDatabaseSO，无配置时回退到内置默认值。
    /// </summary>
    public static class SubFactionRegistry
    {
        private static SubFactionDatabaseSO _database;
        private static readonly Dictionary<SubFactionId, SubFactionDefinition> FallbackDefinitions = BuildFallback();
        private static readonly Dictionary<SubFactionId, SubFactionCombatModifiers> FallbackCombat = BuildFallbackCombat();

        public static SubFactionDatabaseSO Database => _database;

        public static void Initialize(SubFactionDatabaseSO database)
        {
            _database = database;
        }

        public static IReadOnlyDictionary<SubFactionId, SubFactionDefinition> All
        {
            get
            {
                if (_database != null && _database.Configs.Count > 0)
                {
                    var dict = new Dictionary<SubFactionId, SubFactionDefinition>();
                    foreach (var config in _database.Configs)
                    {
                        if (config != null)
                            dict[config.factionId] = config.ToDefinition();
                    }
                    return dict;
                }
                return FallbackDefinitions;
            }
        }

        public static SubFactionDefinition Get(SubFactionId id)
        {
            if (_database != null && _database.TryGet(id, out var config))
                return config.ToDefinition();
            return FallbackDefinitions[id];
        }

        public static SubFactionConfigSO GetConfig(SubFactionId id)
        {
            if (_database != null)
                return _database.Get(id);
            return null;
        }

        public static SubFactionCombatModifiers GetCombatModifiers(SubFactionId id)
        {
            if (_database != null && _database.TryGet(id, out var config))
                return config.combatModifiers;
            return FallbackCombat.TryGetValue(id, out var mods) ? mods : SubFactionCombatModifiers.Default;
        }

        public static string GetLeaderTitle(SubFactionId id)
        {
            if (_database != null && _database.TryGet(id, out var config) && !string.IsNullOrEmpty(config.leaderTitleSuffix))
                return config.leaderTitleSuffix;
            return GameConstants.IsBeastSubFaction(id) ? "战王" : "城主";
        }

        public static IEnumerable<SubFactionDefinition> GetByRace(MainRace race)
        {
            foreach (var pair in All)
            {
                if (pair.Value.Race == race)
                    yield return pair.Value;
            }
        }

        private static Dictionary<SubFactionId, SubFactionDefinition> BuildFallback()
        {
            return new Dictionary<SubFactionId, SubFactionDefinition>
            {
                { SubFactionId.MotorIronRiders, new SubFactionDefinition(SubFactionId.MotorIronRiders, "铁骑团", "机车族重装先锋") },
                { SubFactionId.MotorStormGang, new SubFactionDefinition(SubFactionId.MotorStormGang, "风暴帮", "机车族高速突击") },
                { SubFactionId.MotorDustDevils, new SubFactionDefinition(SubFactionId.MotorDustDevils, "尘魔帮", "机车族沙漠游骑") },
                { SubFactionId.MotorNightRunners, new SubFactionDefinition(SubFactionId.MotorNightRunners, "夜行者", "机车族暗夜斥候") },
                { SubFactionId.MotorSteelWolves, new SubFactionDefinition(SubFactionId.MotorSteelWolves, "钢狼团", "机车族机械狼骑") },
                { SubFactionId.MotorFlameCarts, new SubFactionDefinition(SubFactionId.MotorFlameCarts, "烈焰车帮", "机车族火焰改装") },
                { SubFactionId.BeastIronClaw, new SubFactionDefinition(SubFactionId.BeastIronClaw, "铁爪部", "猛兽族利爪战团") },
                { SubFactionId.BeastShadowFang, new SubFactionDefinition(SubFactionId.BeastShadowFang, "影牙部", "猛兽族暗影猎手") },
                { SubFactionId.BeastThunderHide, new SubFactionDefinition(SubFactionId.BeastThunderHide, "雷皮部", "猛兽族雷霆巨兽") }
            };
        }

        private static Dictionary<SubFactionId, SubFactionCombatModifiers> BuildFallbackCombat()
        {
            var result = new Dictionary<SubFactionId, SubFactionCombatModifiers>();
            foreach (SubFactionId id in System.Enum.GetValues(typeof(SubFactionId)))
                result[id] = SubFactionConfigSO.GetDefaultCombatModifiers(id);
            return result;
        }
    }
}
