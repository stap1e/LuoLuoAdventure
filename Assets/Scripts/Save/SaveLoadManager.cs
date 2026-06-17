using System.Collections.Generic;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Save;
using UnityEngine;

namespace LuoLuoTrip.Save
{
    public class SaveLoadManager : MonoBehaviour
    {
        [SerializeField] private bool _loadOnStart = true;
        [SerializeField] private bool _autoSaveOnQuit = true;
        [SerializeField] private KeyCode _quickSaveKey = KeyCode.F5;
        [SerializeField] private KeyCode _quickLoadKey = KeyCode.F9;
        [SerializeField] private KeyCode _clearSaveKey = KeyCode.F10;
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

            if (Input.GetKeyDown(_clearSaveKey))
                ClearSave();
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
                Debug.LogWarning("[Save] GameContext not initialized, skip save");
                return;
            }

            var combatStates = CollectCombatStates();
            var position = FindPlayerPosition();
            var save = context.ExportSave(_playerCharacterId, position, combatStates);
            CollectEncounterSnapshots(save);
            SaveService.Write(save, _saveFileName);

            if (!silent)
            {
                var chainState = context.MissionChainService?.State;
                var lastOutcome = chainState != null && chainState.CompletedMissions.Count > 0
                    ? chainState.CompletedMissions[chainState.CompletedMissions.Count - 1].Outcome.ToString()
                    : "none";
                var activeMission = chainState?.ActiveMissionId ?? "none";
                var controlled = context.CommanderProfile != null ? "self" : "none";
                Debug.Log($"[Save] Quick save complete (F5) | Commander Lv.{context.CommanderProfile.CommanderLevel} XP:{context.CommanderProfile.Experience} | Controlled:{controlled} | Factions:{context.ReputationService.StandingsCount} | Chain completed:{chainState?.CompletedMissions.Count ?? 0} | Last outcome:{lastOutcome} | Active mission:{activeMission} | Characters:{save.characters.Count} | Encounters:{save.encounterSnapshots.Count}");
            }
        }

        public void LoadGame()
        {
            if (!SaveService.TryRead(_saveFileName, out var save))
            {
                Debug.LogWarning("[Save] No save file found");
                return;
            }

            if (GameBootstrap.Context == null)
            {
                Debug.LogWarning("[Save] GameContext not initialized, cannot load");
                return;
            }

            var context = GameBootstrap.Context;
            context.ApplySave(save);
            RestoreCombatStateFromSave(save);
            RestorePlayerPosition(save);
            RestoreEncounterSnapshots(save);

            var commanderOk = context.CommanderProfile != null;
            var factionOk = context.ReputationService != null;
            var missionOk = context.MissionService != null;
            var chainOk = context.MissionChainService != null;

            var chainState = context.MissionChainService?.State;
            var chainCompleted = chainState?.CompletedMissions.Count ?? 0;
            var chainUnlocked = chainState?.UnlockedMissionIds.Count ?? 0;

            Debug.Log($"[Save] Load complete | Commander:{(commanderOk ? $"Lv.{context.CommanderProfile.CommanderLevel} XP:{context.CommanderProfile.Experience}" : "MISSING")} | Factions:{(factionOk ? $"OK({context.ReputationService.StandingsCount})" : "MISSING")} | Mission:{(missionOk ? "OK" : "MISSING")} | Chain:{(chainOk ? $"completed:{chainCompleted} unlocked:{chainUnlocked}" : "MISSING")} | Encounters:{save.encounterSnapshots?.Count ?? 0} | Version:{save.version} | Time:{save.savedAtUtc}");

            if (save.missionChainState != null)
                Debug.Log($"[Save] MissionChainState restored: {save.missionChainState.CompletedMissions.Count} entries, {save.missionChainState.UnlockedMissionIds.Count} unlocked");
            else
                Debug.LogWarning("[Save] missionChainState is null in save data");

            if (save.version < 2)
                Debug.LogWarning($"[Save] Old save version ({save.version}), new fields use defaults");
        }

        public void ClearSave()
        {
            SaveService.Delete(_saveFileName);
            ClearAllEncounters();
            Debug.Log("[Save] Save file cleared (F10). Encounter snapshots cleared. Restart scene to fully reset runtime objects.");
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

            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.AllCharacters;
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].Data?.Id == _playerCharacterId)
                        return all[i].transform.position;
                }
            }

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

            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.AllCharacters;
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].Data?.Id == save.player.controlledCharacterId)
                    {
                        all[i].transform.position = new Vector3(save.player.posX, save.player.posY, save.player.posZ);
                        return;
                    }
                }
            }

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

            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.AllCharacters;
                for (int i = 0; i < all.Count; i++)
                {
                    var combatant = all[i].Combatant;
                    if (combatant == null || all[i].Data == null) continue;
                    var id = all[i].Data.Id;
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

            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.AllCharacters;
                for (int i = 0; i < all.Count; i++)
                {
                    var combatant = all[i].Combatant;
                    var id = all[i].Data?.Id;
                    if (id != null && lookup.TryGetValue(id, out var entry) && combatant != null)
                        combatant.RestoreRuntimeState(entry.currentHealth, entry.currentStamina, entry.currentPoise);
                }
                return;
            }

            foreach (var combatant in FindObjectsOfType<Combatant>())
            {
                var id = combatant.CharacterEntity?.Data?.Id;
                if (id != null && lookup.TryGetValue(id, out var entry))
                    combatant.RestoreRuntimeState(entry.currentHealth, entry.currentStamina, entry.currentPoise);
            }
        }

        private static void CollectEncounterSnapshots(GameSaveData save)
        {
            save.encounterSnapshots.Clear();
            foreach (var encounter in FindObjectsOfType<EncounterRuntime>())
            {
                if (encounter == null) continue;
                save.encounterSnapshots.Add(encounter.GetSnapshot());
            }
        }

        private static void RestoreEncounterSnapshots(GameSaveData save)
        {
            if (save.encounterSnapshots == null || save.encounterSnapshots.Count == 0) return;

            // Prototype limitation reminder: dynamic spawned units are NOT
            // serialized with full HP/position state. We only restore lifecycle
            // state (started / completed / spawnedWaveIds). In-progress
            // encounters are flagged NeedsRestartAfterLoad on the runtime.
            Debug.LogWarning("[EncounterRuntime] Dynamic units are not fully serialized; restoring lifecycle state only.");

            var encounters = FindObjectsOfType<EncounterRuntime>();
            int restored = 0;
            int cleared = 0;
            int needsRestart = 0;
            foreach (var encounter in encounters)
            {
                if (encounter == null) continue;
                var encounterId = encounter.Definition?.encounterId ?? encounter.gameObject.name;
                EncounterSnapshot match = null;
                for (int i = 0; i < save.encounterSnapshots.Count; i++)
                {
                    if (save.encounterSnapshots[i] != null && save.encounterSnapshots[i].encounterId == encounterId)
                    {
                        match = save.encounterSnapshots[i];
                        break;
                    }
                }
                if (match == null)
                {
                    encounter.ResetEncounter();
                    cleared++;
                    continue;
                }
                encounter.RestoreSnapshot(match);
                restored++;
                if (encounter.NeedsRestartAfterLoad) needsRestart++;
            }

            Debug.Log($"[Save] Encounter snapshots restored: {restored}, reset: {cleared}, needsRestart: {needsRestart}");
        }

        private static void ClearAllEncounters()
        {
            foreach (var encounter in FindObjectsOfType<EncounterRuntime>())
            {
                if (encounter == null) continue;
                encounter.ResetEncounter();
            }
            CharacterRuntimeComponentGuard.ResetWarnings();
        }
    }
}
