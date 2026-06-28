# Demo Flow Design

Date: 2026-06-23
Unity: 2022.3.62f3 LTS

## Goal

This pass consolidates the existing three-mission vertical slice into a readable, maintainable demo framework without adding Mission 4, boss content, model replacement, or combat-system rewrites.

The intended player-facing flow is:

1. **Mission 1: Convoy Energy Conflict**
2. **Mission 2: Border Retaliation**
3. **Mission 3: City Gate Dispute**
4. **All Missions Complete**

The framework is additive: mission runtimes remain programmatic and existing debug shortcuts remain intact.

## Three-mission demo flow

`DemoFlowManager` reads `MissionChainState` and recommends the next demo step:

| State | Condition | Next mission id | Player hint |
|---|---|---|---|
| `ConvoyAvailable` | No completed chain entry for Mission 1 | `convoy_energy_conflict` | Protect convoy, share energy, avoid excessive casualties. |
| `BorderRetaliationAvailable` | Mission 1 complete, Mission 2 incomplete | `border_retaliation` | Travel to the border, survive retaliation, protect allied units. |
| `CityGateAvailable` | Mission 1 + Mission 2 complete, Mission 3 incomplete | `city_gate_dispute` | Go to the city gate, protect CityGateCore and BeastNegotiator, defeat raiders. |
| `AllMissionsComplete` | All three mission ids are complete | none | Review border and city stability. |

`DemoFlowManager` does **not** write `MissionChainState`; mission completion remains owned by mission runtimes and `MissionChainService.RecordMissionResult`.

## MissionDefinitionSO authoring strategy

The three chain missions have authoring assets under `Assets/Data/Missions/`:

- `ConvoyEnergyConflict.asset`
- `BorderRetaliation.asset`
- `CityGateDispute.asset`

Each asset carries:

- `MissionId`
- `DisplayName`
- `Description`
- default objective checklist
- possible outcome summaries

The assets support HUD, validation, and authoring readability. Runtime mission code still works programmatically and keeps existing fallback behavior. These `.asset` and `.meta` files are required authored data and should be versioned; setup menu generation is a safe/idempotent recovery path, not the sole source of truth. See `Assets/Docs/AUTHORING_ASSET_PERSISTENCE.md` for the persistence policy.

Legacy prototype authoring assets such as `ConvoyEscort.asset`, `EnergyRaid.asset`, and `BalanceAllocation.asset` are preserved for compatibility.

## HUD readability goals

### DemoFlowHud

`DemoFlowHud` is an OnGUI guide panel. It shows:

- current recommended mission
- world target name
- objective hint
- debug shortcuts: `F7` CityGate outcome and `F8` CityGate teleport

It does not replace `MissionObjectiveHud`.

### MissionObjectiveHud

`MissionObjectiveHud` shows active mission objectives when a mission runtime is active. If there is no active mission, it falls back to DemoFlow guidance.

Objective checklists are standardized:

- Convoy: protect convoy, share energy, avoid excessive casualties.
- Border: survive retaliation, defeat raiders, protect allied units, keep casualties low.
- CityGate: protect CityGateCore, keep BeastNegotiator alive, defeat BeastRaiders, keep casualties low.

Completed missions show a compact outcome/casualty summary.

### MissionResultSummaryPanel

`MissionResultSummaryPanel` shows:

- mission display name
- outcome
- commander XP
- faction deltas or `No consequence data`
- next mission hint
- modifier/context summary
- readable outcome effect text

The panel supports legacy Convoy/Border outcomes and the five CityGate outcomes.

### Playable demo polish layout

The OnGUI demo layout is intentionally grouped into four non-overlapping readability blocks instead of a final Canvas UI:

- **Left top — DemoFlowHud**: current recommended mission, world target, objective hint, and foldable `DEMO / DEBUG` shortcut help.
- **Left middle — MissionObjectiveHud**: mission started text, primary/optional objectives, suggested action, active checklist, phase, casualty/protected-target status, and final completion/failure summary.
- **Right top — CommanderControlHintPanel / CommanderDebugHud**: selected target, DirectControl / TacticalCommand / SyncAssist allowed/denied status, denial reason, suggestion, and `LastInputRoute`.
- **Right bottom — MissionResultSummaryPanel**: mission result, outcome, faction consequence, commander XP, modifier context, and next mission hint.

`DebugUILayout` owns the shared layout constants and exposes compact fallback helpers for widths below 1024. Tests assert positive dimensions and broad region separation rather than exact pixel coordinates.

### Demo shortcut help

`DemoFlowHud` includes a foldable `DEMO / DEBUG` shortcut section. It must list `1/2/3/F7/F8/F5/F9/F10`, `G/F` for `DefendObjective` / `FocusFire`, `Tab/Q`, `E`, `R`, `Left Click`, and `Space`. The help is display-only and does not change input priority or mission-chain writes.

### Marker coverage

CommanderPrototype scene setup and validation expect readable markers for:

- Mission 1: `Convoy Mission Area`, `Convoy`, `Energy Node`.
- Mission 2: `Border Retaliation Area`, `Raider Spawn`, `Allied Defense Point`.
- Mission 3: `City Gate Mission Area`, `CityGateCore`, `BeastNegotiator`, `BeastRaider Spawn`, `Guard: Defensive`, `Raider: Aggressive`, `Negotiator: Non-combatant`, `Hardliner: Escalation risk`, `CommanderUnit: Tactical only`.

`WorldMarker` keeps labels serialized for scene validation and supplies readable fallbacks for generated setup object names. AI profile labels are debug/readability hints only; they do not change mission-chain ordering or outcome ownership.

## CommanderAction display and expansion layer

`CommanderActionPresenter` is a display layer over commander diagnostics and the new tactical command predictions. It exposes descriptors for:

- `DirectControl`
- `TacticalCommand`
- `SyncAssist`
- `DefendObjective`
- `FocusFire`

Each descriptor includes:

- display name
- allowed/denied status
- denial reason
- suggestion
- target name

`DefendObjective` uses **G** to order a commandable low-rank ally to protect a mission objective such as Convoy, Energy Node, Allied Defense Point, CityGateCore, or BeastNegotiator. `FocusFire` uses **F** to order nearby commandable allies to attack the selected hostile threat. Both actions are additive tactical commands; they do not change mission outcome calculation or replace the existing E DirectControl / TacticalCommand / SyncAssist priority.

The presenter does not grant authority by itself. `ControlPermissionService`, `CommanderControlController`, and `SimpleCombatAI` remain the authority for actual command behavior.

## Manual demo checklist

1. Open `Assets/Scenes/CommanderPrototype.unity`.
2. Press Play.
3. Confirm DemoFlow HUD starts at Mission 1.
4. Confirm a nearby low-rank marker reads `Low-Rank Ally: Press E to Control`.
5. Confirm a high-rank marker reads `High-Rank Unit: Tactical Command Only` and DirectControl denial is readable.
6. Complete or debug-progress Mission 1 and verify the next hint points to Border Retaliation.
7. Complete or debug-progress Mission 2 and verify the next hint points to City Gate Dispute.
8. Press `F8` and confirm the player lands near the City Gate mission area.
9. Press `F7` and confirm the CityGate debug outcome path displays a readable summary without recording a real mission-chain entry.
10. Verify `1/2/3` debug triggers still work and remain tagged as debug/test paths.

## Known limitations

- The demo still uses OnGUI debug/readability panels instead of a final Canvas UI.
- `MissionDefinitionSO` assets are authoring/readability support; mission runtime logic is still partly programmatic.
- Dynamic spawned-unit HP, position, and AI state remain intentionally outside save serialization, matching `ENCOUNTER_PERSISTENCE_DESIGN.md`.
- Full manual validation still requires playing the Unity scene; automated tests cover component presence, state progression, assets, and smoke behavior.
