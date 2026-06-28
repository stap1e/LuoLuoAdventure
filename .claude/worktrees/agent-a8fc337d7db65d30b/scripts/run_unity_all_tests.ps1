<#
.SYNOPSIS
    Run all Unity tests (EditMode then PlayMode) in batchmode.
.DESCRIPTION
    Runs run_unity_editmode_tests.ps1 then run_unity_playmode_tests.ps1 sequentially.
    Stops on first failure. Aggregates exit codes.
.PARAMETER UnityPath
    Full path to Unity.exe. Defaults to 2022.3.62f3 LTS.
.EXAMPLE
    .\run_unity_all_tests.ps1
#>
[CmdletBinding()]
param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot\.."
$ResultsDir = Join-Path $RepoRoot "TestResults"
$SummaryPath = Join-Path $ResultsDir "all-tests-summary.txt"

Write-Host "=== Unity All Tests (EditMode + PlayMode) ===" -ForegroundColor Cyan
Write-Host ""

$editScript = Join-Path $PSScriptRoot "run_unity_editmode_tests.ps1"
$playScript = Join-Path $PSScriptRoot "run_unity_playmode_tests.ps1"

$results = @()

Write-Host "--- Phase 1: EditMode ---" -ForegroundColor Cyan
& $editScript -UnityPath $UnityPath
$editExit = $LASTEXITCODE
$results += [PSCustomObject]@{ Phase = "EditMode"; ExitCode = $editExit; XmlExists = (Test-Path -LiteralPath (Join-Path $ResultsDir "editmode-results.xml")) }
Write-Host "EditMode exit code: $editExit"
Write-Host ""

if ($editExit -ne 0) {
    Write-Host "[WARN] EditMode phase did not produce XML or had failures. Continuing to PlayMode for diagnostic." -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "--- Phase 2: PlayMode ---" -ForegroundColor Cyan
& $playScript -UnityPath $UnityPath
$playExit = $LASTEXITCODE
$results += [PSCustomObject]@{ Phase = "PlayMode"; ExitCode = $playExit; XmlExists = (Test-Path -LiteralPath (Join-Path $ResultsDir "playmode-results.xml")) }
Write-Host "PlayMode exit code: $playExit"
Write-Host ""

Write-Host "=== Summary ===" -ForegroundColor Cyan
foreach ($r in $results) {
    $status = if ($r.XmlExists) { "XML generated" } else { "XML MISSING" }
    $color = if ($r.XmlExists) { "Green" } else { "Red" }
    Write-Host ("  {0,-10} exit={1}  {2}" -f $r.Phase, $r.ExitCode, $status) -ForegroundColor $color
}

$summary = "Unity All Tests Summary`r`n"
$summary += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`r`n"
foreach ($r in $results) {
    $summary += ("  {0,-10} exit={1}  xml={2}`r`n" -f $r.Phase, $r.ExitCode, $(if ($r.XmlExists) { "yes" } else { "NO" }))
}
$summary | Out-File -FilePath $SummaryPath -Encoding utf8

if ($editExit -ne 0 -or $playExit -ne 0) {
    Write-Host ""
    Write-Host "[FAIL] One or more phases failed or did not produce XML." -ForegroundColor Red
    exit 1
}
Write-Host ""
Write-Host "[OK] All phases produced XML." -ForegroundColor Green
exit 0
