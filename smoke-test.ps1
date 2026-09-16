param([int]$TimeoutSeconds=900,[switch]$MapOnly,[switch]$TerrainOnly,[switch]$WarpOnly,[switch]$UnmatchedOnly,[switch]$PhotoOnly,[switch]$DistantOnly,[switch]$WeatherOnly,[switch]$SceneryOnly,[switch]$GlobalCloudsOnly,[switch]$ResidenceOnly,[switch]$GuidanceOnly,[switch]$StabilityOnly,[switch]$LandmarksOnly,[switch]$GearOnly)
$ErrorActionPreference='Stop'
$taskRoot=$PSScriptRoot
$gameRoot=Join-Path $taskRoot 'template_instance'
$logName='RingworldSmoke-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log'
$logPath=Join-Path $gameRoot $logName
$testProcess=$null
try {
    & (Join-Path $taskRoot 'build.ps1') -Install -SmokeTest
    $taskArguments=@('-ringworld-smoketest','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-popupwindow','-logFile',$logName)
    if ($GearOnly) { $taskArguments += '-ringworld-gear-only' }
    if ($LandmarksOnly) { $taskArguments += '-ringworld-landmarks-only' }
    if ($StabilityOnly) { $taskArguments += '-ringworld-stability-only' }
    if ($GuidanceOnly) { $taskArguments += '-ringworld-guidance-only' }
    if ($ResidenceOnly) { $taskArguments += '-ringworld-residence-only' }
    if ($WeatherOnly) { $taskArguments += '-ringworld-weather-only' }
    if ($GlobalCloudsOnly) { $taskArguments += '-ringworld-global-clouds-only' }
    if ($SceneryOnly) { $taskArguments += '-ringworld-scenery-only' }
    if ($DistantOnly) { $taskArguments += '-ringworld-distant-only' }
    if ($PhotoOnly -or $DistantOnly) { $taskArguments += '-ringworld-photo-only' }
    if ($UnmatchedOnly) { $taskArguments += '-ringworld-unmatched-only' }
    if ($MapOnly) { $taskArguments += '-ringworld-map-only' }
    if ($TerrainOnly -or $WarpOnly -or $WeatherOnly) { $taskArguments += '-ringworld-terrain-only' }
    if ($WarpOnly) { $taskArguments += '-ringworld-warp-only' }
    $testProcess=Start-Process -FilePath (Join-Path $gameRoot 'KSP_x64.exe') -WorkingDirectory $gameRoot -ArgumentList $taskArguments -WindowStyle Hidden -PassThru
    if (-not $testProcess.WaitForExit($TimeoutSeconds*1000)) {
        $testProcess.Kill()
        $testProcess.WaitForExit()
        throw "Game smoke test timed out. Log: $logPath"
    }
    $text=Get-Content -LiteralPath $logPath -Raw
    $reportDir=Join-Path $taskRoot 'artifacts\validation'
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    $reportName='game-smoke.txt'
    if ($GearOnly) { $reportName='gear-smoke.txt' }
    if ($LandmarksOnly) { $reportName='landmarks-smoke.txt' }
    if ($StabilityOnly) { $reportName='stability-smoke.txt' }
    if ($GuidanceOnly) { $reportName='guidance-smoke.txt' }
    if ($ResidenceOnly) { $reportName='residence-smoke.txt' }
    if ($PhotoOnly) { $reportName='photo-smoke.txt' }
    if ($DistantOnly) { $reportName='distant-smoke.txt' }
    if ($UnmatchedOnly) { $reportName='unmatched-smoke.txt' }
    if ($MapOnly) { $reportName='map-smoke.txt' }
    if ($TerrainOnly) { $reportName='terrain-smoke.txt' }
    if ($WarpOnly) { $reportName='warp-smoke.txt' }
    if ($WeatherOnly) { $reportName='weather-smoke.txt' }
    if ($GlobalCloudsOnly) { $reportName='global-clouds-smoke.txt' }
    if ($SceneryOnly) { $reportName='scenery-smoke.txt' }
    $text -split '\r?\n' | Where-Object { $_ -match '\[RingworldSmoke\]|\[NivenRingworld\]' } | Set-Content -LiteralPath (Join-Path $reportDir $reportName)
    if ($GearOnly -and $SceneryOnly -and $LandmarksOnly -and $GuidanceOnly) {
        foreach ($name in @('gear','guidance','scenery','landmarks')) {
            $text -split '\r?\n' | Where-Object { $_ -match '\[RingworldSmoke\]|\[NivenRingworld\]' } | Set-Content -LiteralPath (Join-Path $reportDir ($name+'-smoke.txt'))
        }
    }
    if ($text -notmatch '\[RingworldSmoke\] PASS' -or $text -match '\[RingworldSmoke\] FAIL') { throw "Game smoke test failed. Log: $logPath" }
    Write-Host "Game smoke test passed. Log: $logPath"
}
finally {
    if ($testProcess -and -not $testProcess.HasExited) { $testProcess.Kill(); $testProcess.WaitForExit() }
    & (Join-Path $taskRoot 'build.ps1') -Install
}


