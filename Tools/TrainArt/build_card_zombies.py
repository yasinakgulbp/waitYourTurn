"""Original faceted paper monsters; Blender background source and Unity skin data.
Continuous lofted limbs have two-weight joints. No downloaded meshes/textures.
"""
import bpy, json, math, random
from pathlib import Path
from mathutils import Vector
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/Game/Art/CardZombies/SourceData'
SOURCE = ROOT/'ArtSource/CardZombies'
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
payload={'bones':[{'name':n,'parent':p,'position':list(v)} for n,p,v in bones],'parts':[]}
paper=(.76,.77,.75); pale=(.85,.85,.82); shade=(.50,.52,.52); ink=(.10,.12,.13); red=(.48,.055,.085)
mats={}
def mat(c):
 if c not in mats:
  m=bpy.data.materials.new('Card dye '+str(len(mats)));m.diffuse_color=(*c,1);m.use_nodes=True
  s=m.node_tree.nodes.get('Principled BSDF');s.inputs['Base Color'].default_value=(*c,1);s.inputs['Roughness'].default_value=.86;mats[c]=m
 return mats[c]
for idx,(name,scale,width) in enumerate([('Fast',.84,.64),('Normal',1.,.83),('Tough',1.13,1.28),('Boss',1.08,1.5)]):
 rng=random.Random(908+idx);parts=[]
 def piece(label,verts,faces,color,weights):
  mesh=bpy.data.meshes.new(label);mesh.from_pydata([C(v) for v in verts],[],faces);mesh.update()
  o=bpy.data.objects.new(name+' / '+label,mesh);bpy.context.collection.objects.link(o)
  o.data.materials.append(mat(color))
  for vi,(b0,b1,w) in enumerate(weights):
   for b,a in [(b0,w),(b1,1-w)]:
    if a<=0:continue
    g=o.vertex_groups.get(bones[b][0]) or o.vertex_groups.new(name=bones[b][0]);g.add([vi],a,'ADD')
  parts.append((o,color,weights));return o
 def loft(label,rings,color=paper,sides=8):
  # ring: centre, X/Z radii, bone0, bone1, weight0; horizontal folded rings.
  v=[];w=[];f=[]
  for k,(p,rx,rz,b0,b1,a) in enumerate(rings):
   for j in range(sides):
    angle=2*math.pi*j/sides + math.pi/8
    v.append((p[0]+math.cos(angle)*rx,p[1],p[2]+math.sin(angle)*rz));w.append((b0,b1,a))
  f.append(tuple(reversed(range(sides))))
  for k in range(len(rings)-1):
   for j in range(sides):
    a=k*sides+j;b=k*sides+(j+1)%sides;c=b+sides;d=a+sides
    f.extend([(a,b,c),(a,c,d)] if (k+j)%2 else [(a,b,d),(b,c,d)])
  f.append(tuple(range((len(rings)-1)*sides,len(rings)*sides)))
  return piece(label,v,[tuple(reversed(face)) for face in f],color,w)
 def shard(label,center,size,color,bone):
  x,y,z=center;rx,ry,rz=size
  v=[(x-rx,y,z),(x+rx,y,z),(x,y-ry,z),(x,y+ry,z),(x,y,z-rz),(x,y,z+rz)]
  return piece(label,v,[(0,4,2),(2,4,1),(1,4,3),(3,4,0),(2,5,0),(1,5,2),(3,5,1),(0,5,3)],color,[(bone,bone,1)]*6)
 def spike(label,start,end,radius,color,bone,sides=5):
  axis=Vector(end)-Vector(start);t=axis.normalized();u=t.cross(Vector((0,1,0)))
  if u.length<.01:u=t.cross(Vector((1,0,0)))
  u.normalize();v=t.cross(u);verts=[]
  for j in range(sides):
   p=Vector(start)+radius*(math.cos(j*math.tau/sides)*u+math.sin(j*math.tau/sides)*v);verts.append(tuple(p))
  verts.append(end);faces=[tuple(reversed(range(sides)))]+[(j,(j+1)%sides,sides) for j in range(sides)]
  return piece(label,verts,faces,color,[(bone,bone,1)]*len(verts))
 def patch(label,verts,color,bone):
  return piece(label,verts,[tuple(reversed(range(len(verts))))],color,[(bone,bone,1)]*len(verts))
 # A tapered torso with actual abdominal/chest planes, not a cuboid coat.
 heavy=name in ['Tough','Boss']
 loft('Continuous folded torso',[
  ((0,.73,0),.19*width,.12,0,0,1),((0,.85,0),.23*width,.135,0,1,.65),
  ((0,1.00,0),.22*width,.15,1,0,.85),((0,1.14,0),.31*width,.19 if heavy else .145,1,1,1),
  ((0,1.29,-.025),.32*width,.15,1,1,1),((0,1.35,0),.13,.09,1,1,1)],paper,8)
 for s in [-1,1]:
  if heavy:
   shard('Pectoral planes',(s*.17*width,1.19,.145),(.20*width,.14,.068),pale,1)
   for k in range(3):shard('Abdominal fold',(s*.09,.90+k*.077,.143),(.085,.035,.026),shade,1)
  else:
   for k in range(3):
    patch('Drawn rib crease',[(s*.025,1.07+k*.047,.15),(s*.19*width,1.10+k*.047,.12),(s*.17*width,1.11+k*.047,.123)],shade,1)
 for k in range(3):shard('Small red torn chest',(rng.uniform(-.17,.17)*width,1.08+k*.055,.204 if heavy else .155),(.04,.012,.008),red,1)
 loft('Neck', [((0,1.34,.02),.085,.075,1,2,.5),((0,1.44,.02),.085,.075,2,2,1)],shade,6)
 def head(cx,cy,cz,size,bone,label):
  loft(label,[((cx,cy-.12*size,cz+.028*size),.075*size,.09*size,bone,bone,1),
   ((cx,cy-.035*size,cz),.145*size,.126*size,bone,bone,1),
   ((cx,cy+.09*size,cz),.145*size,.12*size,bone,bone,1),
   ((cx,cy+.17*size,cz-.02*size),.09*size,.08*size,bone,bone,1)],pale,7)
  for s in [-1,1]:
   shard('Deep eye socket',(cx+s*.065*size,cy+.025*size,cz+.116*size),(.052*size,.034*size,.013*size),ink,bone)
   shard('Red eye',(cx+s*.062*size,cy+.026*size,cz+.133*size),(.014*size,.011*size,.006*size),red,bone)
   shard('Angry brow',(cx+s*.063*size,cy+.067*size,cz+.124*size),(.066*size,.026*size,.018*size),shade,bone)
  shard('Nose plane',(cx,cy-.005*size,cz+.143*size),(.032*size,.043*size,.041*size),paper,bone)
  patch('Torn mouth',[(cx-.077*size,cy-.05*size,cz+.121*size),(cx,cy-.035*size,cz+.137*size),
   (cx+.077*size,cy-.05*size,cz+.121*size),(cx+.06*size,cy-.106*size,cz+.10*size),(cx-.065*size,cy-.10*size,cz+.10*size)],ink,bone)
  for k in range(5):
   x=cx+(k-2)*.023*size
   patch('Ragged tooth',[(x-.008*size,cy-.053*size,cz+.136*size),(x+.008*size,cy-.053*size,cz+.136*size),(x,cy-.078*size,cz+.13*size)],pale,bone)
 head(0,1.56,.03,1,2,'Skull')
 for s,arm,leg in [(-1,3,9),(1,6,12)]:
  x=s*.31;thick=(.11 if heavy else .063)*width
  # Continuous shoulder-to-wrist skin. Ring weights mix both sides of each elbow.
  loft('Arm with blended elbow',[
   ((x,1.26,.015),thick*1.5,thick*1.3,arm,arm,1),((x,1.15,.05),thick*1.25,thick,arm,arm,1),
   ((x,.98,.14),thick*.72,thick*.72,arm,arm+1,.75),((x,.94,.16),thick*.8,thick*.8,arm,arm+1,.5),
   ((x,.90,.18),thick*.83,thick*.83,arm+1,arm,.8),((x,.81,.255),thick,thick*.8,arm+1,arm+1,1),
   ((x,.72,.30),thick*.58,thick*.58,arm+1,arm+2,.5)],paper,7)
  shard('Palm',(x,.683,.33),(.070 if heavy else .048,.055,.052),pale,arm+2)
  # Three articulated-looking bent claws, rigid to hand; no finger bone/renderer cost.
  for d in range(3):
   xx=x+(d-1)*.044;end=(xx+s*.018,.55,.40)
   spike('Folded digit',(xx,.675,.37),(xx,.595,.405),.023 if heavy else .014,paper,arm+2,4)
   spike('Red claw tip',(xx,.595,.405),end,.015 if heavy else .011,red,arm+2,4)
  spike('Thumb',(x-s*.052,.69,.32),(x-s*.10,.61,.37),.024,paper,arm+2,4)
  if heavy:
   shard('Shoulder plate',(x,1.25,-.005),(.20,.12,.17),pale,arm)
  lx=s*.14
  loft('Leg with blended knee',[
   ((lx,.80,0),.105*width,.115,leg,leg,1),((lx,.62,0),.10*width,.105,leg,leg,1),
   ((lx,.47,.01),.069*width,.075,leg,leg+1,.8),((lx,.43,.02),.075*width,.085,leg,leg+1,.5),
   ((lx,.39,.023),.07*width,.075,leg+1,leg,.8),((lx,.23,.04),.065*width,.067,leg+1,leg+1,1),
   ((lx,.085,.07),.051*width,.059,leg+1,leg+2,.5)],shade if heavy else paper,7)
  loft('Folded foot', [((lx,.018,.12),.08*width,.14,leg+2,leg+2,1),((lx,.09,.12),.082*width,.15,leg+2,leg+2,1),
   ((lx,.13,.09),.06*width,.10,leg+2,leg+2,1)],paper,6)
  patch('Knee tear',[(lx-.035,.46,.103),(lx+.02,.45,.103),(lx+.04,.40,.11)],red,leg+1)
 if name=='Fast':
  spike('Torn shoulder shard',(-.3,1.26,0),(-.46,1.40,-.03),.07,paper,3)
  patch('Red flank fold',[(0,.80,.137),(.13,.89,.13),(.14,.81,.14)],red,0)
 if name=='Boss':
  head(-.26,1.46,-.06,.85,1,'Left fused head');head(.26,1.50,-.08,.90,1,'Right fused head')
  for s,arm in [(-1,3),(1,6)]:
   for k in range(3):spike('Shoulder thorn',(s*(.31+k*.055),1.28-k*.035,-.025),(s*(.42+k*.105),1.62-k*.12,-.05),.075,pale,arm)
   spike('Skull horn',(s*.095,1.69,.035),(s*.19,1.91,0),.048,pale,2)
  shard('Crimson heart',(0,1.17,.255),(.045,.07,.03),red,1)
  for s in [-1,1]:spike('Pelvis plate',(s*.16,.89,.13),(s*.25,.62,.15),.13,shade,0)
 # Export flat facets with two influences; source keeps shared vertices/skin groups.
 v=[];n=[];c=[];w0=[];w1=[];blend=[];tri=[]
 for o,color,weights in parts:
  o.data.calc_loop_triangles()
  for face in o.data.loop_triangles:
   start=len(w0);tint=.94+rng.random()*.09
   for vi in face.vertices:
    v.extend(U(o.data.vertices[vi].co));n.extend(U(face.normal));c.extend([*linear(tuple(min(1,t*tint) for t in color)),1])
    b0,b1,a=weights[vi];w0.append(b0);w1.append(b1);blend.append(a)
   tri.extend([start,start+1,start+2])
 payload['parts'].append({'name':name,'scale':scale,'vertices':v,'normals':n,'colors':c,'weights':w0,'secondWeights':w1,'blends':blend,'triangles':tri})
 bpy.ops.object.select_all(action='DESELECT')
 for o,_,_ in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0][0];bpy.ops.object.join();mesh=bpy.context.object;mesh.name=name+' continuous faceted card mesh'
 bpy.ops.object.armature_add(enter_editmode=True);rig=bpy.context.object;rig.name=name+' 15 bone rig'
 bpy.ops.armature.select_all(action='SELECT');bpy.ops.armature.delete()
 for b,parent,pos in bones:
  eb=rig.data.edit_bones.new(b);eb.head=C(pos);eb.tail=eb.head+Vector((0,0,.12))
  if parent>=0:eb.parent=rig.data.edit_bones[bones[parent][0]]
 bpy.ops.object.mode_set(mode='OBJECT');mod=mesh.modifiers.new('Blended paper joints','ARMATURE');mod.object=rig;mesh.parent=rig
 rig.location.x=(idx-1.5)*1.8;rig.scale=(scale,)*3
 def pose(kind,t):
  for bi,(b,_,_) in enumerate(bones):
   pb=rig.pose.bones[b];pb.rotation_mode='XYZ';angle=0;z=0
   if bi==1:angle=18 if name=='Fast' else 7 if not heavy else 3
   if bi==2:angle=-10 if name=='Fast' else -4
   if bi in [3,6]:angle=-52 if name=='Normal' else -24;z=-8 if bi==3 else 8
   ph=t*math.tau
   if kind=='Walk':
    if bi in [9,12]:angle=math.sin(ph+(math.pi if bi==12 else 0))*(38 if name=='Fast' else 23)
    if bi in [10,13]:angle=max(0,-math.sin(ph+(math.pi if bi==13 else 0)))*46
    if bi in [3,6]:angle+=math.sin(ph+(math.pi if bi==3 else 0))*17
   if kind=='Attack':
    if bi in [3,6]:angle-=65*math.sin(math.pi*t)
    if bi==1:angle+=12*math.sin(math.pi*t)
   if kind=='Hit' and bi in [0,1,2]:angle-=20*math.sin(math.pi*t)
   if kind=='Death':
    if bi==0:angle=-85*t;z=12*t
    if bi in [9,12]:angle+=25*t
   pb.rotation_euler=(math.radians(angle),0,math.radians(z))
 for action,duration in [('Idle',60),('Walk',30),('Attack',30),('Hit',18),('Death',36)]:
  rig.animation_data_clear()
  for f in range(0,duration+1,3):
   pose(action,f/duration)
   for pb in rig.pose.bones:pb.keyframe_insert(data_path='rotation_euler',frame=f)
  rig.animation_data.action.name=name+'_'+action;rig.animation_data.action.use_fake_user=True
 rig.animation_data_clear();pose('Walk',.15 if name=='Fast' else 0)
 rig['motion_owner']='Unity NavMesh; no root motion';mesh['unity_visual_scale']=scale
(OUT/'CardZombies.json').write_text(json.dumps(payload,separators=(',',':')))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.world.color=(.025,)*3
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.025));bpy.context.object.data.materials.append(mat((.19,.21,.23)))
def area(pos,power,size):
 bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=power;o.data.size=size;o.rotation_euler=(Vector((0,0,1))-o.location).to_track_quat('-Z','Y').to_euler()
area((-4,-5,7),1000,6);area((5,2,5),850,5)
bpy.ops.object.camera_add(location=(3,-11,5));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=9;scene.camera=cam
scene.render.resolution_x=1440;scene.render.resolution_y=810;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.render.filepath=str(SOURCE/'CardZombies-preview.png')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'CardZombies.blend'));bpy.ops.render.render(write_still=True)
print('CARD_ZOMBIES '+str([(p['name'],len(p['triangles'])//3) for p in payload['parts']]))
