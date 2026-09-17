#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace NivenRingworld
{
    internal static class CylaSmoke
    {
        private static Color centre;
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            for(int i=0;i<RingQualityPresets.Names.Length;i++)
            {
                var n=f.Settings.Save();RingQualityPresets.Apply(n,i);var check=Settings.Load();check.Apply(n);
                if(RingQualityPresets.Match(check)!=RingQualityPresets.Names[i]||check.AtmosphereBackend!=(i<=5?1:0)){fail("Preset roundtrip/backend: "+i);yield break;}
                if(i==0&&(check.CylaLightSteps!=50||check.LodResolution!=32||check.GenerationBudget!=4||check.PhotoSamples!=64||check.CloudShadow!=1||check.LodRange<check.Geometry.P.Circumference/2)){fail("Maximum preset incomplete");yield break;}
                if(i==10&&(check.LodRange!=200000||check.ForestQuality!=0||check.VisualQuality!=0||check.CloudShadow!=0||check.AmbientParticles||check.RainEnabled||check.LightningEnabled||check.FullRingDetail)){fail("Minimum preset incomplete");yield break;}
            }
            Debug.Log("[RingworldSmoke] QUALITY eleven preset roundtrips, maximum/minimum endpoints passed");
            f.arrivalHeight=100;f.Visit();while(!f.Ready)yield return null;
            for(int i=0;i<45;i++)yield return null;
            Time.timeScale=0;f.Settings.CloudAmount=0;f.Settings.VisualQuality=1;f.Settings.AtmosphereSteps=32;f.Settings.CylaLightSteps=4;
            f.Settings.CylaDither=false;f.Settings.AtmosphereBackend=0;
            for(int i=0;i<4;i++)yield return null;
            Capture("Cyla-original.png");
            f.Settings.AtmosphereBackend=1;for(int i=0;i<6;i++)yield return null;
            if(!f.visuals.CylaActive){fail("Cyla backend unavailable: "+f.visuals.Status);yield break;}
            Debug.Log("[RingworldSmoke] CYLA active: "+f.visuals.Status);
            if(Capture("Cyla-compiled.png")<.04){fail("Cyla daylight sky is black");yield break;}
            foreach(var camera in Camera.allCameras)
                foreach(var buffer in camera.GetCommandBuffers(UnityEngine.Rendering.CameraEvent.BeforeImageEffects))
                    if(buffer.name=="Niven Ringworld / Cyla scattering"&&camera!=FlightCamera.fetch.mainCamera){fail("Cyla attached to a non-flight camera");yield break;}
            Debug.Log("[RingworldSmoke] CYLA camera isolation passed");
            var testCamera=FlightCamera.fetch.mainCamera;var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);marker.name="Cyla foreground depth probe";marker.layer=15;marker.GetComponent<Collider>().enabled=false;
            marker.transform.position=testCamera.transform.position+testCamera.transform.forward*10;marker.transform.localScale=Vector3.one*3;
            var markerMaterial=new Material(Shader.Find("Unlit/Color"));markerMaterial.color=Color.magenta;marker.GetComponent<Renderer>().sharedMaterial=markerMaterial;
            yield return null;Capture("Cyla-depth.png");UnityEngine.Object.Destroy(marker);UnityEngine.Object.Destroy(markerMaterial);
            if(centre.r<.7||centre.b<.7||centre.g>.2){fail("Cyla foreground depth probe changed colour: "+centre);yield break;}
            Debug.Log("[RingworldSmoke] CYLA foreground depth probe passed "+centre);
            MapView.EnterMapView();yield return null;yield return null;
            if(f.visuals.CylaActive){fail("Cyla leaked into map camera");yield break;}
            MapView.ExitMapView();for(int i=0;i<4;i++)yield return null;
            if(!f.visuals.CylaActive){fail("Cyla failed to return from map");yield break;}

            var benchmark=System.Diagnostics.Stopwatch.StartNew();int frames=0;
            foreach(var quality in new[]{new[]{0,16,1,4},new[]{1,16,1,8},new[]{1,16,1,4},new[]{1,32,4,2},new[]{1,64,8,1}})
            {
                f.Settings.AtmosphereBackend=quality[0];f.Settings.AtmosphereSteps=quality[1];f.Settings.CylaLightSteps=quality[2];f.Settings.CylaDivisor=quality[3];
                for(int i=0;i<10;i++)yield return null;benchmark.Restart();frames=0;
                while(benchmark.Elapsed.TotalSeconds<5){frames++;yield return null;}
                Debug.Log("[RingworldSmoke] CYLA rendering-only benchmark frozen physics / "+Screen.width+"x"+Screen.height+" backend="+quality[0]+" view="+quality[1]+" light="+quality[2]+" divisor="+quality[3]+" fps="+(frames/benchmark.Elapsed.TotalSeconds).ToString("F1"));
            }
            f.Settings.AtmosphereSteps=32;f.Settings.CylaLightSteps=4;
            f.Settings.AtmosphereBackend=0;for(int i=0;i<3;i++)yield return null;
            if(f.visuals.CylaActive){fail("Cyla disable failed");yield break;}
            f.Settings.AtmosphereBackend=1;for(int i=0;i<3;i++)yield return null;
            if(!f.visuals.CylaActive){fail("Cyla reenable failed");yield break;}
            Time.timeScale=1;f.Settings.CloudAmount=.45;
            var gameplay=f.Settings.Save();RingQualityPresets.Apply(gameplay,10);f.ApplyOptions(gameplay,false);
            for(int i=0;i<90;i++)yield return null;
            if(f.Settings.AtmosphereBackend!=0||f.visuals.CylaActive||f.visuals.Rendering){fail("Rotten Potato still runs GPU atmosphere");yield break;}
            var movingCamera=FlightCamera.fetch.mainCamera;var startRotation=movingCamera.transform.rotation;
            benchmark.Restart();frames=0;
            while(benchmark.Elapsed.TotalSeconds<10){movingCamera.transform.rotation=startRotation*Quaternion.Euler(0,(float)Math.Sin(benchmark.Elapsed.TotalSeconds)*25,0);frames++;yield return null;}
            Debug.Log("[RingworldSmoke] LOW preset live physics/camera sweep "+Screen.width+"x"+Screen.height+" fps="+(frames/benchmark.Elapsed.TotalSeconds).ToString("F1"));
            movingCamera.transform.rotation=startRotation;
            if(!f.visuals.BeginPhoto(1,10)){fail("Low preset photo entry failed");yield break;}
            var lowClock=System.Diagnostics.Stopwatch.StartNew();while(!f.visuals.PhotoFinished&&lowClock.Elapsed.TotalSeconds<600)yield return null;
            if(!f.visuals.PhotoFinished||!f.visuals.SimplePhoto){fail("Low preset photo did not use simple capture");yield break;}
            f.visuals.EndPhoto();yield return null;
            if(RingQualityPresets.Match(f.Settings)!="Rotten Potato"){fail("Low photo did not restore gameplay preset");yield break;}
            if(!f.visuals.BeginPhoto(1,2)){fail("Selected high preset photo entry failed");yield break;}
            if(f.Settings.AtmosphereBackend!=1||f.Settings.ForestQuality!=3){fail("Photo preset did not apply");yield break;}
            f.visuals.EndPhoto();yield return null;
            if(RingQualityPresets.Match(f.Settings)!="Rotten Potato"||Time.timeScale!=1){fail("Cancelled photo did not restore gameplay");yield break;}
            Debug.Log("[RingworldSmoke] PHOTO low preset capture and high preset cancellation restored gameplay");
            if(!f.visuals.BeginPhoto(1,2)){fail("Cyla photo entry failed");yield break;}
            var clock=System.Diagnostics.Stopwatch.StartNew();while(f.visuals.PhotoActive&&!f.visuals.PhotoFinished&&clock.Elapsed.TotalSeconds<600)yield return null;
            if(!f.visuals.PhotoFinished){fail("Cyla photo incomplete: "+f.visuals.Status);yield break;}
            File.Copy(f.visuals.LastPhoto,Path.Combine(KSPUtil.ApplicationRootPath,"Cyla-photo.png"),true);
            f.visuals.EndPhoto();yield return null;
            if(Time.timeScale!=1||f.visuals.PhotoActive){fail("Cyla photo restoration failed");yield break;}
            Debug.Log("[RingworldSmoke] CYLA toggle and photo completion/restore passed");
        }
        private static double Capture(string name)
        {
            var camera=FlightCamera.fetch.mainCamera;var old=camera.targetTexture;var rt=new RenderTexture(1280,720,24);var active=RenderTexture.active;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(KSPUtil.ApplicationRootPath,name),image.EncodeToPNG());centre=image.GetPixel(640,360);double blue=0;for(int y=600;y<700;y++)for(int x=100;x<1000;x++)blue+=image.GetPixel(x,y).b;return blue/90000;}
            finally{camera.targetTexture=old;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
        }
    }
}
#endif
