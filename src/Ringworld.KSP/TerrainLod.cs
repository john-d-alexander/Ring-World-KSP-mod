using System;
using System.Collections.Generic;
using Ringworld.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace NivenRingworld
{
    // CPU quadtree LOD: globally aligned blocks, shared height function, bounded
    // generation per frame, per-block frustum culling and skirts at resolution seams.
    internal sealed class TerrainLod : IDisposable
    {
        private sealed class Patch
        {
            internal GameObject Root;internal Mesh Mesh,WaterMesh;internal DVec Anchor;internal double Phase;internal bool Scaled;internal Texture2D Texture;
        }
        private AssetBundle visualBundle;private bool nightShader;
        private readonly Settings settings;private readonly Material material,farMaterial,waterMaterial;
        private readonly Dictionary<string,Patch> patches=new Dictionary<string,Patch>();
        private HashSet<string> wanted=new HashSet<string>();
        private List<LodBlock> pending=new List<LodBlock>();
        private long lastX=long.MinValue,lastY;
        internal int Count {get{return patches.Count;}}
        internal int Pending {get{return pending.Count;}}
        internal int ScaledCount {get{int count=0;foreach(var p in patches.Values)if(p.Scaled&&p.Root.activeSelf)count++;return count;}}
        internal TerrainLod(Settings s,Material m,Material water){settings=s;material=m;waterMaterial=water;farMaterial=new Material(m);farMaterial.color=Color.black;farMaterial.EnableKeyword("_EMISSION");farMaterial.SetColor("_EmissionColor",Color.white);farMaterial.SetFloat("_Glossiness",0);
            visualBundle=RingVisualAssets.Acquire();var shader=visualBundle!=null?visualBundle.LoadAsset<Shader>("Assets/Shaders/TerrainNight.shader"):null;
            if(shader!=null&&shader.isSupported){farMaterial.shader=shader;nightShader=true;}
        }
        internal void Update(double along,double across)
        {
            long x=(long)Math.Floor(along/settings.TileSize),y=(long)Math.Floor(across/settings.TileSize);
            if(x!=lastX||y!=lastY)
            {
                lastX=x;lastY=y;wanted.Clear();pending.Clear();
                foreach(var b in TerrainLodPlan.Create(along,across,settings.TileSize,settings.TileRadius,settings.LodRange))
                {
                    if(b.Y>=settings.Geometry.P.Width/2||b.Y+b.Size<=-settings.Geometry.P.Width/2)continue;
                    wanted.Add(b.Key);if(!patches.ContainsKey(b.Key))pending.Add(b);
                }
                pending.Sort((a,b)=>a.DistanceSquared(along,across).CompareTo(b.DistanceSquared(along,across)));
            }
            // Publish completed blocks within the frame budget; the coarse hull fills pending areas.
            for(int i=0;i<settings.GenerationBudget&&pending.Count>0;i++)
            {
                var b=pending[0];pending.RemoveAt(0);patches.Add(b.Key,Build(b));
            }
            // Retire obsolete blocks immediately: showing old parents over new children z-fights.
            {
                foreach(var kv in patches)kv.Value.Root.SetActive(wanted.Contains(kv.Key));
                var remove=new List<string>();foreach(var kv in patches)if(!wanted.Contains(kv.Key))remove.Add(kv.Key);
                foreach(var key in remove){Destroy(patches[key]);patches.Remove(key);}
            }
        }
        private Patch Build(LodBlock b)
        {
            int n=settings.LodResolution;int count=(n+1)*(n+1);
            var p=new Patch{Root=new GameObject("Ring terrain LOD "+b.Key),Anchor=settings.Geometry.Position(b.X,b.Y,0),Phase=settings.Geometry.OrientationRadians};
            p.Scaled=b.Size>=65536&&b.DistanceSquared(lastX*settings.TileSize,lastY*settings.TileSize)>250000.0*250000;
            p.Root.layer=p.Scaled?10:15;
            var colors=new Color[count];
            var wet=new bool[count];var waterUv=new Vector2[count];
            var vertices=new List<Vector3>(count+4*(n+1));var uv=new List<Vector2>(vertices.Capacity);var longitude=new List<Vector2>(vertices.Capacity);var triangles=new List<int>();
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
            {
                double a=b.X+b.Size*x/n,c=b.Y+b.Size*y/n;double rawAcross=c;c=Math.Max(-settings.Geometry.P.Width/2,Math.Min(settings.Geometry.P.Width/2,c));var s=settings.Terrain.Sample(a,Math.Max(-settings.Geometry.P.Width/2+.01,Math.Min(settings.Geometry.P.Width/2-.01,c)));
                double h=(s.Wet?s.WaterHeight+.5:s.Height)-.2;
                vertices.Add(ConvertVector.Unity(settings.Geometry.Position(a,c,h)-p.Anchor));
                wet[y*(n+1)+x]=s.Wet;waterUv[y*(n+1)+x]=new Vector2(s.Wet?(float)Math.Max(0,s.WaterHeight-s.Height):0,0);
                uv.Add(new Vector2((x+.5f)/(n+1),(y+.5f)/(n+1)));longitude.Add(new Vector2((float)(a/settings.Geometry.P.Circumference),0));colors[y*(n+1)+x]=TerrainTint.Color(s);
                if(x<n&&y<n&&rawAcross<settings.Geometry.P.Width/2&&rawAcross+b.Size/n>-settings.Geometry.P.Width/2){int i=y*(n+1)+x;triangles.AddRange(new[]{i,i+n+1,i+1,i+1,i+n+1,i+n+2});}
            }
            var f=RingworldFlight.Instance;
            if(!p.Scaled&&(settings.WaterQuality>0||(f!=null&&f.visuals!=null&&f.visuals.PhotoActive)))
            {
                var waterIndices=new List<int>();var groundIndices=new List<int>();
                for(int i=0;i<triangles.Count;i+=3)
                {
                    var target=wet[triangles[i]]&&wet[triangles[i+1]]&&wet[triangles[i+2]]?waterIndices:groundIndices;
                    target.Add(triangles[i]);target.Add(triangles[i+1]);target.Add(triangles[i+2]);
                }
                triangles=groundIndices;
                if(waterIndices.Count>0)
                {
                    p.WaterMesh=new Mesh{name="Ringworld LOD water"};p.WaterMesh.SetVertices(vertices);p.WaterMesh.uv=waterUv;p.WaterMesh.SetTriangles(waterIndices,0);p.WaterMesh.RecalculateNormals();p.WaterMesh.RecalculateBounds();var bounds=p.WaterMesh.bounds;bounds.Expand(4);p.WaterMesh.bounds=bounds;
                    var water=new GameObject("Ringworld LOD waves");water.layer=15;water.transform.SetParent(p.Root.transform,false);water.AddComponent<MeshFilter>().sharedMesh=p.WaterMesh;
                    var wr=water.AddComponent<MeshRenderer>();wr.sharedMaterial=waterMaterial;wr.shadowCastingMode=ShadowCastingMode.Off;
                    // Terrain crack skirts must stay below wave troughs. Their old
                    // mean-water tops appeared as dark lines between water patches.
                    for(int i=0;i<count;i++)if(wet[i])vertices[i]-=ConvertVector.Unity(settings.Geometry.Up(p.Anchor+ConvertVector.Core(vertices[i])))*(float)(settings.WaveHeight+1);
                }
            }
            // Render-only skirts conceal T-junction gaps; physical ground is exclusively
            // the fine streamed collision mesh, never these large-distance triangles.
            for(int edge=0;edge<4;edge++)
            {
                int previous=-1,previousTop=-1;
                for(int k=0;k<=n;k++)
                {
                    int top=edge==0?k:edge==1?k*(n+1)+n:edge==2?n*(n+1)+n-k:(n-k)*(n+1);
                    int bottom=vertices.Count;var point=p.Anchor+ConvertVector.Core(vertices[top]);
                    vertices.Add(vertices[top]-ConvertVector.Unity(settings.Geometry.Up(point))*(float)Math.Max(50,b.Size/n));uv.Add(uv[top]);longitude.Add(longitude[top]);
                    if(k>0)triangles.AddRange(new[]{previousTop,previous,top,top,previous,bottom,top,previous,previousTop,bottom,previous,top});
                    previous=bottom;previousTop=top;
                }
            }
            if(p.Scaled)for(int i=0;i<vertices.Count;i++)vertices[i]*=(float)ScaledSpace.InverseScaleFactor;
            p.Texture=TerrainTint.Texture(n+1,colors);
            p.Mesh=new Mesh{name="Adaptive ring terrain block"};p.Mesh.SetVertices(vertices);p.Mesh.SetUVs(0,uv);p.Mesh.SetUVs(1,longitude);p.Mesh.SetTriangles(triangles,0);p.Mesh.RecalculateNormals();p.Mesh.RecalculateBounds();
            p.Root.AddComponent<MeshFilter>().sharedMesh=p.Mesh;var renderer=p.Root.AddComponent<MeshRenderer>();renderer.sharedMaterial=p.Scaled?farMaterial:material;var block=new MaterialPropertyBlock();block.SetTexture("_MainTex",p.Texture);if(p.Scaled)block.SetTexture("_EmissionMap",p.Texture);renderer.SetPropertyBlock(block);
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return p;
        }
        internal void Reposition(Vector3d star)
        {
            // Skirts are crack covers below the terrain, not exposed structure.
            // From outside the hull the coarse closed underside owns the view.
            bool interior=true;
            if(MapView.MapIsEnabled&&PlanetariumCamera.Camera!=null)
            {
                var camera=ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position)-star;
                interior=settings.Geometry.Coordinates(ConvertVector.Core(camera)).Altitude>=TerrainGenerator.MinimumHeight;
            }
            foreach(var p in patches.Values)
            {
                p.Root.SetActive(interior);
                double delta=settings.Geometry.OrientationRadians-p.Phase;
                var world=star+ConvertVector.Ksp(RingGeometry.Rotate(p.Anchor,delta));
                p.Root.transform.position=p.Scaled?(Vector3)ScaledSpace.LocalToScaledSpace(world):(Vector3)world;
                p.Root.transform.rotation=Quaternion.AngleAxis((float)(delta*180/Math.PI),Vector3.up);
            }
        }
        private static void Destroy(Patch p){UnityEngine.Object.Destroy(p.Root);UnityEngine.Object.Destroy(p.Mesh);if(p.WaterMesh!=null)UnityEngine.Object.Destroy(p.WaterMesh);UnityEngine.Object.Destroy(p.Texture);}
        public void Dispose(){foreach(var p in patches.Values)Destroy(p);patches.Clear();UnityEngine.Object.Destroy(farMaterial);if(visualBundle!=null)RingVisualAssets.Release();}
        internal void Light(float light){if(!nightShader)farMaterial.SetColor("_EmissionColor",new Color(light,light,light));}
    }
}
