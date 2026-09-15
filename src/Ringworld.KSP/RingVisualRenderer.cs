using System;
using System.IO;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    // One image effect on the final local camera. No planetary material or global
    // graphics preset is replaced. The bundled shader is original project source.
    internal sealed class RingVisualRenderer : MonoBehaviour
    {
        private Camera cameraComponent;
        private DepthTextureMode previousDepth;
        private AssetBundle bundle;
        private Material material;
        private RenderTexture volume,photoBase,photoDepth,photoA,photoB,photoResult;
        private float savedTimeScale;
        private int savedFrameRate,photoFrame,photoSamples;
        private bool savedUi,photoStarted,photoFinished,failed;
        private Vector3 photoPosition;
        private Quaternion photoRotation;
        private float photoFov;
        private const string PhotoLock="NivenRingworld.Photo";
        internal bool PhotoActive {get;private set;}
        internal bool Rendering {get;private set;}
        internal bool PhotoFinished {get{return photoFinished;}}
        internal string LastPhoto {get;private set;}
        internal string Status="Laptop atmosphere active.";
        internal int RenderedFrames {get;private set;}
        private Settings Settings {get{return RingworldFlight.Instance.Settings;}}
        internal void InvalidateWeather(){} // Parameters are now sampled continuously; there is no timed weather cache.
        public void Awake(){cameraComponent=GetComponent<Camera>();previousDepth=cameraComponent.depthTextureMode;}
        private bool EnsureAssets()
        {
            if(material!=null)return true;if(failed)return false;
            try
            {
                if(!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)||!SystemInfo.supports3DTextures)throw new InvalidOperationException("GPU does not support the required volume textures.");
                bundle=RingVisualAssets.Acquire();if(bundle==null)throw new InvalidOperationException("Ringworld visual shader bundle is missing or incompatible.");
                var shader=bundle.LoadAsset<Shader>("Assets/Shaders/RingAtmosphere.shader");
                if(shader==null||!shader.isSupported)throw new InvalidOperationException("Ringworld volumetric shader is unsupported on this graphics API.");
                material=new Material(shader);material.SetTexture("_Noise",bundle.LoadAsset<Texture3D>("Assets/CloudNoise.asset"));
                Debug.Log("[NivenRingworld] GPU atmosphere ready: "+SystemInfo.graphicsDeviceType+" / "+SystemInfo.graphicsDeviceName);
                return true;
            }
            catch(Exception e){failed=true;Status=e.Message+" Using laptop atmosphere.";Debug.LogError("[NivenRingworld] "+Status);return false;}
        }
        internal Shader WaterShader(){return EnsureAssets()?bundle.LoadAsset<Shader>("Assets/Shaders/RingWater.shader"):null;}
        internal void Prepare(bool allowed,Vector3d star)
        {
            if(PhotoActive&&(!allowed||MapView.MapIsEnabled)){EndPhoto();Status="Photo cancelled after camera/scene change.";}
            Rendering=allowed&&(Settings.VisualQuality>0||PhotoActive)&&EnsureAssets();
            if(!Rendering){enabled=PhotoActive;return;}
            var observer=ConvertVector.Core((Vector3d)cameraComponent.transform.position-star);var g=Settings.Geometry;var c=g.Coordinates(observer);
            Rendering=c.Altitude>-1000&&c.Altitude<600000&&Math.Abs(c.Across)<g.P.Width/2+500000&&Settings.Atmosphere;
            if(!Rendering){enabled=PhotoActive;return;}
            if(!enabled){previousDepth=cameraComponent.depthTextureMode;enabled=true;}
            cameraComponent.depthTextureMode|=DepthTextureMode.Depth;
            if(PhotoActive&&photoStarted)
            {cameraComponent.transform.SetPositionAndRotation(photoPosition,photoRotation);cameraComponent.fieldOfView=photoFov;return;}
            double time=Planetarium.GetUniversalTime();
            double extent=Math.Max(150000,Settings.CloudRange*1.15);
            var currentWeather=RingCloudField.Apply(material,Settings,c.Along,c.Across,c.Altitude,time);
            material.SetVector("_CloudHandoff",RingCloudField.Handoff(Settings,true));
            DVec up=g.Up(observer),along=g.SpinVelocity(observer).Unit,across=DVec.Cross(up,along).Unit;
            Func<Vector3,Vector3> local=v=>new Vector3((float)DVec.Dot(ConvertVector.Core(v),along),(float)DVec.Dot(ConvertVector.Core(v),across),(float)DVec.Dot(ConvertVector.Core(v),up));
            // Geometry's positive Across is world +Y. Correct the tangent handedness explicitly.
            across=new DVec(0,1,0);
            material.SetVector("_RayRight",local(cameraComponent.transform.right)/cameraComponent.projectionMatrix.m00);
            material.SetVector("_RayUp",local(cameraComponent.transform.up)/cameraComponent.projectionMatrix.m11);
            material.SetVector("_RayForward",local(cameraComponent.transform.forward));
            material.SetVector("_Sun",local(ConvertVector.Unity((-observer).Unit)));
            material.SetVector("_Habitat",new Vector4((float)c.Altitude,(float)g.P.Radius,(float)c.Across,(float)(g.P.Width/2)));
            material.SetVector("_WeatherMap",new Vector4(0,0,(float)extent,(float)currentWeather.Cloud));
            material.SetVector("_Look",new Vector4((float)Settings.Haze,(float)g.Daylight(c.Along,time),(float)Settings.CloudShadow,(float)Settings.AtmosphereExposure));
            material.SetVector("_Quality",new Vector4(PhotoActive?192:Settings.CloudSteps,PhotoActive?80:Settings.AtmosphereSteps,PhotoActive?8:Settings.VisualQuality==2?6:4,(float)Settings.CloudRange));
            if(!PhotoActive)Status="Weather: "+(currentWeather.Storm>.1?"thunderstorm":currentWeather.Rain>.1?"rain":currentWeather.Cloud>.5?"cloudy":"fair")+" / GPU clouds / "+Settings.CloudSteps+" view steps / "+Settings.AtmosphereSteps+" atmosphere steps.";
        }
        private static RenderTexture Target(int w,int h,RenderTextureFormat format,string name)
        {var rt=new RenderTexture(w,h,0,format,RenderTextureReadWrite.Linear){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};rt.Create();return rt;}
        private static void Free(ref RenderTexture rt){if(rt==null)return;rt.Release();Destroy(rt);rt=null;}
        public void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(!Rendering||material==null){Graphics.Blit(source,destination);return;}
            try
            {
                if(PhotoActive&&photoStarted&&(source.width!=photoBase.width||source.height!=photoBase.height))
                {EndPhoto();Status="Photo cancelled because the viewport size changed. Frame the shot and try again.";Graphics.Blit(source,destination);return;}
                material.SetVector("_FrameSize",new Vector2(source.width,source.height));
                if(PhotoActive){RenderPhoto(source,destination);return;}
                int divisor=Settings.VisualQuality==1?2:1,w=Math.Max(1,source.width/divisor),h=Math.Max(1,source.height/divisor);
                if(volume==null||volume.width!=w||volume.height!=h){Free(ref volume);volume=Target(w,h,RenderTextureFormat.ARGBHalf,"Ringworld realtime atmosphere");}
                material.SetVector("_Photo",Vector4.zero);material.SetFloat("_Jitter",0);
                Graphics.Blit(source,volume,material,0);material.SetTexture("_Volume",volume);Graphics.Blit(source,destination,material,1);RenderedFrames++;
            }
            catch(Exception e){Debug.LogException(e);Status="Visual render failed: "+e.Message;EndPhoto();Rendering=false;failed=true;Graphics.Blit(source,destination);}
        }
        internal bool BeginPhoto(int? sampleOverride=null)
        {
            var f=RingworldFlight.Instance;
            if(PhotoActive)return false;
            if(f==null||!f.Ready||MapView.MapIsEnabled||TimeWarp.CurrentRate>1.0001f||Time.timeScale==0){Status="Photo mode needs normal flight view at 1x. Frame the camera first.";return false;}
            if(!EnsureAssets())return false;
            savedTimeScale=Time.timeScale;savedFrameRate=Application.targetFrameRate;savedUi=KSP.UI.UIMasterController.Instance!=null&&KSP.UI.UIMasterController.Instance.IsUIShowing;
            PhotoActive=true;photoStarted=false;photoFinished=false;photoFrame=0;photoSamples=Math.Max(1,Math.Min(64,sampleOverride??Settings.PhotoSamples));LastPhoto=null;
            f.PhotoTerrain(true);
            Time.timeScale=0;Application.targetFrameRate=15;InputLockManager.SetControlLock(ControlTypes.All,PhotoLock);
            if(savedUi)KSP.UI.UIMasterController.Instance.HideUI();
            Status="Preparing frozen photo. No flight time will pass.";return true;
        }
        private void RenderPhoto(RenderTexture source,RenderTexture destination)
        {
            if(!photoStarted&&!RingworldFlight.Instance.PhotoTerrainReady)
            {Status="Preparing high-quality terrain: "+RingworldFlight.Instance.LodPending+" chunks remaining. Flight is frozen.";Graphics.Blit(source,destination);return;}
            if(!photoStarted)
            {
                photoPosition=cameraComponent.transform.position;photoRotation=cameraComponent.transform.rotation;photoFov=cameraComponent.fieldOfView;
                photoBase=Target(source.width,source.height,RenderTextureFormat.ARGB32,"Ringworld frozen scene");
                photoDepth=Target(source.width,source.height,RenderTextureFormat.RFloat,"Ringworld frozen depth");
                photoA=Target(source.width,source.height,RenderTextureFormat.ARGBHalf,"Ringworld photo accumulation A");photoB=Target(source.width,source.height,RenderTextureFormat.ARGBHalf,"Ringworld photo accumulation B");
                photoResult=Target(source.width,source.height,RenderTextureFormat.ARGB32,"Ringworld photo result");
                Graphics.Blit(source,photoBase);Graphics.Blit(source,photoDepth,material,2);Graphics.Blit(Texture2D.blackTexture,photoA);Graphics.Blit(Texture2D.blackTexture,photoB);
                material.SetTexture("_SavedDepth",photoDepth);photoStarted=true;
                // Initialize all pixels cheaply, then refine one of 16 tiles per rendered frame.
                material.SetVector("_Quality",new Vector4(24,16,2,(float)Settings.CloudRange));material.SetVector("_Photo",new Vector4(1,-1,4,0));material.SetFloat("_Jitter",0);
                Graphics.Blit(photoBase,photoA,material,0);material.SetVector("_Quality",new Vector4(192,80,8,(float)Settings.CloudRange));
            }
            if(!photoFinished)
            {
                int sample=photoFrame/16,tile=photoFrame%16;
                material.SetVector("_Photo",new Vector4(1,tile,4,sample));material.SetFloat("_Jitter",sample+.5f);material.SetTexture("_Previous",photoA);
                Graphics.Blit(photoBase,photoB,material,0);var swap=photoA;photoA=photoB;photoB=swap;
                photoFrame++;Status="Rendering photo: "+(100*photoFrame/(16*photoSamples))+"% — Cancel restores flight.";
            }
            material.SetTexture("_Volume",photoA);Graphics.Blit(photoBase,photoResult,material,1);Graphics.Blit(photoResult,destination);
            if(!photoFinished&&photoFrame>=photoSamples*16)
            {
                var previous=RenderTexture.active;Texture2D image=null;
                try
                {
                    RenderTexture.active=photoResult;image=new Texture2D(photoResult.width,photoResult.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,image.width,image.height),0,0);image.Apply(false);
                    string folder=Path.Combine(KSPUtil.ApplicationRootPath,"Screenshots","Ringworld");Directory.CreateDirectory(folder);
                    LastPhoto=Path.Combine(folder,"Ringworld-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".png");File.WriteAllBytes(LastPhoto,image.EncodeToPNG());
                    File.WriteAllText(Path.ChangeExtension(LastPhoto,"txt"),"Ringworld GPU photo\nUTC: "+DateTime.UtcNow.ToString("O")+"\nUT: "+Planetarium.GetUniversalTime().ToString("R")+"\nWorld seed: "+Settings.Geometry.P.Seed+"\nSize: "+image.width+"x"+image.height+"\nCloud/air/light steps: 192/80/8\nAccumulated samples: "+photoSamples+"\n"+StockGraphics.Description+"\n");
                    photoFinished=true;Status="Photo saved. Resume returns to your normal graphics.\n"+LastPhoto;Debug.Log("[NivenRingworld] PHOTO SAVED "+LastPhoto);
                }
                finally{RenderTexture.active=previous;if(image!=null)Destroy(image);}
            }
        }
        internal void EndPhoto()
        {
            if(!PhotoActive)return;
            var flight=RingworldFlight.Instance;if(flight!=null)flight.PhotoTerrain(false);
            PhotoActive=false;Time.timeScale=savedTimeScale;Application.targetFrameRate=savedFrameRate;InputLockManager.RemoveControlLock(PhotoLock);
            if(savedUi&&KSP.UI.UIMasterController.Instance!=null)KSP.UI.UIMasterController.Instance.ShowUI();
            Free(ref photoBase);Free(ref photoDepth);Free(ref photoA);Free(ref photoB);Free(ref photoResult);
            photoStarted=false;Rendering=false;
        }
        public void OnGUI()
        {
            if(!PhotoActive)return;
            GUILayout.BeginArea(new Rect(20,Screen.height-145,Mathf.Min(700,Screen.width-40),125),GUI.skin.box);
            GUILayout.Label(Status);if(GUILayout.Button(photoFinished?"Resume flight":"Cancel photo"))EndPhoto();GUILayout.EndArea();
        }
        public void OnDisable(){EndPhoto();if(cameraComponent!=null)cameraComponent.depthTextureMode=previousDepth;}
        public void OnDestroy()
        {
            EndPhoto();Free(ref volume);if(cameraComponent!=null)cameraComponent.depthTextureMode=previousDepth;
            if(material!=null)Destroy(material);if(bundle!=null)RingVisualAssets.Release();
        }
    }
}
