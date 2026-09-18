"""Original landmark architecture. Run in Blender through tools/blender_bridge.py.
All scales in manifest are Unity metres (X, height, Z); export meshes are unit sized.
No copyrighted meshes/textures are copied. See docs/developers/ASSET-AUTHORING.md for sources.
"""
import ast, bpy, json, math, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\Users\Hans\Documents\programming\Ring World KSP');OUT=ROOT/'art/landmark-kit'
for folder in ('exports','previews','validation'):(OUT/folder).mkdir(parents=True,exist_ok=True)
if not (OUT/'before-landmarks.blend').exists():bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'before-landmarks.blend'),copy=True)
for previous in list(bpy.data.scenes):
    if previous.name.startswith('Ringworld — Landmark'):
        for obj in list(previous.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.scenes.remove(previous)
scene=bpy.data.scenes.new('Ringworld — Landmark Sources');bpy.context.window.scene=scene;scene.unit_settings.system='METRIC'
atlas=bpy.data.images.load(str(ROOT/'art/first-set/exports/SceneryAtlas.png'),check_existing=True);atlas.pack()
mat=bpy.data.materials.new('RW_LandmarkAtlas');mat.use_nodes=True
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas
mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.65
syntax=ast.parse((ROOT/'art/first-set/build_assets.py').read_text(encoding='utf-8'))
helper=next(n for n in syntax.body if isinstance(n,ast.ClassDef) and n.name=='MeshBuilder')
exec(compile(ast.Module(body=[helper],type_ignores=[]),'MeshBuilder','exec'))

def ring(m,r,z,tube,lod,tile=9,arc=math.tau):
    n=[64,32,12][lod]
    for i in range(n):
        a,b=arc*i/n,arc*(i+1)/n
        m.rod((r*math.cos(a),r*math.sin(a),z),(r*math.cos(b),r*math.sin(b),z),tube,tube,tile,[6,5,4][lod])

def spire(m,x,y,z,h,r,lod):
    n=[12,8,5][lod]
    m.rod((x,y,z),(x,y,z+h*.78),r,r*.58,12,n)
    m.rod((x,y,z+h*.78),(x,y,z+h),r*.72,0,9,n)
    if lod<2:
        for j in range(1,[14,6][lod]):
            t=j/[18,8][lod];rr=r*(1-t*.45)
            m.rod((x,y,z+h*t),(x,y,z+h*t+.008),rr*1.01,rr*1.01,8,n)

def arches(m,count,lod,height=.7):
    for i in range(count):
        x=(i-(count-1)/2)*.38
        for side in (-1,1):m.box((x+side*.145,0,height*.38),(.06,.22,height*.76),12)
        steps=[12,7,4][lod]
        for j in range(steps):
            a,b=j*math.pi/steps,(j+1)*math.pi/steps
            m.rod((x+.145*math.cos(a),0,height*.7+.17*math.sin(a)),(x+.145*math.cos(b),0,height*.7+.17*math.sin(b)),.044,.044,9,4)
    m.box((0,0,height*.97),(count*.38,.30,.07),12)

def make(kind,lod):
    m=MeshBuilder();n=[48,24,12][lod];rng=random.Random(1970)
    if kind in ('palace_lotus','palace_needle','levitation_citadel'):
        # Cantilevered garden terraces, machinery keel, radial bridges, readable silhouette.
        m.rod((0,0,-.48),(0,0,-.12),.04,.65,8,n)
        m.rod((0,0,-.12),(0,0,0),.65,1,12,n)
        ring(m,.96,.015,.023,lod)
        count=8 if kind!='palace_needle' else 5
        for i in range(count):
            a=i*math.tau/count;x,y=.60*math.cos(a),.60*math.sin(a)
            m.rod((x,y,.015),(x,y,.055),.27,.30,12,n//2)
            m.rod((x,y,.057),(x,y,.063),.20,.20,1,n//2)
            spire(m,x,y,.06,.24+(i%3)*.08,.065,lod)
            m.rod((x*.2,y*.2,.11),(x,y,.11),.035,.025,9,4)
        spire(m,0,0,.03,1.1 if kind=='palace_needle' else .60,.14,lod)
        if kind=='levitation_citadel':
            for z in (.22,.40,.58):ring(m,.34,z,.035,lod)
        if lod==0:
            for i in range(32):
                a=i*math.tau/32;m.box((.87*math.cos(a),.87*math.sin(a),-.055),(.035,.035,.05),8)
    elif kind in ('fallen_crescent','fallen_crown','city_rib_canyon'):
        arc=math.pi*1.55 if kind=='fallen_crescent' else math.tau*.85
        ring(m,.85,.11,.12,lod,12,arc);ring(m,.66,.08,.045,lod,8,arc)
        count=[18,10,6][lod]
        for i in range(count):
            a=arc*i/count;x,y=.78*math.cos(a),.78*math.sin(a)
            h=.18+.5*rng.random();m.rod((x,y,0),(x*.85,y*.85,h),.055,.018,9,5)
            if kind=='city_rib_canyon':m.rod((x*.85,y*.85,h),(x*.4,y*.4,h*.75),.018,.012,8,4)
            else:spire(m,x*.75,y*.75,.07,h,.07,lod)
        for i in range([13,7,3][lod]):
            a=rng.random()*math.tau;r=.2+rng.random()*.7
            m.box((r*math.cos(a),r*math.sin(a),.035),(.15,.07,.07),6)
    elif kind in ('archive_vault','garden_arcology','protector_sanctum'):
        for j in range(5):
            r=.75-j*.115;z=j*.13
            m.rod((0,0,z),(0,0,z+.10),r,r*.91,12,n)
            m.rod((0,0,z+.10),(0,0,z+.112),r*.92,r*.92,1 if kind=='garden_arcology' else 9,n)
            if lod<2:
                for i in range([20,10][lod]):
                    a=i*math.tau/[20,10][lod];m.box((r*.97*math.cos(a),r*.97*math.sin(a),z+.06),(.026,.026,.055),8)
        spire(m,0,0,.6,.45,.17,lod)
        if kind=='archive_vault':
            for x in (-.9,.9):spire(m,x,0,0,.55,.10,lod)
        if kind=='protector_sanctum':ring(m,.65,.65,.045,lod)
    elif kind in ('rim_portal','spaceport_gate','elevator_cathedral'):
        for x in (-.65,.65):
            m.box((x,0,.48),(.22,.36,.96),12)
            for y in (-.25,.25):m.rod((x*1.4,y,0),(x,y,.9),.07,.028,9,6)
            if lod<2:
                for z in (.2,.4,.6,.8):m.box((x,-.185,z),(.15,.015,.07),8)
        m.box((0,0,.97),(1.5,.4,.10),9)
        if kind=='elevator_cathedral':
            for x in (-.12,.12):m.box((x,0,1.15),(.035,.035,1.85),8)
            m.box((0,0,1.45),(.38,.3,.32),12)
        elif kind=='spaceport_gate':
            m.box((0,-.7,.05),(1.6,1.5,.1),12)
            for y in (-1.2,-.9,-.6):m.box((0,y,.101),(.7,.025,.008),15)
            for x in (-.45,.45):spire(m,x,.3,0,.35,.10,lod)
        else:arches(m,3,lod,1.0)
    elif kind in ('transit_viaduct','grand_aqueduct','transit_exchange'):
        arches(m,7,lod)
        if kind=='grand_aqueduct':
            for y in (-.12,.12):m.box((0,y,.78),(2.66,.035,.18),9)
            m.box((0,0,.74),(2.66,.20,.015),14)
        elif kind=='transit_exchange':
            for y in (-.5,.5):m.box((0,y,.71),(2.4,.3,.08),12)
            for x in (-.8,0,.8):m.box((x,0,.79),(.14,1.3,.07),9)
            spire(m,0,0,.80,.3,.16,lod)
        else:
            for y in (-.1,.1):m.box((0,y,.74),(2.66,.025,.02),8)
    elif kind in ('flup_cascade','sediment_silos','pump_cathedral','ocean_intake'):
        count=5 if kind=='sediment_silos' else 3
        for i in range(count):
            x=(i-(count-1)/2)*.4;h=.65+.2*(i%2)
            m.rod((x,0,0),(x,0,h),.16,.16,9,n//2)
            m.rod((x,0,h),(x,0,h+.15),.19,.07,12,n//2)
            m.rod((x,-.03,.35),(x,-.55,.18),.11,.14,8,n//2)
            # Dark closed bore inset avoids an expensive hollow tube, but reads as a nozzle.
            m.rod((x,-.56,.177),(x,-.565,.175),.10,.10,10,n//2)
            if lod<2:
                for z in (.16,.34,.52):ring_offset(m,x,z,.165,lod)
        m.box((0,.14,.07),(count*.4,.60,.14),12)
        if kind=='pump_cathedral':
            for x in (-.85,.85):spire(m,x,.1,0,1.4,.15,lod)
            m.box((0,.20,.95),(1.8,.30,.12),12)
        if kind=='flup_cascade':
            for j in range(4):m.box((0,-.7-j*.25,.15-j*.035),(1.4,.26,.07),9)
        if kind=='ocean_intake':
            for i in range([12,7,4][lod]):m.box((-.6+1.2*i/max(1,[12,7,4][lod]-1),-.7,.2),(.025,.05,.4),8)
    elif kind in ('observatory_array','scrith_resonator','weather_spire'):
        spire(m,0,0,0,.9,.16,lod)
        if kind=='weather_spire':
            for z,r in ((.45,.45),(.7,.3),(.95,.15)):ring(m,r,z,.026,lod)
        else:
            for i in range(6):
                a=i*math.tau/6;x,y=.65*math.cos(a),.65*math.sin(a)
                spire(m,x,y,0,.4,.08,lod)
                m.rod((x,y,.35),(x,y,.44),.035,.20,9,n//2)
                m.rod((x,y,.44),(x,y,.45),.175,.175,8,n//2)
                m.rod((0,0,.15),(x,y,.15),.025,.025,9,4)
            if kind=='scrith_resonator':ring(m,.7,.2,.04,lod)
    elif kind in ('terraced_town','stilt_village','reclaimed_temple'):
        count=[22,13,7][lod]
        for i in range(count):
            a=i*2.399963;r=.25+.6*math.sqrt((i+.5)/count);x,y=r*math.cos(a),r*math.sin(a)
            z=.2 if kind=='stilt_village' else .04
            h=.08+.10*(i%4)/3
            m.box((x,y,z+h*.5),(.18,.16,h),6)
            m.rod((x,y,z+h),(x,y,z+h+.07),.15,0,7,4)
            if kind=='stilt_village':
                for dx in (-.06,.06):m.box((x+dx,y,z*.5),(.022,.022,z),0)
        if kind=='reclaimed_temple':
            for j in range(4):m.box((0,0,j*.12+.06),(.7-j*.13,.7-j*.13,.12),12)
            spire(m,0,0,.48,.5,.12,lod)
        else:ring(m,.20,.08,.035,lod,5)
    elif kind in ('canopy_broadleaf','canopy_conifer'):
        for i in range(7):
            a=i*2.39996;r=.32*math.sqrt(i/6);x,y=r*math.cos(a),r*math.sin(a);h=.65+.28*rng.random()
            m.rod((x,y,0),(x,y,h*.8),.018,.009,0,5)
            if kind=='canopy_conifer':
                for j in range(3):m.rod((x,y,h*(.27+j*.19)),(x,y,h*(.65+j*.16)),.18-j*.045,0,3,5 if lod==2 else 8)
            else:m.blob((x,y,h*.77),(.23,.21,h*.25),1+(i%2),i,n=[9,6,4][lod],rings=[4,3,2][lod])
    else: # Four distinctive single trees, exact ground pivot.
        conifer=kind=='tree_redwood';willow=kind=='tree_willow';umbrella=kind=='tree_umbrella'
        m.rod((0,0,0),(.04,0,.90 if conifer else .60),.045,.012,0,[10,7,5][lod])
        count=[16,10,5][lod]
        for i in range(count):
            a=i*2.39996;z=.4+.5*i/count if conifer else .60+.16*(i%3)/2
            r=(1-z)*.37 if conifer else .13+.12*(i%3)/2
            x,y=r*math.cos(a),r*math.sin(a)
            if lod<2:m.rod((0,0,z-.20),(x,y,z),.012,.004,0,5)
            m.blob((x,y,z),(.13 if conifer else .24,.12 if conifer else .21,.08 if umbrella else .20 if willow else .13),3 if conifer else 1+(i%2),i,n=[8,5,4][lod],rings=[4,3,2][lod])
            if willow and lod<2:m.rod((x,y,z),(x*1.3,y*1.3,z-.32),.06,.018,1,5)
    return m

def ring_offset(m,x,z,r,lod):
    n=[12,8,4][lod]
    for i in range(n):
        a,b=i*math.tau/n,(i+1)*math.tau/n;m.rod((x+r*math.cos(a),r*math.sin(a),z),(x+r*math.cos(b),r*math.sin(b),z),.01,.01,8,4)

specs=[
 ('palace_lotus',(2200,1050,2200),'city'),('palace_needle',(1400,1900,1400),'city'),('levitation_citadel',(1800,1000,1800),'city'),
 ('fallen_crescent',(3200,750,2900),'city'),('fallen_crown',(2400,650,2400),'city'),('city_rib_canyon',(1900,560,1900),'city'),
 ('archive_vault',(800,700,650),'city'),('garden_arcology',(1100,800,1100),'city'),('protector_sanctum',(850,850,850),'scrith'),
 ('rim_portal',(1600,1200,500),'terminal'),('spaceport_gate',(2200,1400,2600),'terminal'),('elevator_cathedral',(900,4000,800),'terminal'),
 ('transit_viaduct',(2600,650,300),'terminal'),('grand_aqueduct',(3000,850,320),'island'),('transit_exchange',(1800,600,1000),'city'),
 ('flup_cascade',(1800,1000,1800),'spill'),('sediment_silos',(1600,1100,700),'spill'),('pump_cathedral',(2400,2200,1000),'spill'),('ocean_intake',(1600,900,1400),'island'),
 ('observatory_array',(1000,900,1000),'island'),('scrith_resonator',(1400,950,1400),'scrith'),('weather_spire',(500,1100,500),'outpost'),
 ('terraced_town',(420,85,420),'outpost'),('stilt_village',(360,70,360),'island'),('reclaimed_temple',(500,320,500),'outpost'),
 ('canopy_broadleaf',(65,25,65),'forest'),('canopy_conifer',(55,30,55),'forest'),
 ('tree_oak',(22,24,22),'forest'),('tree_willow',(28,24,28),'wetland'),('tree_redwood',(22,55,22),'forest'),('tree_umbrella',(30,24,30),'grassland')]
manifest={'schema':2,'authoring':'Original interpretations; not exact canonical replicas','assets':[]};entries=[]
for kind,metres,site in specs:
    name=kind+'_01';root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
    objects=[make(kind,lod).object(name+'_LOD'+str(lod),root) for lod in range(3)]
    lo=[min(v.co[k] for v in objects[0].data.vertices) for k in range(3)];hi=[max(v.co[k] for v in objects[0].data.vertices) for k in range(3)]
    tree=kind.startswith('tree_');canopy=kind.startswith('canopy_')
    for obj in objects:
        for vertex in obj.data.vertices:
            if tree:
                vertex.co/=hi[2];vertex.co.z=max(0,vertex.co.z)
            else:
                for k in range(3):vertex.co[k]=(vertex.co[k]-lo[k])/(hi[k]-lo[k])-.5
    category='tree_conifer' if kind=='tree_redwood' else 'tree_broadleaf' if tree else kind
    entry={'id':name,'kind':category,'family':'vegetation' if tree or canopy else 'landmarks','collider':'trunk' if tree else 'none' if canopy else 'surface','sizeMetres':metres,'site':site,'lods':[]}
    if canopy:
        trunks=[];rng=random.Random(1970)
        for i in range(7):
            a=i*2.39996;r=.32*math.sqrt(i/6);x,y=r*math.cos(a),r*math.sin(a);h=.65+.28*rng.random()
            trunks.append({'x':(x-lo[0])/(hi[0]-lo[0])-.5,'y':(h*.3-lo[2])/(hi[2]-lo[2])-.5,'z':-(y-lo[1])/(hi[1]-lo[1])+.5,'height':h*.6/(hi[2]-lo[2]),'radius':.018/(hi[0]-lo[0])})
        entry['trunks']=trunks
    for lod,obj in enumerate(objects):obj.data.calc_loop_triangles();entry['lods'].append({'level':lod,'triangles':len(obj.data.loop_triangles)})
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    bpy.ops.export_scene.fbx(filepath=str(OUT/'exports'/f'{name}.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',use_space_transform=True,bake_space_transform=True,add_leaf_bones=False,bake_anim=False)
    for obj in objects:obj.hide_render=True;obj.hide_set(True)
    manifest['assets'].append(entry);entries.append((root,entry))
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
for page in range(math.ceil(len(entries)/8)):
    review=bpy.data.scenes.new('Ringworld — Landmark Review '+str(page));bpy.context.window.scene=review
    review.world=bpy.data.worlds.new('Landmark studio');review.world.use_nodes=True;review.world.node_tree.nodes['Background'].inputs[1].default_value=.5
    for i,(root,entry) in enumerate(entries[page*8:page*8+8]):
        obj=root.children[0].copy();obj.data=root.children[0].data;obj.parent=None;review.collection.objects.link(obj);obj.hide_render=False;obj.hide_set(False)
        size=entry['sizeMetres'];factor=3/max(size);obj.scale=(size[0]*factor,size[2]*factor,size[1]*factor);obj.location=((i%4-1.5)*4,-(i//4)*4,size[1]*factor/2)
        if entry['collider']=='trunk':obj.scale=(2,2,2);obj.location.z=0
        bpy.ops.object.text_add(location=(obj.location.x,obj.location.y-1.6,.02));label=bpy.context.object;label.data.body=entry['id'].replace('_01','').replace('_',' ').upper();label.data.align_x='CENTER';label.data.size=.15
    bpy.ops.mesh.primitive_plane_add(size=100,location=(0,0,-.05));floor=bpy.data.materials.new('Landmark studio floor');floor.diffuse_color=(.07,.09,.11,1);bpy.context.object.data.materials.append(floor)
    bpy.ops.object.light_add(type='AREA',location=(-4,-3,12));bpy.context.object.data.energy=2600;bpy.context.object.data.size=8
    centre=Vector((0,-2,.7));bpy.ops.object.camera_add(location=centre+Vector((6,-12,17)));camera=bpy.context.object;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=18;review.camera=camera
    review.render.engine='CYCLES';review.cycles.samples=24;review.cycles.use_denoising=True;review.render.resolution_x=1800;review.render.resolution_y=1000;review.render.resolution_percentage=100;review.render.filepath=str(OUT/'previews'/f'landmarks-{page}.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ringworld-Landmarks.blend'))
print(json.dumps({'assets':len(entries),'meshes':len(entries)*3,'manifest':str(OUT/'manifest.json')}))

