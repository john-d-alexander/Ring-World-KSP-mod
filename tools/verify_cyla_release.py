"""Verify the Cyla bundle without weakening the stable release gate."""
import hashlib
import json
import pathlib
import zipfile

root = pathlib.Path(__file__).resolve().parents[1]
archive = root / 'artifacts/NivenRingworld-1.1.1.zip'
report = (root / 'artifacts/validation/cyla-smoke.txt').read_text(encoding='utf-8-sig')
assert '[RingworldSmoke] PASS cyla-only' in report and '[RingworldSmoke] FAIL' not in report
provenance = json.loads((root / 'vendor/Cyla/PROVENANCE.json').read_text(encoding='utf-8-sig'))
with zipfile.ZipFile(archive) as package:
    assert package.testzip() is None, 'ZIP CRC failure'
    names = {n.replace('\\', '/'): n for n in package.namelist()}
    def read(name):
        return package.read(names[name])
    for name in ('GameData/NivenRingworld/Plugins/NivenRingworld.dll',
                 'GameData/NivenRingworld/Plugins/Ringworld.Core.dll',
                 'GameData/NivenRingworld/Assets/ringworldvisuals',
                 'GameData/000_Harmony/0Harmony.dll',
                 'GameData/Cyla/License.md', 'ThirdParty/Cyla/Source/License.md',
                 'ThirdParty/Cyla/Source/Cyla/AtmosphereRenderer.cs',
                 'ThirdParty/Cyla/PROVENANCE.json', 'THIRD-PARTY-NOTICES.md',
                 'docs/CYLA-INTEGRATION.md', 'CREDITS.md', 'docs/RELEASE-1.1.1.md'):
        assert name in names, name
    assert b'Ghassen Lahmar' in read('CREDITS.md')
    assert read('GameData/Cyla/License.md') == (root / 'vendor/Cyla/Source/License.md').read_bytes()
    for name, key in [('GameData/Cyla/Cyla.dll', 'plugin_sha256'),
                      ('GameData/Cyla/Shaders/cylashaders', 'shaders_sha256')]:
        assert hashlib.sha256(read(name)).hexdigest() == provenance[key], name
    for source in (root / 'vendor/Cyla/Source').rglob('*'):
        if source.is_file():
            assert read('ThirdParty/Cyla/Source/' + source.relative_to(root / 'vendor/Cyla/Source').as_posix()) == source.read_bytes()
    dll = read('GameData/NivenRingworld/Plugins/NivenRingworld.dll')
    for forbidden in (b'SmokeTest', b'CylaSmoke', b'GearSmoke', b'StabilityExplosionTrace'):
        assert forbidden not in dll, 'Test code in package'
    assert dll == (root / 'template_instance/GameData/NivenRingworld/Plugins/NivenRingworld.dll').read_bytes()
    for name in names:
        assert not any(term in name.lower() for term in ('assembly-csharp.dll', 'unityengine.dll', '.analysis/', 'template_instance/', '.blend', 'persistent.sfs')), name
digest = hashlib.sha256(archive.read_bytes()).hexdigest()
archive.with_suffix('.zip.sha256').write_text(digest + '  ' + archive.name + '\n', encoding='utf-8')
result = 'PASS: Cyla bundle CRC, pinned binaries, matching source/licenses, installed DLL, no harness/game assemblies.\nSHA256 ' + digest + '\n'
(root / 'artifacts/validation/cyla-release-check.txt').write_text(result, encoding='utf-8')
print(result)
