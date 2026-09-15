using System;
using System.Collections.Generic;
using UnityEngine;
namespace NivenRingworld
{
    // A bounded, collision-free supplement to the inexpensive primitive ground cover.
    internal sealed class BatchedScenery : IDisposable
    {
        private sealed class Shape {internal Vector3[] Vertices,Normals;internal Vector2[] Uv;internal int[] Triangles;internal Material Material;}
        private readonly Dictionary<string,Shape> shapes=new Dictionary<string,Shape>();
        private readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();private readonly List<Vector2> uv=new List<Vector2>();private readonly List<int> triangles=new List<int>();
        private readonly GameObject root;private readonly Mesh mesh;private Material material;
        internal int Count {get;private set;}
        internal BatchedScenery(Transform parent)
        {
            root=new GameObject("Batched Blender ground details");root.layer=15;root.transform.SetParent(parent,false);mesh=new Mesh{name="Shared-atlas close assets"};root.AddComponent<MeshFilter>().sharedMesh=mesh;
            root.AddComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        internal void Reset(){Count=0;vertices.Clear();normals.Clear();uv.Clear();triangles.Clear();}
        internal bool Add(string kind,int lod,Vector3 at,Vector3 up,Vector3 size,float yaw,int limit)
        {
            if(Count>=limit)return false;
            string key=kind+lod;Shape shape;
            if(!shapes.TryGetValue(key,out shape))
            {
                var prefab=SceneryAssets.DetailPrefab(kind);if(prefab==null)return false;
                var group=prefab.GetComponent<LODGroup>();if(group==null)return false;
                var renderer=group.GetLODs()[lod].renderers[0];var filter=renderer.GetComponent<MeshFilter>();var source=filter.sharedMesh;if(!source.isReadable)return false;
                var matrix=prefab.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                shape=new Shape{Vertices=source.vertices,Normals=source.normals,Uv=source.uv,Triangles=source.triangles,Material=renderer.sharedMaterial};
                for(int i=0;i<shape.Vertices.Length;i++){shape.Vertices[i]=matrix.MultiplyPoint3x4(shape.Vertices[i]);shape.Normals[i]=matrix.MultiplyVector(shape.Normals[i]).normalized;}
                shapes[key]=shape;
            }
            if(vertices.Count+shape.Vertices.Length>60000)return false;
            var rotation=Quaternion.AngleAxis(yaw,up)*Quaternion.FromToRotation(Vector3.up,up);
            var transform=Matrix4x4.TRS(at+up*size.y*.5f,rotation,size);var normalTransform=transform.inverse.transpose;
            int offset=vertices.Count;
            for(int i=0;i<shape.Vertices.Length;i++){vertices.Add(transform.MultiplyPoint3x4(shape.Vertices[i]));normals.Add(normalTransform.MultiplyVector(shape.Normals[i]).normalized);uv.Add(shape.Uv[i]);}
            foreach(int i in shape.Triangles)triangles.Add(offset+i);
            if(material==null){material=new Material(shape.Material);root.GetComponent<MeshRenderer>().sharedMaterial=material;}
            Count++;return true;
        }
        internal void Finish(){mesh.Clear();mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();root.SetActive(Count>0);}
        public void Dispose(){UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(mesh);if(material!=null)UnityEngine.Object.Destroy(material);shapes.Clear();}
    }
}
