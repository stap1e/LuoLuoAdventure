# AGENTS.md — LuoLuoAdventure

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

- **Core** — enums/value types: `SubFactionId`, `CharacterRole`, `MainRace`, `RelationshipStance`, `GameConstants`
- **Character** — `CharacterData` (with CommandRank/Trust fields), `CharacterEntity` (binds `CharacterData`), `CharacterInitializer`, `CharacterLevelSystem`
- **Combat** — `Combatant` (HP/ST/Poise state machine), `CombatController` (player input), `SimpleCombatAI`, `DamageCalculator`, `CombatStats`/`CombatStatsCalculator`
  - **Combat/Animation** — `ICombatAnimator`, `AnimatorCombatBridge`, `ProceduralCombatAnimator`, `CombatAnimationDriver`, `CombatAnimatorConfigSO`
  - **Combat/Feedback** — `HitStopService`, `HitFeedbackProfileSO`, `CombatHitFeedbackHub`, `CameraShakeService`
- **Commander** — `ControlMode`, `CommanderProfile`, `CommanderLevelSystem`, `ControlPermissionRequest`, `ControlPermissionResult`, `ControlPermissionService`, `SyncRateCalculator`
- **Faction** — `SubFactionConfigSO`, `SubFactionDatabaseSO`, `SubFactionRegistry`, `FactionRelationshipMatrix`, `FactionRelationshipService`
  - **Faction/Politics** — `FactionStanding`, `FactionStandingDelta`, `FactionPoliticsState`, `FactionReputationService`, `FactionConsequenceApplier`, `DynamicFactionHostilityService`
- **Mission** — `MissionOutcomeType`, `MissionObjective`, `MissionRuntimeState`, `MissionConsequence`, `MissionDefinitionSO`, `MissionConsequenceResolver`, `MissionService`
- **Save** — `SaveLoadManager` (MonoBehaviour), `SaveService` (static I/O), `GameSaveData` (with CommanderSaveEntry, FactionPoliticsSnapshot)
- **Game** — `GameBootstrap` (entry point MonoBehaviour), `GameConfig`, `LuoLuoTripGameContext`, `CommanderPrototypeRuntime`
- **UI** — `CommanderDebugHud`, `FactionStandingDebugPanel`, `MissionResultDebugPanel`

Entry point: `GameBootstrap.Awake()` → loads `SubFactionDatabase` from Resources → creates `LuoLuoTripGameContext` (includes CommanderProfile, ReputationService, MissionService) → initializes world or applies save.

## Required setup before first play

All via Unity top menu **LuoLuoTrip/Setup/** (in order):

1. `Generate All Sub Faction Configs` — creates `Assets/Data/Factions/*.asset` + `Assets/Resources/SubFactionDatabase.asset`
2. `Create Hit Feedback Profile` — `Assets/Data/HitFeedbackProfile.asset`
3. `Create Combat Animator Config` — `Assets/Data/Animation/CombatAnimatorConfig.asset`
4. `Create Game Config Asset` — `Assets/Data/GameConfig.asset`
5. `Create Combat Prototype Scene` — does all of the above + creates `Assets/Scenes/CombatPrototype.unity` with player/enemy/hud
6. `Create Commander Prototype Data` — creates `Assets/Data/Missions/ConvoyEscort.asset`
7. `Create Mission Prototype Data` — creates `Assets/Data/Missions/EnergyRaid.asset` + `BalanceAllocation.asset`
8. `Create Commander Mission Prototype Scene` — does all of the above + creates `Assets/Scenes/CommanderPrototype.unity` with commander/faction/mission debug objects
9. `Generate Placeholder Assets` — creates `Assets/Art/Placeholders/Prefabs/PH_*.prefab` + `Assets/Art/Placeholders/Materials/MAT_PH_*.mat`

If the `LuoLuoTrip` menu is missing, check Console for compile errors and confirm Unity version.

## Runtime controls

### CombatPrototype scene
WASD move, Left-click attack, Space dodge, Q lock-on, Tab switch target, F5 quick-save, F9 quick-load.

### CommanderPrototype scene
Same combat controls, plus:
- Key 1: Trigger MechaVictory test mission
- Key 2: Trigger BeastVictory test mission
- Key 3: Trigger BalancedResolution test mission

## Testing

Tests run inside Unity via the Test Runner window (Window > General > Test Runner).

- **Edit-mode**: `CombatMathTests.cs`, `FactionRelationshipMatrixTests.cs`, `CommanderAuthorityTests.cs`, `FactionPoliticsTests.cs`, `MissionConsequenceResolverTests.cs`, `SaveDataCommanderFactionTests.cs` — pure logic tests, no Play mode needed
- **Play-mode**: `SimpleCombatAITests.cs`, `DynamicHostilityFlowTests.cs` — uses `[UnityTest]` + coroutines, requires Play mode

Edit-mode tests manually call `Combatant.Tick(deltaTime)` with `AutoTickEnabled = false` for deterministic timing. Play-mode tests set `CharacterEntity.HostilityResolver` to control faction hostility.

There is no CLI test runner configured. Tests must be run from the Unity Editor.

## Generated / ScriptableObject assets

The following are **generated** by editor menu items, not hand-authored:

- `Assets/Data/Factions/*.asset` — `SubFactionConfigSO` per `SubFactionId` enum value
- `Assets/Data/Factions/SubFactionDatabase.asset`
- `Assets/Resources/SubFactionDatabase.asset` — copy of the above for `Resources.Load`
- `Assets/Data/HitFeedbackProfile.asset`
- `Assets/Data/Animation/CombatAnimatorConfig.asset`
- `Assets/Data/GameConfig.asset`
- `Assets/Data/Missions/ConvoyEscort.asset` — `MissionDefinitionSO`
- `Assets/Data/Missions/EnergyRaid.asset` — `MissionDefinitionSO`
- `Assets/Data/Missions/BalanceAllocation.asset` — `MissionDefinitionSO`
- `Assets/Art/Placeholders/Prefabs/PH_*.prefab` — Placeholder prefabs with `PH_` prefix
- `Assets/Art/Placeholders/Materials/MAT_PH_*.mat` — Placeholder materials

Animator controllers are also generated at runtime by `CombatAnimatorControllerGenerator`. Do not hand-edit these.

## Key conventions

- `CharacterEntity.Bind(CharacterData)` must be called before `Combatant` is usable — it initializes stats via `CombatStatsCalculator.Calculate`
- `SubFactionRegistry.Initialize(database)` must run before faction queries work; `GameBootstrap` does this in `Awake`
- Faction hostility uses `CharacterEntity.IsHostileTo()` which checks `FactionRelationshipService`; Play-mode tests inject `CharacterEntity.HostilityResolver` to override
- `FactionReputationService.InitializeDefaultPolitics()` must run before dynamic standing queries work; `LuoLuoTripGameContext.InitializeWorld()` does this
- `CommanderProfile.CreateDefault()` creates a Lv.1 commander with low stats; `ControlPermissionService.Evaluate()` determines control mode
- Mission consequences are resolved by `MissionConsequenceResolver.Resolve()` which returns `MissionConsequence` with faction deltas and XP
- Save files go to `Application.persistentDataPath`; `SaveService` is static, `SaveLoadManager` is the MonoBehaviour wrapper
- Save version is 2; old v1 saves load with default values for new fields (commander, factionPolitics, extended character data)
- `.csproj` / `.sln` files are auto-generated by Unity — do not edit, they are gitignored

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

Current status: Vertical Slice 完成，Commander/Faction/Mission 纯逻辑系统已实现，但未接入实际控制流程。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (代码审查通过，待 Unity 内实际编译)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode

### Step 2: 接入 ControlPermissionService 到实际控制流程
- 修改 `CombatController` 使其在尝试控制单位前调用 `ControlPermissionService.Evaluate()`
- 根据返回的 `ControlMode` 决定：DirectControl 正常操作 / TacticalCommand 限制操作 / SyncAssist 短时辅助 / Denied 禁止
- 在 `CommanderDebugHud` 中实时显示当前控制结果和拒绝原因
- 需要决定：玩家"尝试控制"的触发方式（锁定目标时按 E？靠近时自动评估？）

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断
- 修改 `CharacterEntity.IsHostileTo()` 或 `SimpleCombatAI` 的目标选择逻辑
- 结合 `DynamicFactionHostilityService.ShouldAttackPlayer()` 判断动态敌意
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务
- 替代当前的按键 1/2/3 直接结算
- 场景中放置 Convoy（PH_Convoy_Cylinder）和 EnergyNode（PH_EnergyNode_Cylinder）
- 玩家选择：保护运输队 vs 帮猛兽族抢能源 vs 平衡分配
- 根据玩家实际战斗行为（击杀哪方、保护/破坏目标）自动判定 MissionOutcomeType
- 触发 MissionConsequenceResolver 产生后果

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
- Placeholder assets: `Assets/Scripts/Editor/PlaceholderAssetGenerator.cs`
- Asset replacement guide: `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md`

## Roadmap — Next Steps

Current status: Vertical Slice 完成，Commander/Faction/Mission 纯逻辑系统已实现，但未接入实际控制流程。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (代码审查通过，待 Unity 内实际编译)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode

### Step 2: 接入 ControlPermissionService 到实际控制流程
- 修改 `CombatController` 使其在尝试控制单位前调用 `ControlPermissionService.Evaluate()`
- 根据返回的 `ControlMode` 决定：DirectControl 正常操作 / TacticalCommand 限制操作 / SyncAssist 短时辅助 / Denied 禁止
- 在 `CommanderDebugHud` 中实时显示当前控制结果和拒绝原因
- 需要决定：玩家"尝试控制"的触发方式（锁定目标时按 E？靠近时自动评估？）

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断
- 修改 `CharacterEntity.IsHostileTo()` 或 `SimpleCombatAI` 的目标选择逻辑
- 结合 `DynamicFactionHostilityService.ShouldAttackPlayer()` 判断动态敌意
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务
- 替代当前的按键 1/2/3 直接结算
- 场景中放置 Convoy（PH_Convoy_Cylinder）和 EnergyNode（PH_EnergyNode_Cylinder）
- 玩家选择：保护运输队 vs 帮猛兽族抢能源 vs 平衡分配
- 根据玩家实际战斗行为（击杀哪方、保护/破坏目标）自动判定 MissionOutcomeType
- 触发 MissionConsequenceResolver 产生后果

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
- Placeholder assets: `Assets/Scripts/Editor/PlaceholderAssetGenerator.cs`
- Asset replacement guide: `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md`

## Roadmap — Next Steps

Current status: Vertical Slice 完成，Commander/Faction/Mission 纯逻辑系统已实现，但未接入实际控制流程。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (代码审查通过，待 Unity 内实际编译)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode

### Step 2: 接入 ControlPermissionService 到实际控制流程
- 修改 `CombatController` 使其在尝试控制单位前调用 `ControlPermissionService.Evaluate()`
- 根据返回的 `ControlMode` 决定：DirectControl 正常操作 / TacticalCommand 限制操作 / SyncAssist 短时辅助 / Denied 禁止
- 在 `CommanderDebugHud` 中实时显示当前控制结果和拒绝原因
- 需要决定：玩家"尝试控制"的触发方式（锁定目标时按 E？靠近时自动评估？）

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断
- 修改 `CharacterEntity.IsHostileTo()` 或 `SimpleCombatAI` 的目标选择逻辑
- 结合 `DynamicFactionHostilityService.ShouldAttackPlayer()` 判断动态敌意
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务
- 替代当前的按键 1/2/3 直接结算
- 场景中放置 Convoy（PH_Convoy_Cylinder）和 EnergyNode（PH_EnergyNode_Cylinder）
- 玩家选择：保护运输队 vs 帮猛兽族抢能源 vs 平衡分配
- 根据玩家实际战斗行为（击杀哪方、保护/破坏目标）自动判定 MissionOutcomeType
- 触发 MissionConsequenceResolver 产生后果

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
- Placeholder assets: `Assets/Scripts/Editor/PlaceholderAssetGenerator.cs`
- Asset replacement guide: `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md`

## Roadmap — Next Steps

Current status: Vertical Slice 完成，Commander/Faction/Mission 纯逻辑系统已实现，但未接入实际控制流程。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (代码审查通过，待 Unity 内实际编译)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode

### Step 2: 接入 ControlPermissionService 到实际控制流程
- 修改 `CombatController` 使其在尝试控制单位前调用 `ControlPermissionService.Evaluate()`
- 根据返回的 `ControlMode` 决定：DirectControl 正常操作 / TacticalCommand 限制操作 / SyncAssist 短时辅助 / Denied 禁止
- 在 `CommanderDebugHud` 中实时显示当前控制结果和拒绝原因
- 需要决定：玩家"尝试控制"的触发方式（锁定目标时按 E？靠近时自动评估？）

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断
- 修改 `CharacterEntity.IsHostileTo()` 或 `SimpleCombatAI` 的目标选择逻辑
- 结合 `DynamicFactionHostilityService.ShouldAttackPlayer()` 判断动态敌意
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务
- 替代当前的按键 1/2/3 直接结算
- 场景中放置 Convoy（PH_Convoy_Cylinder）和 EnergyNode（PH_EnergyNode_Cylinder）
- 玩家选择：保护运输队 vs 帮猛兽族抢能源 vs 平衡分配
- 根据玩家实际战斗行为（击杀哪方、保护/破坏目标）自动判定 MissionOutcomeType
- 触发 MissionConsequenceResolver 产生后果

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
- Placeholder assets: `Assets/Scripts/Editor/PlaceholderAssetGenerator.cs`
- Asset replacement guide: `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md`

## Roadmap — Next Steps

Current status: Vertical Slice 完成，Commander/Faction/Mission 纯逻辑系统已实现，但未接入实际控制流程。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (代码审查通过，待 Unity 内实际编译)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode

### Step 2: 接入 ControlPermissionService 到实际控制流程
- 修改 `CombatController` 使其在尝试控制单位前调用 `ControlPermissionService.Evaluate()`
- 根据返回的 `ControlMode` 决定：DirectControl 正常操作 / TacticalCommand 限制操作 / SyncAssist 短时辅助 / Denied 禁止
- 在 `CommanderDebugHud` 中实时显示当前控制结果和拒绝原因
- 需要决定：玩家"尝试控制"的触发方式（锁定目标时按 E？靠近时自动评估？）

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断
- 修改 `CharacterEntity.IsHostileTo()` 或 `SimpleCombatAI` 的目标选择逻辑
- 结合 `DynamicFactionHostilityService.ShouldAttackPlayer()` 判断动态敌意
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务
- 替代当前的按键 1/2/3 直接结算
- 场景中放置 Convoy（PH_Convoy_Cylinder）和 EnergyNode（PH_EnergyNode_Cylinder）
- 玩家选择：保护运输队 vs 帮猛兽族抢能源 vs 平衡分配
- 根据玩家实际战斗行为（击杀哪方、保护/破坏目标）自动判定 MissionOutcomeType
- 触发 MissionConsequenceResolver 产生后果

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
- Placeholder assets: `Assets/Scripts/Editor/PlaceholderAssetGenerator.cs`
- Asset replacement guide: `Assets/Docs/ASSET_REPLACEMENT_GUIDE.md`

## Roadmap — Next Steps

Current status: Vertical Slice 完成，Commander/Faction/Mission 纯逻辑系统已实现，但未接入实际控制流程。以下是按优先级排列的后续计划：

### Step 1: Unity 编译验证 ✅ (代码审查通过，待 Unity 内实际编译)
- 在 Unity Editor 中打开项目，确认无编译错误
- 运行全部 EditMode / PlayMode 测试
- 验证 CombatPrototype 和 CommanderPrototype 场景可正常进入 Play Mode

### Step 2: 接入 ControlPermissionService 到实际控制流程
- 修改 `CombatController` 使其在尝试控制单位前调用 `ControlPermissionService.Evaluate()`
- 根据返回的 `ControlMode` 决定：DirectControl 正常操作 / TacticalCommand 限制操作 / SyncAssist 短时辅助 / Denied 禁止
- 在 `CommanderDebugHud` 中实时显示当前控制结果和拒绝原因
- 需要决定：玩家"尝试控制"的触发方式（锁定目标时按 E？靠近时自动评估？）

### Step 3: 接入 DynamicFactionHostilityService 到 AI 敌我判断
- 修改 `CharacterEntity.IsHostileTo()` 或 `SimpleCombatAI` 的目标选择逻辑
- 结合 `DynamicFactionHostilityService.ShouldAttackPlayer()` 判断动态敌意
- 任务后果改变 FactionStanding 后，AI 行为实时变化
- PlayMode 测试验证：完成任务后，先前中立的派系变为敌对

### Step 4: 实现真实 ConvoyEnergyConflict 小任务
- 替代当前的按键 1/2/3 直接结算
- 场景中放置 Convoy（PH_Convoy_Cylinder）和 EnergyNode（PH_EnergyNode_Cylinder）
- 玩家选择：保护运输队 vs 帮猛兽族抢能源 vs 平衡分配
- 根据玩家实际战斗行为（击杀哪方、保护/破坏目标）自动判定 MissionOutcomeType
- 触发 MissionConsequenceResolver 产生后果

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
