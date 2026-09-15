#if RINGWORLD_SMOKE_TEST
using System;
using System.IO;
using UnityEngine;
namespace NivenRingworld
{
    internal static class LibrarySmoke
    {
        internal static void Run()
        {
            SceneryAssets.Acquire();var root=new GameObject("Habitat library test");root.transform.position=new Vector3(40000,40000,40000);
            var cameraObj=new GameObject("Library test camera");var camera=cameraObj.AddComponent<Camera>();camera.enabled=false;camera.transform.position=root.transform.position+new Vector3(-11,15,20);camera.transform.LookAt(root.transform.position+Vector3.up);
            camera.orthographic=true;camera.orthographicSize=8;camera.nearClipPlane=.1f;camera.farClipPlane=80;camera.cullingMask=1<<15;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.20f,.23f);
            var lamp=new GameObject("Library test light");var light=lamp.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1;light.cullingMask=1<<15;lamp.transform.rotation=Quaternion.Euler(50,145,0);
            var target=new RenderTexture(1400,1000,24);camera.targetTexture=target;string output=Path.GetFullPath(Path.Combine(KSPUtil.ApplicationRootPath,"../art/habitat-kit/validation"));Directory.CreateDirectory(output);
            int count=0,page=0;var active=new System.Collections.Generic.List<GameObject>();
            foreach(var entry in SceneryAssets.Catalog)
            {
                if(entry.Key.StartsWith("tree_")||entry.Key=="boulder"||entry.Key=="rural_building")continue;
                var obj=SceneryAssets.Create(entry.Key,0);if(obj==null)throw new Exception("Missing "+entry.Key);obj.transform.SetParent(root.transform,false);
                var lod=obj.GetComponent<LODGroup>();if(lod==null||lod.GetLODs().Length!=3)throw new Exception("LOD layout "+entry.Key);
                foreach(var level in lod.GetLODs())
                {
                    var renderer=level.renderers[0];if(!renderer.sharedMaterial.shader.isSupported||renderer.sharedMaterial.mainTexture==null)throw new Exception("Material "+entry.Key);
                }
                var bounds=lod.GetLODs()[0].renderers[0].bounds;if(Math.Abs(bounds.size.y-1)>.02)throw new Exception("Unit scale "+entry.Key+" "+bounds);
                if(obj.GetComponentsInChildren<Rigidbody>().Length>0)throw new Exception("Unexpected rigidbody "+entry.Key);
                int index=count%8;obj.transform.localScale=Vector3.one*2;obj.transform.localPosition=new Vector3((1.5f-index%4)*3.2f,1,index<4?-2.4f:2.4f);lod.ForceLOD(0);active.Add(obj);count++;
                Debug.Log("[RingworldSmoke] LIBRARY "+entry.Key+" 3 LODs, unit scale, material OK");
                if(active.Count==8){Capture(camera,target,Path.Combine(output,"ksp-library-"+(page++)+".png"));foreach(var item in active){item.SetActive(false);UnityEngine.Object.Destroy(item);}active.Clear();}
            }
            if(active.Count>0)Capture(camera,target,Path.Combine(output,"ksp-library-"+page+".png"));
            if(count!=40)throw new Exception("Expected 40 library assets, got "+count);
            var batch=new BatchedScenery(root.transform);batch.Reset();
            foreach(string kind in new[]{"grass_patch","reed_patch","fern_patch","mirror_sunflower","leaf_litter","pebble_cluster","desert_scrub","mushrooms"})
                if(!batch.Add(kind,1,Vector3.zero,Vector3.up,Vector3.one,0,32))throw new Exception("Ground batching "+kind);
            batch.Finish();if(batch.Count!=8)throw new Exception("Ground batch count");batch.Dispose();
            Debug.Log("[RingworldSmoke] PASS habitat-library: 40 assets / 120 LOD meshes / eight ground batch adapters");
            camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(cameraObj);UnityEngine.Object.Destroy(lamp);UnityEngine.Object.Destroy(root);SceneryAssets.Release();
        }
        private static void Capture(Camera camera,RenderTexture target,string path)
        {
            camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);RenderTexture.active=old;
        }
    }
}
#endif
