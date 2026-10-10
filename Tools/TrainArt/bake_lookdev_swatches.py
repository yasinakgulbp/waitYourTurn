"""Export original 1m-square color swatches, not the runtime train atlas.
All pixels come from the same editable Blender nodes used by the preview.
No editing/cropping of reference or generated images; no Unity file changes.
"""
import bpy
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'ArtSource/SurvivalTrain/LookDev'
bpy.ops.wm.open_mainfile(filepath=str(OUT/'SurvivalTrain-LookDev.blend'))
scene=bpy.context.scene
scene.cycles.samples=1
scene.render.threads_mode='FIXED';scene.render.threads=6
scene.render.bake.margin=4
scene.render.bake.use_clear=True
bpy.ops.object.select_all(action='DESELECT')
bpy.ops.mesh.primitive_plane_add(size=1,location=(0,100,0))
plane=bpy.context.object;plane.name='Temporary metre-sized material swatch'
for name,filename in (
    ('warm aged interior steel','interior-steel-color.png'),
    ('oxblood red satin doors','red-door-color.png'),
    ('fine diamond anti-slip floor','floor-color.png'),
    ('brushed silver edge','silver-trim-color.png')):
    mat=bpy.data.materials['LD / '+name].copy();mat.name='Temporary bake / '+name
    plane.data.materials.clear();plane.data.materials.append(mat)
    ns=mat.node_tree.nodes;ls=mat.node_tree.links
    # Swatches are unlit color, not scene occlusion. Avoid sampling self-AO on
    # the two triangles of the temporary single-sided bake plane.
    for ao in list(ns):
        if ao.bl_idname != 'ShaderNodeAmbientOcclusion':continue
        white=ns.new('ShaderNodeRGB');white.outputs[0].default_value=(1,1,1,1)
        for edge in list(ls):
            if edge.from_node==ao:ls.new(white.outputs[0],edge.to_socket)
    p=ns.get('Principled BSDF');out=ns.get('Material Output')
    emission=ns.new('ShaderNodeEmission')
    socket=p.inputs['Base Color']
    if socket.is_linked:ls.new(socket.links[0].from_socket,emission.inputs['Color'])
    else:emission.inputs['Color'].default_value=socket.default_value
    ls.new(emission.outputs[0],out.inputs['Surface'])
    image=bpy.data.images.new(filename,width=512,height=512,alpha=False)
    target=ns.new('ShaderNodeTexImage');target.image=image;ns.active=target
    bpy.ops.object.bake(type='EMIT')
    image.filepath_raw=str(OUT/filename);image.file_format='PNG';image.save()
    print('BAKED_SWATCH',filename)
print('SWATCHES_DONE: each file is 1m x 1m unlit color only; not game atlas')
