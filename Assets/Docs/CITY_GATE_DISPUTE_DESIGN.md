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

## Commander Control Restrictions

| Unit Type | Rank | DirectControl | TacticalCommand | SyncAssist |
|---|---|---|---|---|
| MechaGateGuard (Minion) | 1 | Yes (if trust ≥ 30) | Yes | Yes |
| MechaHardliner (Minion, rank 2) | 2 | No (AllowDirectControl=false) | Yes | Yes |
| BeastNegotiator (Minion) | 1 | Yes (if trust ≥ 30, cross-race penalty) | Yes | Yes |
| CityLord (CityLord) | 4 | No (hero/leader, requires level 35) | Yes (level 35+) | Yes |
| WarKing (WarKing) | 5 | No (hero/leader, requires level 45) | No | Yes |
| BeastRaider (Minion, hostile) | 1 | No (hostile) | No | No |

Denied reasons:
- "Cannot control hero/leader at this level" — CityLord/WarKing
- "Insufficient level, trust, or capacity" — rank/level/trust mismatch
- "Rank too high" — implicit via MaxDirectControlRank < CommandRank

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
- 1/2/3 keys remain unchanged for Mission 1/2 test triggers

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
