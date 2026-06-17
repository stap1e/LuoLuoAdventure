# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project basics

- Unity project for `LuoLuoTrip` / LuoLuoAdventure using **Unity 2022.3.62f3 LTS**.
- Root namespace: `LuoLuoTrip`.
- Main build target recorded in generated project files: `StandaloneWindows64`.
- Runtime C# uses the Unity-generated `LuoLuoTrip` assembly; editor tooling and tests are split into separate asmdefs.
- Unity-generated `.sln` / `.csproj` files are not source-of-truth; do not hand-edit them.

## Common commands

PowerShell scripts are the supported command-line test path on Windows. They default to:

```powershell
C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe
```

Run from the repository root:

```powershell
# EditMode tests
.\scripts\run_unity_editmode_tests.ps1

# A single EditMode test class or test by full NUnit name
.\scripts\run_unity_editmode_tests.ps1 -TestFilter "LuoLuoTrip.Tests.EditMode.RuntimeServiceLifecycleTests"

# PlayMode tests
.\scripts\run_unity_playmode_tests.ps1

# A single PlayMode test class or test by full NUnit name
.\scripts\run_unity_playmode_tests.ps1 -TestFilter "LuoLuoTrip.Tests.PlayMode.CommanderPrototypeInputSmokeTests"

# EditMode then PlayMode
.\scripts\run_unity_all_tests.ps1

# Parse an existing result file
python scripts\parse_unity_test_results.py TestResults\editmode-results.xml
```

If Unity is installed elsewhere, pass `-UnityPath` to the scripts.

```powershell
.\scripts\run_unity_all_tests.ps1 -UnityPath "D:\Unity\2022.3.62f3\Editor\Unity.exe"
```

Important test-runner detail: the standard `-runTests` scripts intentionally **do not pass `-quit`**. In this Unity/Test Framework combination, `-quit` can make batchmode exit before tests run. See `Assets/Docs/TEST_RUNNER_RELIABILITY.md` before changing test command lines.

Fallback CI runners are available if `-runTests` stops producing XML:

```powershell
.\scripts\run_unity_editmode_tests_ci.ps1
.\scripts\run_unity_editmode_tests_ci.ps1 -TestFilter "LuoLuoTrip.Tests.EditMode.RuntimeServiceLifecycleTests"
.\scripts\run_unity_playmode_tests_ci.ps1
```

There is no dedicated lint command or scripted player-build command in the repository. Use Unity Editor Build Settings for player builds; test scripts are the usual compile/test verification path.

## Unity editor workflows

Useful top-menu items implemented under `Assets/Scripts/Editor/`:

- `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene` creates the main playable prototype scene and required data assets.
- `LuoLuoTrip/Setup/Generate Placeholder Assets` creates placeholder prefabs/materials.
- `LuoLuoTrip/Setup/Create Audio Feedback Profile` and `Create World Marker Profile` create feedback assets and Resources copies.
- `LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation` writes `Assets/Docs/VERTICAL_SLICE_VALIDATION_REPORT.md` and checks scene/data/component assumptions.
- `LuoLuoTrip/Tools/Compatibility/Run Project Compatibility Check` writes `CompatibilityCheck_Report.txt` and checks Unity version, packages, asmdefs, missing scripts, prefab hierarchy, and orphaned `.meta` files.

Playable demo flow from `Assets/Docs/PLAYABLE_DEMO_README.md`: open `Assets/Scenes/CommanderPrototype.unity`, press Play, and follow the tutorial overlay. Main controls are WASD movement, left-click attack, Space dodge, Q lock-on, Tab target select, E interact/control, R release control, F5/F9 save/load, F10 clear save, and 1/2/3 debug mission outcomes.

## Assembly layout

- `Assets/Scripts/LuoLuoTrip.asmdef` (`LuoLuoTrip`): runtime code, all platforms.
- `Assets/Scripts/Editor/LuoLuoTrip.Editor.asmdef` (`LuoLuoTrip.Editor`): editor-only tools, references runtime plus Unity test runner assemblies.
- `Assets/Tests/EditMode/LuoLuoTrip.Tests.EditMode.asmdef`: NUnit EditMode tests, references runtime, gated by `UNITY_INCLUDE_TESTS`.
- `Assets/Tests/PlayMode/LuoLuoTrip.Tests.PlayMode.asmdef`: PlayMode / `[UnityTest]` tests, references runtime, gated by `UNITY_INCLUDE_TESTS`.

Keep `UnityEditor` APIs in `Assets/Scripts/Editor/` or other editor-only assemblies. Runtime scripts should remain in `Assets/Scripts/{domain}/` and not reference editor assemblies.

## Architecture overview

`GameBootstrap` is the scene entry point. On `Awake`, it ensures a camera, initializes `SubFactionRegistry` from `Resources/SubFactionDatabase`, creates the static `LuoLuoTripGameContext`, optionally loads a save, otherwise initializes the world, and logs the world summary.

`LuoLuoTripGameContext` is the runtime service hub. It owns faction relationship services, reputation/hostility services, commander profile, mission services, mission-chain state, faction states, and all world characters. Save/export flows use this context rather than independently rebuilding game state.

Major domains under `Assets/Scripts/`:

- `Core`: enums and constants such as `SubFactionId`, `CharacterRole`, `MainRace`, `RelationshipStance`, and `GameConstants`.
- `Character`: `CharacterData`, runtime entity binding, level/stat initialization, movement, registry, and component guards.
- `Combat`: combat state, player controller, AI, damage/stat calculation, tuning, animation bridge/config, hit feedback, debug HUD/controllers.
- `AI`: navigation abstraction over NavMesh/fallback movement plus higher-level combat navigation.
- `Commander`: commander profile/progression, control permission evaluation, tactical command state, target selection, and runtime control flow.
- `Faction` and `Faction/Politics`: static faction definitions/relationship matrix plus dynamic reputation, standing deltas, consequences, and hostility resolution.
- `Mission` and `Mission/Runtime`: mission definitions, objectives, consequences, chain state, branch modifiers, trigger zones, convoy/energy/border runtime flows, boundaries, retreat, and HUD.
- `Encounter`: dynamic encounter definitions, waves, spawn points, unit handles, casualty tracking, idempotent runtime lifecycle.
- `Save`: serializable save DTOs, static JSON I/O in `SaveService`, and scene-facing `SaveLoadManager`.
- `Audio`, `Feedback`, and `UI`: feedback profiles/services, world markers, health bars, commander/mission/faction panels, and debug/tutorial UI.
- `Game`: `GameBootstrap`, `GameConfig`, `LuoLuoTripGameContext`, `CommanderPrototypeRuntime`, and tutorial/prototype orchestration.

## Runtime state conventions

- `SubFactionRegistry.Initialize(database)` must run before registry-based faction lookups; `GameBootstrap` handles this from Resources.
- `LuoLuoTripGameContext.InitializeWorld()` initializes default faction politics and characters; `ApplySave()` restores characters, relationships, politics, commander profile, and mission chain state.
- `CharacterEntity.Bind(CharacterData)` connects scene objects to character data and combat stats.
- Faction hostility can combine static relationship data and dynamic politics; PlayMode tests often inject hostility resolvers to make scenarios deterministic.
- Mission completion flows through `MissionService` / `MissionConsequenceResolver` and, for real missions, `MissionChainService`; debug mission triggers intentionally avoid polluting mission chain state.
- Save files are JSON under `Application.persistentDataPath`; `SaveService` is static I/O and `SaveLoadManager` is the MonoBehaviour wrapper used by scenes.

## Generated assets and scenes

Many prototype assets are generated by editor menu items rather than hand-authored from scratch, including faction configs/database, feedback profiles, combat tuning/config assets, placeholder prefabs/materials, mission definitions, and prototype scenes. When replacing art, preserve gameplay components on prefab roots and replace visual children rather than rewriting logic prefabs wholesale. See `Assets/Docs/PLAYABLE_DEMO_README.md` and related docs under `Assets/Docs/` for current vertical-slice expectations.
