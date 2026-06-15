# Implementation Summary — Commander / Faction Politics / Mission Consequence Vertical Slice

## 1. Overview

This implementation extends the LuoLuoTrip Soul-like combat prototype into a playable vertical slice featuring:
- **Commander Authority System**: Low-level dual-race commander (机战王) with control permission logic
- **Faction Politics System**: Dynamic faction standing that changes with mission outcomes
- **Mission Consequence System**: Missions with outcomes that affect faction relationships and commander progression
- **Extended Save System**: Persists commander profile, faction politics, and mission history
- **Debug UI**: OnGUI panels for commander stats, faction standing, and mission results
- **Editor Setup**: New menu items and prototype scene for testing

All core logic is pure C# (no MonoBehaviour dependency) for EditMode testability.

## 2. New / Modified Files

### New Files — Commander (Assets/Scripts/Commander/)
- `ControlMode.cs` — Enum: Denied, SyncAssist, TacticalCommand, DirectControl
- `CommanderProfile.cs` — Commander state (level, XP, capacity, trust, sync rate)
- `CommanderLevelSystem.cs` — Level-based stat calculations (capacity, control rank, sync rate)
- `ControlPermissionRequest.cs` — Control request struct + CharacterControlInfo
- `ControlPermissionResult.cs` — Control result with mode, sync rate, reason
- `ControlPermissionService.cs` — Core permission evaluation logic
- `SyncRateCalculator.cs` — Sync rate calculation (level, rank, trust, cross-race penalty)

### New Files — Faction Politics (Assets/Scripts/Faction/Politics/)
- `FactionStanding.cs` — Per-faction dynamic standing (Trust, Respect, Fear, Hostility, ResourcePressure, WarExhaustion)
- `FactionStandingDelta.cs` — Change descriptor for applying deltas
- `FactionPoliticsState.cs` — State container with snapshot/restore
- `FactionReputationService.cs` — API: Initialize, GetStanding, ApplyDelta, IsHostile, GetMainRaceTrust
- `FactionConsequenceApplier.cs` — Applies MissionConsequence to FactionPoliticsState
- `DynamicFactionHostilityService.cs` — Combines dynamic standing with static faction matrix for hostility checks

### New Files — Mission (Assets/Scripts/Mission/)
- `MissionOutcomeType.cs` — Enum: MechaVictory, BeastVictory, BalancedResolution, Failed, PartialSuccess
- `MissionObjective.cs` — Objective with progress tracking
- `MissionRuntimeState.cs` — Mission runtime state with outcome determination
- `MissionConsequence.cs` — Consequence result (XP delta, faction deltas, summary)
- `MissionDefinitionSO.cs` — ScriptableObject mission definition
- `MissionConsequenceResolver.cs` — Resolves outcomes to consequences (core logic)
- `MissionService.cs` — Mission lifecycle management (start, update, complete, apply)

### New Files — UI (Assets/Scripts/UI/)
- `CommanderDebugHud.cs` — OnGUI display for commander stats
- `FactionStandingDebugPanel.cs` — OnGUI display for faction standings
- `MissionResultDebugPanel.cs` — OnGUI display for mission results

### New Files — Editor
- `PlaceholderAssetGenerator.cs` — Generates PH_ prefabs and MAT_PH_ materials under `Assets/Art/Placeholders/`

### New Files — Docs
- `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md` — Asset replacement workflow documentation

### New Files — Game
- `CommanderPrototypeRuntime.cs` — MonoBehaviour connecting HUDs to context, test mission triggers (keys 1/2/3)

### New Files — Tests
- `Assets/Tests/EditMode/CommanderAuthorityTests.cs` — 9 tests
- `Assets/Tests/EditMode/FactionPoliticsTests.cs` — 6 tests
- `Assets/Tests/EditMode/MissionConsequenceResolverTests.cs` — 7 tests
- `Assets/Tests/EditMode/SaveDataCommanderFactionTests.cs` — 4 tests
- `Assets/Tests/PlayMode/DynamicHostilityFlowTests.cs` — 2 tests

### Modified Files
- `Assets/Scripts/Character/CharacterData.cs` — Added CommandRank, RequiredCommanderLevel, TrustToPlayer, IsHeroOrLeader, AllowDirectControl, AllowTacticalCommand with role-based defaults
- `Assets/Scripts/Game/LuoLuoTripGameContext.cs` — Added ReputationService, CommanderProfile, MissionService; updated InitializeWorld/ApplySave/ExportSave
- `Assets/Scripts/Save/GameSaveData.cs` — Added CommanderSaveEntry, FactionPoliticsSnapshot, MissionConsequenceSaveEntry; bumped version to 2; added new CharacterSaveEntry fields
- `Assets/Scripts/Save/SaveService.cs` — Save/restore commander + faction politics + extended character fields
- `Assets/Scripts/Editor/LuoLuoTripSetupMenu.cs` — Added 3 new menu items, updated PrintWorldSummary

## 3. Commander System

**Core principle**: Low-level commanders have limited control authority.

- Lv.1: DirectControl rank 1 only, capacity 2, base sync 20%
- Lv.5: Capacity 4, base sync 35%
- Lv.10: DirectControl rank 2, capacity 6, base sync 50%
- Lv.20: DirectControl rank 3, capacity 10, TacticalCommand rank 4, base sync 65%
- Lv.35: DirectControl rank 4, capacity 15, TacticalCommand rank 5, base sync 80%
- Lv.45: DirectControl rank 5, capacity 20, TacticalCommand rank 5, base sync 95%

**Permission evaluation** checks: commander level vs required level, control rank vs max rank, faction trust threshold, command capacity, cross-race penalty, hero/leader restriction.

**Sync rate** is calculated from: base sync rate, rank difference penalty, trust bonus, cross-race penalty, hero/leader penalty. Clamped to [0, 1].

## 4. Faction Politics System

Each sub-faction has a dynamic `FactionStanding` with 6 attributes (range -100 to 100):
- **Trust**: Willingness to cooperate with the player
- **Respect**: Perception of player's power
- **Fear**: Fear of the player
- **Hostility**: Active hostility (>=40 triggers hostile behavior)
- **ResourcePressure**: Economic/resource strain
- **WarExhaustion**: War fatigue from casualties

The system is **additive** to the existing `FactionRelationshipMatrix` / `FactionRelationshipService`. The original faction matrix handles faction-to-faction static relationships; the new politics system handles faction-to-player dynamic relationships.

`DynamicFactionHostilityService` bridges both systems for combined hostility checks.

## 5. Mission Consequence System

**Outcome types**: MechaVictory, BeastVictory, BalancedResolution, PartialSuccess, Failed

**Consequence resolution rules**:
- MechaVictory: Motor tribes +Trust/Respect, Beast tribes +Hostility
- BeastVictory: Beast tribes +Trust/Respect, Motor tribes +Hostility
- BalancedResolution: All tribes +Trust/+Respect/-Hostility (moderate)
- PartialSuccess: All tribes -Respect/-Trust
- Failed (retreat): All tribes -Respect/-Trust
- High casualties: +WarExhaustion for affected race factions
- BalancedResolution gives most XP (300), victory gives 200, failure gives 30

## 6. Save Extension

- `CommanderSaveEntry`: All CommanderProfile fields
- `FactionPoliticsSnapshot`: All faction standings
- `MissionConsequenceSaveEntry`: Completed mission results
- `CharacterSaveEntry` extended with: commandRank, requiredCommanderLevel, trustToPlayer, isHeroOrLeader, allowDirectControl, allowTacticalCommand
- Save version bumped to 2; old saves (v1) load with default values for new fields

## 7. UI / Editor Setup

**Debug UI** (OnGUI, no prefabs):
- `CommanderDebugHud`: Level, XP, capacity, control ranks, sync rate, last control result
- `FactionStandingDebugPanel`: All 9 sub-faction standings
- `MissionResultDebugPanel`: Outcome, XP, faction deltas, summary (auto-fades after 10s)

**Editor menu items**:
- `LuoLuoTrip/Setup/Create Commander Prototype Data` — ConvoyEscort mission SO
- `LuoLuoTrip/Setup/Create Mission Prototype Data` — EnergyRaid + BalanceAllocation mission SOs
- `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene` — Full prototype scene with:
  - Player (Lv.1 MotorIronRiders Common)
  - Mecha minion (rank 1, controllable)
  - Beast minion (rank 1, hostile)
  - City Lord (rank 4, Denied/SyncAssist expected)
  - War King (rank 5, Denied expected)
  - GameBootstrap + SaveLoadManager + CommanderPrototypeRuntime
  - All 3 debug HUDs

**Runtime test keys** (in CommanderPrototype scene):
- Key 1: Trigger MechaVictory mission
- Key 2: Trigger BeastVictory mission
- Key 3: Trigger BalancedResolution mission

## 8. Tests

### EditMode Tests
| File | Count | Coverage |
|------|-------|----------|
| CommanderAuthorityTests.cs | 9 | Lv1 direct control, high-rank denial, trust-based degrade, capacity limit, sync rate clamp, profile creation, level-up, level system, cross-race penalty |
| FactionPoliticsTests.cs | 6 | All factions initialized, delta application, value clamping, MainRace trust average, hostility threshold, snapshot/restore |
| MissionConsequenceResolverTests.cs | 7 | MechaVictory trust/hostility, BeastVictory trust/hostility, BalancedResolution hostility reduction, high casualties war exhaustion, retreat respect loss, BalancedResolution XP, summary text |
| SaveDataCommanderFactionTests.cs | 4 | New field defaults, CommanderProfile write/restore, FactionPoliticsState write/restore, old version compatibility |

### PlayMode Tests
| File | Count | Coverage |
|------|-------|----------|
| DynamicHostilityFlowTests.cs | 2 | Dynamic hostility change after consequence, full mission integration flow |

## 9. Manual Verification in Unity

1. Open Unity 2022.3.62f3 LTS
2. Run menu: `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene`
3. Open `Assets/Scenes/CommanderPrototype.unity`
4. Play mode:
   - WASD move, Left-click attack, Space dodge, Q lock-on
   - Press 1/2/3 to trigger test missions and see faction standing changes
   - F5 quick-save, F9 quick-load to verify persistence
5. Run EditMode tests in Test Runner window
6. Run PlayMode tests in Test Runner window

## 10. Known Limitations and Future Work

1. **No actual mission gameplay loop** — Missions are resolved instantly via test keys. Real mission gameplay (objectives, combat encounters) needs scene/level design.
2. **Control permission is evaluated but not enforced** — CombatController still allows direct control of any unit. A ControlMode check gate needs to be added to CombatController.
3. **Dynamic hostility not integrated into CharacterEntity.IsHostileTo()** — DynamicFactionHostilityService exists but is not yet wired into the existing hostility resolution chain. This preserves backward compatibility but means NPC hostility doesn't yet change dynamically.
4. **CommanderPrototypeRuntime uses FindObjectsOfType** — Not optimal for production; should use a service locator or injection pattern.
5. **No UI for selecting control targets** — The debug HUD shows info but doesn't allow selecting which unit to control.
6. **MissionDefinitionSO outcome consequence mappings** are defined but not used by MissionConsequenceResolver (which uses hardcoded rules). The SO mappings are for future expansion.
7. **BalanceScore** on CommanderProfile is tracked but not yet used in any gameplay logic.
8. **F5/F9 quick-save/load** has not been tested with the new save format (requires Unity runtime).
9. **No animation/art** for commander-specific actions (sync assist, tactical commands).
10. **Tests cannot be run from CLI** — Unity Test Runner only, as per AGENTS.md.

## 11. Placeholder Asset System

A placeholder asset pipeline has been implemented following the Asset Replacement Guide:

- **PlaceholderAssetGenerator** (Editor script): Generates 8 placeholder prefabs (`PH_*.prefab`) with proper hierarchy (Visual/Collision/Marker), plus 7 color-coded materials (`MAT_PH_*.mat`)
- **Prefab hierarchy**: PrefabRoot contains CharacterEntity + gameplay components; Visual child contains cylinder mesh; Collision child contains colliders; Marker child for debug labels
- **Scene creation uses prefabs**: Both CombatPrototype and CommanderPrototype scenes now instantiate placeholder prefabs instead of raw primitives, with fallback to primitives if prefabs aren't generated
- **ASSET_REPLACEMENT_GUIDE.md**: Documents the replacement workflow, hierarchy conventions, verification checklist, and common mistakes
- **Menu item**: `LuoLuoTrip/Setup/Generate Placeholder Assets` generates all placeholder assets

## 12. Next Steps Roadmap

See **AGENTS.md → Roadmap — Next Steps** for the full prioritized plan. Summary:

1. ✅ Unity 编译验证 (代码审查通过，待 Unity 内实际编译)
2. 接入 ControlPermissionService 到 CombatController 实际控制流程
3. 接入 DynamicFactionHostilityService 到 AI 敌我判断
4. 实现真实 ConvoyEnergyConflict 小任务 (替代按键 1/2/3 直接结算)
5. ✅ 生成圆柱体 Placeholder Prefab Library
6. ✅ 维护 ASSET_REPLACEMENT_GUIDE.md
7. 逐步换模型、动画和技能表现
