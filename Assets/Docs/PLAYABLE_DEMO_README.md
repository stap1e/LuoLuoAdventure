# Playable Demo README

## Unity Version

- **2022.3.62f3 LTS**

## First-Time Setup

Run these from the Unity top menu in order:

1. **LuoLuoTrip/Setup/Generate Placeholder Assets** — creates `Assets/Art/Placeholders/Prefabs/PH_*.prefab` + materials
2. **LuoLuoTrip/Setup/Create Commander Mission Prototype Scene** — creates all data assets + `Assets/Scenes/CommanderPrototype.unity`
3. **LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation** — verifies all required objects exist

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
- **AI Warning**: "!" indicator above enemy during attack windup (0.4s)
- All timings configurable via `Assets/Data/Combat/CombatTuningConfig.asset`

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
- [ ] F5 saves, F9 loads, F10 clears
- [ ] 1/2/3 debug triggers work and don't affect mission chain
- [ ] No Missing Script warnings
- [ ] All debug UI panels visible and non-overlapping

## Known Limitations

- All UI uses OnGUI (debug quality, not production)
- Placeholder art (cylinder prefabs)
- No NavMesh / pathfinding
- No cinematic camera
- No formal dialogue system
- No audio
- Tests must be run from Unity Editor Test Runner (no CLI test runner)
- SimpleCombatAI uses FindObjectsOfType as fallback (registry preferred)
- No network / multiplayer
- No localization
