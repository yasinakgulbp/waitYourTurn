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
# Regression for visible depth fighting at the actual wagon/bridge joints.
# Compare same-facing axis-aligned surfaces; touching edges and opposite-facing
# seam caps are harmless, positive projected overlap on one plane is not.
def flat_triangles(name, offset=0):
    p=parts[name]; v=np.array(p['vertices']).reshape(-1,3)+[offset,0,0]
    t=v[np.array(p['triangles']).reshape(-1,3)]
    n=np.cross(t[:,1]-t[:,0],t[:,2]-t[:,0]);n/=np.linalg.norm(n,axis=1)[:,None]
    return t,n

def overlap_area(a,b):
    def cross(v,w):return v[0]*w[1]-v[1]*w[0]
    if cross(b[1]-b[0],b[2]-b[0])<0:b=b[::-1]
    polygon=list(a)
    for start,end in zip(b,np.roll(b,-1,axis=0)):
        if not polygon:return 0
        output=[];previous=polygon[-1];dp=cross(end-start,previous-start)
        for current in polygon:
            dc=cross(end-start,current-start)
            if (dc>=-1e-9)!=(dp>=-1e-9):output.append(previous+(current-previous)*dp/(dp-dc))
            if dc>=-1e-9:output.append(current)
            previous,dp=current,dc
        polygon=output
    return abs(sum(cross(polygon[i],polygon[(i+1)%len(polygon)]) for i in range(len(polygon))))*.5

for wagon,offset,bridge in [('Wagon6',0,6.5),('Wagon5',13,6.5),('Wagon5',13,19.5),('Wagon4',26,19.5)]:
    deck,dn=flat_triangles(wagon,offset); connector,cn=flat_triangles('Connector',bridge)
    for face,normal in zip(connector,cn):
        axis=np.argmax(abs(normal))
        if abs(normal[axis])<.99999:continue
        axes=[i for i in range(3) if i!=axis]; a=face[:,axes]
        candidates=(dn@normal>.99999)&(abs(deck[:,:,axis]-face[0,axis]).max(axis=1)<1e-5)
        for target in deck[candidates]:
            b=target[:,axes]
            if (np.minimum(a.max(axis=0),b.max(axis=0))-np.maximum(a.min(axis=0),b.min(axis=0))<=1e-6).any():continue
            assert overlap_area(a,b)<1e-7,f'{wagon} / Connector at {bridge}: visible coplanar overlap'
print('PASS: no same-facing coplanar bridge/deck/jamb overlap at all four art joints.')
print('Unity shot/portal/navigation and Android performance checks remain pending installation.')
