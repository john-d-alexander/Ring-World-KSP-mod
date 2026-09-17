using System;
using System.Collections;
using System.Reflection;
using Ringworld.Core;
using UnityEngine;
using UnityEngine.Rendering;
namespace NivenRingworld
{
    // Original adapter to the public Cyla/Atmo material interface. No upstream
    // plugin source or shader binary is embedded in NivenRingworld.
    internal sealed class CylaAtmosphereBridge : IDisposable
    {
        private readonly Camera camera;
        private Material material;
        private Mesh quad;
        private CommandBuffer commands;
        private RenderTexture background,black,white;
        private readonly MaterialPropertyBlock backgroundBinding=new MaterialPropertyBlock(),blackBinding=new MaterialPropertyBlock(),whiteBinding=new MaterialPropertyBlock();
        private static RenderTexture Target(int width,int height,string name){var rt=new RenderTexture(width,height,0,RenderTextureFormat.ARGBHalf){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};rt.Create();return rt;}
        private static void Free(ref RenderTexture rt){if(rt==null)return;rt.Release();UnityEngine.Object.Destroy(rt);rt=null;}
        private bool attached;

        private int frame;
        internal bool Active {get;private set;}
        internal string Status="Cyla not initialized";
        internal CylaAtmosphereBridge(Camera target){camera=target;}
        private bool Load()
        {
            if(material!=null)return true;
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var loader=assembly.GetType("Cyla.ShaderLoader",false);if(loader==null)continue;
                var property=loader.GetProperty("Shaders",BindingFlags.Public|BindingFlags.Static);
                var registry=property==null?null:property.GetValue(null,null) as IDictionary;
                var shader=registry==null?null:registry["Cyla/Atmo"] as Shader;
                if(shader==null||!shader.isSupported){Status="Cyla shader not ready or unsupported";return false;}
                material=new Material(shader){name="Ringworld Cyla atmosphere"};
                material.EnableKeyword("TRANSPARENT_TOP_AND_SIDE");material.DisableKeyword("TRANSPARENT_FLOOR");material.DisableKeyword("UNLIT");
                quad=new Mesh{name="Ringworld Cyla camera quad"};
                quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
                quad.normals=new[]{Vector3.back,Vector3.back,Vector3.back,Vector3.back};
                quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};quad.triangles=new[]{0,2,1,0,3,2};quad.RecalculateBounds();
                
                commands=new CommandBuffer{name="Niven Ringworld / Cyla scattering"};
                Debug.Log("[NivenRingworld] Cyla compiled shader connected: passes="+material.passCount+" / "+SystemInfo.graphicsDeviceType);
                return true;
            }
            Status="Install Cyla 1.1.0 to enable this backend";return false;
        }
        internal void Prepare(bool allowed,Settings s,Vector3d star,bool photo,bool frozen,Material composite)
        {
            Active=false;
            if(!allowed||s.AtmosphereBackend!=1||!s.Atmosphere||s.Haze<=0||composite==null||!Load()){Detach();return;}
            var observer=ConvertVector.Core((Vector3d)camera.transform.position-star);
            var coord=s.Geometry.Coordinates(observer);
            if(coord.Altitude< -1000||coord.Altitude>600000||Math.Abs(coord.Across)>s.Geometry.P.Width/2+500000){Detach();return;}
            if(photo&&frozen){Detach();Active=true;return;}
            camera.depthTextureMode|=DepthTextureMode.Depth;
            var optics=s.Cyla;float thickness=(float)optics["Thickness"];
            // The compiled shader visibly loses precision at the real 15.3e9 m
            // radius. A tangent-aligned optical proxy keeps near-surface floats
            // bounded. It does not alter terrain, atmosphere forces or ring motion.
            double renderRadius=Math.Min(s.Geometry.P.Radius,optics["ProxyRadius"]);
            Vector3 renderCentre=camera.transform.position+ConvertVector.Unity(s.Geometry.Up(observer))*(float)(renderRadius-coord.Altitude);
            renderCentre.y-=(float)coord.Across;
            material.SetFloat("outerRadius",(float)renderRadius);
            material.SetFloat("innerRadius",(float)(renderRadius-thickness));
            material.SetFloat("transparentRadius",(float)(renderRadius-Math.Max(32,thickness*optics["TransparentDepth"])));
            material.SetFloat("height",(float)(s.Geometry.P.Width*optics["WidthScale"]));
            renderCentre+=ConvertVector.Unity(s.Geometry.SpinVelocity(observer).Unit)*(float)optics["OffsetAlong"]+Vector3.up*(float)optics["OffsetAcross"]+ConvertVector.Unity(s.Geometry.Up(observer))*(float)optics["OffsetUp"];
            material.SetVector("centerPosition",renderCentre);material.SetVector("axis",Quaternion.AngleAxis((float)optics["Yaw"],ConvertVector.Unity(s.Geometry.Up(observer)))*Quaternion.AngleAxis((float)optics["Pitch"],ConvertVector.Unity(s.Geometry.SpinVelocity(observer).Unit))*Vector3.up);
            string[] modes={"TRANSPARENT_TOP_AND_SIDE","TRANSPARENT_FLOOR","UNLIT"};for(int i=0;i<modes.Length;i++){if(i==optics.LightingMode)material.EnableKeyword(modes[i]);else material.DisableKeyword(modes[i]);}
            material.SetVector("lightEmitterPosition",renderCentre);
            float sunlight=(float)s.Geometry.Daylight(coord.Along,Planetarium.GetUniversalTime());
            material.SetVector("lightColor",new Vector4(sunlight,sunlight,sunlight,1)*(float)s.AtmosphereExposure);
            material.SetVector("rayleighScattering",optics.Scattering("Rayleigh")*(float)s.Haze);
            material.SetVector("mieScattering",optics.Scattering("Mie")*(float)s.Haze);
            material.SetFloat("rayleighScaleHeight",(float)optics["RayleighHeight"]);material.SetFloat("mieScaleHeight",(float)optics["MieHeight"]);material.SetFloat("miePhaseAsymmetry",(float)optics["Asymmetry"]);
            bool finalPhoto=photo&&RingworldFlight.Instance.PhotoTerrainReady;
            material.SetInt("raymarchingIterations",(int)optics["ViewSteps"]);
            material.SetInt("transmittanceIterations",s.CylaLightSteps);
            material.SetInt("ditheredRaymarching",photo?0:s.CylaDither?1:0);
            material.SetFloat("frame",photo?0:(frame++%1024));
            int divisor=s.CylaDivisor;
            int width=Math.Max(1,camera.pixelWidth),height=Math.Max(1,camera.pixelHeight),w=Math.Max(1,width/divisor),h=Math.Max(1,height/divisor);
            if(background==null||background.width!=width||background.height!=height){Free(ref background);background=Target(width,height,"Cyla camera background");}
            commands.Clear();commands.Blit(BuiltinRenderTextureType.CameraTarget,background);
            if(divisor==1)
            {
                backgroundBinding.SetTexture("_RamaAtmoBackgroundTexture",background);
                commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                commands.DrawMesh(quad,Matrix4x4.identity,material,0,1,backgroundBinding);
            }
            else
            {
                if(black==null||black.width!=w||black.height!=h){Free(ref black);Free(ref white);black=Target(w,h,"Cyla scattering");white=Target(w,h,"Cyla white response");}
                blackBinding.SetTexture("_RamaAtmoBackgroundTexture",Texture2D.blackTexture);whiteBinding.SetTexture("_RamaAtmoBackgroundTexture",Texture2D.whiteTexture);
                commands.SetRenderTarget(black);commands.ClearRenderTarget(false,true,Color.clear);commands.DrawMesh(quad,Matrix4x4.identity,material,0,1,blackBinding);
                commands.SetRenderTarget(white);commands.ClearRenderTarget(false,true,Color.white);commands.DrawMesh(quad,Matrix4x4.identity,material,0,1,whiteBinding);
                composite.SetTexture("_CylaBlack",black);composite.SetTexture("_CylaWhite",white);composite.SetVector("_CylaTexel",new Vector4(1f/w,1f/h,w,h));
                commands.Blit(background,BuiltinRenderTextureType.CameraTarget,composite,3);
            }
            if(!attached){camera.AddCommandBuffer(CameraEvent.BeforeImageEffects,commands);attached=true;}
            Active=true;Status="Cyla local optical approximation / "+(optics["ViewSteps"])+" view, "+(s.CylaLightSteps)+" light steps";
        }
        private void Detach(){if(attached&&camera!=null)camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects,commands);attached=false;}
        public void Dispose(){Detach();

Free(ref background);Free(ref black);Free(ref white);if(commands!=null)commands.Release();if(material!=null)UnityEngine.Object.Destroy(material);if(quad!=null)UnityEngine.Object.Destroy(quad);Active=false;}
    }
}
