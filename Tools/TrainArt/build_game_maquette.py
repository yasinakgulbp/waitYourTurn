"""Native game-scale maquette art. Original gameplay mesh input is never overwritten.
Blender --background --factory-startup --python Tools/TrainArt/build_game_maquette.py
Exports tinted/AO vertex meshes and bounded 20m scenery tiles; no runtime bake.
"""
import bpy, json, math, hashlib, random, sys
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree

ROOT = Path(__file__).resolve().parents[2]
INPUT = ROOT/'Assets/Game/Art/SurvivalTrain/SourceData/TrainMeshes.json'
OUT = ROOT/'Assets/Game/Art/Maquette/SourceData'
SOURCE = ROOT/'ArtSource/GameMaquette'
OUT.mkdir(parents=True, exist_ok=True); SOURCE.mkdir(parents=True, exist_ok=True)
original_hash = hashlib.sha256(INPUT.read_bytes()).hexdigest()
original = json.loads(INPUT.read_text())
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
# Linear colors; Unity vertex colors are explicitly stored in linear space.
paper=(.73,.75,.76); edge=(.87,.88,.88); floor=(.53,.56,.58)
grey=(.36,.39,.42); ink=(.035,.049,.064); red=(.29,.032,.043)
palette=[paper,paper,ink,red,floor,grey,edge,paper,red,paper,edge,ink,grey,grey,paper,grey]
parts=[]; preview=[]

def C(p): return (p[0],-p[2],p[1])
def U(p): return (p[0],p[2],-p[1])

def bake(part):
    v=[Vector(part['vertices'][i:i+3]) for i in range(0,len(part['vertices']),3)]
    n=[Vector(part['normals'][i:i+3]).normalized() for i in range(0,len(part['normals']),3)]
    uv=[part['uv'][i:i+2] for i in range(0,len(part['uv']),2)]
    faces=[tuple(part['triangles'][i:i+3]) for i in range(0,len(part['triangles']),3)]
    # Subdivide only the large flat deck triangles for smooth baked contact shade.
    # The surface positions/bounds and all original openings remain identical.
    for level in range(4):
        refined=[]
        for f in faces:
            tile=int(uv[f[0]][0]*4)+4*(3-int(uv[f[0]][1]*4))
            if tile!=4 or n[f[0]].y<.99 or part['name'] not in ('Wagon6','Wagon5','Wagon4','Connector'):
                refined.append(f); continue
            mids=[]
            for a,b in ((f[0],f[1]),(f[1],f[2]),(f[2],f[0])):
                mids.append(len(v));v.append((v[a]+v[b])*.5);n.append(n[a]);uv.append([(uv[a][j]+uv[b][j])*.5 for j in (0,1)])
            a,b,c=f;ab,bc,ca=mids
            refined.extend(((a,ab,ca),(ab,b,bc),(ca,bc,c),(ab,bc,ca)))
        faces=refined
    bvh=BVHTree.FromPolygons(v,faces,all_triangles=True)
    colors=[];cache={}
    for p,normal,tex in zip(v,n,uv):
        tile=min(15,int(tex[0]*4)+4*(3-int(tex[1]*4)))
        base=palette[tile]
        if part['name']=='DoorLower': base=red
        key=tuple(round(a,5) for a in (*p,*normal))
        ao=cache.get(key)
        if ao is None:
            tangent=normal.cross(Vector((0,1,0)) if abs(normal.y)<.9 else Vector((1,0,0))).normalized()
            bitangent=normal.cross(tangent).normalized(); obstruction=0
            for i in range(18):
                az=i*2.399963; z=math.sqrt((i+.5)/18); r=math.sqrt(1-z*z)
                direction=tangent*(math.cos(az)*r)+bitangent*(math.sin(az)*r)+normal*z
                hit=bvh.ray_cast(p+normal*.005,direction,.65)
                if hit[0] is not None: obstruction+=max(0,1-hit[3]/.65)
            ao=1-.55*obstruction/18;cache[key]=ao
        colors.extend([round(c*ao,5) for c in base]+[1])
    result={'name':part['name'],'vertices':[round(a,6) for p in v for a in p],
            'normals':[round(a,6) for p in n for a in p], 'uv':[a for p in uv for a in p],
            'triangles':[a for f in faces for a in f], 'colors':colors}
    assert len(v)<65536
    return result

def material(name,vertex=False):
    mat=bpy.data.materials.new(name);mat.use_nodes=True
    p=mat.node_tree.nodes.get('Principled BSDF');p.inputs['Roughness'].default_value=.85
    if vertex:
        attr=mat.node_tree.nodes.new('ShaderNodeVertexColor');attr.layer_name='Paper'
        mat.node_tree.links.new(attr.outputs['Color'],p.inputs['Base Color'])
    else:
        p.inputs['Base Color'].default_value=(.15,.21,.25,1);p.inputs['Alpha'].default_value=.08
    return mat

card=material('Matte card - native vertex colors and baked cavities',True)
glass=material('Clear neutral glazing')

def show(part,position=(0,0,0)):
    vertices=[C(part['vertices'][i:i+3]) for i in range(0,len(part['vertices']),3)]
    faces=[part['triangles'][i:i+3] for i in range(0,len(part['triangles']),3)]
    mesh=bpy.data.meshes.new(part['name']);mesh.from_pydata(vertices,[],faces);mesh.update()
    obj=bpy.data.objects.new(part['name'],mesh);bpy.context.collection.objects.link(obj);obj.location=C(position)
    is_glass='Glass' in part['name']
    mesh.materials.append(glass if is_glass else card)
    if not is_glass:
        attr=mesh.color_attributes.new(name='Paper',type='FLOAT_COLOR',domain='POINT')
        for i,item in enumerate(attr.data): item.color=part['colors'][i*4:i*4+4]
    preview.append(obj);return obj

for part in original['parts']:
    baked=part if 'Glass' in part['name'] else bake(part)
    parts.append(baked)
    name=part['name']
    if name.startswith('Wagon'): show(baked,({6:0,5:13,4:26}[int(name[5])],0,0))
    elif name=='Locomotive':show(baked,(37,0,0))
    elif name=='Connector':
        show(baked,(6.5,0,0));show(baked,(19.5,0,0))
    elif name.startswith('Door'):
        for count,offset in ((6,0),(5,13),(4,26)):
            for sign in (-1,1):
                for x in [-3.8,3.8]+([0] if count==6 or count==5 and sign==-1 else []):show(baked,(offset+x,0,sign*2.1))
    print('BAKED',name,len(baked['triangles'])//3,flush=True)

class Builder:
    def __init__(self,name):self.name=name;self.v=[];self.n=[];self.c=[];self.t=[];self.uv=[]
    def face(self,points,color):
        start=len(self.v)//3
        normal=(Vector(points[1])-Vector(points[0])).cross(Vector(points[2])-Vector(points[0])).normalized()
        for p in points:self.v.extend(p);self.n.extend(normal);self.c.extend((*color,1));self.uv.extend((p[0]/20,p[2]/20))
        for i in range(1,len(points)-1):self.t.extend((start,start+i,start+i+1))
    def box(self,p,size,color):
        x,y,z=p;a,b,c=[d*.5 for d in size]
        q=[(x-a,y-b,z-c),(x+a,y-b,z-c),(x+a,y+b,z-c),(x-a,y+b,z-c),
           (x-a,y-b,z+c),(x+a,y-b,z+c),(x+a,y+b,z+c),(x-a,y+b,z+c)]
        for f in ((0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(3,7,6,2),(0,1,5,4)):self.face([q[i] for i in f],color)
    def line(self,a,b,width,color):
        d=Vector(b)-Vector(a);across=Vector((-d.z,0,d.x)).normalized()*width*.5
        self.face([Vector(a)-across,Vector(a)+across,Vector(b)+across,Vector(b)-across],color)
    def tree(self,x,z,scale=1,ground=-.3):
        # Two interlocking, flat, extruded card silhouettes. Low-sided branches,
        # unlike round tree tubes; thickness and flat ends remain visible.
        segments=[((0,0),(0,2.7),.1),((0,.8),(-.65,1.45),.065),((0,1.2),(.8,1.9),.06),
                  ((0,1.55),(-.6,2.3),.055),((0,1.85),(.55,2.6),.05),
                  ((-.35,1.12),(-.75,1.22),.045),((.45,1.59),(.82,1.58),.043),
                  ((-.3,1.92),(-.65,2.6),.038),((.25,2.18),(.58,2.9),.035)]
        for axis in (0,1):
            for a,b,w in segments:
                a=Vector(a)*scale;b=Vector(b)*scale;off=Vector((-(b-a).y,(b-a).x)).normalized()*w*scale*.5
                quad=[a-off,b-off,b+off,a+off]
                def pt(q,t):return (x+(q.x if axis==0 else t),q.y+ground+.07,z+(t if axis==0 else q.x))
                for sign in (-1,1):self.face([pt(q,sign*.022) for q in (quad if sign==1 else list(reversed(quad)))],edge)
                for i in range(4):j=(i+1)%4;self.face([pt(quad[i],-.022),pt(quad[j],-.022),pt(quad[j],.022),pt(quad[i],.022)],paper)
                # Cut silhouette contact shadow, stretched in one light direction.
                def shadow(q):return(x+(q.x if axis==0 else 0)+q.y*.65,ground+.014,z+(0 if axis==0 else q.x)+q.y*.4)
                self.face([shadow(q) for q in quad],(.41,.44,.46))
        self.box((x,ground+.025,z),(.45,.04,.45),paper)
    def drafting(self,x,z,y):
        # Original floor plan: walls, center lines, section hatching and dimension
        # ticks. Ink sits 1mm above its own board, never an overlapping opaque skin.
        c=(.24,.28,.31)
        for k in range(4):
            px=x-1.25+k*.7
            self.line((px,y,z-.7),(px,y,z+.7),.018,c)
            self.line((px+.055,y,z-.7),(px+.055,y,z+.7),.008,c)
        for dz in (-.7,0,.7):self.line((x-1.25,y,z+dz),(x+.85,y,z+dz),.018,c)
        for k in range(6):
            self.line((x-.55+k*.1,y,z-.7),(x-.45+k*.1,y,z-.55),.007,c)
        self.line((x-1.4,y,z+.9),(x+1,y,z+.9),.012,c)
        for px in (-1.25,-.55,.15,.85):
            self.line((x+px,y,z+.78),(x+px,y,z+1.02),.011,c)
            self.line((x+px-.06,y,z+.84),(x+px+.06,y,z+.96),.022,c)
        # Drafting crosshairs and notes reduced to short printed rules.
        for k in range(3):self.line((x+1.2,y,z+.3+k*.08),(x+1.9-k*.14,y,z+.3+k*.08),.008,c)
    def finish(self):return {'name':self.name,'vertices':self.v,'normals':self.n,'uv':self.uv,'triangles':self.t,'colors':self.c}

# Local repeat is EXACTLY 20m; tiles are instanced by the installer, never a
# renderer or coroutine per leaf/line. All geometry is outside combat openings.
terrain=Builder('SceneryCard');prints=Builder('SceneryInk')
for sign in (-1,1):
    terrain.box((10,-.4,sign*9),(20,.2,14),paper)
    for x in (0,4,8,12,16):
        prints.line((x,-.296,sign*2.05),(x,-.296,sign*16),.012,(.50,.53,.55))
    for z in (4,8,12):prints.line((0,-.296,sign*z),(20,-.296,sign*z),.012,(.50,.53,.55))
    for x,z,s in ((2,sign*6.1,.73),(12,sign*7.8,.85)):
        terrain.tree(x,z,s)
    # Sparse original printed drafting panel. Wine red plus charcoal, no
    # copied captions, film stills or photographic faces from the references.
    px=6 if sign<0 else 15;pz=sign*5.7
    prints.face([(px-1.5,-.292,pz-1.15),(px-1.5,-.292,pz+1.15),(px+1.5,-.292,pz+1.15),(px+1.5,-.292,pz-1.15)],red)
    for i in range(6):
        prints.line((px-1.25+i*.5,-.287,pz-.9),(px-1.25+i*.5,-.287,pz+.9),.016,(.13,.025,.032))
    for i in range(5):prints.line((px-1.25,-.287,pz-.9+i*.45),(px+1.25,-.287,pz-.9+i*.45),.016,(.13,.025,.032))
    for i in range(4):prints.line((px-1.15,-.287,pz+1.32+i*.06),(px+.2+i*.17,-.287,pz+1.32+i*.06),.009,grey)
    # Thin printed measurement witness line and ticks.
    prints.line((1,-.291,sign*4.5),(18,-.291,sign*4.5),.014,grey)
    for x in (1,5,9,13,18):prints.line((x,-.290,sign*4.3),(x,-.290,sign*4.7),.025,grey)
    prints.drafting(12,sign*4.8,-.288)
    terrain.box((17,1.4,sign*8.5),(.10,3.4,.14),edge)
    terrain.box((17,3.03,sign*8.5),(.65,.12,.16),paper)

station=Builder('StationCard');stamps=Builder('StationInk')
for sign in (-1,1):
    station.box((13,-.125,sign*9.18),(40,.24,14),paper)
    # Train shadows printed into the model board, not onto gameplay floor.
    stamps.face([(-7,.0004,sign*2.12),(-7,.0004,sign*2.4),(33,.0004,sign*2.4),(33,.0004,sign*2.12)],(.43,.46,.48))
    for x in range(-6,34,4):
        stamps.line((x,.001,sign*2.2),(x,.001,sign*16),.012,(.50,.53,.55))
    for z in (4,8,12):stamps.line((-7,.001,sign*z),(33,.001,sign*z),.012,(.50,.53,.55))
    for x in (-5,3,11,19,27):
        station.box((x,1.25,sign*12),(.22,2.5,.26),edge)
        station.box((x,.03,sign*12),(.50,.07,.58),grey)
        station.tree(x+3,sign*6.8,.72,-.005)
    station.box((13,2.56,sign*13),(40,.18,2.5),paper)
    station.box((13,2.42,sign*12),(40,.11,.18),grey)
    for x in (1,17,29):
        z=sign*5.6
        stamps.face([(x-1.2,.003,z-1),(x-1.2,.003,z+1),(x+1.2,.003,z+1),(x+1.2,.003,z-1)],red)
        for k in range(5):
            stamps.line((x-1+k*.5,.004,z-.85),(x-1+k*.5,.004,z+.85),.018,(.13,.025,.032))
        for k in range(4):stamps.line((x-1,.004,z-.85+k*.55),(x+1,.004,z-.85+k*.55),.014,(.13,.025,.032))
    for x in (7,23):stamps.drafting(x,sign*4.7,.005)
    # Paper platform edge, muted red rather than bright yellow hazard strip.
    for x in range(-6,33,2):station.box((x,.005,sign*2.6),(.5,.01,.075),red)

track=Builder('TrackCard')
for x in range(-32,76):track.box((x,-.31,0),(.17,.09,4.5),grey)
for sign in (-1,1):
    track.box((20,-.21,sign*1.45),(112,.10,.11),ink)
    track.box((20,-.285,sign*1.45),(112,.035,.24),ink)

environment=[b.finish() for b in (terrain,prints,station,stamps,track)]
for item in environment:
    parts.append(item)
    if item['name'].startswith('Scenery'):
        for x in (-20,0,20,40):show(item,(x,0,0))
    else:show(item)

assert hashlib.sha256(INPUT.read_bytes()).hexdigest()==original_hash
(OUT/'MaquetteMeshes.json').write_text(json.dumps({'parts':parts},separators=(',',':')))
manifest={'input_sha256':original_hash,'original_input_unchanged':True,'unit':'metres',
          'train_openings_and_outer_positions':'D48 retained; only coplanar deck tessellation for baked contact shade',
          'baked_ao':'18 hemisphere rays, 0.65m reach; vertex color, offline only',
          'baked_surface_maps':0,'runtime_card_grain_px':64,'scenery_repeat_m':20,'runtime_extra_lights':0,
          'parts':{p['name']:{'vertices':len(p['vertices'])//3,'triangles':len(p['triangles'])//3} for p in parts},
          'stage':'Actual game art candidate; user visual and Android acceptance pending'}
(SOURCE/'manifest.json').write_text(json.dumps(manifest,indent=2))

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
scene.render.threads_mode='FIXED';scene.render.threads=6
scene.render.resolution_x=1440;scene.render.resolution_y=810;scene.render.resolution_percentage=100
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.12,.16,.21,1)
scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.35
scene.view_settings.view_transform='AgX'
for pos,power,size,color in (((7,-7,15),2700,12,(.92,.96,1)),((18,7,10),1700,14,(.72,.84,1))):
    bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=power;o.data.size=size;o.data.color=color
    o.rotation_euler=(Vector((13,0,0))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=C((13,16,-11.2)));cam=bpy.context.object;scene.camera=cam
cam.rotation_euler=(Vector(C((13,.4,0)))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=42
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(SOURCE/'maquette-game-scale-preview.png')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'GameMaquette.blend'))
print('MAQUETTE_MANIFEST',json.dumps(manifest),flush=True)
if '--no-render' not in sys.argv:bpy.ops.render.render(write_still=True)
