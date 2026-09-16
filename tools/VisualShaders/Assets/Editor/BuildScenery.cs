using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BuildScenery
{
    [Serializable] public class Trunk {public float x,y,z,height,radius;}
    [Serializable] public class Entry { public string id,kind,collider; public Trunk[] trunks; }
    [Serializable] public class Catalog { public Entry[] assets; }
    public static void Run()
    {
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
        string source=Path.Combine(root,"art/first-set");
        const string dest="Assets/Scenery";
        Directory.CreateDirectory(dest);
        var entries=new List<Entry>();
        foreach(string directory in new[]{source,Path.Combine(root,"art/habitat-kit"),Path.Combine(root,"art/city-kit"),Path.Combine(root,"art/landmark-kit"),Path.Combine(root,"art/colossus-kit")})
        {
            if(!File.Exists(Path.Combine(directory,"manifest.json")))continue;
            entries.AddRange(JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(directory,"manifest.json"))).assets);
            foreach(string file in Directory.GetFiles(Path.Combine(directory,"exports")))
                if(file.EndsWith(".fbx")||file.EndsWith(".png"))File.Copy(file,Path.Combine(dest,Path.GetFileName(file)),true);
        }
        AssetDatabase.Refresh();
        var textureImporter=(TextureImporter)AssetImporter.GetAtPath(dest+"/SceneryAtlas.png");
        textureImporter.maxTextureSize=512;textureImporter.mipmapEnabled=true;textureImporter.wrapMode=TextureWrapMode.Clamp;
        textureImporter.textureCompression=TextureImporterCompression.Compressed;textureImporter.anisoLevel=2;textureImporter.SaveAndReimport();
        string matPath=dest+"/Scenery.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,matPath);}
        material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dest+"/SceneryAtlas.png");material.SetFloat("_Glossiness",.12f);material.enableInstancing=true;
        var catalog=new Catalog{assets=entries.ToArray()};
        var paths=new List<string>();var report=new List<string>();
        foreach(var entry in catalog.assets)
        {
            string fbx=dest+"/"+entry.id+".fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(fbx);
            importer.importMaterials=false;importer.importAnimation=false;importer.globalScale=1;importer.useFileScale=true;
            importer.meshCompression=ModelImporterMeshCompression.Off;importer.isReadable=entry.collider=="none";importer.addCollider=false;importer.SaveAndReimport();
            var imported=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(fbx));
            var prefab=new GameObject(entry.id);imported.transform.SetParent(prefab.transform,false);
            foreach(var previous in imported.GetComponentsInChildren<LODGroup>())UnityEngine.Object.DestroyImmediate(previous);
            var renderers=imported.GetComponentsInChildren<MeshRenderer>();
            var lods=new LOD[3];
            for(int i=0;i<3;i++)
            {
                var group=renderers.Where(r=>r.name.EndsWith("_LOD"+i,StringComparison.Ordinal)).ToArray();
                if(group.Length!=1)throw new Exception(entry.id+" needs one merged mesh for LOD "+i);
                foreach(var renderer in group){renderer.sharedMaterial=material;renderer.gameObject.layer=15;}
                var bounds=group[0].bounds;
                if(bounds.size.y<(i==0?.75f:.15f)||bounds.size.y>1.25f)throw new Exception(entry.id+" invalid Unity metre scale/up-axis: "+bounds);
                if(entry.kind.StartsWith("tree_")&&Mathf.Abs(bounds.min.y)>.001f)throw new Exception(entry.id+" invalid ground pivot: "+bounds);
                int triangles=group[0].GetComponent<MeshFilter>().sharedMesh.triangles.Length/3;
                report.Add(entry.id+" LOD"+i+" triangles="+triangles+" bounds="+bounds);
                lods[i]=new LOD(entry.collider=="surface"?new[]{.16f,.045f,.001f}[i]:new[]{.12f,.035f,.003f}[i],group);
            }
            var lodGroup=prefab.AddComponent<LODGroup>();lodGroup.SetLODs(lods);lodGroup.RecalculateBounds();
            // Simple contact shapes, independent of visual LOD. No leaf colliders.
            if(entry.trunks!=null)foreach(var trunk in entry.trunks)
            {
                // Unity's FBX import also mirrors X for its handedness conversion.
                var contact=prefab.AddComponent<CapsuleCollider>();contact.direction=1;contact.center=new Vector3(-trunk.x,trunk.y,trunk.z);contact.height=trunk.height;contact.radius=trunk.radius;
                var visual=renderers.Single(r=>r.name.EndsWith("_LOD0"));var trunkMesh=visual.GetComponent<MeshFilter>().sharedMesh;
                var vertices=trunkMesh.vertices;var uv=trunkMesh.uv;bool aligned=false;
                for(int k=0;k<vertices.Length;k++)
                {
                    if(uv[k].x>=.25f||uv[k].y>=.25f)continue; // shared atlas bark tile
                    var point=prefab.transform.InverseTransformPoint(visual.transform.TransformPoint(vertices[k]));
                    var offset=point-contact.center;
                    if(new Vector2(offset.x,offset.z).magnitude<contact.radius*1.6f&&Mathf.Abs(offset.y)<contact.height)aligned=true;
                }
                if(!aligned)throw new Exception(entry.id+" trunk collider does not align with FBX bark mesh: "+contact.center);
            }
            if(entry.collider=="trunk")
            {
                var collider=prefab.AddComponent<CapsuleCollider>();collider.direction=1;collider.center=new Vector3(0,.3f,0);collider.height=.6f;collider.radius=.035f;
            }
            else if(entry.collider=="house")
            {
                var collider=prefab.AddComponent<BoxCollider>();collider.center=new Vector3(0,-.13f,0);collider.size=new Vector3(.8f,.66f,.72f);
                // Convex triangular roof, twelve triangles, no detailed roof contact mesh.
                var roof=new GameObject("Roof contact proxy");roof.transform.SetParent(prefab.transform,false);
                var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.5f,.18f,-.46f),new Vector3(.5f,.18f,-.46f),new Vector3(0,.5f,-.46f),new Vector3(-.5f,.18f,.46f),new Vector3(.5f,.18f,.46f),new Vector3(0,.5f,.46f)};
                mesh.triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,2,5,4,2,4,1,0,1,4,0,4,3};mesh.RecalculateNormals();
                string meshPath=dest+"/"+entry.id+"-collider.asset";AssetDatabase.DeleteAsset(meshPath);AssetDatabase.CreateAsset(mesh,meshPath);
                var contact=roof.AddComponent<MeshCollider>();contact.sharedMesh=mesh;contact.convex=true;
            }
            else if(entry.collider=="box")
            {
                var contact=prefab.AddComponent<BoxCollider>();contact.size=Vector3.one;
            }
            else if(entry.collider!="none")
            {
                var contact=prefab.AddComponent<MeshCollider>();contact.sharedMesh=renderers.Single(r=>r.name.EndsWith("_LOD2")).GetComponent<MeshFilter>().sharedMesh;contact.convex=entry.collider!="surface";
                // FBX applies axis conversion to child transforms; bake the proxy into root space.
                var renderer=renderers.Single(r=>r.name.EndsWith("_LOD2"));var sourceMesh=contact.sharedMesh;
                var proxy=UnityEngine.Object.Instantiate(sourceMesh);proxy.vertices=sourceMesh.vertices.Select(v=>prefab.transform.InverseTransformPoint(renderer.transform.TransformPoint(v))).ToArray();proxy.RecalculateBounds();
                string meshPath=dest+"/"+entry.id+"-collider.asset";AssetDatabase.DeleteAsset(meshPath);AssetDatabase.CreateAsset(proxy,meshPath);contact.sharedMesh=proxy;
            }
            foreach(var t in prefab.GetComponentsInChildren<Transform>())t.gameObject.layer=15;
            string path=dest+"/"+entry.id+".prefab";PrefabUtility.SaveAsPrefabAsset(prefab,path);paths.Add(path);
            UnityEngine.Object.DestroyImmediate(prefab);
        }
        AssetDatabase.SaveAssets();
        // Separate output directory: don't replace the visual bundle's build manifest.
        string output=Path.Combine(root,"art/first-set/bundle");Directory.CreateDirectory(output);
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
        var result=BuildPipeline.BuildAssetBundles(output,new[]{new AssetBundleBuild{assetBundleName="ringworldscenery",assetNames=paths.ToArray()}},BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64);
        if(result==null)throw new Exception("Scenery bundle failed");
        File.Copy(Path.Combine(output,"ringworldscenery"),Path.Combine(root,"GameData/NivenRingworld/Assets/ringworldscenery"),true);
        File.WriteAllText(Path.Combine(root,"GameData/NivenRingworld/Scenery.cfg"),string.Join("\n",entries.Select(e=>"RINGWORLD_BUNDLED_SCENERY_ASSET\n{\n    kind = "+e.kind+"\n    prefab = "+e.id+"\n}\n").ToArray()));
        File.WriteAllLines(Path.Combine(source,"validation/unity-import.txt"),report);
        Debug.Log("RINGWORLD SCENERY BUNDLE BUILT: "+paths.Count+" prefabs");
    }
}
