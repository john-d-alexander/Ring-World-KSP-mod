"""Original Blender asset representatives for every remaining catalog family.
Run via tools/blender_bridge.py. Does not rebuild or replace the first scenery set.
"""
import ast, bpy, json, math, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\Users\Hans\Documents\programming\Ring World KSP');OUT=ROOT/'art/habitat-kit'
for p in ('exports','previews','validation'):(OUT/p).mkdir(parents=True,exist_ok=True)
for previous in list(bpy.data.scenes):
    if previous.name.startswith('Ringworld — Habitat Library'):
        for o in list(previous.objects):bpy.data.objects.remove(o,do_unlink=True)
        bpy.data.scenes.remove(previous)
scene=bpy.data.scenes.new('Ringworld — Habitat Library Sources');bpy.context.window.scene=scene
atlas=bpy.data.images.load(str(ROOT/'art/first-set/exports/SceneryAtlas.png'),check_existing=True);atlas.pack()
mat=bpy.data.materials.new('RW_LibraryAtlas');mat.use_nodes=True
node=mat.node_tree.nodes.get('Principled BSDF');node.inputs['Roughness'].default_value=.73
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas;mat.node_tree.links.new(tex.outputs['Color'],node.inputs['Base Color'])
# Reuse only the mesh helper class, never execute the first-set scene generator.
syntax=ast.parse((ROOT/'art/first-set/build_assets.py').read_text(encoding='utf-8'))
helper=next(n for n in syntax.body if isinstance(n,ast.ClassDef) and n.name=='MeshBuilder')
exec(compile(ast.Module(body=[helper],type_ignores=[]),'MeshBuilder','exec'))

def pipe(m,a,b,r,thickness,tile,n):
    a,b=Vector(a),Vector(b);axis=(b-a).normalized();u=axis.cross(Vector((0,0,1)))
    if u.length<.1:u=axis.cross(Vector((0,1,0)))
    u.normalize();v=axis.cross(u)
    rings=[[center+(u*math.cos(i*math.tau/n)+v*math.sin(i*math.tau/n))*radius for i in range(n)] for center,radius in ((a,r),(b,r),(a,r-thickness),(b,r-thickness))]
    for i in range(n):
        j=(i+1)%n
        for points in ([rings[0][i],rings[0][j],rings[1][j],rings[1][i]], [rings[2][j],rings[2][i],rings[3][i],rings[3][j]], [rings[0][j],rings[0][i],rings[2][i],rings[2][j]], [rings[1][i],rings[1][j],rings[3][j],rings[3][i]]):m.face(points,tile)

def rails(m,lod,z=.35):
    for x in (-.28,.28):m.box((x,0,z),(.055,1,.05),9)
    for i in range([9,5,3][lod]):m.box((0,-.45+i*.9/([9,5,3][lod]-1),z-.055),(.85,.06,.06),8)

def shape(kind,lod):
    m=MeshBuilder();n=[12,8,5][lod];r=random.Random(1970)
    if kind in ('grass_patch','reed_patch','fern_patch','desert_scrub','bush','mirror_sunflower','root_cluster','dead_tree','stump','fallen_log','driftwood','leaf_litter','mushrooms','pebble_cluster'):
        if kind in ('grass_patch','reed_patch','fern_patch','desert_scrub','bush'):
            count=[18,10,5][lod]
            for i in range(count):
                a=i*2.4;d=r.uniform(.02,.38);x,y=math.cos(a)*d,math.sin(a)*d;h=r.uniform(.35,.9)
                if kind=='grass_patch':m.face([(x-.018,y,0),(x+.018,y,0),(x+.10,y+.06,h)],1 if i%3 else 2);m.face([(x+.10,y+.06,h),(x+.018,y,0),(x-.018,y,0)],1)
                else:
                    m.rod((x,y,0),(x+.06,y,h),.009,.003,0 if kind in ('bush','desert_scrub') else 1,4)
                    if kind=='reed_patch':m.rod((x+.06,y,h*.75),(x+.06,y,h),.028,.023,7,5)
                    elif kind in ('bush','desert_scrub'):m.blob((x+.06,y,h),(.17,.13,.16),1 if kind=='bush' else 13,i,n=5,rings=2 if lod else 3)
                    else:
                        for j in range(3 if lod else 5):m.leaf((x+.03,y,h*(.3+j*.12)),.12*(1-j*.12),a+j*.8,1)
        elif kind=='mirror_sunflower':
            m.rod((0,0,0),(.04,0,.8),.022,.011,1,6)
            m.rod((.04,0,.78),(.04,0,.82),.08,.07,8,n)
            for i in range([12,8,6][lod]):
                a=i*math.tau/[12,8,6][lod];m.leaf((.04+math.cos(a)*.20,math.sin(a)*.20,.81),.18,a,9)
            if lod<2:
                for i in range(3):m.leaf((.02,0,.2+i*.15),.14,i*2.4,1)
        elif kind in ('dead_tree','stump','root_cluster'):
            h=.3 if kind!='dead_tree' else 1;m.rod((0,0,0),(.08,0,h),.11,.06,0,n)
            for i in range([7,5,3][lod]):
                a=i*2.4;m.rod((math.cos(a)*.45,math.sin(a)*.45,0),(0,0,.18),.018,.06,0,5)
                if kind=='dead_tree':m.rod((.04,0,.3+i*.08),(math.cos(a)*.35,math.sin(a)*.35,.6+i*.055),.028,.004,0,5)
        elif kind in ('fallen_log','driftwood'):
            m.rod((-.5,0,.13),(.5,.04,.13),.13,.10,0,n)
            if lod<2:
                for i in range(3):m.rod((-.3+i*.3,0,.14),(-.4+i*.3,.25,.26),.06,.01,0,5)
        elif kind=='leaf_litter':
            for i in range([16,9,4][lod]):m.leaf((r.uniform(-.4,.4),r.uniform(-.4,.4),r.uniform(0,.03)),r.uniform(.05,.13),r.random()*6,7 if i%2 else 11)
        elif kind=='mushrooms':
            for i in range([5,3,2][lod]):
                x,y=r.uniform(-.35,.35),r.uniform(-.35,.35);h=r.uniform(.15,.4);m.rod((x,y,0),(x,y,h),.025,.025,15,5);m.blob((x,y,h),(.12,.12,.05),11+i%2,i,n=n,rings=3)
        else:
            for i in range([14,8,4][lod]):m.blob((r.uniform(-.4,.4),r.uniform(-.4,.4),.04),(.08,.065,.04),4+i%2,i,n=[7,5,4][lod],rings=3 if lod==0 else 2)
    elif kind in ('scrith_outcrop','conduit_live','conduit_dark','levitation_grid','debris_beam','city_disk_fragment','crashed_city_disk','ruin_tower','habitat_block'):
        if kind=='scrith_outcrop':
            m.blob((0,0,.07),(.5,.45,.08),9,9,n=n,rings=3)
            if lod<2:
                for i in range(4):m.box((-.3+i*.2,0,.13),(.012,.75,.013),8)
        elif kind in ('conduit_live','conduit_dark','levitation_grid'):
            m.box((0,0,.025),(1,1,.05),8)
            for i in range([7,5,3][lod]):
                x=-.43+i*.86/([7,5,3][lod]-1);m.box((x,0,.07),(.025,.94,.035),14 if kind=='conduit_live' else 9)
                m.box((0,x,.09),(.94,.025,.035),9)
            if kind=='levitation_grid':
                for x in (-.35,.35):
                    for y in (-.35,.35):pipe(m,(x,y,.10),(x,y,.16),.12,.03,9,n)
        elif kind=='debris_beam':
            for y in (-.12,.12):m.box((0,y,.1),(1,.03,.25),8)
            for i in range([6,4,2][lod]):m.rod((-.48+i*.96/[6,4,2][lod],-.12,0),(-.3+i*.96/[6,4,2][lod],.12,.23),.02,.02,11,4)
        elif kind in ('city_disk_fragment','crashed_city_disk'):
            if kind=='crashed_city_disk':
                m.rod((0,0,.04),(0,0,.2),.43,.5,9,n*2);m.rod((0,0,0),(0,0,.05),.2,.43,8,n)
            else:
                for i in range(n):
                    a=-.8+i*1.6/n;b=-.8+(i+1)*1.6/n
                    p=[(math.cos(t)*rr-.35,math.sin(t)*rr,z) for z in (0,.16) for rr,t in ((.25,a),(.5,a),(.5,b),(.25,b))]
                    for face in ((0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)):m.face([p[j] for j in face],9 if face==(4,5,6,7) else 8)
            for i in range([11,7,3][lod]):
                a=i*2.4;rr=r.uniform(.08,.30);h=r.uniform(.12,.42);m.box((math.cos(a)*rr,math.sin(a)*rr,.2+h/2),(.07,.08,h),8 if i%2 else 12)
        else:
            for floor in range(5):
                width=.8-floor*.07;z=.10+floor*.18;m.box((0,0,z),(width,.65,.08),9)
                for x in (-width*.43,width*.43):
                    for y in (-.27,.27):
                        if kind=='ruin_tower' and floor>2 and x>0 and y>0:continue
                        m.box((x,y,z+.07),(.055,.055,.18),8)
                if kind=='habitat_block':m.box((0,.22,z+.065),(width*.85,.10,.12),12)
                if lod==0 and kind=='ruin_tower':m.rod((-.2,-.3,z),(.25,.27,z+.12),.009,.009,11,4)
    elif kind in ('stone_enclosure','watchtower','campfire','bridge_segment','research_station'):
        if kind=='stone_enclosure':
            for i in range([16,12,8][lod]):
                a=.5+i*5.2/[16,12,8][lod];m.blob((math.cos(a)*.4,math.sin(a)*.4,.1),(.1,.08,.1),4,i,n=5,rings=3 if lod==0 else 2)
        elif kind=='campfire':
            for i in range([10,7,5][lod]):
                a=i*math.tau/[10,7,5][lod];m.blob((math.cos(a)*.36,math.sin(a)*.36,.07),(.085,.08,.06),4,i,n=5,rings=2)
            for i in range(3):a=i*2.1;m.rod((math.cos(a)*.23,math.sin(a)*.23,.03),(-math.cos(a)*.23,-math.sin(a)*.23,.12),.04,.035,0,5)
        elif kind=='watchtower':
            for x in (-.3,.3):
                for y in (-.3,.3):m.rod((x,y,0),(x*.8,y*.8,.95),.04,.03,0,6)
            m.box((0,0,.7),(.75,.75,.05),7)
            for x in (-.34,.34):m.rod((x,-.35,.90),(x,.35,.90),.025,.025,0,5)
            for i in range([9,6,3][lod]):m.box((0,-.32,.1+i*.065),(.28,.04,.035),0)
        elif kind=='bridge_segment':
            m.box((0,0,.12),(1,.8,.1),5)
            for y in (-.38,.38):
                m.box((0,y,.4),(1,.03,.035),8)
                for x in (-.45,0,.45):m.rod((x,y,.16),(x,y,.4),.018,.018,8,4)
        else:
            m.box((0,0,.28),(.8,.65,.5),12);m.box((0,0,.57),(.9,.75,.08),9)
            m.box((0,-.332,.26),(.20,.025,.42),8)
            for x in (-.28,.28):m.box((x,-.335,.38),(.16,.02,.12),14)
            m.rod((.25,.15,.6),(.25,.15,.94),.012,.012,8,6);pipe(m,(-.23,.1,.67),(-.23,.1,.71),.16,.035,9,n)
    else:
        # Industrial kit: coherent scrith frames, flanged pipes and service ribs.
        m.box((0,0,.05),(1,1,.1),8)
        if kind in ('flup_outlet','sediment_nozzle','transit_tube'):
            radius=.33 if kind!='sediment_nozzle' else .22;pipe(m,(0,-.47,.45),(0,.47,.45),radius,.04,9,n*2)
            for y in (-.45,.0,.45):pipe(m,(0,y-.015,.45),(0,y+.015,.45),radius+.04,.035,8,n)
            for x in (-.32,.32):m.box((x,0,.2),(.08,.7,.3),12)
        elif kind in ('rim_hatch','rim_airlock'):
            m.box((0,0,.35),(.9,.6,.6),12);m.box((0,-.31,.35),(.68,.035,.48),8)
            m.box((0,-.34,.35),(.015,.04,.48),9)
            for x in (-.4,.4):m.box((x,-.35,.35),(.07,.08,.65),9)
            if lod<2:
                for x in (-.23,.23):pipe(m,(x,-.35,.36),(x,-.38,.36),.09,.025,9,n)
            if kind=='rim_hatch':m.box((0,0,.7),(.5,.48,.1),9)
        elif kind in ('elevator_base','terminal_gantry'):
            for x in (-.37,.37):m.box((x,0,.53),(.16,.5,.94),12)
            m.box((0,0,.93),(.9,.55,.1),9);m.box((0,0,.32),(.50,.55,.09),9)
            for x in (-.22,.22):m.rod((x,0,.15),(x,0,.9),.017,.017,8,5)
        elif kind in ('maglev_segment','transport_causeway'):
            rails(m,lod,.16)
            for y in (-.3,.3):m.box((0,y,.04),(.7,.1,.08),12)
        elif kind=='maintenance_platform':
            for x in (-.4,.4):
                for y in (-.4,.4):m.box((x,y,.28),(.06,.06,.5),8)
            m.box((0,0,.5),(1,1,.07),9)
            for x in (-.47,.47):m.box((x,0,.7),(.025,1,.04),8)
        else: # Pump station and roof machinery.
            for x in (-.25,.25):
                m.rod((x,0,.12),(x,0,.65),.18,.18,9,n);pipe(m,(x,-.45,.30),(x,.25,.30),.09,.025,8,n)
            m.box((0,.31,.4),(.75,.25,.65),12)
            if lod<2:
                for i in range([8,4][lod]):m.box((-.3+i*.6/([8,4][lod]-1),.45,.42),(.025,.025,.40),8)
    return m

families={
 'vegetation':['grass_patch','reed_patch','fern_patch','desert_scrub','bush','mirror_sunflower','root_cluster','dead_tree','stump','fallen_log','driftwood','leaf_litter','mushrooms','pebble_cluster'],
 'ruins':['scrith_outcrop','conduit_live','conduit_dark','levitation_grid','debris_beam','city_disk_fragment','crashed_city_disk','ruin_tower','habitat_block'],
 'settlements':['stone_enclosure','watchtower','campfire','bridge_segment','research_station'],
 'infrastructure':['flup_outlet','sediment_nozzle','transit_tube','rim_hatch','rim_airlock','elevator_base','terminal_gantry','maglev_segment','transport_causeway','maintenance_platform','pump_station','roof_machinery']}
manifest={'schema':1,'authoring':'Original first-pass Ringworld habitat library','atlas':'../first-set/exports/SceneryAtlas.png','assets':[]};roots=[]
for family,kinds in families.items():
    for kind in kinds:
        name=kind+'_01';root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
        objects=[shape(kind,lod).object(name+'_LOD'+str(lod),root) for lod in range(3)]
        lo=[min(v.co[k] for v in objects[0].data.vertices) for k in range(3)];hi=[max(v.co[k] for v in objects[0].data.vertices) for k in range(3)]
        for obj in objects:
            for vertex in obj.data.vertices:
                for k in range(3):vertex.co[k]=(vertex.co[k]-lo[k])/max(.001,hi[k]-lo[k])-.5
        collider='none' if family=='vegetation' else 'box'
        if kind in ('fallen_log','driftwood','stump','dead_tree'):collider='box'
        entry={'id':name,'kind':kind,'collider':collider,'family':family,'nominalSize':[hi[k]-lo[k] for k in range(3)],'lods':[]}
        for lod,obj in enumerate(objects):obj.data.calc_loop_triangles();entry['lods'].append({'level':lod,'triangles':len(obj.data.loop_triangles)})
        bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
        for obj in objects:obj.select_set(True)
        bpy.context.view_layer.objects.active=root
        bpy.ops.export_scene.fbx(filepath=str(OUT/'exports'/f'{name}.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',use_space_transform=True,bake_space_transform=True,add_leaf_bones=False,bake_anim=False)
        for obj in objects:obj.hide_render=True;obj.hide_set(True)
        manifest['assets'].append(entry);roots.append((root,entry))
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')

for family in families:
    review=bpy.data.scenes.new('Ringworld — Habitat Library '+family.title());bpy.context.window.scene=review
    review.world=bpy.data.worlds.new('Library studio '+family);review.world.use_nodes=True;review.world.node_tree.nodes['Background'].inputs[1].default_value=.5
    entries=[pair for pair in roots if pair[1]['family']==family]
    for index,(root,entry) in enumerate(entries):
        obj=root.children[0].copy();obj.data=root.children[0].data;obj.parent=None;review.collection.objects.link(obj);obj.hide_render=False;obj.hide_set(False)
        x=(index%4-1.5)*3.3;y=-(index//4)*3.4;size=entry['nominalSize'];factor=2.3/max(size);obj.scale=tuple(v*factor for v in size);obj.location=(x,y,size[2]*factor/2)
        bpy.ops.object.text_add(location=(x,y-1.30,.025));label=bpy.context.object;label.data.body=entry['kind'].replace('_',' ').upper();label.data.align_x='CENTER';label.data.size=.135
    centre=Vector((0,-((len(entries)-1)//4)*1.7,.6));bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.05))
    floor=bpy.data.materials.new('Studio floor '+family);floor.diffuse_color=(.075,.10,.12,1);bpy.context.object.data.materials.append(floor)
    bpy.ops.object.light_add(type='AREA',location=(-4,-2,14));bpy.context.object.data.energy=2400;bpy.context.object.data.size=9
    bpy.ops.object.camera_add(location=centre+Vector((8,-13,17)));camera=bpy.context.object;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=19;review.camera=camera
    review.render.engine='CYCLES';review.cycles.samples=20;review.cycles.use_denoising=True;review.render.resolution_x=1400;review.render.resolution_y=1100;review.render.resolution_percentage=100;review.render.filepath=str(OUT/'previews'/f'{family}.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ringworld-Habitat-Library.blend'))
print(json.dumps({'assets':len(roots),'families':list(families),'max_triangles':max(a['lods'][0]['triangles'] for a in manifest['assets'])},indent=2))
