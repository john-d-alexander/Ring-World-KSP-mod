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
            internal long X,Y;
            internal readonly List<Mesh> Meshes=new List<Mesh>();
        }
        private readonly Settings settings;
        private readonly Dictionary<string,Tile> tiles=new Dictionary<string,Tile>();
        private readonly List<GameObject> props=new List<GameObject>();
        private readonly List<DVec> propPositions=new List<DVec>();
        private string propSite="";
        private readonly Material terrainMaterial,waterMaterial,buildingMaterial,scrithMaterial,leavesMaterial;
        private readonly Texture2D palette;
        private GameObject distant;
        private Mesh distantMesh;
        private DVec distantAnchor;
        private double distantAlong=double.NaN,distantAcross;
        internal int TileCount { get { return tiles.Count; } }
        internal SurfaceStreamer(Settings s)
        {
            settings=s;
            // A one-row palette avoids requiring a proprietary Unity asset bundle.
            palette=new Texture2D(16,1,TextureFormat.RGBA32,false);palette.filterMode=FilterMode.Point;palette.wrapMode=TextureWrapMode.Clamp;
            Color[] colors={new Color(.02f,.18f,.29f),new Color(.07f,.30f,.40f),new Color(.10f,.36f,.43f),new Color(.28f,.37f,.19f),
                new Color(.40f,.49f,.24f),new Color(.16f,.31f,.16f),new Color(.69f,.57f,.35f),new Color(.41f,.39f,.36f),new Color(.84f,.89f,.89f),
                new Color(.23f,.27f,.30f),new Color(.47f,.45f,.36f),new Color(.28f,.32f,.36f),Color.gray,Color.gray,Color.gray,Color.gray};
            palette.SetPixels(colors);palette.Apply();
            terrainMaterial=Material(new Color(1,1,1));terrainMaterial.mainTexture=palette;
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
            if(id!=propSite){ClearProps();propSite=id;if(id!="")BuildProps(site);}
            if(double.IsNaN(distantAlong)||Math.Abs(settings.Geometry.AlongDistance(p.Along,distantAlong))>1500||Math.Abs(p.Across-distantAcross)>1500)
                BuildDistant(p.Along,p.Across);
            Reposition(star);
        }
        internal void Reposition(Vector3d star)
        {
            foreach(var t in tiles.Values)t.Root.transform.position=(Vector3)(star+ConvertVector.Ksp(t.Anchor));
            for(int i=0;i<props.Count;i++)props[i].transform.position=(Vector3)(star+ConvertVector.Ksp(propPositions[i]));
            if(distant!=null)distant.transform.position=(Vector3)(star+ConvertVector.Ksp(distantAnchor));
        }
        internal void Light(double daylight)
        {
            float light=(float)(.18+.82*daylight);
            terrainMaterial.color=new Color(light,light,light);buildingMaterial.color=new Color(.64f*light,.60f*light,.48f*light);
            waterMaterial.color=new Color(.07f*light,.31f*light,.42f*light);
        }
        private Tile Build(long tx,long ty)
        {
            double size=settings.TileSize,x0=tx*size,y0=ty*size;int n=settings.TileResolution;
            var t=new Tile{Root=new GameObject("Ringworld terrain "+tx+","+ty),Anchor=settings.Geometry.Position(x0,y0,0),X=tx,Y=ty};
            t.Root.layer=15;
            int count=(n+1)*(n+1);var vertices=new Vector3[count];var uv=new Vector2[count];var heights=new double[count];
            var waterVerts=new Vector3[count];var wet=new bool[count];
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
            {
                int i=y*(n+1)+x;double a=x0+size*x/n,b=y0+size*y/n;
                var sample=settings.Terrain.Sample(a,b);heights[i]=sample.Height;
                vertices[i]=ConvertVector.Unity(settings.Geometry.Position(a,b,sample.Height)-t.Anchor);
                uv[i]=new Vector2(((int)sample.Biome+.5f)/16,.5f);
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
            t.Root.AddComponent<MeshFilter>().sharedMesh=ground;t.Root.AddComponent<MeshRenderer>().sharedMaterial=terrainMaterial;
            t.Root.AddComponent<MeshCollider>().sharedMesh=ground;
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
            for(int y=0;y<4;y++)for(int x=0;x<4;x++)
            {
                double a=x0+(x+.5)*size/4,b=y0+(y+.5)*size/4;
                var sample=settings.Terrain.Sample(a,b);if(sample.Wet||sample.Biome==Biome.Rimwall||sample.Biome==Biome.Ruins)continue;
                double variation=settings.Terrain.Noise(a,b,75,97);
                if(sample.Biome==Biome.Forest||(sample.Biome==Biome.Grassland&&variation>.75))
                {
                    float h=(float)(8+12*variation);
                    TileProp(t,"Procedural tree trunk",a,b,sample.Height+h/2,new Vector3(1.5f,h/2,1.5f),scrithMaterial,PrimitiveType.Cylinder);
                    TileProp(t,"Procedural tree canopy",a,b,sample.Height+h,new Vector3(h*.7f,h*.7f,h*.7f),leavesMaterial,PrimitiveType.Sphere);
                }
                else if(variation>.78)
                    TileProp(t,"Weathered boulder",a,b,sample.Height+1,new Vector3(4,3,5),scrithMaterial,PrimitiveType.Sphere);
            }
            double ca=x0+size*.5,cb=y0+size*.5;var center=settings.Terrain.Sample(ca,cb);
            if((center.Biome==Biome.Grassland||center.Biome==Biome.Desert)&&settings.Terrain.Noise(ca,cb,50,101)>.97)
            {
                for(int i=-1;i<=1;i++)
                {
                    double a=ca+i*48,h=settings.Terrain.Sample(a,cb).Height;
                    TileProp(t,"Procedural rural habitation",a,cb,h+6,new Vector3(24,12,30),buildingMaterial,PrimitiveType.Cube);
                }
            }
        }
        private static void Add(List<int> a,int x,int y,int z){a.Add(x);a.Add(y);a.Add(z);}
        private void BuildDistant(double along,double across)
        {
            if(distant!=null)UnityEngine.Object.Destroy(distant);if(distantMesh!=null)UnityEngine.Object.Destroy(distantMesh);
            distantAlong=along;distantAcross=across;distantAnchor=settings.Geometry.Position(along,across,0);
            double[] rings={2200,3500,5000,8000,16000,32000,64000,128000};const int sectors=128;
            var vertices=new Vector3[rings.Length*(sectors+1)];var uv=new Vector2[vertices.Length];var indices=new List<int>();
            for(int j=0;j<rings.Length;j++)for(int i=0;i<=sectors;i++)
            {
                int index=j*(sectors+1)+i;double angle=i*2*Math.PI/sectors;
                double a=along+Math.Cos(angle)*rings[j],b=across+Math.Sin(angle)*rings[j];var sample=settings.Terrain.Sample(a,b);
                double h=sample.Wet?sample.WaterHeight:sample.Height;
                // Lower the overlap under the detailed collider tiles; outer rings are visual-only.
                h-=Math.Max(0,1-(rings[j]-2200)/2800)*450;
                vertices[index]=ConvertVector.Unity(settings.Geometry.Position(a,b,h)-distantAnchor);
                uv[index]=new Vector2(((int)(sample.Wet?Biome.Ocean:sample.Biome)+.5f)/16,.5f);
                if(i==sectors||j==rings.Length-1)continue;
                int x=index,y=index+1,z=index+sectors+1,w=z+1;
                Add(indices,x,y,z);Add(indices,y,w,z);
            }
            distantMesh=new Mesh{name="Ringworld distant terrain"};distantMesh.vertices=vertices;distantMesh.uv=uv;distantMesh.SetTriangles(indices,0);distantMesh.RecalculateNormals();distantMesh.RecalculateBounds();
            distant=new GameObject("Ringworld terrain to 128 km");distant.layer=15;distant.AddComponent<MeshFilter>().sharedMesh=distantMesh;distant.AddComponent<MeshRenderer>().sharedMaterial=terrainMaterial;
        }
        private void Prop(string name,double along,double across,double height,Vector3 size,Material mat,PrimitiveType shape=PrimitiveType.Cube)
        {
            var position=settings.Geometry.Position(along,across,height);
            var obj=GameObject.CreatePrimitive(shape);obj.name=name;obj.layer=15;
            obj.transform.rotation=Quaternion.FromToRotation(Vector3.up,ConvertVector.Unity(settings.Geometry.Up(position)));
            obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=mat;
            props.Add(obj);propPositions.Add(position);
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
                double a=l.Along+(x-n/2)*95,b=l.Across+(y-n/2)*95;
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
        private void ClearProps(){foreach(var p in props)UnityEngine.Object.Destroy(p);props.Clear();propPositions.Clear();}
        private static void Destroy(Tile t){UnityEngine.Object.Destroy(t.Root);foreach(var m in t.Meshes)UnityEngine.Object.Destroy(m);}
        public void Dispose()
        {
            foreach(var t in tiles.Values)Destroy(t);tiles.Clear();ClearProps();
            if(distant!=null)UnityEngine.Object.Destroy(distant);if(distantMesh!=null)UnityEngine.Object.Destroy(distantMesh);
            UnityEngine.Object.Destroy(terrainMaterial);UnityEngine.Object.Destroy(waterMaterial);UnityEngine.Object.Destroy(buildingMaterial);UnityEngine.Object.Destroy(scrithMaterial);UnityEngine.Object.Destroy(leavesMaterial);UnityEngine.Object.Destroy(palette);
        }
    }
}
