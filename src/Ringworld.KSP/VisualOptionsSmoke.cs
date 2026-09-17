#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using HarmonyLib;
using UnityEngine;
namespace NivenRingworld
{
    internal static class VisualOptionsSmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            for(int i=0;i<RingQualityPresets.Names.Length;i++)
            {
                var n=f.Settings.Save();RingQualityPresets.Apply(n,i);var check=Settings.Load();check.Apply(n);
                if(RingQualityPresets.Match(check)!=RingQualityPresets.Names[i]){fail("Expanded preset roundtrip "+i);yield break;}
                if(i==0&&(check.WaterQuality!=4||check.Cyla["ViewSteps"]!=500)){fail("Cow maximum optics/water");yield break;}
                if(i==10&&(check.WaterQuality!=0||check.Cyla["ViewSteps"]!=1)){fail("Rotten minimum optics/water");yield break;}
            }
            var custom=f.Settings.Save();foreach(var d in CylaOptions.Definitions)custom.SetValue(d.Key,((d.Min+d.Max)/2).ToString("R",System.Globalization.CultureInfo.InvariantCulture),true);
            custom.SetValue("cylaLightingMode",2,true);var first=Settings.Load();first.Apply(custom);var second=Settings.Load();second.Apply(first.Save());
            foreach(var d in CylaOptions.Definitions)if(first.Save().GetValue(d.Key)!=second.Save().GetValue(d.Key)){fail("Cyla option roundtrip "+d.Key);yield break;}
            Debug.Log("[RingworldSmoke] VISUAL OPTIONS all 11 presets and 21 Cyla optical values roundtrip; highest tiers checked without rendering");
            f.arrivalHeight=80;f.Visit();while(!f.Ready)yield return null;for(int i=0;i<30;i++)yield return null;
            var surface=(SurfaceStreamer)AccessTools.Field(typeof(RingworldFlight),"surface").GetValue(f);
            var coord=f.Settings.Geometry.Coordinates(f.Position(FlightGlobals.ActiveVessel));bool wet=false;
            for(int i=0;i<20000&&!wet;i++)
            {
                double a=coord.Along+(i%200-100)*200,b=coord.Across+(i/200-50)*200;var t=f.Settings.Terrain.Sample(a,b);
                if(t.Wet&&t.WaterHeight-surface.CameraFloor(a,b)>2){wet=true;if(surface.CameraFloor(a,b)!=surface.CollisionHeight(a,b)){fail("Camera still blocked by water");yield break;}}
            }
            if(!wet){fail("Wet camera-floor fixture missing");yield break;}
            if(f.visuals.WaterShader()==null||!f.visuals.WaterShader().isSupported){fail("Water shader unsupported");yield break;}
            try{CheckWater(f.visuals.WaterShader());}catch(Exception e){fail("Water transparency: "+e.Message);yield break;}
            Debug.Log("[RingworldSmoke] WATER compiled shader supported; camera floor follows submerged ground");
            double ut=Planetarium.GetUniversalTime();
            foreach(int edge in new[]{512,1536})
            {
                if(!f.visuals.BeginPhoto(1,6,edge)){fail("Photo entry "+f.visuals.Status);yield break;}
                var clock=System.Diagnostics.Stopwatch.StartNew();while(f.visuals.PhotoActive&&!f.visuals.PhotoFinished&&clock.Elapsed.TotalSeconds<180)yield return null;
                if(!f.visuals.PhotoFinished){fail("Photo did not finish: "+f.visuals.Status);yield break;}
                var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(f.visuals.LastPhoto));
                int width=image.width,height=image.height;var pixels=image.GetPixels32();int different=0;var baseColor=pixels[0];foreach(var c in pixels)if(c.r!=baseColor.r||c.g!=baseColor.g||c.b!=baseColor.b)different++;
                UnityEngine.Object.Destroy(image);
                if(Math.Max(width,height)!=edge||Math.Abs((double)width/height-(double)Screen.width/Screen.height)>.01||different<pixels.Length/10){fail("Photo size/aspect/content invalid "+width+"x"+height);yield break;}
                File.Copy(f.visuals.LastPhoto,Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-output-"+edge+".png"),true);
                Debug.Log("[RingworldSmoke] PHOTO output render "+width+"x"+height+" with Slow preset; file="+f.visuals.LastPhoto);
                f.visuals.EndPhoto();yield return null;
                if(Time.timeScale!=1||RingQualityPresets.Match(f.Settings)!="Slow"||InputLockManager.GetControlLock("NivenRingworld.Photo")!=ControlTypes.None){fail("Photo restoration failed");yield break;}
            }
            if(!f.visuals.BeginPhoto(1,6,512)){fail("Photo cancellation entry");yield break;}f.visuals.EndPhoto();yield return null;
            if(f.visuals.PhotoActive||Time.timeScale!=1){fail("Photo cancellation failed");yield break;}
            Debug.Log("[RingworldSmoke] PASS visual-options-only");
        }
        private static void CheckWater(Shader shader)
        {
            var cameraObject=new GameObject("Water transparency probe");var camera=cameraObject.AddComponent<Camera>();
            var water=GameObject.CreatePrimitive(PrimitiveType.Quad);var backing=GameObject.CreatePrimitive(PrimitiveType.Quad);
            var material=new Material(shader);var backMaterial=new Material(Shader.Find("Unlit/Color"));
            var target=new RenderTexture(64,64,24);var pixels=new Texture2D(64,64,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try
            {
                camera.enabled=false;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                cameraObject.transform.position=new Vector3(0,0,-5);camera.orthographic=true;camera.orthographicSize=.4f;camera.targetTexture=target;
                water.layer=30;backing.layer=30;backing.transform.position=Vector3.forward;water.GetComponent<Renderer>().sharedMaterial=material;backing.GetComponent<Renderer>().sharedMaterial=backMaterial;
                material.SetVector("_WaveUp",Vector3.back);material.SetVector("_WaveAlong",Vector3.right);material.SetVector("_WaveAcross",Vector3.up);material.SetFloat("_WaterLight",1);
                foreach(int quality in new[]{0,1})
                {
                    material.SetFloat("_WaterQuality",quality);backMaterial.color=Color.red;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,64,64),0,0);pixels.Apply();var red=pixels.GetPixel(32,32);
                    backMaterial.color=Color.blue;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,64,64),0,0);pixels.Apply();var blue=pixels.GetPixel(32,32);
                    if(red.r-blue.r<.1f||blue.b-red.b<.1f)throw new Exception("Underlying surface hidden at quality "+quality+": "+red+" / "+blue);
                    File.WriteAllBytes(Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-water-probe-"+quality+".png"),pixels.EncodeToPNG());
                    Debug.Log("[RingworldSmoke] WATER transparency GPU probe quality="+quality+" red="+red+" blue="+blue);
                }
            }
            finally{RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);UnityEngine.Object.Destroy(water);UnityEngine.Object.Destroy(backing);UnityEngine.Object.Destroy(material);UnityEngine.Object.Destroy(backMaterial);UnityEngine.Object.Destroy(cameraObject);}
        }
    }
}
#endif
