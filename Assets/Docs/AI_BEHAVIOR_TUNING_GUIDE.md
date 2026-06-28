# AI Behavior Tuning Guide

Date: 2026-06-28  
Unity: 2022.3.62f3 LTS

## Goal

This pass keeps the current lightweight `AIBehaviorProfileSO` + `SimpleCombatAI` architecture and makes CityGateDispute behavior more visible, tunable, and testable. It does not add a behavior tree, third-party AI framework, Mission 4, boss content, final models, or Canvas UI.

## Profile tuning goals

| Profile | CityGate role | Tuning goal |
|---|---|---|
| `AggressiveRaider` | BeastRaider units / waves | Prefer `CityGateCore`, convoy/core-like objectives, and protected/negotiator-like targets over generic hostiles. Chase farther than guards, but stop after home/max-engage limits. |
| `DefensiveGuard` | MechaGateGuard / allied defenders | Hold near objective, respond strongly to `DefendObjective`, engage hostile units inside defend radius, and return instead of chasing across the map. |
| `Negotiator` | BeastNegotiator | Non-combatant. Does not initiate attacks or respond as a FocusFire responder. Retreats or creates distance when hostile threats enter the trigger radius. |
| `Hardliner` | MechaHardliner | Creates escalation risk by preferring protected/neutral/negotiator-like targets, but uses bounded chase/engage windows so the player has time to intervene. |
| `CommanderUnit` | CityLord / WarKing / MechaCaptain / BeastElite | Stable tactical-command unit. Can respond to tactical commands where permission rules allow, but DirectControl denial remains owned by character rank/leader permission logic. |
| `NeutralCivilian` | Future non-combatants | Non-combatant retreat profile for future scenario use. |

## Key tuning fields

- `objectivePressureWeight`: attraction to objective-like targets such as `CityGateCore`, convoy, energy, or protected markers.
- `protectedTargetPressureWeight`: attraction to the assigned `protectedTarget` or protected objective-like units.
- `neutralTargetPressureWeight`: escalation pressure for profiles allowed to attack neutral targets.
- `hostileUnitWeight`: generic hostile-unit preference.
- `homeDistancePenalty`: score penalty for drifting away from home/anchor.
- `retreatDistance`: distance a non-combatant tries to move away from a threat.
- `retreatTriggerRadius`: threat radius that triggers retreat checks.
- `guardLeashRadius`: maximum defend-objective leash before returning.
- `guardReturnSpeedMultiplier`: return-to-anchor movement multiplier for guard behavior.
- `hardlinerEscalationBias`: additional score for hardliner escalation targets.
- `negotiatorThreatAvoidanceBias`: non-combatant avoidance tuning for negotiator-like units.
- `maxEngageDuration`: upper bound before disengaging and returning home.
- `decisionRefreshInterval`: profile-specific target refresh cadence.

## CityGateDispute expected behavior

1. Press `F8` to jump to CityGate.
2. `BeastRaider` should move pressure toward `CityGateCore` or protected targets rather than wandering after any nearby unit forever.
3. `MechaGateGuard` should hold near `CityGateCore`; `G DefendObjective` should make this more obvious.
4. `BeastNegotiator` should not attack first and should show non-combatant/protect guidance in HUD.
5. `MechaHardliner` should report or show escalation-risk behavior around protected/negotiator targets without instantly deleting the negotiator.
6. `CityLord`, `WarKing`, `MechaCaptain`, and `BeastElite` remain high-rank/tactical-only readable units.

## CommanderAction interaction

- `G DefendObjective` works best with `DefensiveGuard` and `CommanderUnit` responders.
- `F FocusFire` should be used on raiders or hardliners to suppress pressure.
- `Negotiator` and `NeutralCivilian` profiles do not become FocusFire responders.
- `AggressiveRaider` and `Hardliner` can be valid FocusFire targets, but their profile does not make them player-commandable.
- `CommanderUnit` profile readability does not bypass DirectControl permission checks.

## Observability

`AIBehaviorScenarioMonitor` provides lightweight summaries for CityGate manual validation and PlayMode tests:

- profile label
- last decision
- current target
- distance from home
- retreat/defend/objective/focus-responder flags

`CommanderControlHintPanel` and `CommanderDebugHud` show compact selected-target text:

- `AI Profile: ...`
- `Behavior: ...`
- `Responds: Tactical Yes/No | Defend Yes/No | Focus Yes/No`
- profile-specific suggestion

## Why not a behavior tree now?

The current vertical slice needs readable differentiation, not a new AI framework. Score weights, guard leash, retreat trigger, and bounded engage duration are enough to demonstrate the CityGate roles while keeping tests deterministic and preserving existing combat/commander/save-load systems.

## Known limitations

- No squad tactics or formation behavior.
- No final Canvas UI; profile readability remains OnGUI/debug HUD based.
- Spawned dynamic unit HP/position persistence remains limited by the existing `EncounterSnapshot` lifecycle model.
- Data and Resources copies of AI profiles can drift if edited independently; setup preserves authored tuning instead of forcibly syncing values.
