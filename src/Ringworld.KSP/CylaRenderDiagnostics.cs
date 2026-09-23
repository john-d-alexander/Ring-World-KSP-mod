using System;
using System.Collections;
using UnityEngine;
namespace NivenRingworld
{
    internal static class CylaRenderDiagnostics
    {
        internal static IEnumerator Capture(RenderTexture black,RenderTexture white,RenderTexture background)
        {
            yield return new WaitForEndOfFrame();
            foreach(var source in new[]{black,white,background})
            {
                if(source==null||!source.IsCreated())continue;
                CaptureTarget(source);
            }
        }
        // A diagnostic failure must never interrupt atmosphere rendering. The
        // source may have been released by a resize or a scene change after EOF.
        private static void CaptureTarget(RenderTexture source)
        {
                RenderTexture target=null;Texture2D read=null;var previous=RenderTexture.active;
                try
                {
                    if(!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)||!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
                    {Debug.Log("[NivenRingworld] CYLA TARGET float readback unsupported");return;}
                    target=RenderTexture.GetTemporary(64,32,0,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
                    Graphics.Blit(source,target);RenderTexture.active=target;
                    read=new Texture2D(64,32,TextureFormat.RGBAFloat,false,true);read.ReadPixels(new Rect(0,0,64,32),0,0);read.Apply();
                    var pixels=read.GetPixels();int invalid=0,dark=0;float max=0;double sum=0;
                    foreach(var c in pixels)
                    {
                        float value=Mathf.Max(c.r,Mathf.Max(c.g,c.b));
                        if(!Finite(c.r)||!Finite(c.g)||!Finite(c.b)){invalid++;continue;}
                        if(value<.0001f)dark++;max=Mathf.Max(max,value);sum+=value;
                    }
                    Debug.Log("[NivenRingworld] CYLA TARGET "+source.name+" format="+source.format+" size="+source.width+"x"+source.height+" samples="+pixels.Length+" nonfinite="+invalid+" dark="+dark+" peak="+max+" mean="+(sum/Math.Max(1,pixels.Length-invalid)));
                }
                catch(Exception exception){Debug.LogWarning("[NivenRingworld] CYLA TARGET readback unavailable: "+exception.GetType().Name+": "+exception.Message);}
                finally{RenderTexture.active=previous;if(read!=null)UnityEngine.Object.Destroy(read);if(target!=null)RenderTexture.ReleaseTemporary(target);}
        }
        private static bool Finite(float value){return !float.IsNaN(value)&&!float.IsInfinity(value);}
    }
}
