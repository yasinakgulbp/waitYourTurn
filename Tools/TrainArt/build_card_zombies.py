"""Original folded-card characters. Run with Blender --background --python this file.
Unity owns locomotion; this source owns meshes, common bind skeleton and in-place poses.
"""
import bpy, json, math
from pathlib import Path
from mathutils import Vector, Matrix
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Game/Art/CardZombies/SourceData'
SOURCE = ROOT / 'ArtSource/CardZombies'
OUT.mkdir(parents=True, exist_ok=True); SOURCE.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
def C(p): return Vector((p[0], -p[2], p[1]))
def U(p): return (p[0], p[2], -p[1])
def linear(c): return tuple(v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in c)
bones = [
 ('Hips',-1,(0,.80,0)), ('Chest',0,(0,1.05,0)), ('Head',1,(0,1.40,.02)),
 ('ArmL',1,(-.30,1.22,.02)),('ForearmL',3,(-.31,.94,.16)),('HandL',4,(-.31,.72,.30)),
 ('ArmR',1,(.30,1.22,.02)),('ForearmR',6,(.31,.94,.16)),('HandR',7,(.31,.72,.30)),
 ('ThighL',0,(-.14,.78,0)),('ShinL',9,(-.14,.43,.02)),('FootL',10,(-.14,.09,.07)),
 ('ThighR',0,(.14,.78,0)),('ShinR',12,(.14,.43,.02)),('FootR',13,(.14,.09,.07))]
payload={'bones':[{'name':n,'parent':p,'position':list(v)} for n,p,v in bones], 'parts':[]}
mats={}
def mat(color):
 if color not in mats:
  m=bpy.data.materials.new('Dyed card '+str(len(mats))); m.diffuse_color=(*color,1);m.use_nodes=True
  s=m.node_tree.nodes.get('Principled BSDF');s.inputs['Base Color'].default_value=(*color,1);s.inputs['Roughness'].default_value=.82;mats[color]=m
 return mats[color]
paper=(.78,.79,.75); edge=(.49,.47,.43); ink=(.09,.12,.14); red=(.43,.12,.15)
variants=[('Normal',1.,1.,red),('Fast',.84,.80,(.56,.24,.25)),('Tough',1.13,1.28,(.29,.33,.35)),('Boss',1.45,1.65,(.38,.13,.17))]
for idx,(name,scale,width,coat) in enumerate(variants):
 parts=[]
 def box(label,pos,size,color,bone,bevel=.015,rot=(0,0,0)):
  bpy.ops.mesh.primitive_cube_add(size=1, location=C(pos));o=bpy.context.object;o.name=name+' / '+label
  o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  o.rotation_euler=(math.radians(rot[0]),math.radians(-rot[2]),math.radians(rot[1]))
  if bevel:
   mod=o.modifiers.new('Cut card edges','BEVEL');mod.width=bevel;mod.segments=1
   bpy.ops.object.modifier_apply(modifier=mod.name)
  o.data.materials.append(mat(color));g=o.vertex_groups.new(name=bones[bone][0]);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
  parts.append((o,bone,color));return o
 box('Folded coat',(0,1.11,0),(.51*width,.54,.28),coat,1,.035)
 box('Lower fold',(0,.85,.01),(.48*width,.16,.26),coat,0,.015)
 box('Coat crease',(0,1.10,.149),(.013,.42,.005),edge,1,0)
 box('Collar left',(-.10,1.35,.15),(.14,.10,.035),paper,1,.005,(0,0,-22))
 box('Collar right',(.10,1.35,.15),(.14,.10,.035),paper,1,.005,(0,0,22))
 box('Neck',(0,1.39,.02),(.15,.13,.14),edge,2,.006)
 box('Head',(0,1.56,.045),(.36,.34,.29),paper,2,.04)
 box('Crown folded cap',(0,1.733,.02),(.32,.017,.22),edge,2,.005)
 for side in [-1,1]:
  box('Eye recess',(side*.085,1.59,.19),(.10,.046,.012),ink,2,.006)
  box('Red eye',(side*.09,1.589,.2),(.032,.019,.008),red,2,.002)
  box('Brow',(side*.08,1.63,.199),(.115,.019,.012),edge,2,.003,(0,0,side*12))
 box('Nose fold',(0,1.54,.207),(.047,.056,.049),edge,2,.008)
 box('Mouth',(0,1.472,.19),(.13,.035,.01),ink,2,.004)
 for k in [-1,0,1]: box('Paper teeth',(k*.035,1.48,.198),(.019,.013,.009),paper,2,0)
 # Fold marks and a small torn red cheek identify paper rather than realistic skin.
 box('Cheek tear',(-.14,1.50,.192),(.019,.072,.008),red,2,0,(0,0,18))
 for s,base,leg in [(-1,3,9),(1,6,12)]:
  x=s*.31
  box('Sleeve',(x,1.11,.085),(.18*width,.30,.18),coat,base,.02,(-25,0,0))
  box('Folded forearm',(x,.84,.23),(.13,.28,.13),paper,base+1,.012,(-25,0,0))
  box('Cuff seam',(x,.951,.163),(.142,.018,.145),edge,base+1,.002,(-25,0,0))
  box('Mitten',(x,.684,.326),(.16,.12,.13),paper,base+2,.014)
  box('Knuckle crease',(x,.676,.397),(.11,.009,.007),edge,base+2,0)
  box('Trouser thigh',(s*.14,.61,0),(.19,.34,.20),ink,leg,.016)
  box('Knee fold',(s*.14,.43,.126),(.15,.038,.018),edge,leg+1,.003)
  box('Trouser shin',(s*.14,.26,.035),(.16,.32,.17),ink,leg+1,.015)
  box('Card shoe',(s*.14,.075,.124),(.20,.14,.30),edge,leg+2,.018)
  box('Shoe sole',(s*.14,.015,.124),(.207,.023,.30),ink,leg+2,.005)
 if name in ['Tough','Boss']:
  box('Card breastplate',(0,1.12,.168),(.42*width,.30,.036),edge,1,.015)
  box('Printed red band',(0,1.14,.189),(.37*width,.049,.01),red,1,0)
  for s in [-1,1]: box('Shoulder folds',(s*.31,1.26,.035),(.26,.12,.26),paper,3 if s<0 else 6,.02)
 if name=='Boss':
  box('Crown red band',(0,1.70,.195),(.29,.04,.014),red,2,.002)
  box('Paper apron',(0,.79,.16),(.59,.20,.024),coat,0,.008)
 # Flatten corners/normals into one renderer, one bone weight per rigid paper piece.
 v=[];norm=[];col=[];weights=[];tri=[]
 for o,b,c in parts:
  o.data.calc_loop_triangles()
  for face in o.data.loop_triangles:
   start=len(weights)
   n=o.matrix_world.to_3x3()@face.normal
   shade=.94 if 'seam' in o.name.lower() else 1
   for vi in face.vertices:
    p=o.matrix_world@o.data.vertices[vi].co;v.extend(U(p));norm.extend(U(n));col.extend([*linear(tuple(t*shade for t in c)),1]);weights.append(b)
   # Coordinate conversion preserves handedness; triangles remain CCW.
   tri.extend([start,start+1,start+2])
 payload['parts'].append({'name':name,'scale':scale,'vertices':v,'normals':norm,'colors':col,'weights':weights,'triangles':tri})
 # Editable source: merge mesh pieces but keep deformation groups.
 bpy.ops.object.select_all(action='DESELECT')
 for o,_,_ in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0][0];bpy.ops.object.join();mesh=bpy.context.object;mesh.name=name+' single card mesh'
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.object.armature_add(enter_editmode=True);rig=bpy.context.object;rig.name=name+' 15 bone rig'
 bpy.ops.armature.select_all(action='SELECT');bpy.ops.armature.delete()
 for n,parent,pos in bones:
  b=rig.data.edit_bones.new(n);b.head=C(pos);b.tail=b.head+Vector((0,0,.12))
  if parent>=0:b.parent=rig.data.edit_bones[bones[parent][0]]
 bpy.ops.object.mode_set(mode='OBJECT');mod=mesh.modifiers.new('Paper hinges','ARMATURE');mod.object=rig;mesh.parent=rig
 rig.location.x=(idx-1.5)*2.0;rig.scale=(scale,)*3
 # Source previews use in-place skeletal actions; no translated root tracks.
 for action,duration in [('Idle',60),('Walk',30),('Attack',30)]:
  rig.animation_data_clear()
  for f in range(0,duration+1,5):
   t=f/duration;phase=t*2*math.pi
   for bi,(n,_,_) in enumerate(bones):
    pb=rig.pose.bones[n];pb.rotation_mode='XYZ';angle=0
    if action=='Idle':angle=math.sin(phase)*2 if bi in [1,2] else 0
    if action=='Walk':
     if bi in [9,12]:angle=math.sin(phase+(math.pi if bi==12 else 0))*24
     elif bi in [10,13]:angle=max(0,-math.sin(phase+(math.pi if bi==13 else 0)))*32
     elif bi in [3,6]:angle=-22+math.sin(phase+(math.pi if bi==3 else 0))*12
    if action=='Attack' and bi in [3,6]:angle=-30-60*math.sin(math.pi*t)
    pb.rotation_euler=(math.radians(angle),0,0);pb.keyframe_insert(data_path='rotation_euler',frame=f)
  rig.animation_data.action.name=name+'_'+action
  rig.animation_data.action.use_fake_user=True
 rig.animation_data_clear()
 for pb in rig.pose.bones:pb.rotation_euler=(0,0,0)
 mesh['unity_visual_scale']=scale;rig['motion_owner']='NavMeshAgent only; in-place hinge poses'
(OUT/'CardZombies.json').write_text(json.dumps(payload,separators=(',',':')))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24
scene.world.color=(.045,.045,.045)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.025));floor=bpy.context.object;floor.name='Preview only studio';floor.data.materials.append(mat((.16,.18,.20)))
def area(pos,power,size):
 bpy.ops.object.light_add(type='AREA',location=pos);l=bpy.context.object;l.data.energy=power;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(Vector((0,0,1))-l.location).to_track_quat('-Z','Y').to_euler()
area((-4,-5,7),1000,6);area((5,2,5),850,5)
bpy.ops.object.camera_add(location=(5,-10,6));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1.0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=10;scene.camera=cam
scene.render.resolution_x=1440;scene.render.resolution_y=810;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.render.filepath=str(SOURCE/'CardZombies-preview.png')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'CardZombies.blend'));bpy.ops.render.render(write_still=True)
print('CARD_ZOMBIES '+str([(p['name'],len(p['triangles'])//3) for p in payload['parts']]))
