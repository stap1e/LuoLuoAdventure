# Test Runner Reliability Guide

Date: 2026-06-17 (Phase 2 updated)
Unity: 2022.3.62f3 LTS
Test Framework: com.unity.test-framework@1.4.5

## Overview

This document explains how to run Unity tests, the critical `-quit` pitfall, the recovery strategy, and the CI fallback runner.

## CRITICAL: Do NOT combine -runTests with -quit

### Root cause (Phase 1 finding)

Unity 2022.3.62f3 with test-framework 1.4.5 in batchmode: **`-quit` causes Unity to exit immediately after project load, before the Test Runner begins executing tests.**

Evidence from `TestResults/editmode-editor.log` (Phase 1, with `-quit`):

```
Application.AssetDatabase Initial Refresh End
...
Batchmode quit successfully invoked - shutting down!     <-- line 311
...
[Performance] InitializeOnLoad TestRunnerApiListener     <-- during shutdown dump
[Performance] InitializeOnLoad TestStarter.Initialize    <-- during shutdown dump
...
Exiting batchmode successfully now!
Exiting without the bug reporter. Application will terminate with return code 0
```

The `TestRunnerApiListener` and `TestStarter.Initialize` are `InitializeOnLoad` callbacks that fire during shutdown — too late to start test execution. The actual test pipeline never runs.

### The fix (Phase 2)

**Remove `-quit` from all `-runTests` commands.** Unity's Test Runner will exit the process itself when tests complete (exit code 0 = all pass, 2 = failures). No external `-quit` is needed.

### Verification (Phase 2)

After removing `-quit`:

| Test run | XML generated? | Result |
|---|---|---|
| EditMode `AIStopDistanceTests` (3 tests) | YES | 3 passed, 0 failed |
| EditMode 7 targeted classes (35 tests) | YES | 30 passed, 5 failed |
| PlayMode `DebugController_F2_ResetsHP` (1 test) | YES | 0 passed, 1 failed |

XML generation is **RECOVERED**. The `-quit` removal is the fix.

## How to run tests

### Option 1: Batchmode WITHOUT -quit (RECOMMENDED — recovered)

Standardized scripts in `scripts/` omit `-quit` and rely on Unity's Test Runner to self-exit:

| Script | Platform | Purpose |
|---|---|---|
| `scripts/run_unity_editmode_tests.ps1` | Windows PowerShell | EditMode batchmode (no -quit) |
| `scripts/run_unity_playmode_tests.ps1` | Windows PowerShell | PlayMode batchmode (no -quit) |
| `scripts/run_unity_all_tests.ps1` | Windows PowerShell | Both, sequential |
| `scripts/run_unity_editmode_tests.sh` | macOS/Linux bash | EditMode batchmode (no -quit) |
| `scripts/run_unity_playmode_tests.sh` | macOS/Linux bash | PlayMode batchmode (no -quit) |
| `scripts/parse_unity_test_results.py` | Any Python 3 | Parse NUnit XML or JSON |

Usage (PowerShell):

```powershell
# EditMode (all tests)
.\scripts\run_unity_editmode_tests.ps1

# EditMode (filtered)
.\scripts\run_unity_editmode_tests.ps1 -TestFilter "LuoLuoTrip.Tests.EditMode.RuntimeServiceLifecycleTests"

# PlayMode
.\scripts\run_unity_playmode_tests.ps1

# Both
.\scripts\run_unity_all_tests.ps1
```

### Option 2: CI Fallback Runner (-executeMethod)

If batchmode `-runTests` ever breaks again (e.g. Unity upgrade, package change), use the CI fallback runner:

| Script | Purpose |
|---|---|
| `scripts/run_unity_editmode_tests_ci.ps1` | EditMode via `-executeMethod` |
| `scripts/run_unity_playmode_tests_ci.ps1` | PlayMode via `-executeMethod` |

The fallback runner uses `UnityTestBatchRunner` (`Assets/Scripts/Editor/CI/UnityTestBatchRunner.cs`) which:
1. Calls `TestRunnerApi.Execute()` programmatically.
2. Registers callbacks to collect results.
3. Writes JSON summary to `TestResults/ci-*-summary.json`.
4. Calls `EditorApplication.Exit()` when done (so `-quit` IS safe with this method).

The `-quit` flag is **safe** with `-executeMethod` because the runner controls exit timing via callback, not project-load completion.

Usage:

```powershell
.\scripts\run_unity_editmode_tests_ci.ps1
.\scripts\run_unity_editmode_tests_ci.ps1 -TestFilter "LuoLuoTrip.Tests.EditMode.RuntimeServiceLifecycleTests"
```

### Option 3: Unity Editor Test Runner

1. Close all Unity batchmode instances.
2. Open the project in Unity Editor 2022.3.62f3.
3. Window > General > Test Runner.
4. Select EditMode or PlayMode tab.
5. Click Run All or Run Selected.

## Timeout strategy

Since `-quit` is removed, Unity will self-exit when tests finish. But if tests hang, the process won't exit. The scripts handle this:

- **PowerShell**: `-TimeoutSeconds` parameter (default 600 for EditMode, 900 for PlayMode). The script waits for Unity to self-exit; if you need hard timeout, wrap with external tooling.
- **bash**: `timeout` command wraps the Unity process.

Unity's Test Runner is generally reliable about exiting. The timeout is a safety net, not the primary mechanism.

## XML missing troubleshooting checklist

1. **Did you use `-quit` with `-runTests`?** Remove it. This is the #1 cause.
2. **Is Unity Editor still running?** Close it. Two instances can't open the same project.
3. **Did compilation succeed?** Check log for `error CS` or `Scripts have compiler errors`.
4. **Is the output directory writable?** Check `TestResults/` exists and is writable.
5. **Is `-testResults` an absolute path?** Relative paths may not work in batchmode.
6. **Is `-logFile` specified?** Always specify it for dedicated log output.
7. **Did the Test Runner actually start?** Search log for `TestStarter` — if it appears in the Performance shutdown dump, tests never ran.

## Editor.log locations

| Platform | Default location |
|---|---|
| Windows | `%LOCALAPPDATA%\Unity\Editor\Editor.log` |
| macOS | `~/Library/Logs/Unity/Editor.log` |
| Linux | `~/.config/unity3d/Editor.log` |

When using `-logFile <path>`, Unity writes to the specified path instead.

This project's batchmode logs:
- `TestResults/editmode-editor.log`
- `TestResults/playmode-editor.log`
- `TestResults/ci-editmode-editor.log`
- `TestResults/ci-playmode-editor.log`

## Why compile pass != tests pass

| Stage | What happens | XML generated? |
|---|---|---|
| Unity starts batchmode | Process launches | No |
| Script compilation | C# files compiled to DLLs | No |
| Domain reload | Unity loads compiled assemblies | No |
| Project load | Asset database refresh, initialization | No |
| **With `-quit`: shutdown triggers** | Unity exits | **No** |
| **Without `-quit`: Test Runner starts** | NUnit pipeline runs | **Yes (after tests)** |
| Test Runner finishes | Unity self-exits | XML written |

Compile success only means the C# code is valid. It does not mean any test method was invoked.

## parse_unity_test_results.py

Supports both NUnit XML and JSON summary files:

```bash
# Parse NUnit XML
python scripts/parse_unity_test_results.py TestResults/editmode-results.xml

# Parse JSON (from CI runner)
python scripts/parse_unity_test_results.py TestResults/ci-editmode-summary.json

# With explicit log path
python scripts/parse_unity_test_results.py TestResults/editmode-results.xml TestResults/editmode-editor.log
```

Exit codes: 0 = pass, 1 = fail or file missing, 2 = parse error.
