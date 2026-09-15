"""Original Ringworld-inspired city silhouettes; run through tools/blender_bridge.py."""
import ast, bpy, json, math, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\Users\Hans\Documents\programming\Ring World KSP');OUT=ROOT/'art/city-kit'
for folder in ('exports','previews','validation'):(OUT/folder).mkdir(parents=True,exist_ok=True)
for previous in list(bpy.data.scenes):
    if previous.name.startswith('Ringworld — City Kit'):
        for obj in list(previous.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.scenes.remove(previous)
scene=bpy.data.scenes.new('Ringworld — City Kit Sources');bpy.context.window.scene=scene
atlas=bpy.data.images.load(str(ROOT/'art/first-set/exports/SceneryAtlas.png'),check_existing=True);atlas.pack()
mat=bpy.data.materials.new('RW_CityAtlas');mat.use_nodes=True
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas
mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
syntax=ast.parse((ROOT/'art/first-set/build_assets.py').read_text(encoding='utf-8'))
helper=next(n for n in syntax.body if isinstance(n,ast.ClassDef) and n.name=='MeshBuilder')
exec(compile(ast.Module(body=[helper],type_ignores=[]),'MeshBuilder','exec'))

def tower(m,x,y,z,h,r,lod,ruined=False):
    n=[12,8,6][lod]
    m.rod((x,y,z),(x,y,z+h*.85),r,r*.65,12,n)
    m.rod((x,y,z+h*.85),(x,y,z+h),r*.8,r*.10 if not ruined else r*.55,9,n)
    if lod<2:
        for j in range(1,[7,4][lod]):
            m.rod((x,y,z+h*j/8),(x,y,z+h*j/8+.012),r*(1-j*.04)*1.03,r*(1-j*.04)*1.03,10,n)
    if not ruined:m.rod((x,y,z+h),(x,y,z+h*1.2),.015,.003,9,5)

def city(kind,lod):
    m=MeshBuilder();n=[64,32,16][lod]
    if kind in ('floating_city','fallen_city'):
        ruined=kind=='fallen_city'
        # A broad inhabited disk with a tapered machinery hull and a varied skyline.
        m.rod((0,0,.08),(0,0,.25),.60,1,8,n)
        m.rod((0,0,.25),(0,0,.29),1,1,12,n)
        if not ruined:m.rod((0,0,-.55),(0,0,.08),.025,.60,9,n)
        count=[22,14,8][lod]
        for i in range(count):
            angle=i*2.399963;r=.20+.62*math.sqrt((i+.5)/count)
            h=.13+.25*(.5+.5*math.sin(i*4.7));x,y=r*math.cos(angle),r*math.sin(angle)
            tower(m,x,y,.29,h,.045+.02*(i%3),lod,ruined)
        tower(m,0,0,.29,.65 if not ruined else .33,.13,lod,ruined)
        if lod<2:
            for i in range([20,12][lod]):
                a=i*math.tau/[20,12][lod];x,y=.96*math.cos(a),.96*math.sin(a)
                m.rod((x,y,.18),(x,y,.36),.016,.016,9,4)
            # Garden terraces interrupt the urban texture.
            for i in range(6):
                a=i*math.tau/6;m.rod((.70*math.cos(a),.70*math.sin(a),.291),(.70*math.cos(a),.70*math.sin(a),.298),.10,.10,1,8)
        if ruined:
            for i in range([9,5,2][lod]):
                a=i*2.4;m.rod((math.cos(a)*.7,math.sin(a)*.7,.3),(math.cos(a)*1.15,math.sin(a)*1.15,.07),.028,.015,8,5)
    elif kind=='city_arcology':
        for level in range(4):
            r=.50-level*.095;m.rod((0,0,level*.16),(0,0,(level+1)*.16),r,r,12,n//2)
            m.rod((0,0,(level+1)*.16),(0,0,(level+1)*.16+.02),r*1.10,r*1.10,9,n//2)
        tower(m,0,0,.66,.5,.13,lod)
        if lod<2:
            for i in range(6):
                a=i*math.tau/6;tower(m,.40*math.cos(a),.40*math.sin(a),0,.30,.05,lod)
    elif kind=='ruined_district':
        m.box((0,0,.03),(1.8,1.4,.06),12)
        for i in range([12,8,5][lod]):
            x=(i%4-1.5)*.43;y=(i//4-1)*.44;h=.18+.45*((i*7)%11)/11
            m.box((x,y,h/2+.06),(.25,.27,h),6)
            if lod<2:
                # Uneven broken parapets and exposed frame posts.
                for dx in (-.11,.11):m.box((x+dx,y,h+.09),(.025,.24,.10+.03*(i%3)),8)
            if i%3==0:m.rod((x,y,.09),(x+.32,y+.18,.04),.045,.035,8,5)
    else: # Elevated transit concourse, open beneath the deck.
        m.box((0,0,.34),(1.8,.48,.07),12)
        for x in (-.7,0,.7):
            for y in (-.18,.18):m.box((x,y,.16),(.07,.07,.32),9)
        for y in (-.24,.24):m.box((0,y,.48),(1.8,.03,.05),9)
        if lod<2:
            for x in (-.72,-.36,0,.36,.72):
                for y in (-.24,.24):m.box((x,y,.41),(.025,.025,.14),9)
        for y in (-.10,.10):m.box((0,y,.39),(1.8,.025,.02),8)
    return m

manifest={'schema':1,'authoring':'Original Ringworld-inspired city kit, not canonical replicas','assets':[]};entries=[]
for kind in ('floating_city','fallen_city','city_arcology','ruined_district','city_concourse'):
    name=kind+'_01';root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
    objects=[city(kind,lod).object(name+'_LOD'+str(lod),root) for lod in range(3)]
    lo=[min(v.co[k] for v in objects[0].data.vertices) for k in range(3)];hi=[max(v.co[k] for v in objects[0].data.vertices) for k in range(3)]
    for obj in objects:
        for vertex in obj.data.vertices:
            for k in range(3):vertex.co[k]=(vertex.co[k]-lo[k])/(hi[k]-lo[k])-.5
    entry={'id':name,'kind':kind,'family':'cities','collider':'none' if kind=='floating_city' else 'box','nominalSize':[hi[k]-lo[k] for k in range(3)],'lods':[]}
    for lod,obj in enumerate(objects):obj.data.calc_loop_triangles();entry['lods'].append({'level':lod,'triangles':len(obj.data.loop_triangles)})
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    bpy.ops.export_scene.fbx(filepath=str(OUT/'exports'/f'{name}.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',use_space_transform=True,bake_space_transform=True,add_leaf_bones=False,bake_anim=False)
    for obj in objects:obj.hide_render=True;obj.hide_set(True)
    manifest['assets'].append(entry);entries.append((root,entry))
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
review=bpy.data.scenes.new('Ringworld — City Kit Review');bpy.context.window.scene=review
review.world=bpy.data.worlds.new('City studio');review.world.use_nodes=True;review.world.node_tree.nodes['Background'].inputs[1].default_value=.5
for i,(root,entry) in enumerate(entries):
    obj=root.children[0].copy();obj.data=root.children[0].data;obj.parent=None;review.collection.objects.link(obj);obj.hide_render=False;obj.hide_set(False)
    size=entry['nominalSize'];factor=3/max(size);obj.scale=tuple(v*factor for v in size);obj.location=((i%3-1)*4,-(i//3)*4,size[2]*factor/2)
    bpy.ops.object.text_add(location=(obj.location.x,obj.location.y-1.65,.02));label=bpy.context.object;label.data.body=entry['kind'].replace('_',' ').upper();label.data.align_x='CENTER';label.data.size=.17
bpy.ops.mesh.primitive_plane_add(size=100,location=(0,0,-.05));floor=bpy.data.materials.new('City studio floor');floor.diffuse_color=(.07,.09,.11,1);bpy.context.object.data.materials.append(floor)
bpy.ops.object.light_add(type='AREA',location=(-4,-3,12));bpy.context.object.data.energy=2200;bpy.context.object.data.size=8
centre=Vector((0,-2,.7));bpy.ops.object.camera_add(location=centre+Vector((7,-12,15)));camera=bpy.context.object;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=15;review.camera=camera
review.render.engine='CYCLES';review.cycles.samples=16;review.cycles.use_denoising=True;review.render.resolution_x=1400;review.render.resolution_y=1000;review.render.resolution_percentage=100;review.render.filepath=str(OUT/'previews/city-kit.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ringworld-City-Kit.blend'))
print(json.dumps(manifest))
