"""Offline export preflight; does not replace Unity/device acceptance."""
from pathlib import Path
import json
import numpy as np

root = Path(__file__).resolve().parents[2]
data = json.loads((root/'Assets/Game/Art/SurvivalTrain/SourceData/TrainMeshes.json').read_text())
parts = {p['name']: p for p in data['parts']}
required = {'Wagon6','Wagon6Glass','Wagon5','Wagon5Glass','Wagon4','Wagon4Glass',
            'DoorLower','DoorGlass','Connector','Locomotive'}
assert set(parts) == required
stats = {}
for name, p in parts.items():
    v = np.array(p['vertices']).reshape(-1,3)
    n = np.array(p['normals']).reshape(-1,3)
    uv = np.array(p['uv']).reshape(-1,2)
    tri = np.array(p['triangles']).reshape(-1,3)
    assert len(v) < 65536 and len(v) == len(n) == len(uv), name
    assert all(np.isfinite(a).all() for a in (v,n,uv)), name
    assert tri.min() >= 0 and tri.max() < len(v), name
    assert (uv >= 0).all() and (uv <= 1).all(), name
    cross = np.cross(v[tri[:,1]]-v[tri[:,0]], v[tri[:,2]]-v[tri[:,0]])
    assert (np.linalg.norm(cross,axis=1) > 1e-10).all(), name
    # The coordinate mapping is a rotation; winding and outward normals agree.
    dot = (cross*n[tri].mean(axis=1)).sum(axis=1)
    assert (dot > 0).all(), f'{name}: inconsistent normals/front faces'
    stats[name] = {'vertices':len(v),'triangles':len(tri)}
total = sum(stats[f'Wagon{i}']['triangles'] + stats[f'Wagon{i}Glass']['triangles'] for i in (6,5,4))
total += 15*(stats['DoorLower']['triangles'] + stats['DoorGlass']['triangles'])
total += 2*stats['Connector']['triangles'] + stats['Locomotive']['triangles']
assert total <= 35000, total
print(f'PASS: 10 mesh parts; valid finite UV/indices/normals/winding; UInt16 vertices; {total} rendered triangles.')
print('Unity shot/portal/navigation and Android performance checks remain pending installation.')
