import bpy
for scene in bpy.data.scenes:
    if scene.name.startswith('Ringworld — Landmark Review'):
        bpy.context.window.scene=scene
        bpy.ops.render.render(write_still=True)
print('Four landmark review pages rendered.')
