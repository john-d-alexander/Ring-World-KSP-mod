param([string]$UnityEditor='C:\Program Files\Unity\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$taskProject=Join-Path $PSScriptRoot 'tools\VisualShaders'
$taskLog=Join-Path $PSScriptRoot 'art\first-set\validation\unity-build.log'
$taskUnity=Start-Process -FilePath $UnityEditor -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+$taskProject+'"'),'-executeMethod','BuildScenery.Run','-logFile',('"'+$taskLog+'"')) -WindowStyle Hidden -PassThru
$taskUnity.WaitForExit()
$taskText=Get-Content -LiteralPath $taskLog -Raw
if ($taskUnity.ExitCode -ne 0 -or $taskText -notmatch 'RINGWORLD SCENERY BUNDLE BUILT' -or $taskText -match 'Shader error|error CS\d+') { throw "Scenery build failed. See $taskLog" }
Write-Host "Scenery bundle compiled. See $taskLog"
