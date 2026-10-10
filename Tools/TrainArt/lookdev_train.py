"""Isolated, editable material/lighting trial. Does not write anything to Assets.

Loads the exact game-sized mesh, replaces the old photographic atlas with native
metric procedural shaders, and renders a reference-facing carriage. Lighting and
interior props are LOOK DEVELOPMENT ONLY; not mobile runtime acceptance.
Run: blender --background --python Tools/TrainArt/lookdev_train.py
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'ArtSource/SurvivalTrain/LookDev'
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(ROOT / 'ArtSource/SurvivalTrain/SurvivalTrain.blend'))
surface_atlas=bpy.data.images.load(str(OUT/'OriginalSurfaceAtlas.png'))
surface_atlas.pack()

# World-space coordinates keep a 2cm tread 2cm wide on a 12m deck AND a gangway.
def newmat(name, color, metal, rough):
    mat = bpy.data.materials.new('LD / ' + name); mat.use_nodes = True
    nodes = mat.node_tree.nodes; links = mat.node_tree.links
    p = nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metal
    p.inputs['Roughness'].default_value = rough
    geo = nodes.new('ShaderNodeNewGeometry')
    mat.diffuse_color = (*color, 1)
    return mat, nodes, links, p, geo.outputs['Position']

def node(nodes, kind, **values):
    n = nodes.new(kind)
    for k, v in values.items(): n.inputs[k].default_value = v
    return n

def noise(nodes, links, pos, scale, detail=2):
    n = node(nodes, 'ShaderNodeTexNoise', Scale=scale, Detail=detail, Roughness=.68)
    links.new(pos, n.inputs['Vector']); return n.outputs['Fac']

def mathnode(nodes, links, op, a, b=0):
    n = nodes.new('ShaderNodeMath'); n.operation = op
    for i, v in enumerate((a, b)):
        if hasattr(v, 'node'): links.new(v, n.inputs[i])
        else: n.inputs[i].default_value = v
    return n.outputs[0]

def ramp(nodes, links, fac, lo, hi, bounds=(.18, .8)):
    n = nodes.new('ShaderNodeValToRGB')
    n.color_ramp.elements[0].position = bounds[0]
    n.color_ramp.elements[0].color = (*lo, 1)
    n.color_ramp.elements[1].position = bounds[1]
    n.color_ramp.elements[1].color = (*hi, 1)
    links.new(fac, n.inputs[0]); return n.outputs['Color']

def atlas_color(ns,ls,pos,cell):
    # Dominant-face metric projection, inside ONE atlas cell. Do not stretch an
    # entire photograph to each little beam, or sample neighboring materials.
    geo=pos.node;xyz=ns.new('ShaderNodeSeparateXYZ');ls.new(pos,xyz.inputs[0])
    normals=ns.new('ShaderNodeSeparateXYZ');ls.new(geo.outputs['Normal'],normals.inputs[0])
    ax=[mathnode(ns,ls,'ABSOLUTE',normals.outputs[c]) for c in 'XYZ']
    is_x=mathnode(ns,ls,'MULTIPLY',mathnode(ns,ls,'GREATER_THAN',ax[0],ax[1]),mathnode(ns,ls,'GREATER_THAN',ax[0],ax[2]))
    is_z=mathnode(ns,ls,'MULTIPLY',mathnode(ns,ls,'GREATER_THAN',ax[2],ax[0]),mathnode(ns,ls,'GREATER_THAN',ax[2],ax[1]))
    u=mathnode(ns,ls,'ADD',mathnode(ns,ls,'MULTIPLY',xyz.outputs['Y'],is_x),mathnode(ns,ls,'MULTIPLY',xyz.outputs['X'],mathnode(ns,ls,'SUBTRACT',1,is_x)))
    v=mathnode(ns,ls,'ADD',mathnode(ns,ls,'MULTIPLY',xyz.outputs['Y'],is_z),mathnode(ns,ls,'MULTIPLY',xyz.outputs['Z'],mathnode(ns,ls,'SUBTRACT',1,is_z)))
    uv=[]
    for a,offset in ((u,(cell%2)*.5),(v,(1-cell//2)*.5)):
        a=mathnode(ns,ls,'FRACT',mathnode(ns,ls,'MULTIPLY',a,1/1.5))
        uv.append(mathnode(ns,ls,'ADD',mathnode(ns,ls,'MULTIPLY',a,.498),offset+.001))
    combine=ns.new('ShaderNodeCombineXYZ');ls.new(uv[0],combine.inputs['X']);ls.new(uv[1],combine.inputs['Y'])
    tex=ns.new('ShaderNodeTexImage');tex.image=surface_atlas;tex.interpolation='Linear'
    ls.new(combine.outputs[0],tex.inputs['Vector']);return tex.outputs['Color']

def worn(name, base, metal=.45, rough=.5, variation=.25, scratches=.0012, cell=None):
    mat, ns, ls, p, pos = newmat(name, base, metal, rough)
    broad = noise(ns, ls, pos, 2.4, 3)
    color = ramp(ns, ls, broad, tuple(c*(1-variation) for c in base), tuple(c*(1+variation) for c in base))
    if cell is not None:
        color=atlas_color(ns,ls,pos,cell)
        if cell==1:
            lift=ns.new('ShaderNodeMixRGB');lift.blend_type='MULTIPLY';lift.inputs[0].default_value=1
            ls.new(color,lift.inputs[1]);lift.inputs[2].default_value=(1.75,1.5,1.5,1);color=lift.outputs[0]
    # Irregular fine corrosion and directional scratches, separate from broad
    # discoloration. Tiny dark marks must not become metre-wide blurry clouds.
    fine = noise(ns,ls,pos,72,3)
    speck = ramp(ns,ls,fine,(.33,.35,.36),(1,1,1),(.48,.72))
    grime = ns.new('ShaderNodeMixRGB');grime.blend_type='MULTIPLY';grime.inputs[0].default_value=.52
    ls.new(color,grime.inputs[1]);ls.new(speck,grime.inputs[2]);color=grime.outputs[0]
    directional=ns.new('ShaderNodeVectorMath');directional.operation='MULTIPLY'
    ls.new(pos,directional.inputs[0]);directional.inputs[1].default_value=(1,1,.045)
    scratched=noise(ns,ls,directional.outputs[0],140,2)
    scratchmask=ramp(ns,ls,scratched,(.20,.23,.26),(1,1,1),(.68,.76))
    scratch=ns.new('ShaderNodeMixRGB');scratch.blend_type='MULTIPLY';scratch.inputs[0].default_value=.17
    ls.new(color,scratch.inputs[1]);ls.new(scratchmask,scratch.inputs[2]);color=scratch.outputs[0]
    # Real shading creates depth at joints; this material does not contain fake
    # photographed bolts, stretched panel edges, or a prelit train picture.
    ao = node(ns, 'ShaderNodeAmbientOcclusion', Distance=.18)
    ao.inputs['Color'].default_value = (1, 1, 1, 1)
    mul = ns.new('ShaderNodeMixRGB'); mul.blend_type='MULTIPLY'; mul.inputs[0].default_value=.72
    ls.new(color, mul.inputs[1]); ls.new(ao.outputs['Color'], mul.inputs[2])
    ls.new(mul.outputs[0], p.inputs['Base Color'])
    ls.new(ramp(ns,ls,noise(ns,ls,pos,35), (rough*.86,)*3, (min(.92,rough*1.15),)*3), p.inputs['Roughness'])
    bump = node(ns, 'ShaderNodeBump', Strength=.24, Distance=scratches)
    ls.new(noise(ns,ls,pos,180), bump.inputs['Height']); ls.new(bump.outputs['Normal'],p.inputs['Normal'])
    return mat

warm = worn('warm aged interior steel', (.255,.216,.151), .36, .56, .28, cell=0)
outer = worn('charcoal exterior enamel', (.039,.048,.054), .5, .42, .26, cell=2)
silver = worn('brushed silver edge', (.36,.41,.44), .82, .29, .16, .0007, cell=3)
red = worn('oxblood red satin doors', (.26,.016,.020), .25, .45, .48, cell=1)
rubber = worn('rubber and bellows', (.016,.02,.024), .05, .79, .18)
seatmat = worn('dark upholstered bench', (.035,.043,.048), .08, .72, .14)

floor, ns, ls, p, pos = newmat('fine diamond anti-slip floor', (.04,.044,.048), .30, .63)
sep = ns.new('ShaderNodeSeparateXYZ'); ls.new(pos,sep.inputs[0])
x,y = sep.outputs['X'],sep.outputs['Y']
# A regular fine diamond emboss, 3.7cm pitch, not huge crosshatch or random mud.
u=mathnode(ns,ls,'ADD',x,y); v=mathnode(ns,ls,'SUBTRACT',x,y)
def wave(a):
    return mathnode(ns,ls,'ABSOLUTE',mathnode(ns,ls,'SINE',mathnode(ns,ls,'MULTIPLY',a,120)))
knurl=mathnode(ns,ls,'POWER',mathnode(ns,ls,'MULTIPLY',wave(u),wave(v)),3.8)
color=ramp(ns,ls,knurl,(.018,.023,.029),(.056,.065,.078),(.05,.95))
# Quiet sheet joints are part of this shader. No overlapping seam-strip meshes.
def seam(a, size):
    q=mathnode(ns,ls,'PINGPONG',mathnode(ns,ls,'ADD',a,120),size/2)
    return mathnode(ns,ls,'LESS_THAN',q,.010)
seams=mathnode(ns,ls,'MAXIMUM',seam(x,1.2),seam(y,1.4))
mix=ns.new('ShaderNodeMixRGB'); ls.new(seams,mix.inputs[0]);ls.new(color,mix.inputs[1]);mix.inputs[2].default_value=(.105,.119,.13,1)
ls.new(mix.outputs[0],p.inputs['Base Color'])
b=node(ns,'ShaderNodeBump',Strength=.45,Distance=.002)
ls.new(knurl,b.inputs['Height']);ls.new(b.outputs['Normal'],p.inputs['Normal'])
ls.new(ramp(ns,ls,noise(ns,ls,pos,8),(.57,)*3,(.73,)*3),p.inputs['Roughness'])

glass, ns, ls, p, pos = newmat('smoky glazing', (.042,.070,.078), .3, .16)
out=ns.get('Material Output'); transparent=ns.new('ShaderNodeBsdfTransparent')
mix=ns.new('ShaderNodeMixShader');mix.inputs[0].default_value=.16
ls.new(transparent.outputs[0],mix.inputs[1]);ls.new(p.outputs[0],mix.inputs[2]);ls.new(mix.outputs[0],out.inputs['Surface'])

lamp, ns, ls, p, pos = newmat('warm lamp lens', (.58,.32,.10), .08, .3)
p.inputs['Emission Color'].default_value=(1,.48,.13,1);p.inputs['Emission Strength'].default_value=3.5
vent=worn('vent grille',(.021,.024,.027),.52,.59,.2)
mats=[silver,warm,rubber,red,floor,vent,silver,rubber,red,outer,lamp,glass,rubber,rubber,outer,outer]

# Classify the original face UV cell, then use metric materials. Do not alter
# existing vertices, door apertures, gangway width, or collision authority.
seen=set()
for obj in list(bpy.data.objects):
    if obj.type != 'MESH' or obj.name.startswith('Preview'):
        bpy.data.objects.remove(obj,do_unlink=True);continue
    mesh=obj.data
    if mesh.as_pointer() in seen:continue
    seen.add(mesh.as_pointer())
    uv=mesh.uv_layers.active
    if not uv:continue
    old=[]
    for face in mesh.polygons:
        center=sum((uv.data[i].uv for i in face.loop_indices),Vector((0,0)))/len(face.loop_indices)
        tile=min(3,int(center.x*4))+4*(3-min(3,int(center.y*4)))
        # The inward-facing wall finish is warm even though V3 recolored it gray.
        c=face.center
        if obj.name.startswith('Wagon') and tile in (0,1,9,15) and abs(c.y)>1.75 and c.y*face.normal.y<-.08:
            tile=1
        old.append(tile)
    mesh.materials.clear()
    for m in mats:mesh.materials.append(m)
    for face,tile in zip(mesh.polygons,old):
        # Existing small manufactured bevels get lightly exposed base metal.
        # This is explicit geometry shading, not a painted fake edge in every UV.
        if tile in (1,3,8,9) and sum(abs(v)>.12 for v in face.normal)>=2:
            tile=6
        face.material_index=tile
    mesh.update()

def cube(name, location, size, mat, bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location);o=bpy.context.object;o.name='LD / '+name
    o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        m=o.modifiers.new('Manufactured edge','BEVEL');m.width=bevel;m.segments=2
        bpy.ops.object.modifier_apply(modifier=m.name)
        m=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');m.keep_sharp=True
        bpy.ops.object.modifier_apply(modifier=m.name)
    o.data.materials.append(mat);return o

# Reference-scale bench silhouettes, deliberately outside the walk-through aisle.
# Separate proposal: NOT installed as furniture or colliders in the current game.
for off in (0,13,26):
    for sign in (-1,1):
        for xx in (-2,2):
            # Front center door is at x=13, so keep its approach empty.
            x=off+xx;y=sign*1.46
            cube('bench plinth',(x,y,.24),(1.45,.66,.45),outer,.055)
            cube('bench cushion',(x,y,.52),(1.42,.64,.13),seatmat,.055)
            cube('bench back',(x,sign*1.72,.84),(1.42,.12,.55),seatmat,.045)
            for dx in (-.735,.735):
                cube('bench end silver',(x+dx,y,.45),(.06,.7,.75),silver,.023)
            for dx in (-.80,.80):
                cube('partition pedestal',(x+dx,sign*1.36,.45),(.055,1.10,.90),warm,.012)
                cube('partition polished cap',(x+dx,sign*1.36,.925),(.075,1.12,.05),silver,.018)
        # Lamps flank the open gangway, matching the yellow end accents in reference.
        for xx in (-5.70,5.70):
            cube('lamp housing',(off+xx,sign*1.84,.9),(.16,.10,.30),outer,.025)
            cube('lamp lens',(off+xx,sign*1.77,.9),(.07,.025,.18),lamp,.014)

# Small door markings / recessed lower panels add legibility, not photo frames.
for obj in list(bpy.data.objects):
    if not obj.name.startswith('DoorLower_'):continue
    x,y,z=obj.location
    for dx in (-.34,.34):
        cube('door lower inset',(x+dx,y+math.copysign(.085,y),.35),(.52,.018,.46),red,.015)

ground=worn('dark track bed',(.012,.017,.021),.0,.9,.28,.005)
cube('context ground',(13,0,-1.08),(80,45,.12),ground,0)
for y in (-2.85,2.85):
    cube('running rail',(13,y,-.8),(65,.06,.13),silver,.008)
for x in range(-18,50):
    cube('track sleeper',(x,0,-.91),(.16,6.4,.13),rubber,.015)

def light(name,loc,target,power,color,size,kind='AREA'):
    bpy.ops.object.light_add(type=kind,location=loc);o=bpy.context.object;o.name='LD / '+name
    o.data.energy=power;o.data.color=color
    if kind=='AREA':o.data.shape='DISK';o.data.size=size
    elif kind=='POINT':o.data.shadow_soft_size=size
    o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
    return o

scene=bpy.context.scene
scene.world.use_nodes=True
bg=scene.world.node_tree.nodes.get('Background');bg.inputs['Color'].default_value=(.065,.09,.135,1);bg.inputs['Strength'].default_value=.16
light('cool soft key',(9,4,11),(13,0,0),1700,(.78,.86,1),6)
light('warm interior bounce',(13,-2.3,5),(13,0,.2),420,(1,.75,.47),4)
light('upper edge light',(14,-6,7),(13,0,1),1300,(.7,.8,1),5)
light('readable front body fill',(13,6,4),(13,0,1),430,(1,.88,.76),6)
for x in (7.3,18.7):
    for y in (-1.65,1.65):light('end lamp pool',(x,y,1.20),(x,0,0),20,(1,.57,.19),.16,'POINT')

bpy.ops.object.camera_add(location=(13,11.8,15.8));cam=bpy.context.object;cam.name='LD / reference camera';scene.camera=cam
cam.rotation_euler=(Vector((13,0,.55))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.type='ORTHO';cam.data.ortho_scale=17.8;cam.data.clip_start=.3;cam.data.clip_end=150
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.threads_mode='FIXED';scene.render.threads=6
scene.cycles.max_bounces=6;scene.cycles.transparent_max_bounces=8
scene.render.resolution_x=1600;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast';scene.view_settings.exposure=.4
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(OUT/'material-lighting-preview.png')
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            s=area.spaces.active;s.shading.type='MATERIAL'
            s.region_3d.view_location=Vector((13,0,.55));s.region_3d.view_distance=20
            s.region_3d.view_rotation=cam.rotation_euler.to_quaternion()
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'SurvivalTrain-LookDev.blend'))
(OUT/'material-manifest.json').write_text(json.dumps({
    'purpose':'Isolated reference material / lighting trial, NOT runtime integration',
    'source':'Original ImageGen surface atlas + editable metric Blender procedural floor/roughness/bump nodes; no downloaded third-party textures',
    'units':'meters; unchanged 12 x 4.2m game-sized carriage',
    'floor':'3.7cm diagonal knurl; 1.2 x 1.4m shader panel divisions',
    'interior_props':'Bench/partition visual proposal only; no gameplay colliders',
    'lighting':'Offline Cycles soft shadows and warm/cool area lights; must bake/approximate and verify in Unity before shipping',
    'mobile_status':'Not tested; shader nodes and preview lights are not mobile runtime assets',
    'materials':[m.name for m in set(mats+[seatmat,ground])]
},indent=2),encoding='utf-8')
if '--no-render' not in sys.argv:bpy.ops.render.render(write_still=True)
print('LOOKDEV_DONE',str(OUT))
