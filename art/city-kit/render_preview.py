import bpy
bpy.context.window.scene=bpy.data.scenes['Ringworld — City Kit Review']
bpy.ops.render.render(write_still=True)
