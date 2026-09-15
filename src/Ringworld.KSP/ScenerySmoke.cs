#if RINGWORLD_SMOKE_TEST
using System;
using System.IO;
using UnityEngine;

namespace NivenRingworld
{
    internal static class ScenerySmoke
    {
        internal static void Run()
        {
            SceneryAssets.Acquire();
            var root=new GameObject("Scenery import verification");root.transform.position=new Vector3(20000,20000,20000);
            string[] kinds={"tree_broadleaf","tree_conifer","boulder","rural_building"};
            var groups=new System.Collections.Generic.List<LODGroup>();
            for(int i=0;i<8;i++)
            {
                var obj=SceneryAssets.Create(kinds[i/2],i%2);
                if(obj==null)throw new Exception("Missing scenery "+kinds[i/2]+" variant "+i%2);
                obj.transform.SetParent(root.transform,false);
                var lod=obj.GetComponent<LODGroup>();
                if(lod==null||lod.GetLODs().Length!=3)throw new Exception("Missing three LODs on "+obj.name);
                var levels=lod.GetLODs();
                for(int n=0;n<3;n++)
                {
                    if(levels[n].renderers.Length!=1)throw new Exception("Expected single renderer per LOD on "+obj.name);
                    var renderer=levels[n].renderers[0];var material=renderer.sharedMaterial;
                    if(material==null||material.mainTexture==null||material.shader==null||!material.shader.isSupported)throw new Exception("Unsupported scenery material on "+obj.name);
                    if(renderer.bounds.size.y<.75f||renderer.bounds.size.y>1.1f)throw new Exception("Wrong metre scale / up axis on "+obj.name);
                }
                if(obj.GetComponentsInChildren<Rigidbody>().Length!=0||obj.GetComponentsInChildren<Collider>().Length==0)throw new Exception("Invalid static contacts on "+obj.name);
                if(i<4&&Math.Abs(levels[0].renderers[0].bounds.min.y-root.transform.position.y)>.003)throw new Exception("Tree pivot is not on the ground");
                float size=i<4?6.1f:i<6?2f:3.5f;
                obj.transform.localPosition=new Vector3((1.5f-i%4)*5.2f,i<4?0:size*.5f,i<4?-2.8f:3.8f);
                obj.transform.localScale=Vector3.one*size;
                lod.ForceLOD(0);groups.Add(lod);
                Debug.Log("[RingworldSmoke] SCENERY "+obj.name+" 3 LODs, atlas, metre scale, contacts OK");
            }
            Physics.SyncTransforms();
            foreach(var group in groups)
            {
                RaycastHit hit;var start=group.transform.position+Vector3.up*15;
                if(!Physics.Raycast(start,Vector3.down,out hit,30,1<<15)||!hit.collider.transform.IsChildOf(group.transform))throw new Exception("Contact proxy ray missed "+group.name);
            }
            var cameraObj=new GameObject("Scenery review camera");var camera=cameraObj.AddComponent<Camera>();
            cameraObj.transform.position=root.transform.position+new Vector3(-15,19,24);
            cameraObj.transform.LookAt(root.transform.position+Vector3.up*2);camera.orthographic=true;camera.orthographicSize=9.1f;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=1<<15;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.14f,.18f,.22f);
            // Main-menu lights otherwise add uncontrolled exposure to this isolated review.
            foreach(var existing in UnityEngine.Object.FindObjectsOfType<Light>())existing.enabled=false;
            var lightObj=new GameObject("Scenery review sunlight");var light=lightObj.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1f;light.cullingMask=1<<15;lightObj.transform.rotation=Quaternion.Euler(48,-35,0);
            RenderSettings.ambientLight=new Color(.38f,.38f,.38f);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            var target=new RenderTexture(1500,1050,24);camera.targetTexture=target;
            string output=Path.GetFullPath(Path.Combine(KSPUtil.ApplicationRootPath,"../art/first-set/validation"));Directory.CreateDirectory(output);
            for(int level=0;level<3;level++)
            {
                foreach(var group in groups)group.ForceLOD(level);
                camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
                var image=new Texture2D(1500,1050,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1500,1050),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(output,"ksp-lod"+level+".png"),image.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(image);
            }
            camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(cameraObj);UnityEngine.Object.Destroy(lightObj);UnityEngine.Object.Destroy(root);
            SceneryAssets.Release();
            Debug.Log("[RingworldSmoke] PASS scenery-only: 8 prefabs / 24 LOD meshes / materials / pivots / collision rays / runtime renders");
        }
    }
}
#endif
