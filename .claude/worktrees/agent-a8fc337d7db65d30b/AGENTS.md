# AGENTS.md 鈥?LuoLuoAdventure

Unity combat prototype (Soul-like) using **Unity 2022.3.62f3 LTS**. C# 9.0, .NET Framework 4.7.1, NetStandard 2.1. Root namespace: `LuoLuoTrip`.

Build target: **StandaloneWindows64**.

## Assembly structure

| Assembly | Location | Notes |
|---|---|---|
| `LuoLuoTrip` | `Assets/Scripts/LuoLuoTrip.asmdef` | Runtime code, all platforms |
| `LuoLuoTrip.Editor` | `Assets/Scripts/Editor/LuoLuoTrip.Editor.asmdef` | Editor-only, references LuoLuoTrip |
| `LuoLuoTrip.Tests.EditMode` | `Assets/Tests/EditMode/` | NUnit Edit-mode tests |
| `LuoLuoTrip.Tests.PlayMode` | `Assets/Tests/PlayMode/` | NUnit Play-mode tests (`[UnityTest]`) |

New runtime scripts go under `Assets/Scripts/{domain}/`. New editor scripts go under `Assets/Scripts/Editor/`. Do **not** mix Editor APIs into the runtime assembly.

## Code architecture

Domains under `Assets/Scripts/`:

- **Core** 鈥?enums/value types: `SubFactionId`, `CharacterRole`, `MainRace`, `RelationshipStance`, `GameConstants`
- **Character** 鈥?`CharacterData` (with CommandRank/Trust fields), `CharacterEntity` (binds `CharacterData`), `CharacterInitializer`, `CharacterLevelSystem`
- **Combat** 鈥?`Combatant` (HP/ST/Poise state machine), `CombatController` (player input), `SimpleCombatAI`, `DamageCalculator`, `CombatStats`/`CombatStatsCalculator`
  - **Combat/Animation** 鈥?`ICombatAnimator`, `AnimatorCombatBridge`, `ProceduralCombatAnimator`, `CombatAnimationDriver`, `CombatAnimatorConfigSO`
  - **Combat/Feedback** 鈥?`HitStopService`, `HitFeedbackProfileSO`, `CombatHitFeedbackHub`, `CameraShakeService`
- **AI** 鈥?`NavigationAgentBridge` (NavMesh/fallback movement), `NavigationMoveRequest`, `AICombatNavigationController`
- **Commander** 鈥?`ControlMode`, `CommanderProfile`, `CommanderLevelSystem`, `ControlPermissionRequest`, `ControlPermissionResult`, `ControlPermissionService`, `SyncRateCalculator`, `CommanderCommandType`, `CommanderControlRuntimeState`, `CommanderTargetSelector`, `CommanderControlController`
- **Encounter** 鈥?`EncounterRuntime`, `EncounterDefinition`, `EncounterUnitHandle`, `EncounterWave`, `EncounterSpawnPoint`
- **Faction** 鈥?`SubFactionConfigSO`, `SubFactionDatabaseSO`, `SubFactionRegistry`, `FactionRelationshipMatrix`, `FactionRelationshipService`
  - **Faction/Politics** 鈥?`FactionStanding`, `FactionStandingDelta`, `FactionPoliticsState`, `FactionReputationService`, `FactionConsequenceApplier`, `DynamicFactionHostilityService`
- **Mission** 鈥?`MissionOutcomeType`, `MissionObjective`, `MissionRuntimeState`, `MissionConsequence`, `MissionDefinitionSO`, `MissionConsequenceResolver`, `MissionService`
  - **Mission/Runtime** 鈥?`MissionTriggerZone`, `ConvoyObjective`, `EnergyNodeObjective`, `ConvoyEnergyConflictRuntime`, `BorderRetaliationRuntime`, `MissionObjectiveHud`, `MissionAreaRuntime`, `MissionBoundary`, `RetreatTracker`
- **Save** 鈥?`SaveLoadManager` (MonoBehaviour), `SaveService` (static I/O), `GameSaveData` (with CommanderSaveEntry, FactionPoliticsSnapshot)
- **Game** 鈥?`GameBootstrap` (entry point MonoBehaviour), `GameConfig`, `LuoLuoTripGameContext`, `CommanderPrototypeRuntime`
- **UI** 鈥?`CommanderDebugHud`, `FactionStandingDebugPanel`, `MissionResultDebugPanel`

Entry point: `GameBootstrap.Awake()` 鈫?loads `SubFactionDatabase` from Resources 鈫?creates `LuoLuoTripGameContext` (includes CommanderProfile, ReputationService, MissionService) 鈫?initializes world or applies save.

## Required setup before first play

All via Unity top menu **LuoLuoTrip/Setup/** (in order):

1. `Generate All Sub Faction Configs` 鈥?creates `Assets/Data/Factions/*.asset` + `Assets/Resources/SubFactionDatabase.asset`
2. `Create Hit Feedback Profile` 鈥?`Assets/Data/HitFeedbackProfile.asset`
3. `Create Combat Animator Config` 鈥?`Assets/Data/Animation/CombatAnimatorConfig.asset`
4. `Create Game Config Asset` 鈥?`Assets/Data/GameConfig.asset`
5. `Create Combat Prototype Scene` 鈥?does all of the above + creates `Assets/Scenes/CombatPrototype.unity` with player/enemy/hud
6. `Create Commander Prototype Data` 鈥?creates `Assets/Data/Missions/ConvoyEscort.asset`
7. `Create Mission Prototype Data` 鈥?creates `Assets/Data/Missions/EnergyRaid.asset` + `BalanceAllocation.asset`
8. `Create Commander Mission Prototype Scene` 鈥?does all of the above + creates `Assets/Scenes/CommanderPrototype.unity` with commander/faction/mission debug objects
9. `Generate Placeholder Assets` 鈥?creates `Assets/Art/Placeholders/Prefabs/PH_*.prefab` + `Assets/Art/Placeholders/Materials/MAT_PH_*.mat`

Additional menu items:
- `Create Bootstrap Scene` 鈥?creates `Assets/Scenes/Bootstrap.unity` with GameBootstrap + SaveLoadManager
- `LuoLuoTrip/Debug/Print World Summary` 鈥?logs faction states and commander info to Console
- `LuoLuoTrip/Tools/Compatibility/Run Project Compatibility Check` 鈥?audits Unity version, packages, asmdef, missing scripts, prefab hierarchy, orphaned .meta files
- `LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation` 鈥?validates scene components, prefabs, runtime API separation, FindObjectsOfType usage

If the `LuoLuoTrip` menu is missing, check Console for compile errors and confirm Unity version.

## Runtime controls

### CombatPrototype scene
WASD move, Left-click attack, Space dodge, Q lock-on, Tab switch target, F5 quick-save, F9 quick-load.

### CommanderPrototype scene
Same combat controls, plus:
- Tab: Select next target in range
- E: Attempt to control/command/assist selected target
- R: Release control back to original player unit
- Key 1: Trigger MechaVictory test mission
- Key 2: Trigger BeastVictory test mission
- Key 3: Trigger BalancedResolution test mission

## Testing

Tests run inside Unity via the Test Runner window (Window > General > Test Runner).

- **Edit-mode**: `CombatMathTests.cs`, `FactionRelationshipMatrixTests.cs`, `CommanderAuthorityTests.cs`, `FactionPoliticsTests.cs`, `MissionConsequenceResolverTests.cs`, `SaveDataCommanderFactionTests.cs`, `CommanderControlRuntimeStateTests.cs`, `DynamicHostilityResolverTests.cs`, `ConvoyEnergyConflictLogicTests.cs` 鈥?pure logic tests, no Play mode needed
- **Play-mode**: `SimpleCombatAITests.cs`, `DynamicHostilityFlowTests.cs`, `CommanderControlIntegrationTests.cs`, `DynamicHostilityIntegrationTests.cs`, `MissionGameplayFlowTests.cs` 鈥?uses `[UnityTest]` + coroutines, requires Play mode

Edit-mode tests manually call `Combatant.Tick(deltaTime)` with `AutoTickEnabled = false` for deterministic timing. Play-mode tests set `CharacterEntity.HostilityResolver` to control faction hostility.

There is no CLI test runner configured. Tests must be run from the Unity Editor.

## Generated / ScriptableObject assets

The following are **generated** by editor menu items, not hand-authored:

- `Assets/Data/Factions/*.asset` 鈥?`SubFactionConfigSO` per `SubFactionId` enum value
- `Assets/Data/Factions/SubFactionDatabase.asset`
- `Assets/Resources/SubFactionDatabase.asset` 鈥?copy of the above for `Resources.Load`
- `Assets/Data/HitFeedbackProfile.asset`
- `Assets/Data/Animation/CombatAnimatorConfig.asset`
- `Assets/Data/GameConfig.asset`
- `Assets/Data/Missions/ConvoyEscort.asset` 鈥?`MissionDefinitionSO`
- `Assets/Data/Missions/EnergyRaid.asset` 鈥?`MissionDefinitionSO`
- `Assets/Data/Missions/BalanceAllocation.asset` 鈥?`MissionDefinitionSO`
- `Assets/Art/Placeholders/Prefabs/PH_*.prefab` 鈥?Placeholder prefabs with `PH_` prefix
- `Assets/Art/Placeholders/Materials/MAT_PH_*.mat` 鈥?Placeholder materials

Animator controllers are also generated at runtime by `CombatAnimatorControllerGenerator`. Do not hand-edit these.

## Key conventions

- `CharacterEntity.Bind(CharacterData)` must be called before `Combatant` is usable 鈥?it initializes stats via `CombatStatsCalculator.Calculate`
- `SubFactionRegistry.Initialize(database)` must run before faction queries work; `GameBootstrap` does this in `Awake`
- Faction hostility uses `CharacterEntity.IsHostileTo()` which checks `FactionRelationshipService`; Play-mode tests inject `CharacterEntity.HostilityResolver` to override
- `FactionReputationService.InitializeDefaultPolitics()` must run before dynamic standing queries work; `LuoLuoTripGameContext.InitializeWorld()` does this
- `CommanderProfile.CreateDefault()` creates a Lv.1 commander with low stats; `ControlPermissionService.Evaluate()` determines control mode
- Mission consequences are resolved by `MissionConsequenceResolver.Resolve()` which returns `MissionConsequence` with faction deltas and XP
- Save files go to `Application.persistentDataPath`; `SaveService` is static, `SaveLoadManager` is the MonoBehaviour wrapper
- Save version is 2; old v1 saves load with default values for new fields (commander, factionPolitics, extended character data)
- `.csproj` / `.sln` files are auto-generated by Unity 鈥?do not edit, they are gitignored

## File locations quick reference

- World init: `Assets/Scripts/Game/GameBootstrap.cs`
- Save: `Assets/Scripts/Save/SaveLoadManager.cs`, `SaveService.cs`
- Player control: `Assets/Scripts/Combat/CombatController.cs`
- Combat core: `Assets/Scripts/Combat/Combatant.cs`
- Enemy AI: `Assets/Scripts/Combat/SimpleCombatAI.cs`
- Commander system: `Assets/Scripts/Commander/ControlPermissionService.cs`
- Faction politics: `Assets/Scripts/Faction/Politics/FactionReputationService.cs`
- Mission system: `Assets/Scripts/Mission/MissionService.cs`, `MissionConsequenceResolver.cs`
- Prototype runtime: `Assets/Scripts/Game/CommanderPrototypeRuntime.cs`
- Editor setup menu: `Assets/Scripts/Editor/LuoLuoTripSetupMenu.cs`
- Placeholder assets: `Assets/Scripts/Editor/PlaceholderAssetGenerator.cs`
- Asset replacement guide: `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md`


## Roadmap — Next Steps

Current status: Vertical Slice 完成，Commander/Faction/Mission 已接入实际控制流程和动态敌意，ConvoyEnergyConflict 任务闭环已实现。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (Unity 2022.3.62f3 batchmode 编译通过)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode
- 兼容性审计已完成，详见 `UNITY_VERSION_COMPATIBILITY_REPORT.md`

### Step 2: 接入 ControlPermissionService 到实际控制流程 ✅
- `CommanderControlController` 按 E 调用 `ControlPermissionService.Evaluate()`
- 根据 `ControlMode` 执行 DirectControl / TacticalCommand / SyncAssist / Denied
- `CommanderDebugHud` 实时显示当前控制结果和拒绝原因
- 玩家按 E 触发控制，按 R 释放控制

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断 ✅
- `CommanderControlController` 在 Start 中注入 `CharacterEntity.HostilityResolver`
- resolver 结合静态 FactionRelationshipService + 动态 FactionPoliticsState
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务 ✅
- `ConvoyEnergyConflictRuntime` 管理完整任务流程
- `MissionTriggerZone` 检测玩家进入区域自动启动任务
- `ConvoyObjective` 追踪运输队血量
- `EnergyNodeObjective` 追踪猛兽族占领进度和玩家共享进度
- MechaVictory / BeastVictory / BalancedResolution / Failed 自动判定

### Step 5: 生成圆柱体 Placeholder Prefab Library ✅ (PlaceholderAssetGenerator 已实现)
- `LuoLuoTrip/Setup/Generate Placeholder Assets` 菜单已可用
- 8 个 PH_*.prefab + 7 个 MAT_PH_*.mat
- Prefab 层级：PrefabRoot / Visual / Collision / Marker

### Step 6: 维护 ASSET_REPLACEMENT_GUIDE.md ✅ (已创建)
- `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md` 已编写
- 包含替换流程、验证清单、常见错误
- 每次替换资产后更新验证结果

### Step 7: 逐步换模型、动画和技能表现
- 按 ASSET_REPLACEMENT_GUIDE 的替换优先级执行
- 仅替换 Visual 子对象，不动 PrefabRoot 逻辑组件
- 接入真实 Animator 和 AnimatorCombatBridge
- 为不同 ControlMode 设计不同技能/操作表现
- 目标目录：`Assets/Art/Characters/Mecha/`、`Assets/Art/Characters/Beast/` 等

### Step 8: Playable Demo Hardening Pass ✅
- CombatTuningConfigSO: centralized combat timing config (`Assets/Data/Combat/CombatTuningConfig.asset`)
- DebugUILayout: unified OnGUI panel positioning (`Assets/Scripts/UI/DebugUILayout.cs`)
- All UI panels use DebugUILayout + show/hide toggle
- E key priority: commander target > energy share > nothing
- Debug triggers (1/2/3) do not pollute MissionChainState
- SaveLoadManager: enhanced logging (chain state, active mission, controlled unit)
- F10 clear save: logs "Restart scene to fully reset"
- SetupMenu: generates CombatTuningConfig in Resources
- VerticalSliceValidator: 12 checks including CombatPrototype, CombatTuningConfig, MissionBranchDefinition, SaveLoadManager
- MissionChainRegression: all 4 branches verified
- Input priority documented and tested
- Full documentation: `Assets/Docs/PLAYABLE_DEMO_README.md`

### Step 9: Presentation Pass 1 ✅
- AudioFeedbackService / AudioFeedbackProfileSO with per-event throttling
- WorldMarkerService / WorldMarkerProfileSO with OnGUI billboard rendering
- Enhanced placeholder visuals (multi-primitive Visual subgraphs)
- Commander target / controlled unit / mission objective / AI windup markers
- Audio wired into Combatant, SimpleCombatAI, CommanderControlController, ConvoyEnergyConflictRuntime, BorderRetaliationRuntime, CommanderPrototypeRuntime
- Marker wired into SimpleCombatAI (windup), CommanderControlController (selection + controlled), ConvoyEnergyConflictRuntime, BorderRetaliationRuntime
- Area labels in CommanderPrototype scene (Tutorial, Convoy Mission, Border Retaliation, Advanced Units)
- VerticalSliceValidator: AudioFeedbackProfile, WorldMarkerProfile, EnhancedPlaceholderHierarchy checks
- EditMode tests: AudioFeedbackProfileTests, WorldMarkerProfileTests
- Documentation: PLAYABLE_DEMO_README.md updated with Areas/Audio/Markers/Enhanced Placeholders

### Step 10: Navigation & Encounter Reliability Pass ✅
- NavigationAgentBridge: unified NavMesh/fallback movement API (SetDestination, Stop, Resume, HasReachedDestination, IsPathAvailable)
- NavigationMoveRequest: data class for movement requests (To, Follow, HasReached)
- AICombatNavigationController: higher-level combat navigation (ChaseTarget, FollowTarget, MoveToPosition, StopNavigation)
- SimpleCombatAI updated to use AICombatNavigationController for all movement (replaces direct MoveTowards)
- TacticalCommand navigation integration: FollowPlayer/FollowTarget, HoldPosition/StopNavigation, AttackCurrentTarget/ChaseTarget
- ReleaseControl clears navigation state on all affected units
- CommanderDebugHud shows navigation state + distance + NavMesh usage for tactical commands
- EncounterRuntime: unit tracking, casualty counting, AreAllRaidUnitsDefeated, ApplyMissionModifier
- EncounterDefinition, EncounterUnitHandle, EncounterWave, EncounterSpawnPoint
- MissionAreaRuntime: mission area lifecycle, player inside/outside, retreat management
- MissionBoundary: sphere-based area detection (IsInside)
- RetreatTracker: retreat countdown with HUD display
- MissionTriggerZone: ZoneRadius exposed, no re-trigger after completed
- MissionObjectiveHud: shows INSIDE/OUTSIDE status + retreat countdown
- ConvoyEnergyConflictRuntime: uses EncounterRuntime for unit tracking/casualties, MissionAreaRuntime for retreat
- BorderRetaliationRuntime: uses EncounterRuntime for unit tracking/casualties, MissionAreaRuntime for retreat, ApplyMissionModifier
- SetupMenu: adds NavMeshAgent to AI units, EncounterRuntime + MissionAreaRuntime on mission GameObjects, EncounterSpawnPoint
- VerticalSliceValidator: CheckNavigationAgentBridge, CheckEncounterRuntime (with Encounter property checks), CheckMissionAreaRuntime (with RetreatTracker)
- EditMode tests: NavigationMoveRequestTests, NavigationAgentBridgeFallbackTests, TacticalCommandNavigationTests, EncounterRuntimeTests, EncounterModifierTests, EncounterCasualtyTests, MissionAreaRuntimeTests, RetreatTrackerTests, MissionTriggerZoneRegressionTests
- PlayMode tests: NavigationAgentBridgeSmokeTests, SimpleCombatAINavigationCommandTests, MissionEncounterFlowSmokeTests, RetreatBoundarySmokeTests
- Documentation: NAVIGATION_ENCOUNTER_PASS.md, PLAYABLE_DEMO_README.md updated

