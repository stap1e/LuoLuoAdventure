using System.Collections.Generic;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Save;
using UnityEngine;

namespace LuoLuoTrip.Save
{
    /// <summary>挂载到 GameBootstrap 同级，负责自动存档与快捷键存读</summary>
    public class SaveLoadManager : MonoBehaviour
    {
        [SerializeField] private bool _loadOnStart = true;
        [SerializeField] private bool _autoSaveOnQuit = true;
        [SerializeField] private KeyCode _quickSaveKey = KeyCode.F5;
        [SerializeField] private KeyCode _quickLoadKey = KeyCode.F9;
        [SerializeField] private string _saveFileName = SaveConstants.DefaultSaveFileName;

        private string _playerCharacterId;

        public void SetPlayerCharacterId(string id) => _playerCharacterId = id;

        private void Start()
        {
            if (_loadOnStart && SaveService.SaveExists(_saveFileName))
                LoadGame();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_quickSaveKey))
                SaveGame();

            if (Input.GetKeyDown(_quickLoadKey))
                LoadGame();
        }

        private void OnApplicationQuit()
        {
            if (_autoSaveOnQuit)
                SaveGame(silent: true);
        }

        public void SaveGame(bool silent = false)
        {
            var context = GameBootstrap.Context;
            if (context == null)
            {
                Debug.LogWarning("[Save] GameContext 未初始化，跳过存档");
                return;
            }

            var combatStates = CollectCombatStates();
            var position = FindPlayerPosition();
            var save = context.ExportSave(_playerCharacterId, position, combatStates);
            SaveService.Write(save, _saveFileName);

            if (!silent)
                Debug.Log("[Save] 快速存档完成 (F5)");
        }

        public void LoadGame()
        {
            if (!SaveService.TryRead(_saveFileName, out var save))
                return;

            if (GameBootstrap.Context == null)
            {
                Debug.LogWarning("[Save] GameContext 未初始化，无法读档");
                return;
            }

            GameBootstrap.Context.ApplySave(save);
            RestoreCombatStateFromSave(save);
            RestorePlayerPosition(save);

            Debug.Log($"[Save] 读档完成，存档时间: {save.savedAtUtc}");
        }

        public void NewGame()
        {
            SaveService.Delete(_saveFileName);
            if (GameBootstrap.Context != null)
                GameBootstrap.Context.InitializeWorld();
        }

        private Vector3? FindPlayerPosition()
        {
            if (string.IsNullOrEmpty(_playerCharacterId)) return null;

            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (entity.Data?.Id == _playerCharacterId)
                    return entity.transform.position;
            }
            return null;
        }

        private void RestorePlayerPosition(GameSaveData save)
        {
            if (string.IsNullOrEmpty(save.player.controlledCharacterId)) return;

            _playerCharacterId = save.player.controlledCharacterId;
            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (entity.Data?.Id != save.player.controlledCharacterId) continue;
                entity.transform.position = new Vector3(save.player.posX, save.player.posY, save.player.posZ);
                break;
            }
        }

        private static Dictionary<string, CharacterSaveEntry> CollectCombatStates()
        {
            var lookup = new Dictionary<string, CharacterSaveEntry>();
            foreach (var combatant in FindObjectsOfType<Combatant>())
            {
                if (combatant.CharacterEntity?.Data == null) continue;
                var id = combatant.CharacterEntity.Data.Id;
                lookup[id] = new CharacterSaveEntry
                {
                    id = id,
                    currentHealth = combatant.CurrentHealth,
                    currentStamina = combatant.CurrentStamina,
                    currentPoise = combatant.CurrentPoise
                };
            }
            return lookup;
        }

        private static void RestoreCombatStateFromSave(GameSaveData save)
        {
            var lookup = new Dictionary<string, CharacterSaveEntry>();
            foreach (var entry in save.characters)
            {
                if (entry.currentHealth >= 0f)
                    lookup[entry.id] = entry;
            }

            foreach (var combatant in FindObjectsOfType<Combatant>())
            {
                var id = combatant.CharacterEntity?.Data?.Id;
                if (id != null && lookup.TryGetValue(id, out var entry))
                    combatant.RestoreRuntimeState(entry.currentHealth, entry.currentStamina, entry.currentPoise);
            }
        }
    }
}
