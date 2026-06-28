# NavMesh + Dynamic Encounter Pass

## Overview

This pass makes the CommanderPrototype scene NavMesh-ready and enables dynamic wave-based unit spawning during encounters. MissionModifier multipliers now affect dynamic spawn counts.

## NavMesh Integration

### NavigationAgentBridge
- Already supports dual-mode: NavMesh (when `NavMeshAgent` present + on NavMesh) and fallback (transform-based movement)
- All AI units get `NavMeshAgent` + `NavigationAgentBridge` via `EncounterSpawnPoint.SpawnUnit()` or `SetupMenu`
- Fallback mode auto-activates when no NavMesh is baked, with a one-time warning

### Scene Setup
- `SetupMenu.MarkNavMeshStatic()` marks Ground objects as `NavigationStatic`
- To bake NavMesh: Window > AI > Navigation > Bake (after running Create Commander Mission Prototype Scene)
- Without baking, all AI units run in fallback mode (no NavMesh pathfinding, direct transform movement)

### NavMesh Agent Configuration
- `NavMeshAgent` is added to AI units by `EncounterSpawnPoint.SpawnUnit()` and `SetupMenu.CreateCombatCharacter()`
- Default values are used; override via `NavigationAgentBridge.SetDestination(speed)` at runtime

## Dynamic Encounter System

### EncounterWave
- `EncounterWave` is a serializable data class with: `waveId`, `faction`, `role`, `unitCount`, `delaySeconds`, `spawned`
- `IsReady` returns true when the wave hasn't been spawned yet
- Used by `EncounterRuntime.TickWaves()` for time-based automatic spawning

### EncounterSpawnPoint
- MonoBehaviour placed in scene at spawn locations
- `SpawnUnit(CharacterData, prefab)` creates a fully configured unit with:
  - `CharacterEntity` + `CharacterData.Bind()`
  - `Combatant`
  - `SimpleCombatAI`
  - `NavigationAgentBridge` + `NavMeshAgent`
- `GetSpawnPosition()` / `GetRandomSpawnPosition()` for precise or randomized placement

### EncounterRuntime Wave Spawning
- `SetWaves(List<EncounterWave>)` configures waves and sets `PendingWaveCount`
- `TickWaves(float deltaTime)` advances time and spawns waves whose delay has elapsed
- `SpawnWave(EncounterWave)` creates units at the matching `EncounterSpawnPoint`
- `GetFactionMultiplier(SubFactionId)` returns `BeastHostilityMultiplier` or `MechaSupportMultiplier` from the `EncounterDefinition`
- Spawned units are tracked in `SpawnedUnits` list alongside pre-existing `Units`
- `AreAllRaidUnitsDefeated()` and `CountCasualties()` include both pre-existing and spawned units

### MissionModifier → Dynamic Spawn Flow
1. `BorderRetaliationRuntime.StartRetaliation()` builds `MissionModifier` via `MissionChainService.BuildMissionModifiers()`
2. `ApplyMissionModifier()` sets `BeastHostilityMultiplier` / `MechaSupportMultiplier` on `EncounterDefinition`
3. `ConfigureDynamicWaves()` creates wave configs scaled by modifier:
   - `border_beast_retaliation`: 2 beast waves + mecha support
   - `border_mecha_distrust`: 1 beast wave, no mecha support
   - `border_ceasefire`: no waves
   - default / `border_low_trust`: 1 beast wave + mecha support
4. `TickWaves()` runs every frame during the active mission phase
5. Each wave's `unitCount` is multiplied by the faction multiplier from the `EncounterDefinition`

## SetupMenu Changes

- `MarkNavMeshStatic(GameObject)` marks Ground and children as `NavigationStatic`
- Both prototype scenes mark Ground as NavMesh-static
- 4 spawn points in CommanderPrototype:
  - `SpawnPoint_Beast` (main area) + `SpawnPoint_Mecha` (main area) for ConvoyEnergyConflict
  - `BorderSpawnPoint_Beast` + `BorderSpawnPoint_Mecha` for BorderRetaliation
- ConvoyEnergyConflict has 2 pre-configured waves (beast waves at 15s and 35s)

## VerticalSliceValidator Checks

- **CheckNavMeshSetup**: Reports NavMesh vs fallback mode count, checks Ground is NavigationStatic
- **CheckEncounterWaveConfig**: Reports wave count, spawn point count, warns about waves without spawn points

## Test Coverage

### EditMode
- `EncounterWaveDynamicTests` — wave state, TickWaves timing, no-respawn, faction multiplier, spawned unit tracking
- `EncounterSpawnPointTests` — position, random position, SpawnUnit component creation, data binding
- `MissionModifierDynamicSpawnTests` — multiplier scaling, ApplyMissionModifier effect

### PlayMode
- `NavMeshDynamicEncounterTests` — dynamic wave spawn timing, spawned unit has NavigationBridge, fallback movement

## Manual Validation

1. Run `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene`
2. Window > AI > Navigation > Bake (Ground is already marked NavigationStatic)
3. Open CommanderPrototype, press Play
4. AI units should use NavMesh (blue path lines visible with NavMesh display)
5. Complete tutorial, trigger ConvoyEnergyConflict mission
6. After 15s, beast wave should spawn from SpawnPoint_Beast
7. After 35s, second beast wave should spawn
8. Walk to Border Retaliation area, trigger mission
9. Dynamic waves spawn based on mission modifier
10. Without NavMesh baked, AI still moves via fallback (direct transform movement)
