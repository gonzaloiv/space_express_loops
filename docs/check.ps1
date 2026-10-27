# check.ps1 — run every docs check.
#
# Runs docs/plans/check-plans.ps1 (links, plan status/folder, board) and
# docs/design/check-docs.ps1 (design status, on-board, retired terms).
# Exit code 1 if either fails.
# Run: powershell -NoProfile -File docs/check.ps1

$ErrorActionPreference = 'Stop'

$docsDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$checks = @(
    'plans/check-plans.ps1',
    'design/check-docs.ps1'
)

$failed = $false
foreach ($rel in $checks) {
    $path = Join-Path $docsDir $rel
    if (-not (Test-Path -LiteralPath $path)) {
        Write-Host ("check: missing {0}" -f $rel) -ForegroundColor Red
        $failed = $true
        continue
    }

    & powershell -NoProfile -File $path
    if ($LASTEXITCODE -ne 0) { $failed = $true }
}

if ($failed) {
    Write-Host "check: FAILED" -ForegroundColor Red
    exit 1
}
Write-Host "check: OK" -ForegroundColor Green
exit 0
