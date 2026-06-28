# Encounter Persistence Design

Date: 2026-06-17 (Phase 4 — Encounter Persistence & Mission Lifecycle Hardening)
Unity: 2022.3.62f3 LTS

## Goal

Allow the player to F5-save mid-mission and F9-load without:
- Duplicate enemy spawns
- Null reference exceptions
- Completed encounters re-firing as if never finished
- Mission outcomes being recorded twice

…while keeping the prototype save format **lightweight** — we do NOT serialize per-unit HP, position, AI state, animator state, etc.

## What is saved

`Save/GameSaveData.cs` carries a `List<EncounterSnapshot> encounterSnapshots`. Each snapshot is a thin lifecycle record:

| Field | Purpose |
|---|---|
| `encounterId` | Stable identifier from `EncounterDefinition.encounterId` (or GameObject name as fallback). |
| `hasStarted` | Did `EncounterRuntime.StartEncounter()` fire? |
| `hasCompleted` | Did `EncounterRuntime.CompleteEncounter(outcome)` fire? |
| `lastOutcome` | Outcome string passed to `CompleteEncounter` (e.g. `"MechaVictory"`). |
| `defeatedUnitCount` | Number of dynamic spawned units that were defeated when snapshotted. |
| `totalSpawnedCount` | Total dynamic units spawned this encounter run. |
| `spawnedWaveIds` | Set of wave ids that already spawned (used by the duplicate-wave guard). |
| `needsRestartAfterLoad` | True when an in-progress encounter is restored — see "Restore strategy" below. |

## What is NOT saved

The prototype intentionally does NOT serialize:

- Dynamic spawned-unit `Combatant` HP / stamina / poise
- Dynamic spawned-unit transform position / rotation
- Active AI target / behavior state
- Procedural animator state
- NavMeshAgent path / progress
- Active Mission timers (defense timer, abandon timer)

Reason:
- Dynamic units are created via `EncounterSpawnPoint.SpawnUnit` from `CharacterData.Create` and are not persisted across sessions in any other way.
- The asset pipeline (placeholder prefabs, no real models yet) means re-spawning is cheap.
- Writing a GUID-stable per-unit save would require unit identity, animator clip handles, and AI snapshotting that the prototype does not need.
- Save-file size and risk-of-stale-data are reduced by keeping only lifecycle state.

When the runtime restores any snapshots, it logs once:

```
[EncounterRuntime] Dynamic units are not fully serialized; restoring lifecycle state only.
```

## Restore strategy

`SaveLoadManager.RestoreEncounterSnapshots()` matches each `EncounterRuntime` in the scene to a snapshot by `encounterId`, then calls `EncounterRuntime.RestoreSnapshot()`.

Three lifecycle outcomes:

| Saved state | Restore behavior | Runtime flag |
|---|---|---|
| **completed** (`hasCompleted == true`) | Snapshot reapplied. `StartEncounter()` becomes a no-op via the `hasCompleted` guard. No respawn. | `HasCompleted=true` |
| **in-progress** (`hasStarted=true, hasCompleted=false`) | Snapshot reapplied. The runtime sets `NeedsRestartAfterLoad=true`. Mission scripts may decide to: (a) call `ResetEncounter()` and replay the wave plan, or (b) show the player a "restart suggested" hint. | `NeedsRestartAfterLoad=true` |
| **not-started** | No-op — the runtime stays inactive until the trigger zone fires `StartEncounter()` for the first time. | `HasStarted=false` |

When a scene encounter has no matching snapshot in the save file (newly added encounter, save predates encounter), the manager calls `ResetEncounter()` so the encounter starts from a clean slate.

## Idempotency guarantees

| Action | Guard |
|---|---|
| `EncounterRuntime.SpawnWave(wave)` | `_spawnedWaveIds` HashSet skip + `wave.spawned = true`. Logs `[EncounterRuntime] Skip duplicate wave 'X'`. |
| `EncounterRuntime.StartEncounter()` | No-op when `_hasCompleted` or `_hasStarted`. |
| `EncounterRuntime.CompleteEncounter()` | No-op when `_hasCompleted`. |
| `MissionTriggerZone.ForceStart()` | No-op when `_missionCompleted`. |
| `BorderRetaliationRuntime.ConfigureDynamicWaves()` | `_wavesConfigured` flag prevents stacking. |
| `MissionChainService.RecordMissionResult()` | `_state.HasCompleted(missionId)` block; pass `allowDuplicate=true` for explicit debug reset. |
| `SaveLoadManager.ClearSave()` (F10) | Calls `ResetEncounter()` on all encounters in scene, deletes the save file. |

## Debug triggers (1 / 2 / 3)

`CommanderPrototypeRuntime` retains the prototype Alpha1/Alpha2/Alpha3 keys. Their logs are tagged `[DEBUG TRIGGER]` and the consequence flow tags mission ids with the `test_` prefix. Inside `RecordAndShow`, `mission.StartsWith("test_")` skips `MissionChainService.RecordMissionResult` so the debug keys never pollute MissionChainState.

## Related logs (grep-friendly tags)

```
[EncounterRuntime] StartEncounter '<id>'
[EncounterRuntime] CompleteEncounter '<id>' outcome=<outcome>
[EncounterRuntime] ResetEncounter '<id>'
[EncounterRuntime] ClearSpawnedUnits '<id>' destroyed=N (manual scene units preserved)
[EncounterRuntime] Skip duplicate wave 'X' (already spawned)
[EncounterRuntime] Spawned wave 'X': N <faction> units (behavior=...)
[EncounterRuntime] RestoreSnapshot '<id>' state=<lifecycle> waveIds=N totalSpawned=N
[EncounterRuntime] Dynamic units are not fully serialized; restoring lifecycle state only.
[Save] Encounter snapshots restored: A, reset: B, needsRestart: C
[MissionChain] Skip duplicate mission outcome '<id>' ...
[BorderRetaliation] ConfigureDynamicWaves skipped (already configured for modifier '...')
[BorderRetaliation] Configured N dynamic waves for modifier '...'
[DEBUG TRIGGER] Key 1 / 2 / 3: ...
```

## Known limitations

- Dynamic unit HP / position / AI target / animator state is NOT serialized. Reload restores lifecycle only.
- An in-progress encounter at save time is flagged `NeedsRestartAfterLoad=true`. Mission flow chooses how to react.
- Save format version is `2`. Older saves missing `encounterSnapshots` load with empty list and proceed normally (`SaveLoadManager.RestoreEncounterSnapshots` no-ops on empty).
- Manually placed scene units (registered via `RegisterUnit`) are NOT destroyed by `ClearSpawnedUnits` — only dynamic spawned units are cleared.

## Verification

- 17 EditMode tests cover `EncounterRuntime` lifecycle, idempotency, snapshot, mission trigger guards, and save-load round-trip.
- 3 PlayMode smoke tests cover persistence round-trip, duplicate wave guard, and mission trigger idempotency.
- `VerticalSliceValidator.CheckEncounterPersistence` verifies type/method/field presence + log strings + `MissionChainService.RecordMissionResult.allowDuplicate` parameter + this design doc presence.

See `Assets/Docs/FULL_TEST_BASELINE_REPORT.md` for the latest test counts.
