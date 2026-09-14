using System;
using System.Collections.Generic;
using Ringworld.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace NivenRingworld
{
    internal sealed class SurfaceStreamer : IDisposable
    {
        private sealed class Tile
        {
            internal GameObject Root;
            internal DVec Anchor;
            internal long X,Y;internal Texture2D Texture;
            internal double Phase;
            internal readonly List<Mesh> Meshes=new List<Mesh>();
        }
        private readonly Settings settings;
        private readonly Dictionary<string,Tile> tiles=new Dictionary<string,Tile>();
        private readonly List<GameObject> props=new List<GameObject>();
        private readonly List<DVec> propPositions=new List<DVec>();
        private string propSite="";
        private double propPhase;
        private readonly List<Quaternion> propRotations=new List<Quaternion>();
        private readonly Material terrainMaterial,waterMaterial,buildingMaterial,scrithMaterial,leavesMaterial;
        private readonly Texture2D palette;
        private TerrainLod lod;
        internal void RebuildLod(){lod.Dispose();lod=new TerrainLod(settings,terrainMaterial);}
        private readonly GameObject sunlightObject;private readonly Light sunlight;
        internal int LodCount {get{return lod.Count;}}
        internal int LodPending {get{return lod.Pending;}}
        internal int ScaledLodCount {get{return lod.ScaledCount;}}
        internal int TileCount { get { return tiles.Count; } }
        internal SurfaceStreamer(Settings s)
        {
            settings=s;
            // A one-row palette avoids requiring a proprietary Unity asset bundle.
            palette=new Texture2D(16,1,TextureFormat.RGBA32,false);palette.filterMode=FilterMode.Point;palette.wrapMode=TextureWrapMode.Clamp;
            Color[] colors={new Color(.02f,.18f,.29f),new Color(.07f,.30f,.40f),new Color(.10f,.36f,.43f),new Color(.28f,.37f,.19f),
                new Color(.40f,.49f,.24f),new Color(.16f,.31f,.16f),new Color(.69f,.57f,.35f),new Color(.41f,.39f,.36f),new Color(.84f,.89f,.89f),
                new Color(.23f,.27f,.30f),new Color(.47f,.45f,.36f),new Color(.28f,.32f,.36f),new Color(.31f,.30f,.26f),Color.gray,Color.gray,Color.gray};
            palette.SetPixels(colors);palette.Apply();
            terrainMaterial=Material(new Color(1,1,1));terrainMaterial.mainTexture=palette;terrainMaterial.SetFloat("_Glossiness",.08f);
            lod=new TerrainLod(settings,terrainMaterial);
            sunlightObject=new GameObject("Ringworld habitat sunlight");sunlight=sunlightObject.AddComponent<Light>();
            sunlight.type=LightType.Directional;sunlight.cullingMask=1<<15;sunlight.color=new Color(1,.96f,.88f);sunlight.intensity=0;sunlight.shadows=LightShadows.Soft;sunlight.shadowBias=.05f;sunlight.shadowNormalBias=.4f;
            waterMaterial=Material(new Color(.07f,.31f,.42f));waterMaterial.SetFloat("_Glossiness",.8f);
            buildingMaterial=Material(new Color(.64f,.60f,.48f));scrithMaterial=Material(new Color(.24f,.29f,.32f));
            leavesMaterial=Material(new Color(.15f,.29f,.12f));
        }
        private static Material Material(Color c)
        {
            Shader shader=Shader.Find("Standard")??Shader.Find("KSP/Diffuse")??Shader.Find("Diffuse");
            if(shader==null) throw new InvalidOperationException("No compatible terrain shader found.");
            return new Material(shader){color=c};
        }
        internal void Update(DVec observer,Vector3d star,bool immediate=false)
        {
            RingPoint p=settings.Geometry.Coordinates(observer);
            sunlight.transform.rotation=Quaternion.LookRotation(-ConvertVector.Unity(settings.Geometry.Up(observer)),Vector3.up);
            long cx=(long)Math.Floor(p.Along/settings.TileSize),cy=(long)Math.Floor(p.Across/settings.TileSize);
            int radius=settings.TileRadius;
            var wanted=new HashSet<string>();
            var pending=new List<Tuple<long,long>>();
            for(int y=-radius;y<=radius;y++)for(int x=-radius;x<=radius;x++)
            {
                long tx=cx+x,ty=cy+y;
                string key=tx+":"+ty;wanted.Add(key);
                if(!tiles.ContainsKey(key))pending.Add(Tuple.Create(tx,ty));
            }
            pending.Sort((a,b)=>((a.Item1-cx)*(a.Item1-cx)+(a.Item2-cy)*(a.Item2-cy)).CompareTo((b.Item1-cx)*(b.Item1-cx)+(b.Item2-cy)*(b.Item2-cy)));
            // Keep old colliders until replacement tiles exist. Entry builds all tiles before release.
            int budget=immediate?pending.Count:2;
            for(int i=0;i<Math.Min(budget,pending.Count);i++)
            {
                var c=pending[i];var t=Build(c.Item1,c.Item2);tiles.Add(c.Item1+":"+c.Item2,t);
            }
            var remove=new List<string>();foreach(var kv in tiles)if(!wanted.Contains(kv.Key))remove.Add(kv.Key);
            foreach(string key in remove){Destroy(tiles[key]);tiles.Remove(key);}
            double distance;var site=settings.Terrain.Nearest(p.Along,p.Across,out distance);
            string id=site!=null&&distance<8000?site.Id:"";
            if(id!=propSite){ClearProps();propSite=id;if(id!=""){propPhase=settings.Geometry.OrientationRadians;BuildProps(site);}}
            lod.Update(p.Along,p.Across);
            Reposition(star);
        }
        internal double CollisionHeight(double along,double across)
        {
            // Interpolate the same two triangles used by a streamed grid cell.
            double spacing=settings.TileSize/settings.TileResolution;
            double x=Math.Floor(along/spacing)*spacing,y=Math.Floor(across/spacing)*spacing;
            double u=(along-x)/spacing,v=(across-y)/spacing;
            double a=settings.Terrain.Sample(x,y).Height,b=settings.Terrain.Sample(x+spacing,y).Height;
            double c=settings.Terrain.Sample(x,y+spacing).Height,d=settings.Terrain.Sample(x+spacing,y+spacing).Height;
            return u+v<=1?a+(b-a)*u+(c-a)*v:d+(c-d)*(1-u)+(b-d)*(1-v);
        }
        internal double CameraFloor(double along,double across)
        {
            // Match water triangle interpolation, including partially wet shoreline cells.
            double spacing=settings.TileSize/settings.TileResolution;
            double x=Math.Floor(along/spacing)*spacing,y=Math.Floor(across/spacing)*spacing;
            double u=(along-x)/spacing,v=(across-y)/spacing;
            var a=settings.Terrain.Sample(x,y);var b=settings.Terrain.Sample(x+spacing,y);
            var c=settings.Terrain.Sample(x,y+spacing);var d=settings.Terrain.Sample(x+spacing,y+spacing);
            double floor=CollisionHeight(along,across);
            if(u+v<=1&&(a.Wet||b.Wet||c.Wet))
                floor=Math.Max(floor,WaterVertex(a)+(WaterVertex(b)-WaterVertex(a))*u+(WaterVertex(c)-WaterVertex(a))*v);
            else if(u+v>1&&(b.Wet||c.Wet||d.Wet))
                floor=Math.Max(floor,WaterVertex(d)+(WaterVertex(c)-WaterVertex(d))*(1-u)+(WaterVertex(b)-WaterVertex(d))*(1-v));
            return floor;
        }
        private static double WaterVertex(TerrainSample s){return (double.IsNegativeInfinity(s.WaterHeight)?s.Height-1:s.WaterHeight)+.1;}
        internal void Reposition(Vector3d star)
        {
            foreach(var t in tiles.Values)Place(t.Root,t.Anchor,t.Phase,Quaternion.identity,star);
            for(int i=0;i<props.Count;i++)Place(props[i],propPositions[i],propPhase,propRotations[i],star);
            lod.Reposition(star);
        }
        private void Place(GameObject obj,DVec anchor,double phase,Quaternion rotation,Vector3d star)
        {
            double delta=settings.Geometry.OrientationRadians-phase;
            obj.transform.position=(Vector3)(star+ConvertVector.Ksp(RingGeometry.Rotate(anchor,delta)));
            obj.transform.rotation=Quaternion.AngleAxis((float)(delta*180/Math.PI),Vector3.up)*rotation;
        }
        internal void Light(double daylight)
        {
            float light=(float)(.08+.92*daylight);
            sunlight.intensity=(float)(daylight*.5);lod.Light(light);
            terrainMaterial.color=new Color(light,light,light);buildingMaterial.color=new Color(.64f*light,.60f*light,.48f*light);
            waterMaterial.color=new Color(.07f*light,.31f*light,.42f*light);
            scrithMaterial.color=new Color(.24f*light,.29f*light,.32f*light);leavesMaterial.color=new Color(.15f*light,.29f*light,.12f*light);
        }
        private Tile Build(long tx,long ty)
        {
            double size=settings.TileSize,x0=tx*size,y0=ty*size;int n=settings.TileResolution;
            var t=new Tile{Root=new GameObject("Ringworld terrain "+tx+","+ty),Anchor=settings.Geometry.Position(x0,y0,0),X=tx,Y=ty,Phase=settings.Geometry.OrientationRadians};
            t.Root.layer=15;
            int count=(n+1)*(n+1);var vertices=new Vector3[count];var uv=new Vector2[count];var heights=new double[count];
            var colors=new Color[count];
            var waterVerts=new Vector3[count];var wet=new bool[count];
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
            {
                int i=y*(n+1)+x;double a=x0+size*x/n,b=y0+size*y/n;
                var sample=settings.Terrain.Sample(a,b);heights[i]=sample.Height;
                vertices[i]=ConvertVector.Unity(settings.Geometry.Position(a,b,sample.Height)-t.Anchor);
                uv[i]=new Vector2((x+.5f)/(n+1),(y+.5f)/(n+1));colors[i]=TerrainTint.Color(sample);
                wet[i]=sample.Wet;
                double water=double.IsNegativeInfinity(sample.WaterHeight)?sample.Height-1:sample.WaterHeight;
                waterVerts[i]=ConvertVector.Unity(settings.Geometry.Position(a,b,water+.1)-t.Anchor);
            }
            var indices=new List<int>(n*n*6);var waterIndices=new List<int>();
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                int a=y*(n+1)+x,b=a+1,c=a+n+1,d=c+1;
                // Across cross Along points inward with this ring parameterization.
                Add(indices,a,c,b);Add(indices,b,c,d);
                if(wet[a]||wet[b]||wet[c])Add(waterIndices,a,c,b);
                if(wet[b]||wet[d]||wet[c])Add(waterIndices,b,c,d);
            }
            Mesh ground=new Mesh{name="Ringworld ground"};ground.vertices=vertices;ground.uv=uv;ground.SetTriangles(indices,0);ground.RecalculateNormals();ground.RecalculateBounds();t.Meshes.Add(ground);
            t.Texture=TerrainTint.Texture(n+1,colors);
            t.Root.AddComponent<MeshFilter>().sharedMesh=ground;var renderer=t.Root.AddComponent<MeshRenderer>();renderer.sharedMaterial=terrainMaterial;
            var colorBlock=new MaterialPropertyBlock();colorBlock.SetTexture("_MainTex",t.Texture);renderer.SetPropertyBlock(colorBlock);
            var shellVertices=new Vector3[count*2];Array.Copy(vertices,shellVertices,count);
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
                shellVertices[count+y*(n+1)+x]=ConvertVector.Unity(settings.Geometry.Position(x0+size*x/n,y0+size*y/n,settings.UndersideAltitude)-t.Anchor);
            var shellIndices=GroundShell.Triangles(n);
            var shell=new Mesh{name="Closed ring collision shell"};shell.vertices=shellVertices;shell.triangles=shellIndices;shell.RecalculateBounds();t.Meshes.Add(shell);
            t.Root.AddComponent<MeshCollider>().sharedMesh=shell;
            // Render only underside and edges here; the terrain renderer owns the top.
            var undersideIndices=new int[shellIndices.Length-n*n*6];Array.Copy(shellIndices,n*n*6,undersideIndices,0,undersideIndices.Length);
            var undersideMesh=new Mesh{name="Scrith underside and edges"};undersideMesh.vertices=shellVertices;undersideMesh.triangles=undersideIndices;undersideMesh.RecalculateNormals();undersideMesh.RecalculateBounds();t.Meshes.Add(undersideMesh);
            var underside=new GameObject("Ring structural underside");underside.layer=15;underside.transform.SetParent(t.Root.transform,false);
            underside.AddComponent<MeshFilter>().sharedMesh=undersideMesh;var undersideRenderer=underside.AddComponent<MeshRenderer>();undersideRenderer.sharedMaterial=scrithMaterial;
            undersideRenderer.shadowCastingMode=ShadowCastingMode.Off;
            AddRimWalls(t,x0,y0,size);
            AddScenery(t,x0,y0,size);
            if(waterIndices.Count>0)
            {
                var water=new GameObject("Water");water.layer=15;water.transform.SetParent(t.Root.transform,false);
                var mesh=new Mesh{name="Ringworld water"};mesh.vertices=waterVerts;mesh.SetTriangles(waterIndices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();t.Meshes.Add(mesh);
                water.AddComponent<MeshFilter>().sharedMesh=mesh;water.AddComponent<MeshRenderer>().sharedMaterial=waterMaterial;
            }
            return t;
        }
        private void AddRimWalls(Tile t,double x0,double y0,double size)
        {
            foreach(int sign in new[]{-1,1})
            {
                double edge=sign*settings.Geometry.P.Width/2;
                if(edge<y0||edge>=y0+size)continue;
                var v=new Vector3[4];
                v[0]=ConvertVector.Unity(settings.Geometry.Position(x0,edge,-100)-t.Anchor);
                v[1]=ConvertVector.Unity(settings.Geometry.Position(x0+size,edge,-100)-t.Anchor);
                v[2]=ConvertVector.Unity(settings.Geometry.Position(x0,edge,settings.Geometry.P.WallHeight)-t.Anchor);
                v[3]=ConvertVector.Unity(settings.Geometry.Position(x0+size,edge,settings.Geometry.P.WallHeight)-t.Anchor);
                var mesh=new Mesh{name="Atmosphere retaining rim wall"};mesh.vertices=v;mesh.triangles=new[]{0,1,2,1,3,2,2,1,0,2,3,1};mesh.RecalculateNormals();mesh.RecalculateBounds();t.Meshes.Add(mesh);
                var wall=new GameObject("Scrith rim wall");wall.layer=15;wall.transform.SetParent(t.Root.transform,false);
                wall.AddComponent<MeshFilter>().sharedMesh=mesh;wall.AddComponent<MeshRenderer>().sharedMaterial=scrithMaterial;wall.AddComponent<MeshCollider>().sharedMesh=mesh;
            }
        }
        private void TileProp(Tile t,string name,double a,double b,double altitude,Vector3 scale,Material mat,PrimitiveType shape)
        {
            var point=settings.Geometry.Position(a,b,altitude);
            var obj=GameObject.CreatePrimitive(shape);obj.name=name;obj.layer=15;obj.transform.SetParent(t.Root.transform,false);
            obj.transform.localPosition=ConvertVector.Unity(point-t.Anchor);
            obj.transform.localRotation=Quaternion.FromToRotation(Vector3.up,ConvertVector.Unity(settings.Geometry.Up(point)));
            obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=mat;
        }
        private void AddScenery(Tile t,double x0,double y0,double size)
        {
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)
            {
                long cellX=t.X*8+x,cellY=t.Y*8+y;
                double a=x0+(x+.08+.84*settings.Terrain.Scatter(cellX,cellY,91))*size/8;
                double b=y0+(y+.08+.84*settings.Terrain.Scatter(cellX,cellY,93))*size/8;
                var sample=settings.Terrain.Sample(a,b);if(sample.Wet||sample.Biome==Biome.Rimwall||sample.Biome==Biome.Ruins)continue;
                double variation=settings.Terrain.Scatter(cellX,cellY,97);
                double grove=settings.Terrain.Noise(a,b,1100,99);
                if((sample.Biome==Biome.Forest&&variation<grove*.85*settings.ForestDensity)||(sample.Biome==Biome.Grassland&&variation>1-.08*settings.ForestDensity&&grove>.55))
                {
                    float h=(float)(8+12*variation);
                    TileProp(t,"Procedural tree trunk",a,b,sample.Height+h/2,new Vector3(1.5f,h/2,1.5f),scrithMaterial,PrimitiveType.Cylinder);
                    TileProp(t,"Procedural tree canopy",a,b,sample.Height+h,new Vector3(h*.7f,h*.7f,h*.7f),leavesMaterial,PrimitiveType.Sphere);
                }
                else if(variation>.9&&sample.Biome!=Biome.Road)
                    TileProp(t,"Weathered boulder",a,b,sample.Height+1,new Vector3(4,3,5),scrithMaterial,PrimitiveType.Sphere);
            }
            double ca=x0+size*(.2+.6*settings.Terrain.Scatter(t.X,t.Y,103)),cb=y0+size*(.2+.6*settings.Terrain.Scatter(t.X,t.Y,107));var center=settings.Terrain.Sample(ca,cb);
            if((center.Biome==Biome.Grassland||center.Biome==Biome.Desert)&&settings.Terrain.Noise(ca,cb,50,101)>.97)
            {
                for(int i=-2;i<=2;i++)
                {
                    double a=ca+i*48+(settings.Terrain.Scatter(t.X+i,t.Y,109)-.5)*25;
                    double b=cb+(settings.Terrain.Scatter(t.X+i,t.Y,113)-.5)*120;
                    var sample=settings.Terrain.Sample(a,b);if(sample.Wet||sample.Biome==Biome.Road)continue;
                    float height=(float)(5+settings.Terrain.Scatter(t.X+i,t.Y,127)*19);
                    TileProp(t,"Weathered rural habitation",a,b,sample.Height+height/2,new Vector3(18,height,24),buildingMaterial,PrimitiveType.Cube);
                }
            }
        }
        private static void Add(List<int> a,int x,int y,int z){a.Add(x);a.Add(y);a.Add(z);}
        private void Prop(string name,double along,double across,double height,Vector3 size,Material mat,PrimitiveType shape=PrimitiveType.Cube)
        {
            var position=settings.Geometry.Position(along,across,height);
            var obj=GameObject.CreatePrimitive(shape);obj.name=name;obj.layer=15;
            obj.transform.rotation=Quaternion.FromToRotation(Vector3.up,ConvertVector.Unity(settings.Geometry.Up(position)));
            obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=mat;
            props.Add(obj);propPositions.Add(position);propRotations.Add(obj.transform.rotation);
        }
        private void BuildProps(Landmark l)
        {
            if(l.Kind=="ocean"||l.Kind=="mountain"||l.Kind=="puncture"||l.Kind=="waterway")return;
            double h=settings.Terrain.Sample(l.Along,l.Across).Height;
            Prop("Research plinth",l.Along+45,l.Across,h+3,new Vector3(9,6,9),scrithMaterial);
            if(l.Kind=="scrith")
            { Prop("Exposed scrith plate",l.Along,l.Across,h+.3,new Vector3(90,.6f,90),scrithMaterial);return; }
            int n=l.Kind=="city"?7:3;
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                if(x==n/2&&y==n/2)continue;
                double jitterA=(settings.Terrain.Scatter(x,y,151)-.5)*22,jitterB=(settings.Terrain.Scatter(x,y,157)-.5)*22;
                double a=l.Along+(x-n/2)*95+jitterA,b=l.Across+(y-n/2)*95+jitterB;
                float bh=l.Kind=="city"?18+(x*17+y*31)%105:12+(x*7+y*13)%15;
                double floor=settings.Terrain.Sample(a,b).Height;
                Prop("Habitat block",a,b,floor+bh/2,new Vector3(36,bh,42),buildingMaterial);
                Prop("Roof machinery",a,b,floor+bh+2,new Vector3(24,4,30),scrithMaterial);
                // Dark window bands are shallow physical trim, requiring no imported textures.
                for(int f=1;f<bh/9;f++)Prop("Window belt",a,b,floor+f*9,new Vector3(36.3f,1.8f,42.3f),scrithMaterial);
            }
            Prop("Transport causeway",l.Along,l.Across,h+.5,new Vector3(28,1,650),scrithMaterial);
            if(l.Kind=="terminal")Prop("Rim transport gantry",l.Along+130,l.Across,h+45,new Vector3(20,90,20),scrithMaterial);
        }
        private void ClearProps(){foreach(var p in props)UnityEngine.Object.Destroy(p);props.Clear();propPositions.Clear();propRotations.Clear();}
        private static void Destroy(Tile t){UnityEngine.Object.Destroy(t.Texture);UnityEngine.Object.Destroy(t.Root);foreach(var m in t.Meshes)UnityEngine.Object.Destroy(m);}
        public void Dispose()
        {
            foreach(var t in tiles.Values)Destroy(t);tiles.Clear();ClearProps();
            lod.Dispose();UnityEngine.Object.Destroy(sunlightObject);
            UnityEngine.Object.Destroy(terrainMaterial);UnityEngine.Object.Destroy(waterMaterial);UnityEngine.Object.Destroy(buildingMaterial);UnityEngine.Object.Destroy(scrithMaterial);UnityEngine.Object.Destroy(leavesMaterial);UnityEngine.Object.Destroy(palette);
        }
    }
}
