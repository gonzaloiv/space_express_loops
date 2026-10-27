# check-docs.ps1 — validate the design docs.
#
# Checks, under docs/design/:
#   1. every design doc (except README) has a **Status:** line;
#   2. every design doc is referenced from docs/design/README.md;
#   3. no design doc uses the retired gate names (the vision §7 go/no-go is the "validation").
#
# Link targets across docs/ are already checked by docs/plans/check-plans.ps1.
# Exit code 1 if any problem is found.
# Run: powershell -NoProfile -File docs/design/check-docs.ps1

$ErrorActionPreference = 'Stop'

$designDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$readme    = Join-Path $designDir 'README.md'

$problems = New-Object System.Collections.Generic.List[string]
$readmeText = if (Test-Path -LiteralPath $readme) { [System.IO.File]::ReadAllText($readme) } else { '' }

$retired = @('device gate', 'Gate B', 'lens gate')

foreach ($f in Get-ChildItem -Path $designDir -Recurse -Filter *.md -File) {
    if ($f.Name -eq 'README.md') { continue }
    $rel = $f.FullName.Substring($designDir.Length + 1)

    $text = [System.IO.File]::ReadAllText($f.FullName)

    if ($text -notmatch '(?m)^\*\*Status:\*\*') {
        $problems.Add("NO STATUS      $rel")
    }

    if (-not $readmeText.Contains($f.Name)) {
        $problems.Add("NOT ON BOARD   $rel")
    }

    foreach ($term in $retired) {
        if ($text -match [regex]::Escape($term)) {
            $problems.Add("RETIRED TERM   {0}  ('{1}' -> 'validation')" -f $rel, $term)
        }
    }
}

if ($problems.Count -gt 0) {
    Write-Host ("check-docs: {0} problem(s)" -f $problems.Count) -ForegroundColor Red
    foreach ($problem in $problems) { Write-Host ("  {0}" -f $problem) }
    exit 1
}
Write-Host "check-docs: OK" -ForegroundColor Green
exit 0
