import bpy
for scene in bpy.data.scenes:
    if scene.name.startswith('Ringworld — Habitat Library ') and scene.camera:
        bpy.context.window.scene=scene
        bpy.ops.render.render(write_still=True)
print('Four habitat library review sheets rendered.')
