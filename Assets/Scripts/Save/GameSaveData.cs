using System;
using System.Collections.Generic;

namespace LuoLuoTrip.Save
{
    public static class SaveConstants
    {
        public const int CurrentSaveVersion = 1;
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
    }

    [Serializable]
    public class PlayerSaveEntry
    {
        public string controlledCharacterId;
        public float posX;
        public float posY;
        public float posZ;
    }
}
