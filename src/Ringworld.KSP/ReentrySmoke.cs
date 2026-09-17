#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using HarmonyLib;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class ReentrySmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            var v=FlightGlobals.ActiveVessel;
            bool renderOnly=Array.IndexOf(Environment.GetCommandLineArgs(),"-ringworld-render-only")>=0;
            CheatOptions.IgnoreMaxTemperature=true;
            Debug.Log("[RingworldSmoke] REENTRY visual-only fixture: temperature damage disabled; gravity and drag unchanged");
            foreach(bool large in renderOnly?new[]{false}:new[]{false,true})
            {
                if(f.Active)f.Leave();
                var options=f.Settings.Save();RingQualityPresets.Apply(options,10);
                options.SetValue("radius",large?100000000000.0:15300000000.0,true);
                options.SetValue("width",large?50000000000.0:160500000.0,true);
                options.SetValue("dynamicWeather",false,true);options.SetValue("cloudAmount",.45,true);
                f.ApplyOptions(options,true);
                int destination=f.Settings.Terrain.Landmarks.FindIndex(l=>l.Id=="rim");
                AccessTools.Field(typeof(RingworldFlight),"destination").SetValue(f,destination);
                // Controlled initial orbit only: no gravity adjustment during descent.
                // Use the user's long elapsed time and terminal-adjacent approach.
                Planetarium.SetUniversalTime(129*21600.0);
                f.arrivalHeight=renderOnly?55000:400000;f.Visit();while(!f.Ready)yield return null;
                int cameraCompletions=RingCameraBlend.Completed;
                v.SetWorldVelocity(ConvertVector.Ksp(f.Settings.Geometry.Up(f.Position(v))*(renderOnly?-3300:-2000)));f.Leave();
                FlightCamera.fetch.SetDistanceImmediate(50);FlightCamera.CamPitch=.15f;
                if(renderOnly)
                {
                    yield return new WaitForSecondsRealtime(3);
                    foreach(var camera in UnityEngine.Object.FindObjectsOfType<Camera>())
                        Debug.Log("[RingworldSmoke] RENDER camera="+camera.name+" enabled="+camera.enabled+" depth="+camera.depth+" mask="+camera.cullingMask+" clear="+camera.clearFlags+" near="+camera.nearClipPlane+" far="+camera.farClipPlane);
                }
                int sample=0;double[] heights=renderOnly?new[]{10000.0}:new[]{220000.0,55000,10000};float end=Time.realtimeSinceStartup+500;
                int frames=Time.frameCount;float begin=Time.realtimeSinceStartup;
                while(sample<heights.Length&&Time.realtimeSinceStartup<end)
                {
                    double altitude=f.Settings.Geometry.Coordinates(f.Position(v)).Altitude;
                    if(altitude<heights[sample])
                    {
                        yield return new WaitForEndOfFrame();
                        string name="Ringworld-reentry-"+(large?"large":"normal")+"-"+sample+".png";
                        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(KSPUtil.ApplicationRootPath,name));
                        Debug.Log("[RingworldSmoke] REENTRY "+name+" altitude="+altitude+" speed="+f.Velocity(v).Length+" owned="+f.Owns(v)+" preset="+RingQualityPresets.Match(f.Settings));
                        foreach(var camera in FlightCamera.fetch.cameras)Debug.Log("[RingworldSmoke] REENTRY camera="+camera.name+" near="+camera.nearClipPlane+" far="+camera.farClipPlane);
                        sample++;
                    }
                    if(v==null||v.state==Vessel.State.DEAD){fail("Reentry fixture lost before visual samples");yield break;}
                    yield return null;
                }
                if(sample!=heights.Length){fail("Reentry samples timed out");yield break;}
                if(!renderOnly&&(RingCameraBlend.Completed<cameraCompletions+2||RingCameraBlend.FirstFrameAngle>.1f))
                {fail("Camera entry/exit blend did not complete continuously");yield break;}
                if(!renderOnly)Debug.Log("[RingworldSmoke] CAMERA entry and exit blends completed; first-frame angle="+RingCameraBlend.FirstFrameAngle);
                Debug.Log("[RingworldSmoke] REENTRY large="+large+" fps="+((Time.frameCount-frames)/(Time.realtimeSinceStartup-begin))+" gravity="+f.Settings.Geometry.P.Gravity);
                if(!large)
                {
                    // Isolate render layers after the unmodified descent samples.
                    // This is a diagnostic hold, not part of the descent measurement.
                    v.SetWorldVelocity(Vector3d.zero);
                    foreach(string objectName in new[]{"Ringworld global cloud shell","Ringworld camera-relative scaled ribbon","Scrith rim wall","Ringworld single-scattering sky"})
                    {
                        var hidden=new System.Collections.Generic.List<Renderer>();
                        foreach(var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
                            if(renderer.gameObject.name==objectName&&!renderer.forceRenderingOff){renderer.forceRenderingOff=true;hidden.Add(renderer);}
                        yield return null;yield return new WaitForEndOfFrame();
                        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-isolate-"+objectName.Replace(' ','-')+".png"));
                        yield return null;
                        foreach(var renderer in hidden)if(renderer!=null)renderer.forceRenderingOff=false;
                        Debug.Log("[RingworldSmoke] REENTRY isolated "+objectName+" count="+hidden.Count);
                    }
                    if(renderOnly)
                    {
                        var ribbon=GameObject.Find("Ringworld camera-relative scaled ribbon").GetComponent<MeshFilter>().sharedMesh;
                        for(int submesh=0;submesh<2;submesh++)
                        {
                            var indices=ribbon.GetTriangles(submesh);ribbon.SetTriangles(new int[0],submesh);
                            yield return null;yield return new WaitForEndOfFrame();
                            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-isolate-submesh-"+submesh+".png"));
                            yield return null;ribbon.SetTriangles(indices,submesh);
                        }
                        var renderer=GameObject.Find("Ringworld camera-relative scaled ribbon").GetComponent<MeshRenderer>();
                        var original=renderer.sharedMaterials;var testMaterial=new Material(original[1]);testMaterial.SetFloat("_Detail",0);
                        renderer.sharedMaterials=new[]{testMaterial,original[1]};
                        yield return null;yield return new WaitForEndOfFrame();
                        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(KSPUtil.ApplicationRootPath,"Ringworld-isolate-explicit-wall-shader.png"));
                        yield return null;renderer.sharedMaterials=original;UnityEngine.Object.Destroy(testMaterial);
                    }
                }
            }
            Debug.Log(renderOnly?"[RingworldSmoke] PASS static render diagnostics; inspect screenshots":"[RingworldSmoke] PASS reentry visual sampling at normal and enlarged dimensions; inspect six screenshots");
        }
    }
}
#endif
