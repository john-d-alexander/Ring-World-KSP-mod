"""Generate runtime placement config and reviewable asset inventory from the manifest."""
import json, math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
assets=json.loads((Path(__file__).parent/'manifest.json').read_text())['assets']
groups={};cfg=['// Original landmark placements. Dimensions and offsets in metres.']
rows=['# Landmark asset inventory','','Original Blender designs. Metre dimensions are width / height / depth. Three LODs per asset.','', '| Asset | Size (m) | Triangles LOD0 / 1 / 2 | Placement |','|---|---|---|---|']
for a in assets:
    rows.append('| '+a['id']+' | '+' / '.join(map(str,a['sizeMetres']))+' | '+' / '.join(str(l['triangles']) for l in a['lods'])+' | '+a['site']+' |')
    if a['family']!='landmarks':continue
    site={'terminal':'rim','island':'map','outpost':'arrival'}.get(a['site'],a['site']);i=groups.get(site,0);groups[site]=i+1
    angle=i*2.399963;r=4500+(i//5)*5000
    da=round(r*math.cos(angle));db=round(r*math.sin(angle))
    if site=='rim':da=(i-2)*4500;db=-4500-(i%2)*4000
    w,h,d=a['sizeMetres'];lift=2500 if a['kind'].startswith('palace_') or a['kind']=='levitation_citadel' else 0
    cfg.append(f'RINGWORLD_LANDMARK_ASSET\n{{\n    kind = {a["kind"]}\n    site = {site}\n    along = {da}\n    across = {db}\n    width = {w}\n    height = {h}\n    depth = {d}\n    aboveGround = {lift}\n    range = 60000\n}}')
(ROOT/'GameData/NivenRingworld/Landmarks.cfg').write_text('\n\n'.join(cfg)+'\n')
(ROOT/'docs/reference/LANDMARK-INVENTORY.md').write_text('\n'.join(rows)+'\n')
print(f'{len(assets)} assets; {sum(groups.values())} configured architectural landmarks')
