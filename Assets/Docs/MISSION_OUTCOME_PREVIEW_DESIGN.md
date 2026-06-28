# Mission Outcome Preview Design

Date: 2026-06-29  
Unity: 2022.3.62f3 LTS

## Goal

Mission Outcome Preview makes the existing three-mission vertical slice more readable without adding Mission 4, Boss content, formal Canvas UI, model replacement, or combat-system rewrites. During the mission, the debug HUD can show:

- likely outcome if the mission completed now,
- top risk factors causing that outcome,
- commander XP and faction consequence preview,
- previous mission outcome effects on the current mission,
- suggested commander actions such as `G` DefendObjective and `F` FocusFire.

The feature is a visibility layer. It does not complete missions, write mission-chain history, apply faction deltas, or award XP.

## Preview vs final outcome boundary

`MissionOutcomePreviewService` builds preview data from live runtime state, mission-chain state, and synthetic/cloned `MissionRuntimeState` objects. It reuses `MissionConsequenceResolver.Resolve(...)` to estimate faction deltas and commander XP, but it never calls mission completion APIs or `MissionChainService.RecordMissionResult`.

Final settlement remains owned by mission runtimes and `MissionService` / `MissionChainService`. `MissionResultSummaryPanel` and the preview HUD share `MissionOutcomeTextLibrary` so fallback outcome summaries do not drift.

## Data model

- `MissionOutcomePreview` — likely outcome, confidence, summaries, XP preview, previous effect text, next hint, risks, consequences, and high-level flags.
- `MissionOutcomeRisk` — risk id, label, severity, current/threshold values, suggestion, and critical flag.
- `MissionConsequencePreview` — faction target and compact delta text derived from `FactionStandingDelta`.

All three are runtime-safe and have no `UnityEditor` dependencies.

## CityGateDispute risk model

CityGate is the deepest preview path and reuses `CityGateDisputeRuntime.ResolveOutcome(...)`.

Likely outcomes:

| Outcome | Preview trigger |
|---|---|
| `BalancedMediation` | core alive, negotiator alive, raiders defeated, casualties within balanced thresholds |
| `MechaSuppression` | core alive and raiders defeated, but negotiator dead or casualties too high |
| `BeastNegotiation` | core/negotiator alive, raiders not defeated yet, beast casualties within threshold |
| `FailedEscalation` | CityGateCore destroyed |
| `PartialContainment` | core saved but optional objectives/casualty thresholds are damaged |

Risks include:

- CityGateCore HP low/destroyed,
- BeastNegotiator HP low/dead,
- Mecha casualties high,
- Beast casualties high,
- Hardliner escalation risk from high total casualties,
- Raider pressure high when raiders remain active.

Suggestions intentionally reference current prototype actions:

- `Use G DefendObjective on CityGateCore.`
- `Use F FocusFire on BeastRaider.`
- `FocusFire Hardliner or use TacticalCommand if available.`
- `Protect Negotiator; command Guard to defend.`

## Convoy / Border preview model

Convoy preview reads current convoy/energy/encounter state when present and falls back to active mission state when only service data exists.

It surfaces:

- convoy HP low,
- EnergyNode contested,
- casualties high,
- raider pressure high,
- suggestions to defend the convoy, share energy, and focus raiders.

Border preview reads `BorderRetaliationRuntime.CurrentModifier`, encounter casualties, and raid clearance when present. It surfaces:

- allied casualties high,
- raider wave not cleared,
- low-trust/retreat pressure,
- suggestions to defend the allied defense point, focus the raider leader, and keep casualties low.

Both previews are conservative estimates, not replacements for final mission outcome logic.

## Previous outcome effect visibility

`BuildPreviousOutcomeEffectText(...)` translates mission-chain history into compact HUD text:

- no prior result: `No previous outcome modifier.`
- Convoy `MechaVictory`: Beast retaliation intensified.
- Convoy `BalancedResolution`: Border hostility reduced.
- Border `BalancedResolution`: CityGate mainstream hostility reduced.
- Border `Failed` / `PartialSuccess`: CityGate tension increased.
- completed CityGate: future stability hint.

The service reads chain state only. It does not alter `MissionChainService` core logic.

## HUD design

`MissionOutcomePreviewHud` is an additive OnGUI/debug panel. It uses `DebugUILayout.MissionOutcomePreview`, refreshes at a low interval, caches display lines, and limits visible risks to the top three.

Displayed content:

- likely outcome,
- confidence,
- effect and consequence preview,
- commander XP preview,
- previous outcome effect,
- top risks and suggestions,
- compact faction consequence lines,
- next mission hint,
- explicit `Preview only — no XP or chain writes.` note.

The panel intentionally remains debug-style and does not introduce Canvas UI.

## Relationship to DefendObjective / FocusFire / AI profiles

The preview does not command units by itself. It only explains which existing commands are relevant to current risks:

- CityGateCore or allied defense risk → `G` DefendObjective.
- Raider pressure → `F` FocusFire.
- Negotiator danger → protect/defend with guard-like units.
- Hardliner escalation → FocusFire Hardliner or use tactical command where allowed.

AI behavior profiles remain tuning/readability support. Outcome calculation still depends on mission objectives, casualties, protected-target survival, and runtime timers.

## Known limitations

- Preview confidence is best-effort and intentionally conservative.
- Convoy/Border estimates are less detailed than CityGate because their runtime resolution has fewer dedicated static preview helpers.
- Dynamic spawned-unit HP/position remains outside save serialization, matching existing encounter persistence limitations.
- Manual validation is still required for visual layout and live feel.
