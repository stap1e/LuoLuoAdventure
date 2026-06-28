using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    /// <summary>
    /// 世界初始角色生成器：
    /// - 猛兽族每阵营 1 名 100 级战王
    /// - 机车族每阵营 1 名 20 级城主
    /// - 小兵与普通角色默认 1 级
    /// </summary>
    public static class CharacterInitializer
    {
        public static List<CharacterData> CreateWorldLeaders()
        {
            var leaders = new List<CharacterData>();

            foreach (var def in SubFactionRegistry.GetByRace(MainRace.BeastTribe))
            {
                var title = SubFactionRegistry.GetLeaderTitle(def.Id);
                leaders.Add(CharacterData.Create(
                    id: $"war_king_{def.Id}",
                    displayName: $"{def.DisplayName}·{title}",
                    faction: def.Id,
                    role: CharacterRole.WarKing));
            }

            foreach (var def in SubFactionRegistry.GetByRace(MainRace.MotorTribe))
            {
                var title = SubFactionRegistry.GetLeaderTitle(def.Id);
                leaders.Add(CharacterData.Create(
                    id: $"city_lord_{def.Id}",
                    displayName: $"{def.DisplayName}·{title}",
                    faction: def.Id,
                    role: CharacterRole.CityLord));
            }

            return leaders;
        }

        public static CharacterData CreateMinion(string id, string displayName, SubFactionId faction) =>
            CharacterData.Create(id, displayName, faction, CharacterRole.Minion);

        public static CharacterData CreateCommonCharacter(string id, string displayName, SubFactionId faction) =>
            CharacterData.Create(id, displayName, faction, CharacterRole.Common);

        public static List<CharacterData> CreateDefaultMinionSquads(int minionsPerFaction = 5)
        {
            var minions = new List<CharacterData>();
            foreach (var pair in SubFactionRegistry.All)
            {
                var def = pair.Value;
                for (int i = 0; i < minionsPerFaction; i++)
                {
                    minions.Add(CreateMinion(
                        $"minion_{def.Id}_{i}",
                        $"{def.DisplayName}·小兵{i + 1}",
                        def.Id));
                }
            }
            return minions;
        }
    }
}
