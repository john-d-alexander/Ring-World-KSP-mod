import bpy
scene=bpy.data.scenes['Ringworld — Colossus Review']
bpy.context.window.scene=scene
floor=bpy.data.materials['Colossus studio floor']
floor.use_nodes=True
floor.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.055,.07,.09,1)
floor.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.9
bpy.ops.wm.save_as_mainfile(filepath=bpy.data.filepath)
bpy.ops.render.render(write_still=True)
print(scene.render.filepath)
