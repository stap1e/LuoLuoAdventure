# Unity Version Compatibility Report

Generated: 2026-06-16

## 1. Current Project Version

**ProjectVersion.txt**: `2022.3.62f3 (96770f90f904ca7)`

## 2. Recommended Target Unity Version

**Unity 2022.3.62f3 LTS**

### Rationale
- ProjectVersion.txt is already set to 2022.3.62f3
- No Unity 6-only API usage found in the codebase
- All packages in manifest.json are compatible with Unity 2022.3
- Compilation verified successfully with Unity 2022.3.62f3 in batchmode
- The project uses `FindObjectsOfType<T>` (obsolete in Unity 6 but valid in 2022.3)
- Upgrading to Unity 6 would require API migration (FindObjectsOfType -> FindObjectsByType) with no benefit at current scope

## 3. Package Manifest Check

All packages in `Packages/manifest.json` are compatible with Unity 2022.3.62f3:

| Package | Version | Status |
|---|---|---|
| com.unity.collab-proxy | 2.6.0 | Compatible |
| com.unity.feature.development | 1.0.2 | Compatible |
| com.unity.inputsystem | 1.7.0 | Compatible |
| com.unity.test-framework | 1.4.5 | Compatible |
| com.unity.textmeshpro | 3.0.6 | Compatible |
| com.unity.timeline | 1.7.6 | Compatible |
| com.unity.toolchain.win-x86_64-linux-x86_64 | 2.0.11 | Compatible |
| com.unity.ugui | 1.0.0 | Compatible |
| com.unity.modules.* | 1.0.0 | Compatible |

No Unity 6-exclusive packages found. No package version conflicts detected. `packages-lock.json` is consistent with `manifest.json`.

## 4. Assembly Definition Check

| Assembly | IncludePlatforms | References | Status |
|---|---|---|---|
| LuoLuoTrip | All | None | OK |
| LuoLuoTrip.Editor | Editor | LuoLuoTrip | OK |
| LuoLuoTrip.Tests.EditMode | Editor | LuoLuoTrip | OK |
| LuoLuoTrip.Tests.PlayMode | All | LuoLuoTrip | OK |

- Runtime assembly (LuoLuoTrip) does NOT reference Editor assembly
- Editor assembly correctly includes "Editor" platform only
- Test assemblies correctly reference LuoLuoTrip runtime assembly
- Both test assemblies have `UNITY_INCLUDE_TESTS` define constraint and `TestAssemblies` optional reference

## 5. Runtime/Editor API Separation Check

**Result: CLEAN** — No UnityEditor API references found in runtime scripts.

All UnityEditor API usage is properly confined to `Assets/Scripts/Editor/`:
- `LuoLuoTripSetupMenu.cs` — uses AssetDatabase, MenuItem, SerializedObject, EditorSceneManager, PrefabUtility
- `PlaceholderAssetGenerator.cs` — uses AssetDatabase, MenuItem, PrefabUtility
- `CombatAnimatorControllerGenerator.cs` — uses AssetDatabase

All three files have `#if UNITY_EDITOR` guards and belong to the `LuoLuoTrip.Editor` assembly.

## 6. Unity 6-Only API Check

**Result: CLEAN** — No Unity 6-only APIs detected.

Specifically checked:
- `FindAnyObjectByType<T>` / `Object.FindAnyObjectByType<T>` — NOT used (Unity 6 replacement for FindObjectOfType)
- `FindObjectsByType<T>` — NOT used (Unity 6 replacement for FindObjectsOfType)
- C# 10/11 features (`required`, `file`, `record struct`, `init`, `Span<T>`, `nint`) — NOT used

## 7. C# Version Compatibility

**Result: CLEAN** — All C# code is compatible with C# 9.0 / Unity 2022.3 Roslyn compiler.

Unity 2022.3 uses Roslyn with `-langversion:9.0`. No unsupported C# 10/11 features found.

## 8. Obsolete API Warnings

`FindObjectsOfType<T>` is used in 14 locations across runtime scripts. This API is:
- **Valid** in Unity 2022.3 (generates no error, only a deprecation warning in later versions)
- **Obsolete** in Unity 6 (must be replaced with `FindObjectsByType<T>(FindObjectsSortMode)`)

No changes made. If upgrading to Unity 6 in the future, these 14 call sites would need migration.

`FindObjectOfType<T>` is used in 1 location:
- `CommanderControlController.cs:46` — `FindObjectOfType<CommanderDebugHud>()`

Same situation: valid in 2022.3, obsolete in Unity 6.

## 9. Scene and Prefab Check

| Asset | Status |
|---|---|
| Assets/Scenes/CombatPrototype.unity | EXISTS |
| Assets/Scenes/CommanderPrototype.unity | MISSING — must be generated via `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene` |
| Assets/Art/Placeholders/Prefabs/PH_*.prefab | MISSING — must be generated via `LuoLuoTrip/Setup/Generate Placeholder Assets` |
| Assets/Art/Placeholders/Materials/MAT_PH_*.mat | MISSING — generated together with prefabs |
| Assets/Docs/ASSET_REPLACEMENT_GUIDE.md | EXISTS |

Missing assets are **generated** assets that must be created through Unity Editor setup menu items. They cannot be created from CLI. The `Assets/Art/Placeholders/` directory is gitignored.

## 10. Editor Menu Consistency

All 9 documented menu items in AGENTS.md match the actual code exactly:

| # | Menu Path | Match |
|---|---|---|
| 1 | LuoLuoTrip/Setup/Generate All Sub Faction Configs | Yes |
| 2 | LuoLuoTrip/Setup/Create Hit Feedback Profile | Yes |
| 3 | LuoLuoTrip/Setup/Create Combat Animator Config | Yes |
| 4 | LuoLuoTrip/Setup/Create Game Config Asset | Yes |
| 5 | LuoLuoTrip/Setup/Create Combat Prototype Scene | Yes |
| 6 | LuoLuoTrip/Setup/Create Commander Prototype Data | Yes |
| 7 | LuoLuoTrip/Setup/Create Mission Prototype Data | Yes |
| 8 | LuoLuoTrip/Setup/Create Commander Mission Prototype Scene | Yes |
| 9 | LuoLuoTrip/Setup/Generate Placeholder Assets | Yes |

**Undocumented menu items** (exist in code but not in AGENTS.md):
- `LuoLuoTrip/Setup/Create Bootstrap Scene`
- `LuoLuoTrip/Debug/Print World Summary`

**New menu item added** by this audit:
- `LuoLuoTrip/Tools/Compatibility/Run Project Compatibility Check`

## 11. Test Runner Check

| Item | Status |
|---|---|
| EditMode tests discoverable | Yes (asmdef correct) |
| PlayMode tests discoverable | Yes (asmdef correct) |
| Test assembly references | OK — both reference LuoLuoTrip only |
| Unity 6-only API in tests | None found |
| HostilityResolver cleanup | OK — SimpleCombatAITests uses try/finally + TearDown |
| Static state pollution | No issues found |

**Batchmode test execution**: Not completed. Unity 2022.3 batchmode `-runTests` did not produce test result XML. Tests must be run from the Unity Editor Test Runner window.

## 12. Fixes Applied

| Fix | File | Description |
|---|---|---|
| CRITICAL | FactionPoliticsState.cs:73 | `RestoreFromSnapshot` had type mismatch: `FactionPoliticsEntry` assigned to `FactionStanding` dictionary. Fixed by converting entry fields to new FactionStanding struct. |
| CRITICAL | SaveDataCommanderFactionTests.cs:1 | Missing `using LuoLuoTrip.Save;` — `GameSaveData` and `CommanderSaveEntry` are in `LuoLuoTrip.Save` namespace, not visible from `LuoLuoTrip.Tests.EditMode` without import. |
| MINOR | SimpleCombatAITests.cs | Added `[TearDown]` method to reset `CharacterEntity.HostilityResolver = null` as safety net beyond existing try/finally blocks. |
| MINOR | MissionObjectiveHud.cs | Removed duplicate file from `Assets/Scripts/UI/` (was `LuoLuoTrip.UI` namespace); correct version remains in `Assets/Scripts/Mission/Runtime/` (`LuoLuoTrip` namespace). |
| NEW | ProjectCompatibilityChecker.cs | Added `LuoLuoTrip/Tools/Compatibility/Run Project Compatibility Check` editor menu for in-Unity compatibility auditing. |

## 13. Items Requiring Manual Unity Verification

1. **Open Unity 2022.3.62f3** and confirm no compile errors in Console
2. **Run EditMode tests** in Test Runner window (Window > General > Test Runner)
3. **Run PlayMode tests** in Test Runner window
4. **Generate missing assets**:
   - `LuoLuoTrip/Setup/Generate Placeholder Assets`
   - `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene`
5. **Verify CombatPrototype scene** plays correctly (WASD, attack, dodge, lock-on)
6. **Verify CommanderPrototype scene** plays correctly (Tab/E/R, mission flow)
7. **Run compatibility check**: `LuoLuoTrip/Tools/Compatibility/Run Project Compatibility Check`
8. **Verify no Missing Scripts** in generated scenes and prefabs

## 14. Unity 6.4 / 6000.4.5f1 Compatibility Notes

If upgrading to Unity 6 in the future, the following changes would be required:
- Replace all 14 `FindObjectsOfType<T>()` calls with `FindObjectsByType<T>(FindObjectsSortMode.None)`
- Replace 1 `FindObjectOfType<T>()` call with `FindFirstObjectByType<T>()`
- Update `ProjectVersion.txt` to `6000.4.5f1`
- Re-resolve package versions (some packages may have different versions for Unity 6)
- Test framework API may have changed; verify test runner compatibility
- The `com.unity.toolchain.win-x86_64-linux-x86_64` package may need version update

**Recommendation**: Do NOT upgrade to Unity 6 at this time. Stay on 2022.3.62f3 LTS for stability.

## 15. .csproj / .sln Status

Auto-generated `.csproj` and `.sln` files exist in the project root but are properly gitignored. No hand-edited `.csproj` or `.sln` files found. No action needed.

## 16. Orphaned .meta Files

No orphaned `.meta` files found. The previously deleted `Assets/Scripts/UI/MissionObjectiveHud.cs` has no leftover `.meta` file.
