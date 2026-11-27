param(
    [string]$Job,
    [int]$Seeds = 1,
    [int]$Workers = 0,
    [ValidateSet("scan", "bake", "answers")][string]$Mode = "scan",
    [string]$ProjectPath,
    [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.6.10f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
if (-not $ProjectPath) {
    $ProjectPath = Join-Path $PSScriptRoot "..\..\..\our_space_bake"
}

if ($Workers -le 0) {
    $Workers = [Math]::Max(1, [Environment]::ProcessorCount - 1)
}
if ($Mode -ne "answers" -and [string]::IsNullOrEmpty($Job)) {
    throw "-Job is required for -Mode $Mode."
}

$project = (Resolve-Path $ProjectPath).Path
$logs = Join-Path $project "Logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null

$method = switch ($Mode) {
    "bake" { "PuzzleBatchBake.Bake" }
    "answers" { "PuzzleAnswerBackfill.BackfillBatch" }
    default { "PuzzleBatchBake.Scan" }
}
$execute = "DigitalLove.Game.SpaceShaping.Creator.EditorTools.$method"

# Unity.exe is a GUI-subsystem program: "&" returns before it finishes, so wait on the process.
$logName = if ($Mode -eq "answers") { "answers" } else { $Job }
$arguments = @(
    "-batchmode", "-nographics", "-projectPath", "`"$project`"", "-executeMethod", $execute
)
if ($Mode -ne "answers") {
    $arguments += @("-job", $Job, "-seeds", $Seeds, "-workers", $Workers)
}
$arguments += @("-logFile", "`"$(Join-Path $logs "bake-$logName.log")`"")
# Wait on Unity itself: -Wait would also wait on any helper Unity leaves behind.
$unityProcess = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -NoNewWindow
$unityProcess.WaitForExit()
exit $unityProcess.ExitCode
