param([int]$TimeoutSeconds=900)
$ErrorActionPreference='Stop'
$taskRoot=$PSScriptRoot
$gameRoot=Join-Path $taskRoot 'template_instance'
$logName='RingworldSmoke-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log'
$logPath=Join-Path $gameRoot $logName
$testProcess=$null
try {
    & (Join-Path $taskRoot 'build.ps1') -Install -SmokeTest
    $testProcess=Start-Process -FilePath (Join-Path $gameRoot 'KSP_x64.exe') -WorkingDirectory $gameRoot -ArgumentList @('-ringworld-smoketest','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-popupwindow','-logFile',$logName) -WindowStyle Hidden -PassThru
    if (-not $testProcess.WaitForExit($TimeoutSeconds*1000)) {
        $testProcess.Kill()
        $testProcess.WaitForExit()
        throw "Game smoke test timed out. Log: $logPath"
    }
    $text=Get-Content -LiteralPath $logPath -Raw
    $reportDir=Join-Path $taskRoot 'artifacts\validation'
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    $text -split '\r?\n' | Where-Object { $_ -match '\[RingworldSmoke\]|\[NivenRingworld\]' } | Set-Content -LiteralPath (Join-Path $reportDir 'game-smoke.txt')
    if ($text -notmatch '\[RingworldSmoke\] PASS' -or $text -match '\[RingworldSmoke\] FAIL') { throw "Game smoke test failed. Log: $logPath" }
    Write-Host "Game smoke test passed. Log: $logPath"
}
finally {
    if ($testProcess -and -not $testProcess.HasExited) { $testProcess.Kill(); $testProcess.WaitForExit() }
    & (Join-Path $taskRoot 'build.ps1') -Install
}
