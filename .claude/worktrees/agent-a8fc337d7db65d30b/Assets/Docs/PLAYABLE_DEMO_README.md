# Playable Demo README

## Unity Version

- **2022.3.62f3 LTS**

## First-Time Setup

Run these from the Unity top menu in order:

1. **LuoLuoTrip/Setup/Generate Placeholder Assets** — creates `Assets/Art/Placeholders/Prefabs/PH_*.prefab` + materials
2. **LuoLuoTrip/Setup/Create Audio Feedback Profile** *(optional, auto-run by step 4)* — creates `Assets/Data/Audio/AudioFeedbackProfile.asset` + Resources copy
3. **LuoLuoTrip/Setup/Create World Marker Profile** *(optional, auto-run by step 4)* — creates `Assets/Data/Feedback/WorldMarkerProfile.asset` + Resources copy
4. **LuoLuoTrip/Setup/Create Commander Mission Prototype Scene** — creates all data assets + `Assets/Scenes/CommanderPrototype.unity`
5. **LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation** — verifies all required objects exist
6. *(Optional)* **LuoLuoTrip/Setup/Regenerate Enhanced Placeholders (Force)** — rebuilds multi-primitive placeholder visuals from scratch

## How to Run

1. Open `Assets/Scenes/CommanderPrototype.unity`
2. Press Play
3. Follow the 8-step tutorial overlay

## Controls

| Key | Action |
|---|---|
| WASD | Move |
| Left Click | Attack |
| Space | Dodge (invulnerable during dodge) |
| Q | Lock-on toggle |
| Tab | Select next target in range |
| E | Interact (commander control / energy share) |
| R | Release control |
| F5 | Quick save |
| F9 | Quick load |
| F10 | Clear save |
| 1 | Debug: Test MechaVictory |
| 2 | Debug: Test BeastVictory |
| 3 | Debug: Test BalancedResolution |

## E Key Priority

1. If a commander target is selected (Tab) → E triggers commander control
2. If no target selected and player is at EnergyNode → E triggers energy sharing
3. If mission already completed/failed → E does nothing

## Tutorial Flow

TutorialFlowRuntime guides through 8 steps:
1. Welcome
2. Movement (WASD)
3. Lock-on (Q)
4. Attack (Left Click)
5. Dodge (Space)
6. Commander select (Tab)
7. Commander control (E)
8. Commander release (R)

## Mission 1: ConvoyEnergyConflict

- Auto-starts when player enters the trigger zone
- Objectives: Protect convoy, Stop beast raid, Share energy at node
- Outcomes:
  - **MechaVictory**: All beasts defeated, convoy intact
  - **BeastVictory**: Convoy destroyed or energy node captured by beasts
  - **BalancedResolution**: Energy shared, minimal casualties
  - **Failed**: Player retreats (leaves zone for 10s)

## Mission 2: BorderRetaliation

- Unlocked after completing ConvoyEnergyConflict
- Trigger zone at position (25, 0, 0)
- Branches based on Mission 1 outcome:

| Mission 1 Outcome | Mission 2 Branch | Effect |
|---|---|---|
| MechaVictory | Beast Retaliation | Beast hostility x1.5 |
| BeastVictory | Mecha Distrust | Mecha support x0.5, Captain tactical-only |
| BalancedResolution | Ceasefire | Ceasefire active, hostility -15 |
| Failed/PartialSuccess | Low Trust | Hostility +10, evacuation mode |

## Commander Control Modes

| Mode | Condition | Effect |
|---|---|---|
| DirectControl | Commander rank >= target rank, same race | Take direct control of unit |
| TacticalCommand | Commander rank sufficient, cross-race ok | Issue follow/hold/attack commands |
| SyncAssist | Low sync rate but some trust | Temporary damage/defense buff |
| Denied | Insufficient rank or trust | Cannot control |

## Combat Feel

- **Attack Windup**: 0.25s before damage frame
- **Attack Recovery**: 0.3s after attack, no actions
- **Dodge Invulnerability**: 0.3s invulnerable window during 0.35s dodge
- **Stagger**: 1.2s when poise depleted, no actions
- **AI Warning**: "[!]" world-marker label above enemy during attack windup (0.4s)
- All timings configurable via `Assets/Data/Combat/CombatTuningConfig.asset`

## Demo Areas

| Area | Position | Purpose |
|---|---|---|
| Tutorial | (0, 0, 0) | Initial spawn + tutorial steps |
| Convoy Mission | (0, 0, 5) | Mission 1 trigger + convoy/energy node |
| Border Retaliation | (25, 0, 0) | Mission 2 trigger + objective marker |
| Advanced Units | (22, 0, -2) | Rank 2/3 unit showcase (Captain, Elite, Deputy) |

Each area is marked with a colored ground ring and a floating world-marker label for navigation.

## Audio Feedback

`AudioFeedbackService` (singleton MonoBehaviour) plays 2D/3D one-shot SFX driven by `AudioFeedbackProfileSO` events:

| Event | Trigger | Spatial |
|---|---|---|
| AttackStart | Player or AI begins attack windup | 3D |
| Hit | Damage lands | 3D |
| Dodge | Player dodge | 3D |
| Stagger | Combatant enters stagger | 3D |
| AIWindupWarning | AI windup begins (throttled to 0.5s) | 3D |
| DirectControlSuccess / TacticalCommandIssued / SyncAssistActive / DeniedControl | E-key control attempt | UI 2D |
| MissionComplete / MissionFailed | Mission ends | UI 2D |
| LevelUp / FactionDelta | Commander level / standing change | UI 2D |

The profile auto-populates one entry per `AudioEventId` enum value. Drop `AudioClip[]` into entries to provide sound; missing clips are silently skipped (no errors).

## World Markers

`WorldMarkerService` (singleton MonoBehaviour) renders OnGUI text labels above world-space objects, billboarded to `Camera.main`:

| Type | Label | Used By |
|---|---|---|
| SelectedCommanderTarget | `[TARGET]` | CommanderControlController on Tab selection |
| LockOnTarget | `[LOCK]` | CombatController on Q lock-on |
| MissionObjective | `[OBJ]` | Convoy, Border objective marker, Area labels |
| Interactable | `[E]` | EnergyNode |
| AIWindupWarning | `[!]` | SimpleCombatAI during attack windup |
| ControlledUnit | `[YOU]` | Currently controlled commander unit |
| HostileUnit / FriendlyUnit | (color only) | Reserved for future faction tinting |
| SyncAssistActive | `[SYNC]` | SyncAssist control mode |

Markers can be disabled at runtime via `WorldMarkerService.Instance.MarkersEnabled = false`. Defaults are provided in code so the service works even with no profile asset.

## Enhanced Placeholders

Each `PH_*.prefab` uses a multi-primitive Visual subgraph for readability:

- **PlayerCommander**: Capsule body + Sphere head + Crown + Core + FacingFin
- **MechaMinion / CityLord**: Hull + Cockpit + FrontFin + 4 Wheels (CityLord adds Antenna)
- **BeastMinion / WarKing**: Body + Head + 2 Horns + 2 Claws + 4 Legs
- **Convoy**: 2 Cargo crates + EnergyTank + 4 Wheels
- **EnergyNode**: Base + Core sphere + Pillar + Ring
- **ObjectiveMarker**: Pillar + Banner + Apex sphere

Hierarchy is preserved as `PrefabRoot / Visual / Collision / Marker` — only the Visual subgraph is enhanced. Use **Regenerate Enhanced Placeholders (Force)** to rebuild after edits.

## Save/Load

- **F5**: Quick save — saves commander, factions, mission chain, character states
- **F9**: Quick load — restores all saved state
- **F10**: Clear save — deletes save file (restart scene to fully reset)

## Manual Validation Checklist

- [ ] CommanderPrototype scene loads without errors
- [ ] Tutorial overlay appears and advances through 8 steps
- [ ] Player can move with WASD
- [ ] Player can attack with Left Click (windup → attack → recovery visible)
- [ ] Player can dodge with Space (invulnerable during dodge)
- [ ] Tab selects a commander target, E attempts control
- [ ] R releases control
- [ ] Mission 1 starts when entering trigger zone
- [ ] Mission 1 completes and unlocks Mission 2
- [ ] Mission 2 branch matches Mission 1 outcome
- [ ] AI units navigate using NavigationAgentBridge (fallback without NavMesh)
- [ ] TacticalCommand FollowPlayer uses navigation to follow
- [ ] TacticalCommand HoldPosition stops navigation
- [ ] TacticalCommand AttackCurrentTarget navigates to target
- [ ] R release clears navigation and command state
- [ ] Retreat countdown appears when player leaves mission area
- [ ] Returning to mission area resets retreat countdown
- [ ] EncounterRuntime tracks units and casualties correctly
- [ ] F5 saves, F9 loads, F10 clears
- [ ] 1/2/3 debug triggers work and don't affect mission chain
- [ ] No Missing Script warnings
- [ ] All debug UI panels visible and non-overlapping

## Navigation

AI units use `NavigationAgentBridge` for movement:

- If `NavMeshAgent` is present and NavMesh is baked → uses NavMesh pathfinding
- Otherwise → falls back to transform-based direct movement (one-time warning)
- `AICombatNavigationController` provides higher-level: ChaseTarget, FollowTarget, MoveToPosition
- CommanderDebugHud shows navigation state (Moving/Idle/Stopped) and distance

## Encounter System

`EncounterRuntime` tracks mission units:

- Registers units by faction automatically when mission starts
- Provides `AreAllRaidUnitsDefeated`, `CountCasualties`, `GetAliveUnits` queries
- Both ConvoyEnergyConflict and BorderRetaliation use EncounterRuntime
- Falls back to legacy `CharacterRuntimeRegistry` + `FindObjectsOfType` if unavailable

## Mission Areas & Retreat

- `MissionAreaRuntime` manages player inside/outside mission boundary
- `RetreatTracker` counts time outside; triggers mission failure after threshold
- `MissionObjectiveHud` shows INSIDE/OUTSIDE status and retreat countdown
- Mission boundaries auto-configure from MissionTriggerZone

## Known Limitations

- All UI uses OnGUI (debug quality, not production)
- Placeholder art (multi-primitive cube/cylinder/sphere prefabs)
- No baked NavMesh in demo scenes — AI uses transform fallback movement
- NavigationAgentBridge with NavMeshAgent works if NavMesh is baked; demo scenes do not bake it
- No cinematic camera
- No formal dialogue system
- AudioFeedbackProfile entries ship with no clips by default — system runs silent until clips are added
- Tests must be run from Unity Editor Test Runner (no CLI test runner)
- SimpleCombatAI uses FindObjectsOfType as fallback (registry preferred)
- EncounterRuntime does not dynamically spawn units
- No network / multiplayer
- No localization
