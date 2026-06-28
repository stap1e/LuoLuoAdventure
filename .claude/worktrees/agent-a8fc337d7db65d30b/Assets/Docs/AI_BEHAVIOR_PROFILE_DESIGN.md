# AI Behavior Profile Design

Date: 2026-06-27  
Unity: 2022.3.62f3 LTS

## Why AIBehaviorProfileSO exists

`AIBehaviorProfileSO` is a lightweight, data-driven behavior layer for the existing `SimpleCombatAI`. It makes units in the current three-mission vertical slice feel different without adding Mission 4, a boss, final models, Canvas UI, or a combat-system rewrite.

The profile is additive:

- null profile keeps the existing default AI path;
- combat math, damage windows, movement fallback, and commander permission rules remain owned by existing systems;
- mission outcomes still come from mission runtime objective state, not from profile state.

## Default profiles

| Profile | Role | Core behavior |
|---|---|---|
| `AggressiveRaider` | Beast raiders / attack waves | Initiates combat, prefers objectives/protected targets, chases farther from home, does not easily respond to player FocusFire/DefendObjective commands. |
| `DefensiveGuard` | Gate guards / low-rank allies | Initiates combat defensively, holds when idle, responds to DefendObjective, and limits chase distance from home/protected point. |
| `Negotiator` | BeastNegotiator / non-combatant story unit | Cannot initiate combat, ignores FocusFire, can retreat at higher health thresholds, should be protected. |
| `Hardliner` | MechaHardliner / escalation unit | Initiates combat, prefers protected/negotiator-like targets, can attack neutral protected targets to create escalation risk. |
| `CommanderUnit` | CityLord / WarKing / MechaCaptain / BeastElite | Responds to TacticalCommand / DefendObjective / FocusFire, but DirectControl denial remains governed by commander permission rules. |
| `NeutralCivilian` | Future non-combatants | Cannot initiate combat or attack neutral units; can retreat. |

## CityGateDispute assignment

- `BeastRaider_01` and CityGate BeastRaider waves use `AggressiveRaider`.
- `MechaGateGuard` uses `DefensiveGuard` and protects CityGateCore.
- `BeastNegotiator` uses `Negotiator` and should not proactively attack.
- `MechaHardliner` uses `Hardliner` and targets the negotiator/protected targets as an escalation risk.
- `CityLord_HighRank`, `WarKing_HighRank`, `MechaCaptain_Rank2`, and `BeastElite_Rank2` use `CommanderUnit`.
- `CityGateCore_Objective` remains a protected objective with `Combatant`; it does not receive active AI behavior.

## CommanderAction relationship

Profiles are compatibility gates and display hints; they do not grant authority by themselves.

- `DefensiveGuard` and `CommanderUnit` respond to `DefendObjective`.
- `CommanderUnit` responds to `TacticalCommand` and `FocusFire` where existing permission logic allows.
- `Negotiator` and `NeutralCivilian` do not respond to `FocusFire` and cannot initiate attacks.
- `AggressiveRaider` is not made player-commandable by its profile; existing faction/trust/permission checks still decide whether any command can apply.
- High-rank units keep DirectControl denial through `ControlPermissionService` / character data.

## Mission outcome relationship

AI profiles influence moment-to-moment target choice and readable behavior. They do not directly complete or fail objectives.

CityGateDispute still resolves through:

- CityGateCore alive/dead;
- BeastNegotiator alive/dead;
- BeastRaider defeat state;
- Mecha/Beast casualty thresholds;
- mission timer / mediation window.

## Save/load relationship

Profiles are scene/asset references and are not a new dynamic save payload. Dynamic spawned-unit HP, position, and detailed AI state remain outside save serialization, matching `EncounterSnapshot` lifecycle limitations.

## Authoring persistence

The six default profile assets are authored data and must be committed with their `.meta` files in both locations:

- `Assets/Data/AIProfiles/*.asset` — authoring copies used by setup, scene assignment, validation, and tests.
- `Assets/Resources/AIProfiles/*.asset` — runtime-loadable copies used by `AIBehaviorProfileSO.LoadDefault()`.

`LuoLuoTrip/Setup/Create AI Behavior Profiles` is idempotent: it creates missing assets and backfills empty identity/readability fields, but it does not reset existing numeric/boolean tuning. See `Assets/Docs/AUTHORING_ASSET_PERSISTENCE.md` for the git/asset policy.

## Known limitations

- No squad formations or group tactics.
- No final Canvas UI; readability remains OnGUI/debug HUD based.
- Target scoring is intentionally lightweight and conservative.
- Runtime fallback profiles can be created if Resources assets are missing, but the setup menu and validator are expected to create/check the authored assets.
