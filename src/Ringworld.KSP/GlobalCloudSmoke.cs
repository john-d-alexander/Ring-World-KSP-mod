#if RINGWORLD_SMOKE_TEST
using System;
using System.IO;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class GlobalCloudSmoke
    {
        internal static void Run()
        {
            var s=Settings.Load();s.CloudAmount=.8;s.DynamicWeather=false;s.FullRingDetail=false;
            var parent=new GameObject("Global cloud validation");var bundle=RingVisualAssets.Acquire();
            var clouds=new GlobalClouds(parent.transform,s,bundle);clouds.Update(s,null,null,0);
            var obj=GameObject.Find("Ringworld global cloud shell");if(obj==null)throw new Exception("Global cloud renderer missing");
            var mesh=obj.GetComponent<MeshFilter>().sharedMesh;var material=obj.GetComponent<MeshRenderer>().sharedMaterial;
            if(mesh.vertexCount!=65536||!material.shader.isSupported)throw new Exception("Invalid cloud shell mesh/shader");
            var field=mesh.uv2;
            for(int i=0;i<16384;i++)
            {
                float error=Mathf.Repeat(field[i*4+2].x-field[((i+1)%16384)*4].x+.5f,1)-.5f;
                if(Mathf.Abs(error)>.00002f)throw new Exception("Cloud field seam at segment "+i+": "+error);
            }
            var cameraObj=new GameObject("Cloud validation camera");var camera=cameraObj.AddComponent<Camera>();camera.enabled=false;
            double along=s.Geometry.P.Circumference*.0123;
            cameraObj.transform.position=ConvertVector.Unity(s.Geometry.Position(along,0,1000000)*ScaledSpace.InverseScaleFactor);
            cameraObj.transform.LookAt(ConvertVector.Unity(s.Geometry.Position(along,0,5500)*ScaledSpace.InverseScaleFactor),Vector3.up);
            camera.orthographic=true;camera.orthographicSize=(float)(180000*ScaledSpace.InverseScaleFactor);camera.nearClipPlane=.01f;camera.farClipPlane=1000;camera.cullingMask=1<<10;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            var target=new RenderTexture(900,600,24);camera.targetTexture=target;
            string output=Path.GetFullPath(Path.Combine(KSPUtil.ApplicationRootPath,"../artifacts/validation/global-clouds"));Directory.CreateDirectory(output);
            float dayPhase=(float)RingGeometry.Wrap(20*.0123-.5,1);material.SetFloat("_DayPhase",dayPhase);
            double day=Capture(camera,target,Path.Combine(output,"day.png"));
            material.SetFloat("_DayPhase",(float)(20*.0123));double night=Capture(camera,target,Path.Combine(output,"night.png"));
            material.SetFloat("_DayPhase",dayPhase);material.SetVector("_Local",new Vector4(.0123f,.5f,5500,1));material.SetFloat("_LocalAmount",(float)s.Weather(along,0,0).Cloud);material.SetVector("_CloudHandoff",RingCloudField.Handoff(s,false));
            double span=s.Geometry.P.Circumference/16384,sector=Math.Floor(along/span);material.SetFloat("_SegmentLength",(float)span);material.SetVector("_LocalChart",new Vector4((float)sector,(float)(along-sector*span),0,5500));
            double transition=Capture(camera,target,Path.Combine(output,"handoff.png"));
            if(day<.01||night>day*.4||transition>=day*.98)throw new Exception("Cloud shading/handoff failed: "+day+" / "+night+" / "+transition);
            s.CloudAmount=0;clouds.Update(s,null,null,0);if(obj.activeSelf)throw new Exception("Cloud off setting ignored");
            Debug.Log("[RingworldSmoke] PASS global-clouds-only: 16384 continuous UV segments; day="+day+" night="+night+" handoff="+transition+"; low-detail and off settings OK");
            camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(cameraObj);clouds.Dispose();UnityEngine.Object.Destroy(parent);RingVisualAssets.Release();
        }
        private static double Capture(Camera camera,RenderTexture target,string path)
        {
            camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();RenderTexture.active=old;
            double sum=0;foreach(var color in pixels.GetPixels32())sum+=(color.r+color.g+color.b)/(255.0*3*target.width*target.height);
            File.WriteAllBytes(path,pixels.EncodeToPNG());UnityEngine.Object.Destroy(pixels);return sum;
        }
    }
}
#endif
