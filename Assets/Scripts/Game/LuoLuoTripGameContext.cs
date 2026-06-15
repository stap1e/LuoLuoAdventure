using System;
using System.Collections.Generic;
using System.Linq;
using LuoLuoTrip.Save;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>阵营状态：领袖、成员统计</summary>
    [Serializable]
    public class SubFactionState
    {
        public SubFactionId Id;
        public CharacterData Leader;
        public List<CharacterData> Members = new List<CharacterData>();

        public SubFactionDefinition Definition => SubFactionRegistry.Get(Id);

        public IEnumerable<CharacterData> AliveMembers => Members.Where(m => m.IsAlive);
    }

    /// <summary>游戏全局上下文，聚合阵营关系与世界角色</summary>
    public class LuoLuoTripGameContext
    {
        public FactionRelationshipService RelationshipService { get; }
        public FactionReputationService ReputationService { get; }
        public DynamicFactionHostilityService DynamicHostilityService { get; }
        public CommanderProfile CommanderProfile { get; }
        public MissionService MissionService { get; }
        public Dictionary<SubFactionId, SubFactionState> FactionStates { get; } = new Dictionary<SubFactionId, SubFactionState>();
        public List<CharacterData> AllCharacters { get; } = new List<CharacterData>();

        public LuoLuoTripGameContext(FactionRelationshipService relationshipService = null)
        {
            RelationshipService = relationshipService ?? new FactionRelationshipService();
            ReputationService = new FactionReputationService();
            DynamicHostilityService = new DynamicFactionHostilityService(ReputationService, RelationshipService);
            CommanderProfile = CommanderProfile.CreateDefault();
            MissionService = new MissionService(ReputationService, CommanderProfile);
        }

        public void InitializeWorld(bool spawnMinionSquads = true, int minionsPerFaction = 5)
        {
            FactionStates.Clear();
            AllCharacters.Clear();
            ReputationService.InitializeDefaultPolitics();

            foreach (var pair in SubFactionRegistry.All)
                FactionStates[pair.Key] = new SubFactionState { Id = pair.Key };

            var leaders = CharacterInitializer.CreateWorldLeaders();
            RegisterCharacters(leaders, assignAsLeader: true);

            if (spawnMinionSquads)
            {
                var minions = CharacterInitializer.CreateDefaultMinionSquads(minionsPerFaction);
                RegisterCharacters(minions, assignAsLeader: false);
            }
        }

        public void ApplySave(GameSaveData save)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));

            FactionStates.Clear();
            AllCharacters.Clear();

            foreach (var pair in SubFactionRegistry.All)
                FactionStates[pair.Key] = new SubFactionState { Id = pair.Key };

            foreach (var entry in save.characters)
            {
                var character = new CharacterData(
                    entry.id,
                    entry.displayName,
                    entry.faction,
                    entry.role,
                    entry.level)
                {
                    IsAlive = entry.isAlive,
                    CommandRank = entry.commandRank,
                    RequiredCommanderLevel = entry.requiredCommanderLevel,
                    TrustToPlayer = entry.trustToPlayer,
                    IsHeroOrLeader = entry.isHeroOrLeader,
                    AllowDirectControl = entry.allowDirectControl,
                    AllowTacticalCommand = entry.allowTacticalCommand
                };
                RegisterCharacter(character, IsLeaderRole(character.Role));
            }

            if (save.relationships?.Entries != null && save.relationships.Entries.Count > 0)
                RelationshipService.LoadSnapshot(save.relationships);

            if (save.factionPolitics?.Entries != null && save.factionPolitics.Entries.Count > 0)
                ReputationService.LoadSnapshot(save.factionPolitics);

            if (save.commander != null)
            {
                CommanderProfile.CommanderLevel = save.commander.commanderLevel;
                CommanderProfile.Experience = save.commander.experience;
                CommanderProfile.CommandCapacity = save.commander.commandCapacity;
                CommanderProfile.MaxDirectControlRank = save.commander.maxDirectControlRank;
                CommanderProfile.MaxTacticalCommandRank = save.commander.maxTacticalCommandRank;
                CommanderProfile.BaseSyncRate = save.commander.baseSyncRate;
                CommanderProfile.MechaTrust = save.commander.mechaTrust;
                CommanderProfile.BeastTrust = save.commander.beastTrust;
                CommanderProfile.BalanceScore = save.commander.balanceScore;
                CommanderProfile.Clamp();
            }
        }

        public GameSaveData ExportSave(
            string playerCharacterId = null,
            Vector3? playerPosition = null,
            IReadOnlyDictionary<string, CharacterSaveEntry> combatStates = null)
        {
            return SaveService.CreateFromContext(this, playerCharacterId, playerPosition, combatStates);
        }

        public void RegisterCharacter(CharacterData character, bool assignAsLeader = false)
        {
            RegisterCharacters(new[] { character }, assignAsLeader);
        }

        public void RegisterCharacters(IEnumerable<CharacterData> characters, bool assignAsLeader = false)
        {
            foreach (var character in characters)
            {
                AllCharacters.Add(character);

                if (!FactionStates.TryGetValue(character.Faction, out var state))
                {
                    state = new SubFactionState { Id = character.Faction };
                    FactionStates[character.Faction] = state;
                }

                state.Members.Add(character);

                if (assignAsLeader || IsLeaderRole(character.Role))
                    state.Leader = character;
            }
        }

        public CharacterData FindCharacter(string id) =>
            AllCharacters.FirstOrDefault(c => c.Id == id);

        public SubFactionState GetFactionState(SubFactionId id) => FactionStates[id];

        public CharacterData GetLeader(SubFactionId id) => FactionStates[id].Leader;

        public IEnumerable<CharacterData> GetCharactersByFaction(SubFactionId id) =>
            FactionStates[id].Members;

        public IEnumerable<CharacterData> GetCharactersByRace(MainRace race) =>
            AllCharacters.Where(c => c.Race == race);

        public bool AreFactionsHostile(SubFactionId a, SubFactionId b) =>
            RelationshipService.Matrix.IsHostile(a, b);

        private static bool IsLeaderRole(CharacterRole role) =>
            role is CharacterRole.WarKing or CharacterRole.CityLord;
    }
}
