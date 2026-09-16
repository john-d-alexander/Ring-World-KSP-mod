param(
    [string]$KspRoot = (Join-Path $PSScriptRoot 'template_instance'),
    [switch]$Install,
    [switch]$SmokeTest,
    [switch]$Package
)
$ErrorActionPreference = 'Stop'
$taskRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$gameRoot = (Resolve-Path -LiteralPath $KspRoot).Path
if (-not (Test-Path -LiteralPath (Join-Path $gameRoot 'KSP_x64_Data\Managed\Assembly-CSharp.dll'))) { throw 'KspRoot is not a KSP 1 installation.' }
& dotnet run --project (Join-Path $taskRoot 'tests\Ringworld.Tests') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Core verification failed.' }
$buildArguments = @('build', (Join-Path $taskRoot 'src\Ringworld.KSP\Ringworld.KSP.csproj'), '-c', 'Release', '--nologo', "-p:KspRoot=$gameRoot")
if ($SmokeTest) { $buildArguments += '-p:SmokeTest=true' }
& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) { throw 'Plugin build failed.' }
$stage = Join-Path $taskRoot 'artifacts\NivenRingworld\GameData\NivenRingworld'
New-Item -ItemType Directory -Path (Join-Path $stage 'Plugins') -Force | Out-Null
Copy-Item -Path (Join-Path $taskRoot 'GameData\NivenRingworld\*') -Destination $stage -Recurse -Force
$harmonyStage = Join-Path $taskRoot 'artifacts\NivenRingworld\GameData\000_Harmony'
New-Item -ItemType Directory -Path $harmonyStage -Force | Out-Null
Copy-Item -Path (Join-Path $taskRoot 'vendor\HarmonyKSP\GameData\000_Harmony\*') -Destination $harmonyStage -Force
$compiled = Join-Path $taskRoot 'src\Ringworld.KSP\bin\Release\net472'
foreach ($dll in @('NivenRingworld.dll','Ringworld.Core.dll')) { Copy-Item -LiteralPath (Join-Path $compiled $dll) -Destination (Join-Path $stage 'Plugins') -Force }
if ($Install) {
    $target = Join-Path $gameRoot 'GameData\NivenRingworld'
    New-Item -ItemType Directory -Path $target -Force | Out-Null
    Copy-Item -Path (Join-Path $stage '*') -Destination $target -Recurse -Force
    $harmonyTarget = Join-Path $gameRoot 'GameData\000_Harmony'
    New-Item -ItemType Directory -Path $harmonyTarget -Force | Out-Null
    Copy-Item -Path (Join-Path $harmonyStage '*') -Destination $harmonyTarget -Force
    Write-Host "Installed into $target"
}
if ($Package -and -not $SmokeTest) {
    foreach ($doc in @('README.md','LICENSE','docs\KNOWN-LIMITATIONS.md','docs\CANON-AND-SCALE.md','docs\VALIDATION.md','docs\ORBITAL-ARRIVAL.md','docs\GROUND-AND-EVA.md','docs\TERRAIN-LOD.md','docs\SETTINGS-AND-HORIZON.md','docs\ORBITS-WARP-AND-ASSETS.md','docs\BIOMES-AND-GRAPHICS.md','docs\ASSET-TRACKER.md','docs\BIOME-ASSET-CATALOG.md','docs\BIOME-FREQUENCY-SURVEY.txt','docs\STOCK-WARP-AND-RENDERING.md','docs\HIGH-END-VISUALS.md','docs\WEATHER-AND-NIGHT.md','docs\BLENDER-ASSETS.md','docs\GLOBAL-CLOUDS.md','docs\GRAPHICS-DIAGNOSIS.md','docs\RESIDENCE-AND-ENCOUNTERS.md','docs\HABITAT-LIBRARY.md','docs\MOD-INTEROPERABILITY.md','docs\LANDMARK-ASSETS.md','docs\LANDMARK-INVENTORY.md','docs\RELEASE-1.0.0.md','docs\RELEASE-1.0.1.md','docs\RELEASE-1.0.2.md','docs\RELEASE-1.0.3.md','docs\QUALITY-PRESETS.md','docs\CKAN-PUBLISHING.md','docs\COLOSSI-AND-FORESTS.md')) {
        $source = Join-Path $taskRoot $doc
        if (Test-Path -LiteralPath $source) {
            $docTarget = Join-Path (Join-Path $taskRoot 'artifacts\NivenRingworld') $doc
            New-Item -ItemType Directory -Path (Split-Path -Parent $docTarget) -Force | Out-Null
            Copy-Item -LiteralPath $source -Destination $docTarget -Force
        }
    }
    $releaseVersion = ([xml](Get-Content -LiteralPath (Join-Path $taskRoot 'src\Ringworld.KSP\Ringworld.KSP.csproj') -Raw)).Project.PropertyGroup.Version
    Compress-Archive -Path (Join-Path $taskRoot 'artifacts\NivenRingworld\*') -DestinationPath (Join-Path $taskRoot "artifacts\NivenRingworld-$releaseVersion.zip") -Force
}
Write-Host "Build staged in $stage"



