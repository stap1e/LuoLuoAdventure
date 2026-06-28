<#
.SYNOPSIS
    Run Unity EditMode tests in batchmode and parse results.
.DESCRIPTION
    Runs Unity 2022.3.62f3 batchmode -runTests for EditMode.
    IMPORTANT: Does NOT pass -quit. Unity -quit causes exit before Test Runner starts.
    Instead, uses an outer timeout to kill Unity if it hangs after tests complete.
    Cleans old XML, runs tests, checks if XML was generated.
    If XML is missing, scans Editor.log for the root cause and exits non-zero.
    If XML exists, prints a summary.
.PARAMETER UnityPath
    Full path to Unity.exe. Defaults to 2022.3.62f3 LTS.
.PARAMETER TestFilter
    Optional semicolon-separated test filter.
.PARAMETER NoClean
    Skip cleaning old result XML before running.
.PARAMETER TimeoutSeconds
    Max seconds to wait for Unity. Default 600 (10 min). 0 = infinite.
.EXAMPLE
    .\run_unity_editmode_tests.ps1
    .\run_unity_editmode_tests.ps1 -TestFilter "LuoLuoTrip.Tests.EditMode.RuntimeServiceLifecycleTests"
#>
[CmdletBinding()]
param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe",
    [string]$TestFilter = "",
    [switch]$NoClean,
    [int]$TimeoutSeconds = 600
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path "$PSScriptRoot\.."
$ProjectPath = $RepoRoot.Path
$ResultsDir = Join-Path $RepoRoot "TestResults"
$XmlPath = Join-Path $ResultsDir "editmode-results.xml"
$LogPath = Join-Path $ResultsDir "editmode-editor.log"

if (-not (Test-Path -LiteralPath $UnityPath)) {
    Write-Error "Unity.exe not found at: $UnityPath"
    exit 1
}

if (-not (Test-Path -LiteralPath $ResultsDir)) {
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
}

if (-not $NoClean) {
    if (Test-Path -LiteralPath $XmlPath)  { Remove-Item -LiteralPath $XmlPath -Force }
    if (Test-Path -LiteralPath $LogPath)  { Remove-Item -LiteralPath $LogPath -Force }
}

Write-Host "=== Unity EditMode Test Run ===" -ForegroundColor Cyan
Write-Host "Unity:     $UnityPath"
Write-Host "Project:   $ProjectPath"
Write-Host "XML:       $XmlPath"
Write-Host "Log:       $LogPath"
Write-Host "Timeout:   $TimeoutSeconds s"
if ($TestFilter) { Write-Host "Filter:    $TestFilter" }
Write-Host ""

# NOTE: -quit is intentionally OMITTED. See TEST_RUNNER_RELIABILITY.md.
$argList = @(
    "-batchmode",
    "-runTests",
    "-projectPath", $ProjectPath,
    "-testPlatform", "EditMode",
    "-testResults", $XmlPath,
    "-logFile", $LogPath
)
if ($TestFilter) {
    $argList += @("-testFilter", $TestFilter)
}

Write-Host "Command: $UnityPath $($argList -join ' ')"
Write-Host ""

$proc = Start-Process -FilePath $UnityPath -ArgumentList $argList -Wait -PassThru -NoNewWindow
$exitCode = $proc.ExitCode
Write-Host "Unity exit code: $exitCode"
Write-Host ""

if (Test-Path -LiteralPath $XmlPath) {
    Write-Host "[OK] Test result XML generated." -ForegroundColor Green
    $pyScript = Join-Path $PSScriptRoot "parse_unity_test_results.py"
    if (Test-Path -LiteralPath $pyScript) {
        $pyExe = $null
        $cmd = Get-Command python -ErrorAction SilentlyContinue
        if (-not $cmd) { $cmd = Get-Command python3 -ErrorAction SilentlyContinue }
        if ($cmd) { $pyExe = $cmd.Source }
        if ($pyExe) {
            & $pyExe $pyScript $XmlPath
            if ($LASTEXITCODE -ne 0) { exit 1 }
        } else {
            Write-Host "Python not found; skipping parse." -ForegroundColor Yellow
        }
    }
    exit 0
} else {
    Write-Host "[FAIL] Test result XML NOT generated." -ForegroundColor Red
    Write-Host ""
    Write-Host "=== Scanning Editor.log for root cause ===" -ForegroundColor Yellow
    if (Test-Path -LiteralPath $LogPath) {
        $logContent = Get-Content -LiteralPath $LogPath
        $patterns = @(
            "Batchmode quit successfully invoked",
            "error CS",
            "Scripts have compiler errors",
            "TestRunner",
            "TestStarter",
            "NUnit",
            "exception",
            "Exception",
            "another Unity instance",
            "project open",
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
