"""Run through tools/blender_bridge.py in Blender. Original, seeded scenery kit.
Blender Z-up metres; FBX exports convert to Unity Y-up. No downloaded art.
"""
import bpy, math, random, json
from pathlib import Path
from mathutils import Vector

ROOT = Path(r'C:\Users\Hans\Documents\programming\Ring World KSP')
OUT = ROOT / 'art/first-set'
for folder in ('exports', 'previews', 'validation'):
    (OUT / folder).mkdir(parents=True, exist_ok=True)
if not (OUT / 'before-ringworld.blend').exists():
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT / 'before-ringworld.blend'), copy=True)
# Rebuild only this script's two scenes; retain the user's original scene.
for previous in list(bpy.data.scenes):
    if previous.name.startswith(('Ringworld — First Scenery Set','Ringworld — Scenery Review')):
        for obj in list(previous.objects):bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.scenes.remove(previous)
scene = bpy.data.scenes.new('Ringworld — First Scenery Set')
bpy.context.window.scene = scene
scene.unit_settings.system = 'METRIC'
rng = random.Random(1970)

# One original 512px surface atlas for bark, foliage, stone, plaster, roof and metal.
colors = [(0.24,.16,.10),(.12,.25,.065),(.24,.36,.10),(.07,.18,.105),
          (.34,.35,.30),(.47,.45,.37),(.67,.55,.36),(.35,.26,.135),
          (.13,.18,.19),(.45,.49,.48),(.10,.085,.06),(.45,.26,.13),
          (.55,.57,.52),(.25,.28,.19),(.22,.29,.28),(.63,.60,.48)]
atlas=bpy.data.images.new('Ringworld_SceneryAtlas',width=512,height=512,alpha=True)
pixels=[]
for y in range(512):
    for x in range(512):
        tile=(y//128)*4+x//128; u=x%128; v=y%128
        grain=rng.uniform(-.035,.035)
        if tile==0: grain+=.07*math.sin(u*.45+2*math.sin(v*.035))+.035*math.sin(u*1.9)
        elif tile==7: grain+=.045*math.sin(u*1.8+math.sin(v*.05))-.045*(v%22<2)
        elif tile in (4,5,12): grain+=.045*math.sin(u*.10+math.sin(v*.08))*math.cos(v*.11)
        elif tile==6: grain-=.06*(v%32<2 or (u+(64 if (v//32)%2 else 0))%96<2)
        pixels.extend([max(0,min(1,c+grain)) for c in colors[tile]]+[1])
atlas.pixels.foreach_set(pixels);atlas.filepath_raw=str(OUT/'exports/SceneryAtlas.png');atlas.file_format='PNG';atlas.save();atlas.pack()
mat=bpy.data.materials.new('RW_SceneryAtlas');mat.use_nodes=True
shader=mat.node_tree.nodes.get('Principled BSDF');shader.inputs['Roughness'].default_value=.86
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas;mat.node_tree.links.new(tex.outputs['Color'],shader.inputs['Base Color'])

class MeshBuilder:
    def __init__(self): self.v=[];self.f=[];self.tiles=[]
    def face(self,points,tile):
        start=len(self.v);self.v.extend(points);self.f.append(tuple(range(start,start+len(points))));self.tiles.append(tile)
    def rod(self,a,b,r1,r2,tile=0,n=7):
        a,b=Vector(a),Vector(b);axis=(b-a).normalized();u=axis.cross(Vector((0,1,0)))
        if u.length<.01:u=axis.cross(Vector((1,0,0)))
        u.normalize();v=axis.cross(u);rings=[]
        for center,r in ((a,r1),(b,r2)):
            rings.append([center+r*(u*math.cos(i*2*math.pi/n)+v*math.sin(i*2*math.pi/n)) for i in range(n)])
        for i in range(n):j=(i+1)%n;self.face([rings[0][i],rings[0][j],rings[1][j],rings[1][i]],tile)
        self.face(list(reversed(rings[0])),tile);self.face(rings[1],tile)
    def box(self,c,s,tile):
        x,y,z=c;a,b,d=[q/2 for q in s]
        p=[(x+dx*a,y+dy*b,z+dz*d) for dx,dy,dz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
        for ids in ((0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)):self.face([p[i] for i in ids],tile)
    def blob(self,c,s,tile,seed,n=9,rings=4):
        r=random.Random(seed);verts=[]
        for j in range(1,rings):
            phi=math.pi*j/rings
            verts.append([Vector(c)+Vector((s[0]*math.sin(phi)*math.cos(i*2*math.pi/n),s[1]*math.sin(phi)*math.sin(i*2*math.pi/n),s[2]*math.cos(phi)))*r.uniform(.85,1.13) for i in range(n)])
        top=Vector(c)+Vector((0,0,s[2]));bottom=Vector(c)-Vector((0,0,s[2]))
        for i in range(n):
            k=(i+1)%n;self.face([top,verts[0][i],verts[0][k]],tile)
            for j in range(len(verts)-1):self.face([verts[j][i],verts[j+1][i],verts[j+1][k],verts[j][k]],tile)
            self.face([verts[-1][i],bottom,verts[-1][k]],tile)
    def leaf(self,c,size,angle,tile):
        # A folded, pointed leaf silhouette, solid geometry: no alpha overdraw.
        c=Vector(c);u=Vector((math.cos(angle),math.sin(angle),.18))*size;v=Vector((-math.sin(angle),math.cos(angle),.3))*size*.44
        p=[c-u,c+v,c+u,c-v];ridge=c+Vector((0,0,size*.2))
        for i in range(4):
            tri=[p[i],p[(i+1)%4],ridge];self.face(tri,tile);self.face(list(reversed(tri)),tile)
    def object(self,name,parent):
        mesh=bpy.data.meshes.new(name);mesh.from_pydata(self.v,[],self.f);mesh.materials.append(mat);mesh.update()
        uv=mesh.uv_layers.new(name='AtlasUV')
        for p,tile in zip(mesh.polygons,self.tiles):
            xy=[(0.04,.04),(.96,.04),(.96,.96),(.04,.96)]
            for j,li in enumerate(p.loop_indices):
                a,b=xy[j%4];uv.data[li].uv=((tile%4+a)/4,(tile//4+b)/4)
        obj=bpy.data.objects.new(name,mesh);scene.collection.objects.link(obj);obj.parent=parent
        return obj

assets=[]
def tree(name,conifer,variant):
    root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
    for lod in range(3):
        m=MeshBuilder();r=random.Random(100+variant);n=7 if lod==0 else 5
        lean=(-.055 if variant==0 else .075)
        m.rod((0,0,0),(lean,.015,.97 if conifer else .72),.036,.008,0,n)
        if lod==0:
            for i in range(5):
                a=i*math.tau/5;m.rod((math.cos(a)*.10,math.sin(a)*.10,0),(0,0,.17),.012,.02,0,5)
        if conifer:
            for tier in range(7 if lod<2 else 4):
                z=.24+tier*(.098 if lod<2 else .16);radius=(1-z)*(.42 if variant==0 else .32)
                branches=7 if lod==0 else 5
                for i in range(branches):
                    angle=i*math.tau/branches+tier*.83
                    end=(lean*z+math.cos(angle)*radius,math.sin(angle)*radius,z-.035)
                    if lod==0:m.rod((lean*z,0,z+.03),end,.006,.0015,0,5)
                    m.blob(end,(radius*.52,radius*.48,.075),3 if i%3 else 1,500+tier*10+i,n=5 if lod==0 else 4,rings=3 if lod==0 else 2)
            m.blob((lean,0,.91),(.06,.06,.09),3,5,n=5,rings=3)
        else:
            for i in range(11 if lod<2 else 7):
                angle=i*2.39996;rad=.10+(i%3)*.067;z=.60+(i%4)*.065
                center=Vector((lean+math.cos(angle)*rad,math.sin(angle)*rad,z))
                if lod<2:m.rod((lean*.7,0,.37+i*.017),center,.015,.003,0,n)
                scale=(.15,.13,.145) if variant==0 else (.16,.14,.105)
                m.blob(center,scale,1 if i%3 else 2,400+i,n=7 if lod==0 else 5,rings=3 if lod<2 else 2)
                if lod==0:
                    for j in range(15):
                        a=r.random()*math.tau;d=r.uniform(.07,.18)
                        p=center+Vector((math.cos(a)*d,math.sin(a)*d,r.uniform(-.08,.10)))
                        m.leaf(p,r.uniform(.024,.045),a,2 if j%4==0 else 1)
        m.object(name+'_LOD'+str(lod),root)
    # Exact same normalization for all levels avoids LOD position shifts.
    zmax=max(v.co.z for v in root.children[0].data.vertices)
    for o in root.children:
        for v in o.data.vertices:
            v.co/=zmax;v.co.z=max(0,v.co.z)
    root['kind']='tree_conifer' if conifer else 'tree_broadleaf'
    root['collider']='trunk';assets.append(root)

def rock(name,variant):
    root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
    for lod in range(3):
        m=MeshBuilder();m.blob((0,0,0),(.5,.5,.5),4+variant,71+variant,n=[13,8,5][lod],rings=[6,4,3][lod])
        o=m.object(name+'_LOD'+str(lod),root)
        for axis in range(3):
            lo=min(v.co[axis] for v in o.data.vertices);hi=max(v.co[axis] for v in o.data.vertices)
            for v in o.data.vertices:v.co[axis]=(v.co[axis]-lo)/(hi-lo)-.5
    root['kind']='boulder';root['collider']='rock';assets.append(root)

def dwelling(name,variant):
    root=bpy.data.objects.new(name,None);scene.collection.objects.link(root)
    for lod in range(3):
        m=MeshBuilder();m.box((0,0,-.13),(.80,.72,.66),6)
        # Raised stone footing, heavy pitched roof and deep dark doorway.
        m.box((0,0,-.445),(.88,.8,.11),5)
        roof=[(-.5,-.46,.18),(.5,-.46,.18),(0,-.46,.50),(-.5,.46,.18),(.5,.46,.18),(0,.46,.50)]
        for ids in ((0,2,1),(3,4,5),(0,3,5,2),(2,5,4,1),(0,1,4,3)):m.face([roof[i] for i in reversed(ids)],7 if variant==0 else 11)
        m.box((-.10,-.369,-.21),(.18,.02,.47),10)
        for x in (-.28,.25):m.box((x,-.37,-.02),(.105,.025,.14),10)
        if lod<2:
            for x in (-.4,.4):m.box((x,-.378,-.14),(.045,.05,.63),0)
            m.box((0,-.383,.135),(.84,.04,.045),0)
            for x in (-.204,.004):m.box((x,-.386,-.20),(.027,.035,.50),0)
            m.box((-.1,-.386,.047),(.25,.035,.045),0)
            m.box((-.1,-.422,-.465),(.27,.12,.07),4)
            if variant==1:m.box((.27,.22,.29),(.10,.12,.4),5)
        if lod==0:
            for y in (-.46,.46):m.rod((-.5,y,.18),(0,y,.505),.018,.018,0,5);m.rod((0,y,.505),(.5,y,.18),.018,.018,0,5)
            for i in range(9):
                y=-.43+i*.107;m.rod((-.49,y,.19),(0,y,.51),.005,.005,7,4);m.rod((0,y,.51),(.49,y,.19),.005,.005,7,4)
            for i in range(6):m.box((-.38+i*.15,-.409,-.44),(.13,.035,.08),4 if i%2 else 5)
        m.object(name+'_LOD'+str(lod),root)
    root['kind']='rural_building';root['collider']='house';assets.append(root)

tree('broadleaf_spreading_01',False,0);tree('broadleaf_windswept_02',False,1)
tree('conifer_layered_01',True,0);tree('conifer_slender_02',True,1)
rock('boulder_weathered_01',0);rock('boulder_sandstone_02',1)
dwelling('habitation_thatch_01',0);dwelling('habitation_reclaimed_02',1)

manifest={'schema':1,'authoring':'Original Ringworld scenery first set; no third-party assets',
          'atlas':'SceneryAtlas.png','blender':bpy.app.version_string,'assets':[]}
for root in assets:
    children=list(root.children)
    for obj in children:obj.hide_render=False;obj.hide_set(False)
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for obj in children:obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    bpy.ops.export_scene.fbx(filepath=str(OUT/'exports'/f'{root.name}.fbx'),use_selection=True,object_types={'MESH','EMPTY'},
                             axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',
                             use_space_transform=True,bake_space_transform=True,
                             use_custom_props=True,add_leaf_bones=False,bake_anim=False,path_mode='AUTO')
    entry={'id':root.name,'kind':root['kind'],'collider':root['collider'],'lods':[]}
    for lod,obj in enumerate(children):
        obj.data.calc_loop_triangles()
        points=[v.co for v in obj.data.vertices]
        entry['lods'].append({'level':lod,'triangles':len(obj.data.loop_triangles),
                             'min':[min(p[k] for p in points) for k in range(3)],'max':[max(p[k] for p in points) for k in range(3)]})
        obj.hide_render=True;obj.hide_set(True)
    manifest['assets'].append(entry)
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')

# A separate art-review scene: sources above stay at normalized export origin.
preview=bpy.data.scenes.new('Ringworld — Scenery Review');bpy.context.window.scene=preview
preview.world=bpy.data.worlds.new('RW studio daylight');preview.world.use_nodes=True
preview.world.node_tree.nodes['Background'].inputs[0].default_value=(.18,.23,.29,1)
preview.world.node_tree.nodes['Background'].inputs[1].default_value=.45
for index,root in enumerate(assets):
    display=root.children[0].copy();display.data=root.children[0].data;display.parent=None;preview.collection.objects.link(display)
    display.hide_render=False;display.hide_set(False)
    x=(index%4-1.5)*5.2;y=2.8 if index<4 else -3.8
    size=6.1 if index<4 else (2.0 if index<6 else 3.5)
    display.location=(x,y,0 if index<4 else size*.5);display.scale=(size,size,size)
    bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=2.05,depth=.16,location=(x,y,-.12))
    base=bpy.context.object;base.name='Review plinth'
    material=bpy.data.materials.get('Studio slate') or bpy.data.materials.new('Studio slate');material.diffuse_color=(.095,.125,.145,1);base.data.materials.append(material)
    bpy.ops.object.text_add(location=(x,y-2.2,.015),rotation=(0,0,0))
    label=bpy.context.object;label.data.body=root.name.replace('_',' ').upper();label.data.align_x='CENTER';label.data.size=.16;label.data.extrude=.001
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.23));bpy.context.object.name='Review floor';bpy.context.object.data.materials.append(material)
bpy.ops.object.light_add(type='AREA',location=(-6,-7,16));bpy.context.object.data.energy=2600;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=12
bpy.ops.object.light_add(type='AREA',location=(8,8,10));bpy.context.object.data.energy=1900;bpy.context.object.data.size=10
bpy.ops.object.camera_add(location=(15,-24,19));camera=bpy.context.object
camera.rotation_euler=(Vector((0,0,2))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=26;preview.camera=camera
preview.render.engine='CYCLES';preview.cycles.samples=24;preview.cycles.use_denoising=True
preview.render.resolution_x=1500;preview.render.resolution_y=1050;preview.render.resolution_percentage=100
preview.render.image_settings.file_format='PNG';preview.render.filepath=str(OUT/'previews/first-scenery-set.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ringworld-Scenery-First-Set.blend'))
print(json.dumps({'created':len(assets),'source':str(OUT/'Ringworld-Scenery-First-Set.blend'),'triangles':{a['id']:[l['triangles'] for l in a['lods']] for a in manifest['assets']}},indent=2))
