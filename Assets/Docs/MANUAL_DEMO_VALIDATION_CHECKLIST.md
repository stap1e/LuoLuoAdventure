# Manual Demo Validation Checklist

Date: 2026-06-23  
Unity: 2022.3.62f3 LTS  
Scene: `Assets/Scenes/CommanderPrototype.unity`

## 1. Scene preparation

- Run `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene` if the scene needs to be regenerated.
- Confirm required authored assets are present and not git-ignored (`Assets/Data/Missions`, `Assets/Data/AIProfiles`, `Assets/Data/Combat/CombatTuningConfig.asset`, and runtime-loaded Resources copies). See `Assets/Docs/AUTHORING_ASSET_PERSISTENCE.md`.
- Open `Assets/Scenes/CommanderPrototype.unity`.
- Press Play.
- Confirm there are no compile errors or missing-script errors in the Console.

## 2. Basic controls

- `WASD`: move the current player-controlled unit.
- Mouse aim / camera-facing movement: confirm the player can orient and navigate.
- `Left Click`: attack.
- `Space`: dodge.
- `Tab` / `Q`: select or cycle commander targets.
- `E`: DirectControl / TacticalCommand / SyncAssist / interact, depending on the selected target and context.
- `R`: release direct control or cancel command/sync assist.

Expected pass criteria:

- Movement and combat controls respond.
- HUD does not block the center of the screen.
- DemoFlow, mission objective, commander control, and result summary panels occupy separate readable regions.

## 3. Commander control validation

### E with no target

- Clear the selected target or start without selecting a unit.
- Press `E`.
- Expected:
  - a nearby low-rank controllable unit is auto-acquired, **or**
  - HUD displays `No controllable target nearby` with a suggestion to use `Tab/Q` or move closer.
- `E` must never silently fail.

### Control low-rank unit

- Select the low-rank marker: `Low-Rank Ally: Press E to Control`.
- Confirm the HUD says `DirectControl: Allowed`.
- Press `E`.
- Expected:
  - camera/input ownership changes clearly to the controlled unit,
  - `LastInputRoute` is readable,
  - `R` releases control.

### Deny high-rank unit

- Select the high-rank marker: `High-Rank Unit: Tactical Command Only`.
- Confirm the HUD says `DirectControl: Denied`.
- Confirm a readable reason appears, such as:
  - `Leader unit`,
  - `Rank too high`,
  - `Trust too low`,
  - `Direct control disabled`.
- Confirm a suggestion appears, such as:
  - `Try Tactical Command`,
  - `Try SyncAssist`,
  - `Select lower-rank unit`.

### DefendObjective / FocusFire tactical commands

- Select or stand near `Low-Rank Ally: Can Receive Commands` / `Low-Rank Ally: Press E to Control`.
- Select or stand near Convoy, Energy Node, Allied Defense Point, CityGateCore, or BeastNegotiator.
- Press `G`.
- Expected:
  - HUD shows `DefendObjective: Allowed` before or after the command when a valid ally/objective pair is available,
  - command status reads `Command: Defend CityGateCore` or equivalent objective name,
  - ally moves toward / holds near the objective and engages threats that enter the defend radius,
  - invalid ally/objective cases show a denial reason instead of failing silently.
- Select a hostile `BeastRaider` / `MechaHardliner` threat near commandable allies.
- Press `F`.
- Expected:
  - HUD shows `FocusFire: Allowed`, responder count, and remaining duration,
  - nearby allies receive the focus target,
  - target death or duration expiry clears the command and restores default AI behavior.

### EnergyNode priority

- Stand near `Energy Node` with a selected commander target.
- Press `E`.
- Expected: selected-target commander control/denial is handled first; EnergyNode does not steal the input.
- Clear the selected target and stand near EnergyNode.
- Expected: mission interaction may proceed normally.

## 4. Mission 1 validation — Convoy Energy Conflict

- Find marker: `Convoy Mission Area`.
- Confirm objective markers are visible and spatially separated:
  - `Convoy`,
  - `Energy Node`.
- Enter the mission area.
- Confirm `MissionObjectiveHud` shows:
  - mission started text,
  - primary objective,
  - optional/suggested action,
  - in-progress checklist.
- Complete through play or use debug shortcut `1`.
- Confirm `MissionResultSummaryPanel` shows:
  - readable outcome,
  - faction consequence or `No consequence data`,
  - commander XP,
  - next mission hint.

## 5. Mission 2 validation — Border Retaliation

- Find marker: `Border Retaliation Area`.
- Confirm visible markers:
  - `Raider Spawn`,
  - `Allied Defense Point`.
- Start the mission.
- Confirm dynamic wave behavior begins without duplicate spawns.
- Complete through play or use debug shortcut `2` / existing `3` branch as needed.
- Confirm result summary is readable.
- Save/load check during or after the mission:
  - press `F5`,
  - press `F9`,
  - confirm no duplicate waves and no null-reference errors.

## 6. Mission 3 validation — City Gate Dispute

- Press `F8`.
- Expected: player teleports near the CityGate area and can see CityGate markers.
- Confirm visible markers:
  - `City Gate Mission Area`,
  - `CityGateCore`,
  - `BeastNegotiator`,
  - `BeastRaider Spawn`,
  - `Guard: Defensive`,
  - `Raider: Aggressive`,
  - `Negotiator: Non-combatant`,
  - `Hardliner: Escalation risk`,
  - `CommanderUnit: Tactical only`.
- Confirm selected-target HUD can show `Default AI` or a profile label plus command responsiveness.
- Enter the CityGate trigger.
- Confirm objective checklist includes:
  - protect `CityGateCore`,
  - protect `BeastNegotiator`,
  - defeat raiders,
  - keep casualties low.
- During play, verify:
  - `CityGateCore` remains readable as protected target alive/dead,
  - `BeastNegotiator` remains readable as protected target alive/dead,
  - raider defeat progress is readable.

### AI behavior tuning validation

- Observe `BeastRaider` and confirm it pressures `CityGateCore` or a protected/non-combatant target instead of wandering aimlessly.
- Observe `MechaGateGuard` and confirm it holds near `CityGateCore`; after `G DefendObjective`, it should defend the objective and avoid chasing beyond its leash.
- Observe `BeastNegotiator` and confirm it does not initiate combat; when a hostile approaches, it should show retreat/non-combatant behavior.
- Observe `MechaHardliner` and confirm it moves toward or threatens `BeastNegotiator` / protected targets as an escalation risk without instantly killing the negotiator.
- Select Raider / Guard / Negotiator / Hardliner / CommanderUnit examples and confirm HUD text includes profile label, behavior, command response flags, and a short suggestion.
- Press `F` on a Raider and confirm eligible allied responders focus fire while Negotiator does not become a responder.
- Press `G` on `CityGateCore` and confirm eligible guard/ally behavior changes to defend/hold near the objective.
- Press `F5`, then `F9`, and confirm profile assignments remain readable and CityGate waves are not duplicated.

- Press `F7` to trigger the CityGate BalancedMediation debug outcome.
- Expected:
  - readable result summary appears,
  - debug path is labeled `[DEBUG TRIGGER]`,
  - MissionChainState is not polluted by the `test_citygate` debug mission id.

## 7. Save/load validation

- `F5`: save.
- `F9`: load.
- `F10`: clear save.
- Expected:
  - no duplicate dynamic enemies after load,
  - no duplicate waves after load,
  - no null-reference errors,
  - completed encounters do not respawn as active waves,
  - in-progress encounter lifecycle warnings remain readable if dynamic unit state cannot be fully restored.

## 8. Debug shortcut help validation

Confirm the DemoFlow or shortcut HUD is explicitly labeled `DEMO / DEBUG` and lists:

- `1`: Debug Mission 1 outcome
- `2`: Debug Mission 2 outcome
- `3`: Debug existing mission trigger
- `F7`: Debug CityGate BalancedMediation
- `F8`: Teleport to CityGate
- `F5`: Save
- `F9`: Load
- `F10`: Clear Save
- `G`: DefendObjective
- `F`: FocusFire
- `Tab/Q`: Select target
- `E`: DirectControl / Command / Interact
- `R`: Release control
- `Left Click`: Attack
- `Space`: Dodge

Confirm shortcut help can be folded/unfolded with `H`.

## 9. Expected pass criteria

- Player always knows the next objective.
- Mission 1 / Mission 2 / Mission 3 locations are discoverable through world markers.
- `E` never silently fails.
- Low-rank control and high-rank denial are both readable.
- Task result and consequence text are readable.
- Debug shortcuts remain available and clearly marked as demo/debug.
- Save/load does not duplicate enemies or waves.
- No Console errors or null references during the manual pass.
- Full EditMode and PlayMode suites finish with 0 failed.
