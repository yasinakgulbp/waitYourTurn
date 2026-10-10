"""Actual close-up of the saved 3D material trial, not an image edit."""
import bpy
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'ArtSource/SurvivalTrain/LookDev'
bpy.ops.wm.open_mainfile(filepath=str(OUT/'SurvivalTrain-LookDev.blend'))
s=bpy.context.scene;c=s.camera
c.location=(9.8,3.4,5.8)
c.rotation_euler=(Vector((10.2,-.7,.65))-c.location).to_track_quat('-Z','Y').to_euler()
c.data.ortho_scale=6.6
s.render.resolution_x=1100;s.render.resolution_y=800
s.cycles.samples=20;s.render.threads_mode='FIXED';s.render.threads=6
s.render.filepath=str(OUT/'material-detail.png')
bpy.ops.render.render(write_still=True)
print('DETAIL_RENDER_DONE')
