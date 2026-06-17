# Legacy Test Suite Triage Report

Date: 2026-06-17
Unity: 2022.3.62f3 LTS

## Current full-suite baseline

- EditMode: 399 total, 356 passed, 43 failed
- PlayMode: 99 total, 90 passed, 9 failed
- EditMode XML: `C:\Users\16025\AppData\Local\Temp\luoluo_triage_edit_full.xml`
- PlayMode XML: `C:\Users\16025\AppData\Local\Temp\luoluo_triage_play_full.xml`
- Parsed EditMode failures: `C:\Users\16025\AppData\Local\Temp\luoluo_edit_failures.json`
- Parsed PlayMode failures: `C:\Users\16025\AppData\Local\Temp\luoluo_play_failures.json`

Unity batchmode `-runTests` did not emit the targeted XML in this environment, matching the existing project note that Test Runner execution should be done from the Unity Editor. Compile-only batchmode passed with return code 0 after the triage fixes.

## Classifications

### REAL_BUG / FIXED

| Test(s) | Cause | Fix |
|---|---|---|
| `AIStopDistanceTests.EffectiveStopDistance_FallbacksToAttackRange_WhenZero` | `SimpleCombatAI.EffectiveStopDistance` dereferenced `_self` before `Awake` in EditMode fixtures. | Added lazy `EnsureReferences()` and default stats fallback in `Assets/Scripts/Combat/SimpleCombatAI.cs`. |
| `CombatControllerInputTests.*`, `CombatRootMovementTests.ApplyMoveInput_MovesRootOnXZ_KeepsY` | Public `CombatController.ApplyMoveInput` assumed `_self`/refs were initialized by `Awake`. | Added lazy `EnsureReferences()` in `ApplyMoveInput` in `Assets/Scripts/Combat/CombatController.cs`. |
| `MissionAreaRuntimeTests.Activate_SetsIsActive`, `MarkComplete_SetsIsComplete` | Public lifecycle methods assumed `_retreatTracker` was initialized by `Awake`. | Added lazy `EnsureRetreatTracker()` in `Assets/Scripts/Mission/Runtime/MissionAreaRuntime.cs`. |
| `HealthBarPresenterTests.*` | Presenter public access / `LateUpdate` could run before normal lifecycle initialization. | Added lazy bar/reference setup and safer `LateUpdate` path in `Assets/Scripts/UI/CombatantHealthBarPresenter.cs`. |
| `HitFlashFeedbackTests.*` | `RendererCount` did not lazy-resolve; EditMode used `renderer.material`, causing Unity material leak errors. | Added lazy `RendererCount` and switched tinting/restores to `sharedMaterial` in `Assets/Scripts/Combat/Feedback/HitFlashFeedback.cs`. |
| `RuntimeServiceLifecycleTests.*DuplicateDoesNotDestroyHostGO`, `*InstanceIsNullAfterDestruction` | Singleton `Instance` was not EditMode-safe when components were added without runtime lifecycle ordering. | Added lazy `FindObjectOfType` fallback to `HitStopService`, `CameraShakeService`, `CombatHitFeedbackHub`, `AudioFeedbackService`, `WorldMarkerService`. |
| `RuntimeServiceLifecycleTests.*OnSharedHost_DoesNotUseDestroyGameObject` | Tests scan raw source for `Destroy(gameObject)` and comments still contained that string. | Removed misleading source text from service XML docs; implementation already uses `Destroy(this)`. |
| `RuntimeServiceLifecycleTests.HitStopService_RestoresTimeScaleOnDestroy` | Same lifecycle issue could leave `Instance` unresolved in EditMode. | Covered by lazy singleton patch; `OnDestroy` already restores time when instance matches. |
| `CombatPrototypeFullLoopSmokeTests.DebugController_F2_ResetsHP` | Legacy PlayMode test sends `ResetAllHP`, but receiver was private. | Exposed `CombatPrototypeDebugController.ResetAllHP()` as public for `SendMessage` compatibility. |

### ACTIVE_WINDOW_DAMAGE_CHANGE / OBSOLETE_ASSERTION

| Test(s) | Current assessment |
|---|---|
| `AIAttackWindowTests.AI_CooldownPreventsSpam` | Timing/cooldown assertion appears to predate active-window combat timing. Update expected ticks/state windows rather than reverting gameplay. |
| `CombatHitWindowTests.TwoSequentialAttacks_ProduceTwoHits` | Test expects `Idle` earlier than current attack recovery window. Update fixture to tick through configured recovery/cooldown. |
| `CombatMathTests.Tick_BlocksAttackUntilCooldownExpires` | Current combat uses windup/active/recovery and `AttackCooldownRemaining`; assertion should target configured completion point. |
| `CombatTimingTests.*` | Expected timings appear stale against `CombatTuningConfigSO` active-window defaults. Update assertions to current config semantics. |
| `CombatTuningConfigTests.ApplyToCombatant_SetsTimingValues`, `CombatFeelSmokeTests.CombatTuningConfig_AppliesToCombatant` | Expected `0.5f` but current config applies `0.35f`; likely old default/assertion. Confirm intended balance before changing data. |

### ROOT_VISUAL_REFACTOR / OBSOLETE_ASSERTION

| Test(s) | Current assessment |
|---|---|
| `AnimationClipBindingSafetyTests.GeneratedPlaceholderClip_BindsToVisualPath_NotRoot` | Test likely scans old generator pattern; current animation path rules should be verified against generated controller/clip output. |
| `AnimationClipBindingSafetyTests.RootPathBinding_DetectionLogic_FlagsViolation` | Detection helper appears stale. Update test helper to current binding representation. |
| `ProceduralAnimatorVisualOnlyTests.*` | Likely stale fixture/lifecycle expectation after procedural animator fallback changes. Verify strict-mode semantics before changing runtime. |

### INPUT_OWNERSHIP_CHANGE / OBSOLETE_ASSERTION

| Test(s) | Current assessment |
|---|---|
| `CombatPrototypeMovementSmokeTests.PlayerWithInputDisabled_PositionDoesNotChange` | Failure compares visually identical vectors; likely exact `Vector3` equality after motor/root changes. Use tolerance/assert XZ. |
| `InputOwnershipRegressionTests.DirectControl_PositionNoChangeOnDisabledInput` | Same exact-vector/tolerance issue; input ownership should assert `SetInputEnabled` semantics, not component enabled state. |

### SCENE_SETUP_MISSING / PLAYMODE_ENVIRONMENT

| Test(s) | Current assessment |
|---|---|
| `CommanderPrototypeInputSmokeTests.CommanderControlController_HasHasSelectedTarget` | Scene fixture likely missing expected selected target / object name after setup changes. Re-run after regenerating CommanderPrototype or update scene fixture lookup. |
| `CommanderPrototypeInputSmokeTests.CommanderControlRuntimeState_SyncAssistAppliesBuffs` | Could be scene setup or authority/buff threshold drift. Needs targeted PlayMode repro in Editor. |
| `NavigationAgentBridgeSmokeTests.Bridge_FallbackMovement_MovesTowardsDestination`, `NavMeshDynamicEncounterTests.NavigationBridge_FallbackMovement_WhenNoNavMesh` | Fallback movement may be blocked by component state or test timing. Needs targeted PlayMode repro; classify as PLAYMODE_ENVIRONMENT until verified. |

### UNKNOWN / NEEDS DESIGN CONFIRMATION

| Test(s) | Current assessment |
|---|---|
| `CameraRuntimeBootstrapTests.EnsureMainCamera_AddsMissingCameraComponent` | Message shows equal object names but assertion failed; likely Unity object lifetime/reference comparison issue. Needs source review before patch. |
| `CharacterMovementMotorTests.ClampToGroundPlane_RestoresGroundY` | Expected old ground clamp (`0.5`) but actual remains `5`. Need decide if motor should clamp Y or preserve root Y. |
| `CommanderLevelProgressionTests.CanLevelUp_TrueWhenEnoughXP` | Could be XP curve change or stale threshold expectation. Needs design confirmation. |
| `DebugUILayoutTests.*` | Layout assertions stale after HUD additions, or real overlap. Needs visual/design confirmation. |
| `DynamicHostilityResolverTests.BalancedResolution_LowersHostility_ReturnsNonHostile`, `DynamicHostilityIntegrationTests.BalancedResolution_ReducesHostility` | Needs design confirmation: static hostility may now dominate dynamic standing, or resolver threshold is wrong. |
| `TacticalCommandStateTests.*` | Could be command state semantics changed with navigation integration. Needs source review. |

## Verification performed

- Unity compile-only batchmode after patches: passed, return code 0.
- Final compile log: `C:\Users\16025\AppData\Local\Temp\luoluo_compile_after_debug_receiver.log`
- Targeted `-runTests` attempted but no XML emitted in this environment.

## Recommended next run

From Unity Editor Test Runner:

1. Run targeted EditMode:
   - `AIStopDistanceTests`
   - `CombatControllerInputTests`
   - `CombatRootMovementTests`
   - `HealthBarPresenterTests`
   - `HitFlashFeedbackTests`
   - `MissionAreaRuntimeTests`
   - `RuntimeServiceLifecycleTests`
2. Run targeted PlayMode:
   - `CombatPrototypeFullLoopSmokeTests.DebugController_F2_ResetsHP`
3. Re-run full EditMode and PlayMode suites and refresh the parsed failure JSON.
