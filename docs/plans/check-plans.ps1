# check-plans.ps1 — validate the plans board.
#
# Checks, under docs/:
#   1. every markdown link target resolves;
#   2. every plan's **Status:** state matches its folder (in-progress -> now, backlog -> next|later);
#   3. every plan is referenced on the board (docs/plans/README.md).
#
# Exit code 1 if any problem is found. Run: powershell -NoProfile -File docs/plans/check-plans.ps1

$ErrorActionPreference = 'Stop'

$plansDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$docsDir  = Split-Path -Parent $plansDir
$board    = Join-Path $plansDir 'README.md'

$problems = New-Object System.Collections.Generic.List[string]

function Get-AllowedStates([string] $folder) {
    switch ($folder) {
        'in-progress' { return @('now') }
        'backlog'     { return @('next', 'later') }
        default       { return @() }
    }
}

# 1. link targets
foreach ($f in Get-ChildItem -Path $docsDir -Recurse -Filter *.md -File) {
    $text = [System.IO.File]::ReadAllText($f.FullName)
    foreach ($m in [regex]::Matches($text, '\]\(([^)]+)\)')) {
        $link = $m.Groups[1].Value
        if ($link -match '^(https?:|#|mailto:)') { continue }
        $target = $link.Split('#')[0]
        if ($target -eq '') { continue }
        if (-not (Test-Path -LiteralPath (Join-Path $f.DirectoryName $target))) {
            $rel = $f.FullName.Substring($docsDir.Length + 1)
            $problems.Add("BROKEN LINK    $rel  ->  $link")
        }
    }
}

# 2. status vs folder, and 3. on the board
$boardText = if (Test-Path -LiteralPath $board) { [System.IO.File]::ReadAllText($board) } else { '' }
foreach ($p in Get-ChildItem -Path $plansDir -Recurse -Filter *.md -File) {
    $rel = $p.FullName.Substring($plansDir.Length + 1)
    if ($rel -notmatch '^(?<folder>[^\\/]+)[\\/]') { continue }
    $folder  = $Matches['folder']
    $allowed = @(Get-AllowedStates $folder)
    if ($allowed.Count -eq 0) { continue }

    $text = [System.IO.File]::ReadAllText($p.FullName)
    $m = [regex]::Match($text, '(?m)^\*\*Status:\*\*\s*([^\r\n]+)')
    if (-not $m.Success) {
        $problems.Add("NO STATUS      {0}" -f $rel)
    }
    else {
        $state = ($m.Groups[1].Value -split '·')[0].Trim().ToLower().Split(' ')[0]
        if ($allowed -notcontains $state) {
            $problems.Add(("STATE MISMATCH {0}  (status '{1}', folder expects {2})" -f $rel, $state, ($allowed -join '/')))
        }
    }
    if (-not $boardText.Contains($p.Name)) {
        $problems.Add("NOT ON BOARD   {0}" -f $rel)
    }
}

if ($problems.Count -gt 0) {
    Write-Host ("check-plans: {0} problem(s)" -f $problems.Count) -ForegroundColor Red
    foreach ($problem in $problems) { Write-Host ("  {0}" -f $problem) }
    exit 1
}
Write-Host "check-plans: OK" -ForegroundColor Green
exit 0
