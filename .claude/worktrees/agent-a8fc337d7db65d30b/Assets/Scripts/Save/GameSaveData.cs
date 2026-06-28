using System;
using System.Collections.Generic;

namespace LuoLuoTrip.Save
{
    public static class SaveConstants
    {
        public const int CurrentSaveVersion = 2;
        public const string DefaultSaveFileName = "luoluotrip_save.json";
    }

    [Serializable]
    public class GameSaveData
    {
        public int version = SaveConstants.CurrentSaveVersion;
        public string savedAtUtc;
        public List<CharacterSaveEntry> characters = new List<CharacterSaveEntry>();
        public FactionRelationshipSnapshot relationships = new FactionRelationshipSnapshot();
        public PlayerSaveEntry player = new PlayerSaveEntry();
        public CommanderSaveEntry commander = new CommanderSaveEntry();
        public FactionPoliticsSnapshot factionPolitics = new FactionPoliticsSnapshot();
        public List<MissionConsequenceSaveEntry> completedMissions = new List<MissionConsequenceSaveEntry>();
        public MissionChainState missionChainState = new MissionChainState();
        public List<EncounterSnapshot> encounterSnapshots = new List<EncounterSnapshot>();
    }

    [Serializable]
    public class EncounterSnapshot
    {
        public string encounterId;
        public bool hasStarted;
        public bool hasCompleted;
        public string lastOutcome;
        public int defeatedUnitCount;
        public int totalSpawnedCount;
        public List<string> spawnedWaveIds = new List<string>();
        // Lifecycle hint: when an in-progress encounter is loaded, the runtime
        // marks NeedsRestartAfterLoad=true because dynamic unit HP/position is
        // not serialized in the prototype. Consumers may decide to ResetEncounter
        // and replay waves, or display a "restart suggested" hint.
        public bool needsRestartAfterLoad;
    }

    [Serializable]
    public class CharacterSaveEntry
    {
        public string id;
        public string displayName;
        public SubFactionId faction;
        public CharacterRole role;
        public int level;
        public bool isAlive;
        public float currentHealth = -1f;
        public float currentStamina = -1f;
        public float currentPoise = -1f;
        public int commandRank = 1;
        public int requiredCommanderLevel = 1;
        public int trustToPlayer;
        public bool isHeroOrLeader;
        public bool allowDirectControl = true;
        public bool allowTacticalCommand = true;
    }

    [Serializable]
    public class PlayerSaveEntry
    {
        public string controlledCharacterId;
        public float posX;
        public float posY;
        public float posZ;
    }

    [Serializable]
    public class CommanderSaveEntry
    {
        public int commanderLevel = 1;
        public int experience;
        public int commandCapacity = 2;
        public int maxDirectControlRank = 1;
        public int maxTacticalCommandRank = 2;
        public float baseSyncRate = 0.2f;
        public int mechaTrust;
        public int beastTrust;
        public int balanceScore;
    }

    [Serializable]
    public class MissionConsequenceSaveEntry
    {
        public string missionId;
        public MissionOutcomeType outcome;
        public int commanderExperienceDelta;
        public string summaryText;
    }
}
