"""Generate version-specific CKAN metadata from the verified, unbundled release ZIP.
The .netkan source is submitted separately; neither file belongs inside the ZIP.
"""
import hashlib,json,pathlib,zipfile
root=pathlib.Path(__file__).resolve().parents[1]
version=json.loads((root/'GameData/NivenRingworld/NivenRingworld.version').read_text())['VERSION']
version='.'.join(str(version[k]) for k in ('MAJOR','MINOR','PATCH'))
archive=root/f'artifacts/NivenRingworld-{version}.zip'
meta=json.loads((root/'distribution/NivenRingworld.netkan').read_text())
meta={k:v for k,v in meta.items() if not k.startswith('$') and not k.startswith('x_netkan')}
meta.update(version=version,ksp_version='1.12.5',download=f'https://github.com/theplatecrafter/Ring-World-KSP-mod/releases/download/Release-v{version}/{archive.name}',download_size=archive.stat().st_size,download_hash={'sha256':hashlib.sha256(archive.read_bytes()).hexdigest()},download_content_type='application/zip')
with zipfile.ZipFile(archive) as z:
    meta['install_size']=sum(i.file_size for i in z.infolist() if i.filename.replace('\\','/').startswith('GameData/NivenRingworld/'))
output=root/f'distribution/NivenRingworld-{version}.ckan'
output.write_text(json.dumps(meta,indent=2)+'\n',encoding='utf-8')
print(output)
