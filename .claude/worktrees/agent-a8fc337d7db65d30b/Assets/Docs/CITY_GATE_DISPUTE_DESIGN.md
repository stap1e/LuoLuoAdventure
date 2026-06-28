# City Gate Dispute Design (Mission 3)

Date: 2026-06-17
Unity: 2022.3.62f3 LTS

## Mission 3 Design Goal

CityGateDispute is the third mission in the prototype vertical slice. It tests:
- Multi-faction internal conflict (Mecha hardliners vs gate guards, Beast negotiators vs raiders)
- Player as low-rank commander who cannot directly control high-rank units
- Resource point protection (CityGateCore)
- Mediation vs escalation branching outcomes
- EncounterSnapshot persistence for mid-mission save/load

## Phase Flow

```
NotStarted → Tension → Skirmish(Active) → MediationWindow → Resolved/Failed
```

| Phase | Trigger | Behavior |
|---|---|---|
| NotStarted | Player enters CityGateDispute trigger zone | Idle, waiting for trigger |
| Tension | `_triggerZone.MissionStarted` | Both sides face off; `_skirmishDelay` seconds before combat |
| Active (Skirmish) | Timer expires | BeastRaider waves spawn; extremists engage; mediation timer starts |
| Resolving | Outcome condition met | MissionService.CompleteMissionWithOutcome called |
| Completed | Outcome != FailedEscalation | EncounterRuntime.CompleteEncounter, trigger marked completed |
| Failed | Outcome == FailedEscalation | Same as Completed but with Failed phase |

## Objectives

1. **protect_core** — CityGateCore must survive (Combatant.IsAlive)
2. **protect_negotiator** — BeastNegotiator must survive
3. **defeat_raiders** — All BeastRaiders must be defeated/repelled
4. **keep_casualties_low** — Mecha and Beast casualties stay within mediation thresholds

## Branch Outcomes

| Outcome | Conditions | Faction Consequences | XP |
|---|---|---|---|
| **BalancedMediation** | Core alive, Negotiator alive, Raiders defeated, Mecha casualties ≤ 2, Beast casualties ≤ 4 | Both factions: trust +8, hostility -12 | 350 |
| **MechaSuppression** | Core alive, Raiders defeated, but Negotiator dead OR casualties too high | Mecha trust +10; Beast hostility +20, trust -10 | 250 |
| **BeastNegotiation** | Negotiator alive, Core alive, Beast casualties low, timer expired (raiders not all defeated) | Beast hostility -15, trust +5; Mecha trust -5, respect -5 | 250 |
| **FailedEscalation** | Core destroyed | Both factions: hostility +15, respect -10 | 30 |
| **PartialContainment** | Core saved but casualties exceed balanced threshold (within partial max) | Both factions: respect -3, trust -3 | 100 |

## Faction Consequences (Design)

- **BalancedMediation**: Mainstream hostility drops below 40 threshold (aligned with DynamicHostility design). Extremist/rogue units may still exist. This is the "best" outcome.
- **MechaSuppression**: Mecha hardliners feel vindicated; Beast hostility rises sharply. Future encounters have more beast waves.
- **BeastNegotiation**: Beast negotiator succeeds; Beast hostility drops. But Mecha support decreases (they feel sidelined).
- **FailedEscalation**: Both sides radicalized. Future wave pressure increases.
- **PartialContainment**: Status quo with minor trust erosion. Future encounters at medium intensity.

## AI Behavior Profiles

CityGateDispute uses `AIBehaviorProfileSO` to make the existing units behave differently without changing outcome logic:

| Unit | Profile | Behavior meaning |
|---|---|---|
| BeastRaider / BeastRaider waves | AggressiveRaider | Pressures CityGateCore / protected targets and chases farther. |
| MechaGateGuard | DefensiveGuard | Holds near protected objectives and avoids chasing too far. |
| BeastNegotiator | Negotiator | Non-combatant; does not initiate attacks and should be protected. |
| MechaHardliner | Hardliner | Escalation risk; can target protected/neutral negotiation targets. |
| CityLord / WarKing / MechaCaptain / BeastElite | CommanderUnit | Tactical-command capable high-rank units; DirectControl denial remains unchanged. |

Profiles only affect AI target choice, chase/defend tendencies, and HUD/debug readability. CityGate outcomes still depend on CityGateCore, BeastNegotiator, raider defeat, casualties, and timer conditions.

## Commander Control Restrictions

| Unit Type | Rank | DirectControl | TacticalCommand | SyncAssist | DefendObjective / FocusFire |
|---|---|---|---|---|---|
| MechaGateGuard (Minion) | 1 | Yes (if trust ≥ 30) | Yes | Yes | Can defend objectives / respond to FocusFire |
| MechaHardliner (Minion, rank 2) | 2 | No (AllowDirectControl=false) | Yes | Yes | Can be tactical-only ally or hostile focus target depending on setup |
| BeastNegotiator (Minion) | 1 | Yes (if trust ≥ 30, cross-race penalty) | Yes | Yes | Can be protected by DefendObjective |
| CityLord (CityLord) | 4 | No (hero/leader, requires level 35) | Yes (level 35+) | Yes | DirectControl remains denied |
| WarKing (WarKing) | 5 | No (hero/leader, requires level 45) | No | Yes | DirectControl remains denied |
| BeastRaider (Minion, hostile) | 1 | No (hostile) | No | No | Valid FocusFire target |

Denied reasons:
- "Cannot control hero/leader at this level" — CityLord/WarKing
- "Insufficient level, trust, or capacity" — rank/level/trust mismatch
- "Rank too high" — implicit via MaxDirectControlRank < CommandRank

## Commander Control Rules

- Low-rank units (`CommandRank <= commander.MaxDirectControlRank`, non-leader, `AllowDirectControl=true`, sufficient trust) are valid DirectControl targets.
- High-rank / leader units remain denied for DirectControl by design: CityLord, WarKing, MechaCaptain, BeastElite, and MechaHardliner when `AllowDirectControl=false`.
- Denial must be readable in CommanderDebugHud / CommanderControlHintPanel: `Leader unit`, `Rank too high`, `Trust too low`, `Commander level too low`, `Direct control disabled`, `PlayerDead`, or `No controllable target nearby`.

## CommanderAction Expansion

This pass adds two additive tactical commands on top of the existing DirectControl / TacticalCommand / SyncAssist model:

- **DefendObjective (`G`)** — orders a commandable low-rank ally to move to and hold around a mission objective such as Convoy, Energy Node, Allied Defense Point, CityGateCore, or BeastNegotiator. The ally engages hostile threats inside the defend leash and avoids chasing too far away from the protected target.
- **FocusFire (`F`)** — orders nearby commandable allies to attack the selected hostile target for a short duration. If the target dies or the duration expires, responders clear the forced target and resume default AI.

The actions are command helpers only. They do not change CityGate outcome calculation: protecting CityGateCore, keeping BeastNegotiator alive, defeating raiders, and keeping casualties low remain the objective source of truth.

HUD/feedback requirements:

- CommanderActionPresenter shows five descriptors: DirectControl, TacticalCommand, SyncAssist, DefendObjective, FocusFire.
- CommanderControlHintPanel and CommanderDebugHud show G/F hints, command status, responder count, and duration where applicable.
- Demo shortcut help lists `G: DefendObjective` and `F: FocusFire` while preserving `1/2/3/F7/F8/F5/F9/F10`.
- Logs use `[CommanderAction] DefendObjective issued/denied`, `[CommanderAction] FocusFire issued/denied`, `[AICommand] Defending objective`, and `[AICommand] FocusFire target`.

Known limitations: no squad formation, no final Canvas UI, no Mission 4/Boss/model replacement, and no combat-system rewrite. The implementation is intentionally single-command / nearby-responder focused for the prototype vertical slice.

## E Input Priority

When the player presses **E** in CommanderPrototype:

1. If a selected commander target exists, CommanderControl handles E first.
2. If no selected target exists, CommanderControl auto-acquires the nearest eligible low-rank DirectControl unit.
3. If no commander target is available, EnergyNode / mission interaction may consume E.
4. If nothing is available, the HUD shows `No controllable target nearby` and suggests Tab/Q or moving closer.

EnergyNode must not steal E while a selected commander target exists.

## DirectControl vs TacticalCommand vs SyncAssist

HUD surfaces show all three permission lanes for the selected target:

- `DirectControl: Allowed/Denied`
- `TacticalCommand: Allowed/Denied`
- `SyncAssist: Allowed/Denied`

If DirectControl is denied but TacticalCommand or SyncAssist is possible, the hint suggests trying that route instead of making the E press feel silent.

## High-rank denial feedback

Examples:

- `Target: WarKing` → `DirectControl: DENIED` → `Reason: Leader unit` → `Suggestion: Try Tactical Command or Sync Assist`
- `Target: MechaHardliner` → `Reason: Direct control disabled`
- `Target: BeastRaider_01` or `MechaGateGuard` → `DirectControl: ALLOWED` when trust/level permit.

## CityGate objective guidance

Mission 3 displays as `Mission 3: City Gate Dispute` and uses the OnGUI objective checklist:

- Protect CityGateCore
- Keep BeastNegotiator alive
- Defeat BeastRaiders
- Keep casualties low

Scene/world markers identify City Gate, CityGateCore, BeastNegotiator, BeastRaider spawn, and high-rank examples.

The playable demo polish pass keeps CityGate as Mission 3 and adds explicit scene/readability marker coverage:

- `City Gate Mission Area`
- `CityGateCore`
- `BeastNegotiator`
- `BeastRaider Spawn`
- `Low-Rank Ally: Press E to Control`
- `High-Rank Unit: Tactical Command Only`

After `F8`, the player should land close enough to see the CityGate markers and understand the next manual validation step.

## F8 Debug Teleport

- **F8** teleports the player near `CityGateDisputeTrigger` for demo setup.
- Log prefix: `[DEBUG TRIGGER]`.
- The teleport does not record a mission outcome and does not mutate `MissionChainState`.
- If no player controller is found, it warns once and does not throw.

## Manual E-control validation checklist

1. Press E with no target: auto-acquire a nearby low-rank unit or show `No controllable target nearby`.
2. Select CityLord/WarKing: E is denied with leader/high-rank reason.
3. Select MechaGateGuard or BeastRaider_01: E DirectControls when trust/level pass.
4. Stand near EnergyNode with selected target: selected target control/denial has priority.
5. Clear target near EnergyNode: mission interaction may proceed.

## Encounter Persistence

CityGateDispute uses EncounterRuntime for BeastRaider wave spawning:
- `encounterId = "city_gate_dispute"`
- Waves: `citygate_beast_raid_1`, `citygate_beast_raid_2`
- Snapshot fields: standard EncounterSnapshot (lifecycle only, no dynamic unit HP/position)
- F5 saves encounter snapshot; F9 restores lifecycle state
- Completed encounter does not respawn
- In-progress encounter sets `NeedsRestartAfterLoad=true`
- Dynamic unit HP/position not serialized — warning logged on restore

## MissionChain Integration

- `MissionChainOrder`: `["convoy_energy_conflict", "border_retaliation", "city_gate_dispute"]`
- Completing `border_retaliation` unlocks `city_gate_dispute`
- `BuildMissionModifiers("city_gate_dispute")` generates modifiers from `border_retaliation` outcome:
  - MechaVictory → `citygate_hardliner_pressure` (BeastHostility x1.3)
  - BeastVictory → `citygate_beast_emboldened` (BeastHostility x1.4)
  - BalancedResolution → `citygate_ceasefire_fragile` (ceasefire, hostility -10)
  - PartialSuccess/Failed → `citygate_low_trust` (low trust, hostility +8)
- `RecordMissionResult` blocks duplicate `city_gate_dispute` entries (allowDuplicate=true for debug)

## Debug Trigger

- **F7**: Test CityGateDispute BalancedMediation (not recorded to chain, log tagged `[DEBUG TRIGGER]`)
- **F8**: Teleport player near CityGateDispute area for manual demo validation (not recorded to chain)
- 1/2/3 keys remain unchanged for Mission 1/2 test triggers

## Framework Consolidation Notes

- CityGateDispute remains Mission 3; no Mission 4 or boss content is introduced by the demo-flow pass.
- `Assets/Data/Missions/CityGateDispute.asset` is the MissionDefinitionSO authoring source for display names, objectives, and possible outcome summaries.
- `DemoFlowManager` reads `MissionChainState` to recommend CityGate after `border_retaliation`; it does not write chain state.
- `DemoFlowHud` and `MissionObjectiveHud` are additive OnGUI readability panels. They do not replace runtime mission logic.
- `CommanderActionPresenter` is display-only and surfaces DirectControl / TacticalCommand / SyncAssist status without changing permission rules.
- See `Assets/Docs/DEMO_FLOW_DESIGN.md` for the full three-mission framework description.

## Manual Validation Checklist

1. Open CommanderPrototype scene
2. Walk to CityGateDispute area (50, 0, 0)
3. Trigger starts mission → Tension phase
4. Wait for Skirmish → BeastRaider waves spawn
5. Protect CityGateCore and BeastNegotiator
6. Defeat BeastRaiders → BalancedMediation outcome
7. Check CommanderDebugHud for control denial on CityLord/WarKing
8. F5 save mid-mission → F9 load → no duplicate waves
9. F10 clear → encounter resets
10. Verify `[DEBUG TRIGGER] F7` logs and does not pollute MissionChainState
