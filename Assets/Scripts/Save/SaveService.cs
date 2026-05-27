using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LuoLuoTrip.Save
{
    /// <summary>JSON 存档读写，路径位于 Application.persistentDataPath</summary>
    public static class SaveService
    {
        public static string GetSavePath(string fileName = SaveConstants.DefaultSaveFileName) =>
            Path.Combine(Application.persistentDataPath, fileName);

        public static bool SaveExists(string fileName = SaveConstants.DefaultSaveFileName) =>
            File.Exists(GetSavePath(fileName));

        public static GameSaveData CreateFromContext(
            LuoLuoTripGameContext context,
            string playerCharacterId = null,
            Vector3? playerPosition = null,
            IReadOnlyDictionary<string, CharacterSaveEntry> combatStates = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var save = new GameSaveData
            {
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                relationships = context.RelationshipService.SaveSnapshot()
            };

            foreach (var character in context.AllCharacters)
            {
                var entry = new CharacterSaveEntry
                {
                    id = character.Id,
                    displayName = character.DisplayName,
                    faction = character.Faction,
                    role = character.Role,
                    level = character.Level,
                    isAlive = character.IsAlive
                };

                if (combatStates != null && combatStates.TryGetValue(character.Id, out var combat))
                {
                    entry.currentHealth = combat.currentHealth;
                    entry.currentStamina = combat.currentStamina;
                    entry.currentPoise = combat.currentPoise;
                }

                save.characters.Add(entry);
            }

            if (!string.IsNullOrEmpty(playerCharacterId))
            {
                save.player.controlledCharacterId = playerCharacterId;
                if (playerPosition.HasValue)
                {
                    var p = playerPosition.Value;
                    save.player.posX = p.x;
                    save.player.posY = p.y;
                    save.player.posZ = p.z;
                }
            }

            return save;
        }

        public static void Write(GameSaveData saveData, string fileName = SaveConstants.DefaultSaveFileName)
        {
            var path = GetSavePath(fileName);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonUtility.ToJson(saveData, prettyPrint: true);
            File.WriteAllText(path, json);
            Debug.Log($"[Save] 已写入: {path}");
        }

        public static GameSaveData Read(string fileName = SaveConstants.DefaultSaveFileName)
        {
            var path = GetSavePath(fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("存档不存在", path);

            var json = File.ReadAllText(path);
            var save = JsonUtility.FromJson<GameSaveData>(json);
            if (save == null)
                throw new InvalidDataException("存档解析失败");

            if (save.version > SaveConstants.CurrentSaveVersion)
                Debug.LogWarning($"[Save] 存档版本 {save.version} 高于当前支持版本 {SaveConstants.CurrentSaveVersion}");

            return save;
        }

        public static bool TryRead(out GameSaveData saveData, string fileName = SaveConstants.DefaultSaveFileName)
        {
            return TryRead(fileName, out saveData);
        }

        public static bool TryRead(string fileName, out GameSaveData saveData)
        {
            saveData = null;
            try
            {
                if (!SaveExists(fileName)) return false;
                saveData = Read(fileName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Save] 读取失败: {ex.Message}");
                return false;
            }
        }

        public static void Delete(string fileName = SaveConstants.DefaultSaveFileName)
        {
            var path = GetSavePath(fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[Save] 已删除: {path}");
            }
        }
    }
}
