#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using HarmonyLib;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class WallSmoke
    {
        internal static IEnumerator Run(RingworldFlight flight,Action<string> fail)
        {
            var options=flight.Settings.Save();RingQualityPresets.Apply(options,10);
            options.SetValue("cloudAmount",0,true);options.SetValue("dynamicWeather",false,true);
            flight.ApplyOptions(options,true);
            int destination=flight.Settings.Terrain.Landmarks.FindIndex(l=>l.Id=="rim");
            AccessTools.Field(typeof(RingworldFlight),"destination").SetValue(flight,destination);
            flight.arrivalHeight=10;flight.Visit();while(!flight.Ready)yield return null;
            yield return new WaitForSecondsRealtime(3);
            var go=new GameObject("Wall regression camera");var camera=go.AddComponent<Camera>();camera.enabled=false;
            camera.cullingMask=1<<10;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.magenta;
            camera.nearClipPlane=.2f;camera.farClipPlane=1e9f;camera.fieldOfView=70;
            var target=new RenderTexture(640,360,24);camera.targetTexture=target;
            var image=new Texture2D(640,360,TextureFormat.RGB24,false);
            var g=flight.Settings.Geometry;double along=flight.Settings.Terrain.Landmarks[destination].Along;
            foreach(int side in new[]{-1,1})foreach(double heading in new[]{0.0,75.0,88.0})
            {
                var eye=g.Position(along,side*(g.P.Width/2-2000),50);
                var tangent=DVec.Cross(new DVec(0,1,0),eye).Unit;
                var direction=new DVec(0,side*Math.Cos(heading*Math.PI/180),0)+tangent*Math.Sin(heading*Math.PI/180);
                camera.transform.position=(Vector3)ScaledSpace.LocalToScaledSpace(flight.Star.position+ConvertVector.Ksp(eye));
                camera.transform.rotation=Quaternion.LookRotation(ConvertVector.Unity(direction),ConvertVector.Unity(g.Up(eye)));
                camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,640,360),0,0);image.Apply();RenderTexture.active=old;
                File.WriteAllBytes(Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-wall-"+side+"-"+heading+".png"),image.EncodeToPNG());
                var centre=image.GetPixel(320,180);
                Debug.Log("[RingworldSmoke] WALL side="+side+" heading="+heading+" centre="+centre);
                if(centre.r>.04f||centre.g>.04f||centre.b>.04f){fail("Distant wall missing from centre ray");yield break;}
                yield return null;
            }
            // Compare the actual flight-camera composition with the local shell
            // present/hidden. Both views must retain the full scaled wall.
            float oldScale=Time.timeScale;Time.timeScale=0;
            var eyeLocal=flight.Position(FlightGlobals.ActiveVessel);
            var look=(new DVec(0,1,0)*.3+DVec.Cross(new DVec(0,1,0),eyeLocal).Unit*.95).Unit;
            var rotation=Quaternion.LookRotation(ConvertVector.Unity(look),ConvertVector.Unity(g.Up(eyeLocal)));
            Camera.CameraCallback aim=c=>{if(c==FlightCamera.fetch.mainCamera||(ScaledCamera.Instance!=null&&c==ScaledCamera.Instance.cam))c.transform.rotation=rotation;};
            Camera.onPreCull+=aim;
            var walls=new System.Collections.Generic.List<MeshRenderer>();
            foreach(var renderer in UnityEngine.Object.FindObjectsOfType<MeshRenderer>())if(renderer.name=="Scrith rim wall")walls.Add(renderer);
            foreach(bool hidden in new[]{false,true})
            {
                foreach(var renderer in walls)renderer.forceRenderingOff=hidden;
                yield return null;yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-wall-composite-"+(hidden?"scaled-only":"joined")+".png"));
                yield return null;
            }
            foreach(var renderer in walls)renderer.forceRenderingOff=false;
            Camera.onPreCull-=aim;Time.timeScale=oldScale;
            UnityEngine.Object.Destroy(image);target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(go);
            Debug.Log("[RingworldSmoke] PASS wall render sampling; inspect captures");
        }
    }
}
#endif
