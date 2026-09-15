"""Validate the artist-facing scale/LOD contract and record deliverable hashes."""
import hashlib
import json
import math
from pathlib import Path

root = Path(__file__).resolve().parent
manifest = json.loads((root / 'manifest.json').read_text(encoding='utf-8'))
assert len(manifest['assets']) == 8
assert len({asset['id'] for asset in manifest['assets']}) == 8
for asset in manifest['assets']:
    levels = asset['lods']
    assert [level['level'] for level in levels] == [0, 1, 2], asset['id']
    assert levels[0]['triangles'] > levels[1]['triangles'] > levels[2]['triangles'], asset['id']
    for level in levels:
        assert all(math.isfinite(v) for bound in ('min', 'max') for v in level[bound])
        if asset['kind'].startswith('tree_'):
            assert abs(level['min'][2]) < .0001, asset['id']
            # Lower LODs omit individual tip leaves; the shared scale must not be rebaked per LOD.
            assert abs(level['max'][2] - 1) < (.0001 if level['level'] == 0 else .05), asset['id']
        elif asset['kind'] == 'boulder':
            assert all(abs(v + .5) < .0001 for v in level['min']), asset['id']
            assert all(abs(v - .5) < .0001 for v in level['max']), asset['id']
    assert (root / 'exports' / (asset['id'] + '.fbx')).stat().st_size > 1000
files = [root / 'Ringworld-Scenery-First-Set.blend', root / 'manifest.json']
files += sorted((root / 'exports').glob('*'))
hashes = {str(path.relative_to(root)): hashlib.sha256(path.read_bytes()).hexdigest() for path in files}
(root / 'validation/source-hashes.json').write_text(json.dumps(hashes, indent=2), encoding='utf-8')
print('PASS: 8 assets, 24 decreasing LOD budgets, finite bounds, tree ground pivots, unit scales, FBX deliverables.')
