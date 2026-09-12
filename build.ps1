param(
    [string]$KspRoot = (Join-Path $PSScriptRoot 'template_instance'),
    [switch]$Install,
    [switch]$SmokeTest
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
$compiled = Join-Path $taskRoot 'src\Ringworld.KSP\bin\Release\net472'
foreach ($dll in @('NivenRingworld.dll','Ringworld.Core.dll')) { Copy-Item -LiteralPath (Join-Path $compiled $dll) -Destination (Join-Path $stage 'Plugins') -Force }
if ($Install) {
    $target = Join-Path $gameRoot 'GameData\NivenRingworld'
    New-Item -ItemType Directory -Path $target -Force | Out-Null
    Copy-Item -Path (Join-Path $stage '*') -Destination $target -Recurse -Force
    Write-Host "Installed into $target"
}
if (-not $SmokeTest) {
    foreach ($doc in @('README.md','LICENSE','docs\KNOWN-LIMITATIONS.md','docs\CANON-AND-SCALE.md')) {
        $source = Join-Path $taskRoot $doc
        if (Test-Path -LiteralPath $source) { Copy-Item -LiteralPath $source -Destination (Join-Path $taskRoot 'artifacts\NivenRingworld') -Force }
    }
    Compress-Archive -Path (Join-Path $taskRoot 'artifacts\NivenRingworld\*') -DestinationPath (Join-Path $taskRoot 'artifacts\NivenRingworld-0.1.0.zip') -Force
}
Write-Host "Build staged in $stage"
