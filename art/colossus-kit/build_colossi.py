"""Original Ringworld-inspired superstructures, not replicas of named canonical buildings.
Run in the user's Blender through tools/blender_bridge.py. Dimensions are Unity metres.
"""
import ast, bpy, json, math, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\Users\Hans\Documents\programming\Ring World KSP');OUT=ROOT/'art/colossus-kit'
for folder in ('exports','previews','validation'):(OUT/folder).mkdir(parents=True,exist_ok=True)
for old in list(bpy.data.scenes):
    if old.name.startswith('Ringworld — Colossus'):
        for obj in list(old.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.scenes.remove(old)
scene=bpy.data.scenes.new('Ringworld — Colossus Sources');bpy.context.window.scene=scene
scene.unit_settings.system='METRIC'
atlas=bpy.data.images.load(str(ROOT/'art/first-set/exports/SceneryAtlas.png'),check_existing=True);atlas.pack()
mat=bpy.data.materials.new('RW_ColossusAtlas');mat.use_nodes=True
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas
mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.72
syntax=ast.parse((ROOT/'art/first-set/build_assets.py').read_text(encoding='utf-8'))
helper=next(n for n in syntax.body if isinstance(n,ast.ClassDef) and n.name=='MeshBuilder')
exec(compile(ast.Module(body=[helper],type_ignores=[]),'MeshBuilder','exec'))

def hoop(m,r,z,t,lod,arc=math.tau,vertical=False):
    n=[72,36,16][lod]
    for i in range(n):
        a,b=arc*i/n,arc*(i+1)/n
        p=(r*math.cos(a),r*math.sin(a),z);q=(r*math.cos(b),r*math.sin(b),z)
        if vertical:p=(p[0],0,p[1]+z);q=(q[0],0,q[1]+z)
        m.rod(p,q,t,t,9,[6,5,4][lod])

def tower(m,x,y,h,lod,r=.025):
    m.rod((x,y,0),(x,y,h*.92),r,r*.55,12,[8,6,4][lod]);m.rod((x,y,h*.92),(x,y,h),r*.65,0,9,4)
    if lod<2:
        for z in range(1,[18,6][lod]):m.box((x,y,h*z/[20,8][lod]),(r*1.6,r*1.6,h*.012),8)

def make(kind,lod):
    m=MeshBuilder();n=[64,32,16][lod];rng=random.Random(1970)
    if kind=='continent_crown':
        # A vast levitated urban plate with a hanging machinery keel and stepped gardens.
        m.rod((0,0,-.25),(0,0,0),.07,1,8,n)
        for j in range(4):
            r=1-j*.19;z=j*.075;m.rod((0,0,z),(0,0,z+.06),r,r*.97,12,n)
            m.rod((0,0,z+.061),(0,0,z+.063),r*.91,r*.91,1,n)
        hoop(m,.97,.07,.018,lod)
        for i in range([120,48,20][lod]):
            a=i*2.399963;r=.15+.72*math.sqrt((i+.5)/[120,48,20][lod]);x,y=r*math.cos(a),r*math.sin(a)
            tower(m,x,y,.3+.22*rng.random(),lod,.016)
        tower(m,0,0,.9,lod,.08)
        for i in range(12):
            a=i*math.tau/12;m.rod((.20*math.cos(a),.20*math.sin(a),-.17),(1.04*math.cos(a),1.04*math.sin(a),.04),.012,.03,9,4)
    elif kind=='rim_gate_titan':
        for x in (-.44,.44):
            m.box((x,0,.47),(.17,.24,.94),12)
            for y in (-.18,.18):m.rod((x*1.35,y,0),(x,y,.97),.06,.016,9,5)
            for z in (.25,.50,.75):m.box((x,0,z),(.23,.34,.025),8)
        m.box((0,0,.95),(1.06,.36,.1),9)
        for i in range(-3,4):m.rod((i*.09,-.13,.92),(i*.09,-.13,.42),.003,.003,8,4)
        if lod<2:
            for i in range([36,12][lod]):m.box((-.5+i/[35,11][lod],-.19,.985),(.008,.015,.012),12)
    elif kind=='skyway_leviathan':
        m.box((0,0,.70),(2.5,.15,.035),12)
        for j in range(9):
            x=-1.16+j*.29
            for y in (-.055,.055):
                m.rod((x,y,0),(x,y,.72),.018,.014,9,4)
                m.rod((x-.13,y,.7),(x,y,.95),.008,.008,8,4)
                m.rod((x,y,.95),(x+.13,y,.7),.008,.008,8,4)
            m.box((x,0,.72),(.14,.28,.06),9)
        for y in (-.055,.055):m.box((0,y,.73),(2.5,.003,.008),8)
    elif kind=='world_flup_mouth':
        # A terraced mouth wider than a city, open through the centre.
        for i in range(5):hoop(m,1-i*.13,.1+i*.12,.075,lod,math.pi*1.9,True)
        for x in (-1.05,1.05):
            m.box((x,.25,.36),(.18,.8,.72),12)
            for y in (0,.3,.6):tower(m,x,y,.85,lod,.025)
        for j in range(7):m.box((0,-.2-j*.15,-.48-j*.018),(1.7,.16,.035),9)
    elif kind=='scrith_memory_spire':
        for side in (-1,1):
            m.rod((side*.16,0,0),(side*.035,0,1),.11,.02,12,4)
            m.rod((side*.22,.1,0),(side*.05,0,.85),.04,.012,9,4)
        for j in range([48,18,6][lod]):
            z=(j+.5)/[48,18,6][lod];m.box((0,-.015,z),(.28*(1-z)+.025,.04,.0025),8)
        hoop(m,.25,.24,.012,lod);hoop(m,.19,.51,.01,lod);hoop(m,.11,.78,.008,lod)
    elif kind=='broken_city_halo':
        hoop(m,1,.1,.095,lod,math.pi*1.7);hoop(m,.79,.12,.035,lod,math.pi*1.7)
        for i in range([48,24,14][lod]):
            a=math.pi*1.7*i/[48,24,14][lod];x,y=.93*math.cos(a),.93*math.sin(a)
            m.rod((x,y,.10),(x*.80,y*.80,.25+rng.random()*.28),.02,.008,9,4)
        for j in range(10):m.box((.8+rng.random()*.4,-.8+rng.random()*.6,.04),(.16,.07,.08),12)
    elif kind=='atmosphere_harp':
        for x in (-.6,.6):tower(m,x,0,1,lod,.05)
        m.rod((-.6,0,.95),(.6,0,.95),.055,.055,12,6)
        for i in range(17):
            x=-.54+i*.0675;m.rod((x,0,.05),(x,0,.92),.0025,.0025,8,4)
            for z in (.25,.5,.75):m.box((x,0,z),(.023,.075,.008),9)
        m.box((0,0,.025),(1.4,.4,.05),12)
    elif kind=='rim_engine_reliquary':
        for j in range(6):
            z=j*.14;hoop(m,.50-z*.25,z,.045,lod)
        for i in range(12):
            a=i*math.tau/12;x,y=math.cos(a),math.sin(a)
            m.rod((x*.5,y*.5,0),(x*.3,y*.3,.9),.022,.016,12,5)
        m.rod((0,0,0),(0,0,.3),.12,.05,8,n)
        for x in (-.7,.7):tower(m,x,0,.65,lod,.045)
    return m

specs=[
 ('continent_crown',(48000,12000,48000),'city',80000,-50000,8000),
 ('rim_gate_titan',(28000,80000,16000),'rim',45000,-30000,0),
 ('skyway_leviathan',(120000,18000,10000),'city',0,95000,0),
 ('world_flup_mouth',(60000,22000,26000),'spill',50000,0,0),
 ('scrith_memory_spire',(9000,45000,9000),'arrival',26000,0,0),
 ('broken_city_halo',(90000,8000,90000),'city',-115000,-70000,0),
 ('atmosphere_harp',(36000,50000,12000),'scrith',45000,30000,0),
 ('rim_engine_reliquary',(42000,65000,32000),'rim',-50000,-55000,0)]
manifest={'assets':[]};cfg=['// Rare seeded templates; dimensions in metres.\nRINGWORLD_COLOSSUS_DISTRIBUTION\n{\n cellSize = 2000000\n occupancy = 0.15\n}']
for kind,size,site,a,b,lift in specs:
    name=kind+'_01';root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
    objects=[make(kind,lod).object(name+'_LOD'+str(lod),root) for lod in range(3)]
    lo=[min(v.co[k] for v in objects[0].data.vertices) for k in range(3)];hi=[max(v.co[k] for v in objects[0].data.vertices) for k in range(3)]
    entry={'id':name,'kind':kind,'family':'colossi','collider':'surface','sizeMetres':size,'theme':site,'placement':'seeded-cell','lods':[]}
    for lod,obj in enumerate(objects):
        for vertex in obj.data.vertices:
            for k in range(3):vertex.co[k]=(vertex.co[k]-lo[k])/(hi[k]-lo[k])-.5
        obj.data.calc_loop_triangles();entry['lods'].append({'level':lod,'triangles':len(obj.data.loop_triangles)})
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    bpy.ops.export_scene.fbx(filepath=str(OUT/'exports'/f'{name}.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',use_space_transform=True,bake_space_transform=True,add_leaf_bones=False,bake_anim=False)
    for obj in objects:obj.hide_render=True;obj.hide_set(True)
    manifest['assets'].append(entry)
    cfg.append(f'RINGWORLD_COLOSSUS_ASSET\n{{\n kind = {kind}\n width = {size[0]}\n height = {size[1]}\n depth = {size[2]}\n aboveGround = {lift}\n range = 250000\n}}')
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2))
(ROOT/'GameData/NivenRingworld/Colossi.cfg').write_text('\n\n'.join(cfg)+'\n')
review=bpy.data.scenes.new('Ringworld — Colossus Review');bpy.context.window.scene=review
review.world=bpy.data.worlds.new('Colossus studio');review.world.use_nodes=True;review.world.node_tree.nodes['Background'].inputs[1].default_value=.5
for i,entry in enumerate(manifest['assets']):
    source=bpy.data.objects[entry['id']+'_LOD0'];obj=source.copy();obj.data=source.data;obj.parent=None;review.collection.objects.link(obj);obj.hide_render=False;obj.hide_set(False)
    w,h,d=entry['sizeMetres'];factor=3/max(w,h,d);obj.scale=(w*factor,d*factor,h*factor);obj.location=((i%4-1.5)*4,-(i//4)*4,h*factor/2)
    bpy.ops.object.text_add(location=(obj.location.x,obj.location.y-1.6,.02));label=bpy.context.object;label.data.body=entry['kind'].replace('_',' ').upper()+f'\n{w/1000:g} x {h/1000:g} x {d/1000:g} km';label.data.align_x='CENTER';label.data.size=.14
bpy.ops.mesh.primitive_plane_add(size=100,location=(0,0,-.05));floor=bpy.data.materials.new('Colossus studio floor');floor.diffuse_color=(.055,.07,.09,1);floor.use_nodes=True;floor.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.055,.07,.09,1);floor.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.9;bpy.context.object.data.materials.append(floor)
bpy.ops.object.light_add(type='AREA',location=(-4,-3,12));bpy.context.object.data.energy=3000;bpy.context.object.data.size=8
centre=Vector((0,-2,.7));bpy.ops.object.camera_add(location=centre+Vector((6,-12,17)));camera=bpy.context.object;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=18;review.camera=camera
review.render.engine='CYCLES';review.cycles.samples=16;review.cycles.use_denoising=True;review.render.resolution_x=1800;review.render.resolution_y=1100;review.render.resolution_percentage=100;review.render.filepath=str(OUT/'previews/colossi.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ringworld-Colossi.blend'))
print(json.dumps({'assets':len(manifest['assets']),'meshes':24,'saved':str(OUT/'Ringworld-Colossi.blend')}))
