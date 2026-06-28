# Navigation & Encounter Reliability Pass

## Objective
Make existing two missions' AI movement, encounters, mission areas, and TacticalCommand more stable and closer to a playable demo.

## NavigationAgentBridge Design

NavigationAgentBridge (MonoBehaviour) provides a unified navigation API:

- SetDestination(Vector3 / NavigationMoveRequest) - request movement to target
- Stop() / Resume() - pause/resume navigation
- ClearRequest() - cancel current request
- HasReachedDestination() - check arrival
- IsPathAvailable() - check if path exists
- TickFallback(float) - call each frame for fallback movement
- State - NavigationState enum: Idle, Moving, Stopped

### NavMeshAgent vs Fallback

- If NavMeshAgent component exists on the GameObject, bridge uses it automatically
- If no NavMeshAgent or NavMesh is unavailable, falls back to transform-based movement
- Fallback prints one-time warning, not per-frame
- UseNavMesh property reports which mode is active

## AICombatNavigationController

Higher-level navigation controller that sits alongside SimpleCombatAI:

- ChaseTarget(Transform) - navigate toward target at chase speed
- FollowTarget(Transform) - navigate to follow distance, then stop
- MoveToPosition(Vector3) - navigate to a fixed point
- StopNavigation() / ClearNavigation() - halt or cancel navigation
- IsInAttackRange(Transform) - check if in melee range
- Tick(float) - must be called each frame; handles fallback ticking

## TacticalCommand Navigation Integration

| Command | Navigation Behavior |
|---|---|
| FollowPlayer | Uses AICombatNavigationController.FollowTarget to follow commander unit, stops at follow distance |
| HoldPosition | Stops navigation, holds current position, does not chase unless attacked or ForcedAttackTarget set |
| AttackCurrentTarget | Uses navigation to approach target, enters attack windup when in range, auto-clears if target dies |
| ReleaseControl | Clears TacticalCommandState, clears AI FollowTarget/HoldPosition/ForcedAttackTarget, stops navigation |

### HUD Display
- CommanderDebugHud shows: command type, navigation state (Moving/Idle/Stopped), distance to target, NavMesh usage
- CommanderControlHintPanel shows: current command, cancel option

## EncounterRuntime Design

EncounterRuntime (MonoBehaviour) tracks units in an encounter:

- RegisterUnit(CharacterEntity) / UnregisterUnit(CharacterEntity) - add/remove units
- RegisterUnitsBySubFaction(SubFactionId) - bulk register by faction
- GetAliveUnits(MainRace / SubFactionId) - query living units
- AreAllRaidUnitsDefeated(SubFactionId) - check if all raid units are dead
- CountCasualties(MainRace / SubFactionId) - count dead units
- ApplyMissionModifier(MissionModifier) - apply hostility/support multipliers

Supporting types:
- EncounterDefinition - serializable config (encounterId, attackerFaction, defenderFaction, multipliers)
- EncounterUnitHandle - wraps CharacterEntity with Race and WasAliveAtStart tracking
- EncounterWave - serializable wave definition (for future dynamic spawning)
- EncounterSpawnPoint - MonoBehaviour marker for spawn positions in scenes

## MissionAreaRuntime / RetreatTracker Design

### MissionAreaRuntime
MonoBehaviour managing mission area state:

- Activate(missionId) / Deactivate() / MarkComplete() - lifecycle
- Tick(float) - updates player inside/outside status and retreat timer
- ShouldTriggerRetreat() - returns true when retreat threshold exceeded
- IsPlayerInside - whether player is within boundary
- Auto-creates MissionBoundary from MissionTriggerZone on Start

### MissionBoundary
Defines a mission area (center + radius):
- IsInside(Vector3) - point-in-circle test (XZ plane)
- ConfigureFromTriggerZone(MissionTriggerZone) - auto-configure from existing trigger zone

### RetreatTracker
Pure C# class (no MonoBehaviour) for retreat countdown:
- Tick(deltaTime, playerInside) - increment timer when outside, reset when inside
- IsRetreating - true when timer >= threshold
- Progress - 0-1 normalized progress toward retreat
- Configure(retreatTime) / Reset() - setup and reset

## ConvoyEnergyConflict Integration

- Uses EncounterRuntime for beast unit tracking and casualty counting
- Uses MissionAreaRuntime + RetreatTracker for retreat detection (replaces manual abandonTimer)
- Falls back to legacy FindObjectsOfType logic if EncounterRuntime is unavailable
- AreAllRaidUnitsDefeated used for MechaVictory check
- Auto-creates EncounterRuntime + MissionAreaRuntime child objects in Start if not present

## BorderRetaliation Integration

- Uses EncounterRuntime for unit tracking and casualty counting
- Uses MissionAreaRuntime for retreat detection
- ApplyMissionModifier called on EncounterRuntime to set hostility multipliers
- All 4 branches (BeastRetaliation, MechaDistrust, Ceasefire, LowTrust) use EncounterRuntime queries
- Falls back to legacy logic if EncounterRuntime unavailable

## Manual Validation Steps

1. Open Unity → confirm no compile errors
2. Run LuoLuoTrip/Setup/Create Commander Mission Prototype Scene
3. Press Play in CommanderPrototype
4. Verify AI units move toward player (using fallback navigation since no NavMesh baked)
5. Tab-select target, press E for TacticalCommand → verify unit follows using navigation
6. Press R → verify unit stops following, navigation clears
7. Walk into mission trigger zone → verify mission activates
8. Walk out of mission area → verify retreat countdown appears in HUD
9. Return to mission area → verify countdown resets
10. Complete mission → verify EncounterRuntime correctly reports casualties
11. Walk to BorderRetaliation trigger → verify second mission activates
12. Run LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation → all checks pass

## Known Limitations

- No NavMesh baked in demo scenes → all navigation uses fallback transform movement
- NavMeshAgent is added to AI units but only active if NavMesh is baked
- EncounterRuntime does not dynamically spawn units (references scene units only)
- MissionBoundary uses simple sphere check, not complex geometry
- OnGUI-based HUD for retreat countdown (debug quality)
- No third mission, no boss
- Tests requiring Unity Editor Test Runner:
  - All PlayMode tests (NavigationAgentBridgeSmokeTests, SimpleCombatAINavigationCommandTests, MissionEncounterFlowSmokeTests, RetreatBoundarySmokeTests)
  - Some EditMode tests that depend on CharacterEntity/Combatant components
