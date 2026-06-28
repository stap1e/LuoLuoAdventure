<#
.SYNOPSIS
    Run Unity PlayMode tests via CI fallback runner (-executeMethod).
.DESCRIPTION
    Uses UnityTestBatchRunner.RunPlayModeTests instead of -runTests.
    This is a fallback for environments where -runTests + -quit does not work.
    The runner writes JSON summary. The -quit flag is SAFE here because
    the CI runner calls EditorApplication.Exit itself after tests complete.
.PARAMETER UnityPath
    Full path to Unity.exe. Defaults to 2022.3.62f3 LTS.
.PARAMETER TestFilter
    Optional semicolon-separated test filter (set via LUOLUO_TEST_FILTER env var).
.EXAMPLE
    .\run_unity_playmode_tests_ci.ps1
#>
[CmdletBinding()]
param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe",
    [string]$TestFilter = ""
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path "$PSScriptRoot\.."
$ProjectPath = $RepoRoot.Path
$ResultsDir = Join-Path $RepoRoot "TestResults"
$JsonPath = Join-Path $ResultsDir "ci-playmode-summary.json"
$LogPath = Join-Path $ResultsDir "ci-playmode-editor.log"

if (-not (Test-Path -LiteralPath $UnityPath)) {
    Write-Error "Unity.exe not found at: $UnityPath"
    exit 1
}

if (-not (Test-Path -LiteralPath $ResultsDir)) {
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
}

if (Test-Path -LiteralPath $JsonPath) { Remove-Item -LiteralPath $JsonPath -Force }
if (Test-Path -LiteralPath $LogPath)  { Remove-Item -LiteralPath $LogPath -Force }

Write-Host "=== Unity PlayMode CI Test Run ===" -ForegroundColor Cyan
Write-Host "Unity:     $UnityPath"
Write-Host "Project:   $ProjectPath"
Write-Host "JSON:      $JsonPath"
Write-Host "Log:       $LogPath"
if ($TestFilter) { Write-Host "Filter:    $TestFilter" }
Write-Host ""

$env:LUOLUO_TEST_FILTER = $TestFilter

$argList = @(
    "-batchmode",
    "-projectPath", $ProjectPath,
    "-executeMethod", "LuoLuoTrip.Editor.CI.UnityTestBatchRunner.RunPlayModeTests",
    "-logFile", $LogPath,
    "-quit"
)

Write-Host "Command: $UnityPath $($argList -join ' ')"
Write-Host ""

$proc = Start-Process -FilePath $UnityPath -ArgumentList $argList -Wait -PassThru -NoNewWindow
$exitCode = $proc.ExitCode
Write-Host "Unity exit code: $exitCode"
Write-Host ""

if (Test-Path -LiteralPath $JsonPath) {
    Write-Host "[OK] CI summary JSON generated." -ForegroundColor Green
    $pyScript = Join-Path $PSScriptRoot "parse_unity_test_results.py"
    if (Test-Path -LiteralPath $pyScript) {
        $pyExe = $null
        $cmd = Get-Command python -ErrorAction SilentlyContinue
        if (-not $cmd) { $cmd = Get-Command python3 -ErrorAction SilentlyContinue }
        if ($cmd) { $pyExe = $cmd.Source }
        if ($pyExe) {
            & $pyExe $pyScript $JsonPath
            if ($LASTEXITCODE -ne 0) { exit 1 }
        } else {
            Write-Host "Python not found; skipping parse." -ForegroundColor Yellow
        }
    }
    exit 0
} else {
    Write-Host "[FAIL] CI summary JSON NOT generated." -ForegroundColor Red
    Write-Host ""
    Write-Host "=== Scanning Editor.log for root cause ===" -ForegroundColor Yellow
    if (Test-Path -LiteralPath $LogPath) {
        $logContent = Get-Content -LiteralPath $LogPath
        $patterns = @(
            "CI Runner",
            "error CS",
            "Scripts have compiler errors",
            "exception",
            "Exception",
            "executeMethod",
            "License",
            "Exiting batchmode"
        )
        foreach ($line in $logContent) {
            foreach ($pat in $patterns) {
                if ($line -match $pat) {
                    Write-Host "  $line"
                    break
                }
            }
        }
    } else {
        Write-Host "  Editor.log not found at: $LogPath"
    }
    Write-Host ""
    Write-Host "See: Assets/Docs/TEST_RUNNER_RELIABILITY.md" -ForegroundColor Yellow
    exit 1
}
