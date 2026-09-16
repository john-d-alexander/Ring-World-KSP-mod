"""Check the distributable, rather than trusting the build directory."""
import hashlib, json, pathlib, zipfile
root=pathlib.Path(__file__).resolve().parents[1]
release=json.loads((root/'GameData/NivenRingworld/NivenRingworld.version').read_text(encoding='utf-8-sig'))['VERSION']
version_text='.'.join(str(release[k]) for k in ('MAJOR','MINOR','PATCH'))
archive=root/f'artifacts/NivenRingworld-{version_text}.zip'
for name in ('gear','guidance','residence','scenery','landmarks','game'):
    report=(root/f'artifacts/validation/{name}-smoke.txt').read_text(encoding='utf-8-sig')
    assert '[RingworldSmoke] PASS' in report and '[RingworldSmoke] FAIL' not in report, name+' regression not passed'
with zipfile.ZipFile(archive) as package:
    assert package.testzip() is None,'ZIP CRC failure'
    names={n.replace('\\','/'):n for n in package.namelist()}
    for required in ('GameData/NivenRingworld/Plugins/NivenRingworld.dll','GameData/NivenRingworld/Plugins/Ringworld.Core.dll','GameData/000_Harmony/0Harmony.dll','GameData/000_Harmony/LICENSE','GameData/NivenRingworld/Assets/ringworldscenery','GameData/NivenRingworld/Landmarks.cfg','docs/LANDMARK-ASSETS.md','docs/LANDMARK-INVENTORY.md',f'docs/RELEASE-{version_text}.md','docs/COLOSSI-AND-FORESTS.md','GameData/NivenRingworld/Colossi.cfg','LICENSE'):
        assert required in names,required
    version=json.loads(package.read(names['GameData/NivenRingworld/NivenRingworld.version']))
    assert version['VERSION']==release
    if release['PATCH'] >= 3:
        assert 'GameData/NivenRingworld/LICENSE' in names
        assert 'docs/QUALITY-PRESETS.md' in names and 'docs/CKAN-PUBLISHING.md' in names
        quality_report=(root/'artifacts/validation/landmarks-smoke.txt').read_text(encoding='utf-8-sig')
        assert 'QUALITY all 11 presets roundtrip' in quality_report
        assert 'QUALITY Economy near vertices=' in quality_report

    dll=package.read(names['GameData/NivenRingworld/Plugins/NivenRingworld.dll'])
    for forbidden in (b'GearSmoke',b'StabilitySmoke',b'LandmarkSmoke',b'GuidanceSmoke',b'SmokeTest',b'StabilityExplosionTrace'):
        assert forbidden not in dll,'Test harness in release DLL: '+repr(forbidden)
    for n in names:
        assert not any(s in n.lower() for s in ('assembly-csharp','unityengine','.analysis/','template_instance/','.blend','persistent.sfs')),'Non-distributable file: '+n
    registry=package.read(names['GameData/NivenRingworld/Scenery.cfg']).decode()
    assert registry.count('RINGWORLD_BUNDLED_SCENERY_ASSET')==92,'Incomplete scenery registry'
    assert package.read(names['GameData/NivenRingworld/Landmarks.cfg']).count(b'RINGWORLD_LANDMARK_ASSET')==25
    assert package.read(names['GameData/NivenRingworld/Colossi.cfg']).count(b'RINGWORLD_COLOSSUS_ASSET')==8
digest=hashlib.sha256(archive.read_bytes()).hexdigest()
(archive.with_suffix('.zip.sha256')).write_text(digest+'  '+archive.name+'\n')
report=f'PASS: v{version_text} package CRC, required files, 92-prefab registry, 25 landmarks + 8 rare templates, regression gates, no harness/game assemblies.\nSHA256 {digest}\n'
(root/'artifacts/validation/release-check.txt').write_text(report)
print(report)
