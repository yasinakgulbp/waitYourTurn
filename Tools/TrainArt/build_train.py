"""Blender source + FBX handoff + exact Unity-coordinate mesh payload.
Run: blender --background --factory-startup --python Tools/TrainArt/build_train.py
1m=1 unit. Unity (X,Y,Z) -> Blender (X,-Z,Y). Runtime never runs this generator.
"""
import bpy, json, math, sys
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/Game/Art/SurvivalTrain'
SOURCE=ROOT/'ArtSource/SurvivalTrain'; SOURCE.mkdir(parents=True,exist_ok=True)
(OUT/'SourceData').mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
parts=[]; groups={}; materials={}
atlas=bpy.data.images.load(str(OUT/'Textures/TrainAtlas_Albedo.jpg'))
normal=bpy.data.images.load(str(OUT/'Textures/TrainAtlas_Normal.png')); normal.colorspace_settings.name='Non-Color'
packed=bpy.data.images.load(str(OUT/'Textures/TrainAtlas_MetalSmooth.png')); packed.colorspace_settings.name='Non-Color'
def material(kind):
    if kind in materials:return materials[kind]
    m=bpy.data.materials.new(kind); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF')
    if kind=='Glass':
        p.inputs['Base Color'].default_value=(.085,.18,.20,1); p.inputs['Metallic'].default_value=.2
        p.inputs['Roughness'].default_value=.19; p.inputs['Alpha'].default_value=.18
        m.diffuse_color=(.085,.18,.20,.18)
    else:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=atlas;m.node_tree.links.new(t.outputs['Color'],p.inputs['Base Color'])
        mask=m.node_tree.nodes.new('ShaderNodeTexImage');mask.image=packed
        split=m.node_tree.nodes.new('ShaderNodeSeparateColor');m.node_tree.links.new(mask.outputs['Color'],split.inputs['Color'])
        m.node_tree.links.new(split.outputs['Red'],p.inputs['Metallic'])
        inverse=m.node_tree.nodes.new('ShaderNodeMath');inverse.operation='SUBTRACT';inverse.inputs[0].default_value=1
        m.node_tree.links.new(mask.outputs['Alpha'],inverse.inputs[1]);m.node_tree.links.new(inverse.outputs[0],p.inputs['Roughness'])
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=normal
        n=m.node_tree.nodes.new('ShaderNodeNormalMap');n.inputs['Strength'].default_value=.5
        m.node_tree.links.new(t.outputs['Color'],n.inputs['Color']);m.node_tree.links.new(n.outputs['Normal'],p.inputs['Normal'])
    materials[kind]=m;return m
def C(p):return (p[0],-p[2],p[1])
def U(p):return (p[0],p[2],-p[1])
def record(o,group,tile):
    # All transforms are baked before assigning box-projected atlas UVs.
    mesh=o.data
    while mesh.uv_layers: mesh.uv_layers.remove(mesh.uv_layers[0])
    uv=mesh.uv_layers.new(name='TrainAtlas')
    coords=[v.co for v in mesh.vertices]
    lo=Vector([min(c[i] for c in coords) for i in range(3)]); hi=Vector([max(c[i] for c in coords) for i in range(3)])
    span=hi-lo
    for face in mesh.polygons:
        face_tile=tile(face) if callable(tile) else tile
        axis=max(range(3),key=lambda i:abs(face.normal[i])); a,b=[i for i in range(3) if i!=axis]
        for li in face.loop_indices:
            v=coords[mesh.loops[li].vertex_index]
            s=(v[a]-lo[a])/max(span[a],.00001);t=(v[b]-lo[b])/max(span[b],.00001)
            uv.data[li].uv=((face_tile%4+.025+s*.95)/4,(3-face_tile//4+.025+t*.95)/4)
    o.data.materials.append(material('Glass' if group.endswith('Glass') else 'TrainAtlas'))
    groups.setdefault(group,[]).append(o)
    return o
def box(name,p,size,tile=0,bevel=.025,group='Body'):
    bpy.ops.mesh.primitive_cube_add(size=1,location=C(p));o=bpy.context.object;o.name=name
    o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel>0:
        m=o.modifiers.new('Small manufactured edge radius','BEVEL');m.width=min(bevel,min(size)*.22);m.segments=1
        bpy.ops.object.modifier_apply(modifier=m.name)
        for f in o.data.polygons:f.use_smooth=False
        m=o.modifiers.new('Face-weighted normals','WEIGHTED_NORMAL');m.keep_sharp=True
        bpy.ops.object.modifier_apply(modifier=m.name)
    return record(o,group,tile)
def wheel(p,group):
    # Axle along train's cross-axis, cylinders kept low-sided and below the floor.
    bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=.34,depth=.18,location=C(p),rotation=(math.pi/2,0,0))
    o=bpy.context.object;o.name='Wheel - 12 sided';bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    record(o,group,2)
def formed_skin(a,b,sign,group):
    # Pressed sheet with a curved exterior knee; interior footprint remains unchanged.
    profile=[(.00,2.01),(.00,2.13),(.12,2.17),(.48,2.17),(.70,2.10),(.70,2.01)]
    vertices=[C((x,y,sign*z)) for x in (a,b) for y,z in profile]
    n=len(profile);faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    if sign<0:faces=[tuple(reversed(f)) for f in faces]
    mesh=bpy.data.meshes.new('Formed steel sheet');mesh.from_pydata(vertices,[],faces);mesh.update()
    o=bpy.data.objects.new('Pressed lower shell',mesh);bpy.context.collection.objects.link(o)
    record(o,group,lambda f:9 if f.normal.y*sign<-.2 else 1)
def rolled_rim(group):
    # Rounded rectangle, 30cm corners, an actual formed rail rather than cube seams.
    radius=.30;points=[]
    for cx,cz,start in ((5.73,1.88,0),(-5.73,1.88,90),(-5.73,-1.88,180),(5.73,-1.88,270)):
        for i in range(7):
            angle=math.radians(start+i*15);points.append((cx+radius*math.cos(angle),cz+radius*math.sin(angle)))
    verts=[]
    for i,(x,z) in enumerate(points):
        prev=Vector(points[(i-1)%len(points)]);nxt=Vector(points[(i+1)%len(points)])
        d=(nxt-prev).normalized();n=Vector((d.y,-d.x))
        for width,y in ((-.05,2.075),(.05,2.075),(.05,2.175),(-.05,2.175)):
            verts.append(C((x+n.x*width,y,z+n.y*width)))
    faces=[]
    for i in range(len(points)):
        j=(i+1)%len(points)
        for k in range(4):faces.append((i*4+k,i*4+(k+1)%4,j*4+(k+1)%4,j*4+k))
    mesh=bpy.data.meshes.new('Rounded perimeter rail');mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new('Continuous rolled corner rail',mesh);bpy.context.collection.objects.link(o);record(o,group,6)
    for sx in (-1,1):
        for sz in (-1,1):
            # Rounded exterior vertical protection sits over the solid corner bay.
            bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=.10,depth=2.08,location=C((sx*5.92,1.04,sz*2.06)))
            o=bpy.context.object;o.name='Rounded corner guard';record(o,group,6)
def sidewall(a,b,sign,group):
    if b-a<.01:return
    z=sign*2.1;mid=(a+b)/2
    if a<=-5.999 or b>=5.999:
        box('Solid corner bay', (mid,1.05,z),(b-a,2.1,.18),9,.04,group)
        # Panel ventilation remains on shot-blocking metal only.
        box('Corner ventilation',(mid,.4,z+sign*.097),(max(.3,b-a-.25),.42,.016),5,.003,group)
        box('Transit stencil',(mid,1.14,z-sign*.103),(.85,.30,.012),15,.002,group)
    else:
        skin=lambda f:9 if f.normal.y*sign<-.2 else 1
        for x in (a+.21,b-.21):box('Window wide border',(x,1.05,z),(.42,2.1,.18),skin,.018,group)
        usable=b-a-.84;panes=math.ceil((usable+.32)/(2.2+.32));width=(usable-(panes-1)*.32)/panes
        for i in range(panes):
            left=a+.42+i*(width+.32)
            # Sheet sections meet posts edge-to-edge: no overlapping/coplanar skins.
            formed_skin(left,left+width,sign,group)
            box('Window shoulder plate',(left+width/2,.725,z),(width,.05,.18),1,.008,group)
            box('Upper worn metal',(left+width/2,1.825,z),(width,.55,.18),skin,.016,group)
            box('Clear side glass',(left+width/2,1.15,z),(width,.8,.035),11,0,group+'Glass')
            # Slim metallic trim lies OUTSIDE the clear shot aperture.
            for y in (.725,1.575):box('Window gasket horizontal',(left+width/2,y,z-sign*.1),(width,.045,.035),2,.006,group)
            for x in (left-.025,left+width+.025):box('Inset window gasket',(x,1.15,z-sign*.102),(.045,.8,.028),2,.004,group)
            if i<panes-1:box('Window structural post',(left+width+.16,1.05,z),(.32,2.1,.18),skin,.02,group)
    # Low fluting gives metro bodywork; texture details avoid thousands of bolts.
    for y in (.11,.23,.35,.47):box('Exterior lower fluting',(mid,y,z+sign*.098),(b-a,.024,.025),6,.005,group)
def wagon(count,open_left,open_right):
    group='Wagon'+str(count)
    for i in range(12):
        for j in range(3):box('Anti-slip floor plate',(-5.5+i,-.055,(j-1)*1.394),(.993,.11,1.389),4,.003,group)
    for z in (-.7,.7):box('Floor longitudinal service seam',(0,.004,z),(11.8,.008,.035),6,.001,group)
    for x in (-4.5,-2.5,-.5,1.5,3.5):box('Floor transverse plate seam',(x,.005,0),(.025,.01,4.05),6,.001,group)
    box('Underframe',(0,-.32,0),(11.7,.42,3.9),2,.05,group)
    for x in (-3.95,3.95):
        box('Bogie frame',(x,-.55,0),(1.8,.25,3.6),5,.025,group)
        for dx in (-.55,.55):
            for z in (-1.78,1.78):wheel((x+dx,-.62,z),group)
    for sign in (-1,1):
        slots=[-3.8,3.8]
        if count==6 or count==5 and sign==-1:slots.insert(1,0)
        previous=-6
        for x in slots:
            sidewall(previous,x-.725,sign,group);previous=x+.725
            for edge in (-1,1):box('Door structural jamb',(x+edge*.76,1.05,sign*2.1),(.07,2.1,.18),3,.012,group)
            box('Permanent door header',(x,2.05,sign*2.1),(1.45,.1,.18),3,.01,group)
            box('Exterior door sill',(x,-.12,sign*2.21),(1.45,.13,.3),6,.012,group)
            box('Door threshold inset',(x,.008,sign*1.38),(.6,.012,.045),6,.002,group)
        sidewall(previous,6,sign,group)
    for sign,opened in ((-1,open_left),(1,open_right)):
        if opened:
            for z in (-1.8,1.8):box('Open end jamb',(sign*5.9,1.05,z),(.2,2.1,.6),0,.03,group)
            box('Open end upper beam',(sign*5.9,2.175,0),(.2,.15,3),6,.022,group)
        else:
            box('Closed end bulkhead',(sign*5.9,1.05,0),(.2,2.1,4.2),9,.035,group)
            box('Bulkhead service panel',(sign*5.785,1.05,0),(.02,1.1,1.5),5,.004,group)
    rolled_rim(group)
    return group

wagon(6,False,True);wagon(5,True,True);wagon(4,True,False)
# One reusable lower door / glazing pair, authored at the unchanged door root.
for x in (-.363,.363):
    box('Red split lower door',(x,.36,0),(.72,.72,.16),8,.02,'DoorLower')
for x in (-.66,.66):box('Red outer door leaf frame',(x,1.36,0),(.13,1.28,.16),3,.017,'DoorLower')
box('Red upper leaf',(0,1.84,0),(1.45,.32,.16),3,.02,'DoorLower')
box('Upper split leaf seam',(0,1.44,0),(.035,.98,.16),3,.005,'DoorLower')
box('Door clear upper glazing',(0,1.2,0),(1.19,.96,.025),11,0,'DoorGlass')
# The metal seam starts at .95m, above the usual .85-.90m combat line.
# Its shot collider is authored explicitly by the Unity installer.

box('Gangway treadplate',(0,-.065,0),(1.4,.13,3.2),4,.015,'Connector')
for s in (-1,1):
    box('Gangway side base',(0,.55,s*1.6),(1.4,1.1,.2),2,.025,'Connector')
    for i in range(10):
        x=-.63+i*.14
        box('Bellows pleat',(x,.56,s*1.735),(.055,1.08,.18),12,.01,'Connector')
    box('Gangway metal rail',(0,1.075,s*1.6),(1.4,.05,.21),6,.015,'Connector')
# No cross-bar/door or roof across the 3m passage.
box('Automated cab',(0,1,0),(8,2,4.2),9,.17,'Locomotive')
box('Closed curved roof',(0,2.15,0),(8.04,.3,4.24),0,.065,'Locomotive')
box('Cab underframe',(0,-.31,0),(7.8,.5,3.95),2,.06,'Locomotive')
for s in (-1,1):
    box('Cab red waist stripe',(0,.53,s*2.108),(7.8,.18,.025),3,.01,'Locomotive')
    box('Cab inset window',(2.55,1.48,s*2.116),(1.5,.64,.03),11,.008,'Locomotive')
    for y in (.16,.27,.38):box('Cab lower ribs',(0,y,s*2.12),(7.8,.027,.03),6,.004,'Locomotive')
    box('Headlamp',(4.015,.85,s*1.4),(.08,.23,.3),10,.01,'Locomotive')
for x in (-2.6,-1.9,-1.2):box('Roof service vent',(x,2.317,0),(.52,.045,1.5),5,.006,'Locomotive')
box('Cab windshield',(4.017,1.51,0),(.035,.67,3.4),11,.012,'Locomotive')
for x in (-2.7,2.7):
    for z in (-1.8,1.8):wheel((x,-.63,z),'Locomotive')

payload={'parts':[]};stats={};exports=[]
for name,objects in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
    bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    o.data.calc_loop_triangles()
    # Export split vertices so hard edges and atlas UV islands remain correct.
    vertices=[];normals=[];uvs=[];triangles=[];cache={}
    uv=o.data.uv_layers.active.data
    for tri in o.data.loop_triangles:
        for li in tri.loops:
            vi=o.data.loops[li].vertex_index;v=o.matrix_world@o.data.vertices[vi].co
            n=o.data.corner_normals[li].vector
            pos=U(v);nor=U(n);tex=tuple(uv[li].uv)
            key=tuple(round(a,6) for a in (*pos,*nor,*tex))
            if key not in cache:
                cache[key]=len(vertices)//3;vertices.extend(key[:3]);normals.extend(key[3:6]);uvs.extend(key[6:8])
            triangles.append(cache[key])
    # U is a proper axis rotation (determinant +1), not a reflection.
    # Keep winding consistent with the exported outward normals.
    payload['parts'].append({'name':name,'vertices':vertices,'normals':normals,'uv':uvs,'triangles':triangles})
    stats[name]={'vertices':len(vertices)//3,'triangles':len(triangles)//3}
    exports.append(o)
    if name.startswith('Wagon5'):o.location.x=13
    if name.startswith('Wagon4'):o.location.x=26
    if name=='Locomotive':o.location.x=37
    if name=='Connector':o.location.x=6.5
    if name.startswith('Door'):o.location=C((-3.8,0,-2.1))
# Assemble editable Blender scene with linked door and gangway instances.
for count,xoffset in ((6,0),(5,13),(4,26)):
    for s in (-1,1):
        slots=[-3.8,3.8]+([0] if count==6 or count==5 and s==-1 else [])
        for x in slots:
            for name in ('DoorLower','DoorGlass'):
                source=bpy.data.objects[name];o=source.copy();o.data=source.data;bpy.context.collection.objects.link(o)
                o.name=f'{name}_{count}_{s}_{x}';o.location=C((xoffset+x,0,s*2.1))
source=bpy.data.objects['Connector'];o=source.copy();o.data=source.data;bpy.context.collection.objects.link(o);o.location.x=19.5
for name in ('DoorLower','DoorGlass'):bpy.data.objects[name].hide_render=True;bpy.data.objects[name].hide_set(True)

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.11,.13,.16,1)
scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.35
# Presentation only in Blender source, never exported into gameplay.
bpy.ops.object.light_add(type='AREA',location=(13,-7,14));key=bpy.context.object;key.name='Preview key only';key.data.energy=4000;key.data.shape='DISK';key.data.size=18
bpy.ops.object.light_add(type='AREA',location=(12,5,8));fill=bpy.context.object;fill.name='Preview fill only';fill.data.energy=2400;fill.data.size=14
bpy.ops.mesh.primitive_plane_add(size=150,location=(13,0,-1.05));ground=bpy.context.object;ground.name='Preview ground only'
mat=bpy.data.materials.new('Preview dark ground');mat.use_nodes=True
mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.016,.021,.025,1)
mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.85;ground.data.materials.append(mat)
bpy.ops.object.camera_add(location=C((14.2,16,-11.2)));cam=bpy.context.object;cam.name='Preview camera only';scene.camera=cam
direction=Vector(C((13,0,0)))-cam.location;cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler();cam.data.type='PERSP';cam.data.lens=38
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(SOURCE/'train-preview.png')
# Convenient solid/material viewport at the same game-facing angle.
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_location=Vector(C((13,0,0)))
            area.spaces.active.region_3d.view_distance=22
            area.spaces.active.region_3d.view_rotation=cam.rotation_euler.to_quaternion()
            area.spaces.active.shading.type='MATERIAL'
for image in (atlas,normal,packed):image.pack()
(OUT/'SourceData/TrainMeshes.json').write_text(json.dumps(payload,separators=(',',':')))
(SOURCE/'mesh-budget.json').write_text(json.dumps(stats,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'SurvivalTrain.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in exports:o.hide_set(False);o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(SOURCE/'SurvivalTrain.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
print('TRAIN_BUDGET',json.dumps(stats))
if '--no-render' not in sys.argv:bpy.ops.render.render(write_still=True)
