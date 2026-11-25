param(
    [Parameter(Mandatory = $true)][string]$Job,
    [int]$Seeds = 1,
    [ValidateSet("scan", "bake")][string]$Mode = "scan",
    [string]$ProjectPath,
    [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.6.10f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
if (-not $ProjectPath) {
    $ProjectPath = Join-Path $PSScriptRoot "..\..\..\our_space_bake"
}

$project = (Resolve-Path $ProjectPath).Path
$logs = Join-Path $project "Logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null

$method = if ($Mode -eq "bake") { "PuzzleBatchBake.Bake" } else { "PuzzleBatchBake.Scan" }
$execute = "DigitalLove.Game.SpaceShaping.Creator.EditorTools.$method"

& $Unity -batchmode -nographics -projectPath $project -executeMethod $execute -job $Job -seeds $Seeds -logFile (Join-Path $logs "bake-$Job.log")
exit $LASTEXITCODE
