#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace NivenRingworld
{
    [DefaultExecutionOrder(32000)]
    internal sealed class CylaProbePose:MonoBehaviour
    {
        internal RingworldFlight Flight;internal Vector3 Position;internal Quaternion Rotation;
        private void OnPreCull(){transform.position=Position;transform.rotation=Rotation;Flight.visuals.Prepare(true,Flight.Center);}
    }
    internal static class CylaBlackSkySmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            Screen.SetResolution(1280,720,false);f.arrivalHeight=100;f.Visit();while(!f.Ready)yield return null;
            for(int i=0;i<30;i++)yield return null;
            var debris=GameObject.CreatePrimitive(PrimitiveType.Cube);debris.name="Ring loose-object regression";
            debris.transform.position=FlightGlobals.ActiveVessel.transform.position+ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(FlightGlobals.ActiveVessel)))*30;
            var loose=physicalObject.ConvertToPhysicalObject(FlightGlobals.ActiveVessel.rootPart,debris);loose.origDrag=0;loose.rb.useGravity=false;
            for(int i=0;i<5;i++)yield return new WaitForFixedUpdate();
            var debrisUp=ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(FlightGlobals.ActiveVessel)));
            float startSpeed=Vector3.Dot(loose.rb.velocity,debrisUp),elapsed=0;
            for(int i=0;i<20;i++){yield return new WaitForFixedUpdate();elapsed+=Time.fixedDeltaTime;}
            float measured=(Vector3.Dot(loose.rb.velocity,debrisUp)-startSpeed)/elapsed;
            Debug.Log("[RingworldSmoke] LOOSE OBJECT acceleration="+measured);
            UnityEngine.Object.Destroy(debris);
            if(measured> -8||measured< -12){fail("Detached-object gravity incorrect");yield break;}
            Time.timeScale=0;f.Settings.CloudAmount=0;f.Settings.VisualQuality=0;
            f.Settings.CylaLightSteps=2;f.Settings.CylaDivisor=4;
            var options=f.Settings.Save();options.SetValue("cylaViewSteps",24,true);f.Settings.Cyla.Load(options);
            var cam=FlightCamera.fetch.mainCamera;var pose=cam.gameObject.AddComponent<CylaProbePose>();pose.Flight=f;
            var observer=ConvertVector.Core((Vector3d)cam.transform.position-f.Center);
            var up=ConvertVector.Unity(f.Settings.Geometry.Up(observer));
            var along=ConvertVector.Unity(f.Settings.Geometry.SpinVelocity(observer).Unit);
            var originalPosition=cam.transform.position;
            foreach(float height in new[]{2000f,20000f})
            foreach(float units in new[]{.00001f,.0001f,.0005f,.001f})
            {
                int steps=128;CylaAtmosphereBridge.ProbeUnits=units;
                pose.Position=originalPosition+up*(height-(float)f.Settings.Geometry.Coordinates(observer).Altitude);
                pose.Rotation=Quaternion.LookRotation((along-up*.3f).normalized,up);
                cam.nearClipPlane=.21f;f.Settings.CylaDivisor=8;options.SetValue("cylaViewSteps",steps,true);options.SetValue("cylaProxyRadius",100000000,true);f.Settings.Cyla.Load(options);f.Settings.AtmosphereBackend=1;f.Settings.CylaDither=false;
                for(int i=0;i<8;i++)yield return null;
                yield return new WaitForEndOfFrame();Capture("Cyla-zoom-"+height+"-units-"+units+".png");
            }
            double savedTime=Planetarium.GetUniversalTime();
            var g=f.Settings.Geometry;var c=g.Coordinates(ConvertVector.Core((Vector3d)pose.Position-f.Center));
            double phase=20*c.Along/g.P.Circumference;
            double nightTime=(Math.Ceiling(savedTime/g.P.DaySeconds-phase)+phase)*g.P.DaySeconds;
            try
            {
                SunFlare flare=null;foreach(var candidate in UnityEngine.Object.FindObjectsOfType<SunFlare>())if(candidate.sun==f.Star&&candidate.sunFlare!=null){flare=candidate;break;}
                if(flare==null){fail("Panel regression: stock Sun flare unavailable");yield break;}
                Planetarium.SetUniversalTime(nightTime);
                for(int i=0;i<10;i++)yield return null;
                yield return new WaitForEndOfFrame();float night=flare.sunFlare.brightness;
                Planetarium.SetUniversalTime(nightTime+g.P.DaySeconds*.5);
                for(int i=0;i<10;i++)yield return null;
                yield return new WaitForEndOfFrame();float day=flare.sunFlare.brightness;
                Debug.Log("[RingworldSmoke] PANEL FLARE night="+night+" day="+day);
                if(night>.0001f||day<=.001f){fail("Panel Sun flare did not hide at night and return by day");yield break;}
            }
            finally{Planetarium.SetUniversalTime(savedTime);}
            UnityEngine.Object.Destroy(pose);Time.timeScale=1;
            Debug.Log("[RingworldSmoke] PASS zoom diagnostic captures (visual review required)");
        }
        internal static IEnumerator RunSave(RingworldFlight f,Action<string> fail)
        {
            for(int i=0;i<60;i++)yield return null;
            var opts=f.Settings.Save();RingQualityPresets.Apply(opts,6);f.ApplyOptions(opts,true);
            for(int i=0;i<30;i++)yield return null;
            var debris=GameObject.CreatePrimitive(PrimitiveType.Cube);debris.name="Ring loose-object regression";
            debris.transform.position=FlightGlobals.ActiveVessel.transform.position+ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(FlightGlobals.ActiveVessel)))*30;
            var loose=physicalObject.ConvertToPhysicalObject(FlightGlobals.ActiveVessel.rootPart,debris);loose.origDrag=0;loose.rb.useGravity=false;
            for(int i=0;i<5;i++)yield return new WaitForFixedUpdate();
            var debrisUp=ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(FlightGlobals.ActiveVessel)));
            float startSpeed=Vector3.Dot(loose.rb.velocity,debrisUp),elapsed=0;
            for(int i=0;i<20;i++){yield return new WaitForFixedUpdate();elapsed+=Time.fixedDeltaTime;}
            float measured=(Vector3.Dot(loose.rb.velocity,debrisUp)-startSpeed)/elapsed;
            Debug.Log("[RingworldSmoke] LOOSE OBJECT acceleration="+measured);
            UnityEngine.Object.Destroy(debris);
            if(measured> -8||measured< -12){fail("Detached-object gravity incorrect");yield break;}
            Time.timeScale=0;f.Settings.CloudAmount=0;f.Settings.VisualQuality=0;
            f.Settings.CylaLightSteps=1;f.Settings.CylaDither=false;
            opts=f.Settings.Save();opts.SetValue("cylaViewSteps",16,true);f.Settings.Cyla.Load(opts);
            int native=0;foreach(var component in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>())if(component.GetType().FullName=="Cyla.AtmosphereRenderer")native++;
            Debug.Log("[RingworldSmoke] Cyla save regression native atmosphere components="+native);
            var cam=FlightCamera.fetch.mainCamera;
            var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);marker.layer=15;marker.GetComponent<Collider>().enabled=false;
            marker.transform.position=cam.transform.position+cam.transform.forward*2;marker.transform.localScale=Vector3.one*.4f;
            var mat=new Material(Shader.Find("Standard"));mat.color=Color.black;mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.magenta);marker.GetComponent<Renderer>().sharedMaterial=mat;
            f.Settings.AtmosphereBackend=0;
            for(int i=0;i<10;i++)yield return null;
            yield return new WaitForEndOfFrame();Capture("Cyla-save-original-marker.png");var baseline=lastCentre;
            bool dark=false;
            foreach(float units in new[]{.00001f,.0001f,.0005f,.001f})foreach(int divisor in new[]{8,1})
            {
                CylaAtmosphereBridge.ProbeUnits=units;
                opts.SetValue("cylaLightingMode",0,true);f.Settings.Cyla.Load(opts);
                f.Settings.AtmosphereBackend=1;f.Settings.CylaDivisor=divisor;
                for(int i=0;i<10;i++)yield return null;
                yield return new WaitForEndOfFrame();Capture("Cyla-units-"+units+"-divisor-"+divisor+".png");
                if(!f.visuals.CylaActive||lastBlue<.10){dark=true;Debug.Log("[RingworldSmoke] DARK candidate units="+units+" divisor="+divisor);}
                if(Math.Abs(lastCentre.r-baseline.r)>.1||Math.Abs(lastCentre.g-baseline.g)>.1||Math.Abs(lastCentre.b-baseline.b)>.1){fail("Cyla foreground marker changed: "+baseline+" -> "+lastCentre);yield break;}
            }
            UnityEngine.Object.Destroy(marker);UnityEngine.Object.Destroy(mat);Time.timeScale=1;
            Debug.Log("[RingworldSmoke] PASS coordinate comparison completed; dark candidates="+dark+" (visual review required)");
        }
        private static Color lastCentre;
        private static double lastBlue;
        private static void Capture(string name)
        {
            var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;RenderTexture.active=null;
            try
            {
                image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(KSPUtil.ApplicationRootPath,name),image.EncodeToPNG());
                double blue=0;int samples=0;
                for(int y=(int)(Screen.height*.75);y<(int)(Screen.height*.90);y+=4)
                    for(int x=Screen.width/3;x<Screen.width*2/3;x+=4){blue+=image.GetPixel(x,y).b;samples++;}
                lastCentre=image.GetPixel((int)(Screen.width*.54),(int)(Screen.height*.48));lastBlue=blue/Math.Max(1,samples);
                Debug.Log("[RingworldSmoke] CYLA sky "+name+" blue="+(blue/Math.Max(1,samples)).ToString("F5")+" centre="+image.GetPixel((int)(Screen.width*.54),(int)(Screen.height*.48)));
            }
            finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(image);}
        }
    }
}
#endif
