import bpy
obj=bpy.data.objects.get('canopy_broadleaf_01')
print([(min(v.co.y for v in c.data.vertices),max(v.co.y for v in c.data.vertices)) for c in obj.children])
