#!/usr/bin/env bash
#
# Run Unity EditMode tests in batchmode.
# IMPORTANT: Does NOT pass -quit. -quit causes Unity to exit before Test Runner starts.
# Usage: ./run_unity_editmode_tests.sh [UNITY_PATH] [TEST_FILTER] [TIMEOUT_SECONDS]
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
RESULTS_DIR="$REPO_ROOT/TestResults"

UNITY_PATH="${1:-/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity}"
TEST_FILTER="${2:-}"
TIMEOUT_SECONDS="${3:-600}"

XML_PATH="$RESULTS_DIR/editmode-results.xml"
LOG_PATH="$RESULTS_DIR/editmode-editor.log"

if [ ! -f "$UNITY_PATH" ]; then
    echo "ERROR: Unity executable not found at: $UNITY_PATH" >&2
    exit 1
fi

mkdir -p "$RESULTS_DIR"

# Clean old results
rm -f "$XML_PATH" "$LOG_PATH"

echo "=== Unity EditMode Test Run ==="
echo "Unity:   $UNITY_PATH"
echo "Project: $REPO_ROOT"
echo "XML:     $XML_PATH"
echo "Log:     $LOG_PATH"
echo "Timeout: $TIMEOUT_SECONDS s"
if [ -n "$TEST_FILTER" ]; then
    echo "Filter:  $TEST_FILTER"
fi
echo ""

# NOTE: -quit is intentionally OMITTED.
CMD_ARGS=(
    -batchmode
    -runTests
    -projectPath "$REPO_ROOT"
    -testPlatform EditMode
    -testResults "$XML_PATH"
    -logFile "$LOG_PATH"
)
if [ -n "$TEST_FILTER" ]; then
    CMD_ARGS+=(-testFilter "$TEST_FILTER")
fi

echo "Command: $UNITY_PATH ${CMD_ARGS[*]}"
echo ""

if [ "$TIMEOUT_SECONDS" -gt 0 ] 2>/dev/null; then
    timeout "$TIMEOUT_SECONDS" "$UNITY_PATH" "${CMD_ARGS[@]}" || true
else
    "$UNITY_PATH" "${CMD_ARGS[@]}" || true
fi
EXIT_CODE=$?
echo "Unity exit code: $EXIT_CODE"
echo ""

if [ -f "$XML_PATH" ]; then
    echo "[OK] Test result XML generated."
    PY_SCRIPT="$SCRIPT_DIR/parse_unity_test_results.py"
    if [ -f "$PY_SCRIPT" ]; then
        PYTHON_EXE=$(command -v python3 || command -v python || true)
        if [ -n "$PYTHON_EXE" ]; then
            "$PYTHON_EXE" "$PY_SCRIPT" "$XML_PATH"
            if [ $? -ne 0 ]; then exit 1; fi
        else
            echo "Python not found; skipping parse."
        fi
    fi
    exit 0
else
    echo "[FAIL] Test result XML NOT generated." >&2
    echo ""
    echo "=== Scanning Editor.log for root cause ==="
    if [ -f "$LOG_PATH" ]; then
        grep -iE "Batchmode quit successfully invoked|error CS|Scripts have compiler errors|TestRunner|TestStarter|NUnit|exception|Exception|another Unity instance|project open|License|Exiting batchmode" "$LOG_PATH" || true
    else
        echo "  Editor.log not found at: $LOG_PATH"
    fi
    echo ""
    echo "See: Assets/Docs/TEST_RUNNER_RELIABILITY.md"
    exit 1
fi
