using System;
using System.Collections.Generic;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    [KSPAddon(KSPAddon.Startup.FlightAndKSC,false)]
    public sealed class ScaledRing : MonoBehaviour
    {
        private GameObject root,squares;
        private Mesh ring;
        private Vector3[] flightVertices,mapVertices;private bool mapGeometry;
        private Material material,dark,farMaterial,wallMaterial;
        private AssetBundle visualBundle;
        private bool farAttempted,farActive;
        private MeshRenderer ribbonRenderer;
        private GlobalClouds globalClouds;
        internal bool DetailActive { get { return farActive; } }
        private CelestialBody star;
        private Settings settings;
        private ConfigNode loadedOptions;
        public void Start()
        {
            if(!HighLogic.LoadedSceneIsFlight&&HighLogic.LoadedScene!=GameScenes.SPACECENTER&&HighLogic.LoadedScene!=GameScenes.TRACKSTATION)return;
            try
            {
                settings=Settings.Load();loadedOptions=RingworldScenario.Instance!=null?RingworldScenario.Instance.GetOptions():null;if(loadedOptions!=null)settings.Apply(loadedOptions);star=FlightGlobals.Bodies.Find(b=>b.name=="Sun");if(star==null||ScaledSpace.Instance==null)return;
                Shader shader=Shader.Find("Unlit/Color")??Shader.Find("KSP/Unlit");if(shader==null)return;
                material=new Material(shader){color=new Color(.34f,.47f,.32f)};dark=new Material(shader){color=new Color(.012f,.015f,.019f)};
                root=new GameObject("Niven Ringworld scaled habitat");root.layer=10;
                int n=8192;var verts=new Vector3[(n+1)*8];mapVertices=new Vector3[verts.Length];var uv=new Vector2[verts.Length];
                // Global scaled-space float coordinates lose metres near this enormous radius.
                // The fallback is render-only; keep an explicit roundoff margin below local terrain.
                double floorDrop=settings.Geometry.P.Radius*(1/Math.Cos(Math.PI/n)-1)-TerrainGenerator.MinimumHeight+settings.StructuralThickness+Math.Max(512,settings.Geometry.P.Radius*1e-6);
                double wallThickness=settings.StructuralThickness;var triangles=new List<int>();var floorTriangles=new List<int>();double s=ScaledSpace.InverseScaleFactor;
                double w=settings.Geometry.P.Width/2;
                double[] cross={-w-wallThickness,-w-wallThickness,-w,-w,w,w,w+wallThickness,w+wallThickness};
                double[] height={-floorDrop-settings.StructuralThickness,settings.Geometry.P.WallHeight,settings.Geometry.P.WallHeight,-floorDrop,-floorDrop,settings.Geometry.P.WallHeight,settings.Geometry.P.WallHeight,-floorDrop-settings.StructuralThickness};
                for(int i=0;i<=n;i++)
                {
                    double along=i*settings.Geometry.P.Circumference/n;
                    for(int j=0;j<8;j++){verts[i*8+j]=ConvertVector.Unity(settings.Geometry.Position(along,cross[j],height[j])*s);mapVertices[i*8+j]=ConvertVector.Unity(settings.Geometry.Position(along,cross[j],height[j]+(height[j]<0?Math.Max(512,settings.Geometry.P.Radius*1e-6):0))*s);uv[i*8+j]=new Vector2((float)((double)i/n),(float)(cross[j]/settings.Geometry.P.Width+.5));}
                    if(i==n)continue;
                    for(int j=0;j<8;j++)
                    {
                        int a=i*8+j,b=i*8+(j+1)%8,c=a+8,d=b+8;
                        // The closed cross-section has outward-facing triangles. Back-face culling
                        // prevents the underside from competing with the interior at scaled-space depth precision.
                        (j==3?floorTriangles:triangles).AddRange(new[]{a,b,c,b,d,c});
                    }
                }
                flightVertices=verts;mapGeometry=false;
                ring=new Mesh{name="Ringworld scaled ribbon",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};ring.vertices=verts;ring.uv=uv;ring.subMeshCount=2;ring.SetTriangles(triangles,0);ring.SetTriangles(floorTriangles,1);ring.RecalculateBounds();
                root.AddComponent<MeshFilter>().sharedMesh=ring;ribbonRenderer=root.AddComponent<MeshRenderer>();ribbonRenderer.sharedMaterials=new[]{dark,material};
                squares=new GameObject("Twenty shadow squares");squares.transform.SetParent(root.transform,false);squares.layer=10;
                double squareRadius=settings.Geometry.P.Radius*(46.0/153.0);
                for(int i=0;i<20;i++)
                {
                    double a=i*2*Math.PI/20;
                    var square=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(square.GetComponent<Collider>());
                    square.name="Shadow square "+(i+1);square.layer=10;square.transform.SetParent(squares.transform,false);
                    square.transform.localPosition=new Vector3((float)(Math.Cos(a)*squareRadius*s),0,(float)(-Math.Sin(a)*squareRadius*s));
                    square.transform.localRotation=Quaternion.Euler(0,(float)(a*180/Math.PI),0);
                    square.transform.localScale=new Vector3((float)(1000*s),(float)(settings.Geometry.P.Width*s),(float)(settings.Geometry.P.Radius*(4.0/153)*s));
                    square.GetComponent<Renderer>().sharedMaterial=dark;
                }
                UpdateDistantSurface(RingworldFlight.Instance);
                Debug.Log("[NivenRingworld] Scaled ribbon and 20 shadow squares created.");
            }
            catch(Exception e){Debug.LogException(e);}
        }
        public void LateUpdate()
        {
            if(root==null||star==null)return;
            if(RingworldScenario.Instance!=null&&loadedOptions!=RingworldScenario.Instance.GetOptions()){OnDestroy();Start();if(root==null)return;}
            var current=RingworldFlight.Instance;
            if(current!=null&&current.Settings!=null&&(settings.Geometry.P.Radius!=current.Settings.Geometry.P.Radius||settings.Geometry.P.Width!=current.Settings.Geometry.P.Width||settings.Geometry.P.WallHeight!=current.Settings.Geometry.P.WallHeight||settings.Geometry.P.Gravity!=current.Settings.Geometry.P.Gravity))
            {OnDestroy();Start();if(root==null)return;}
            bool mapView=MapView.MapIsEnabled||HighLogic.LoadedScene==GameScenes.TRACKSTATION;
            if(mapGeometry!=mapView){mapGeometry=mapView;ring.vertices=mapGeometry?mapVertices:flightVertices;ring.RecalculateBounds();}
            UpdateDistantSurface(current);
            root.transform.position=(Vector3)ScaledSpace.LocalToScaledSpace(star.position);
            Shader.SetGlobalVector("_RingScaledCenter",root.transform.position);Shader.SetGlobalVector("_RingScaledSize",new Vector4((float)(settings.Geometry.P.Radius*ScaledSpace.InverseScaleFactor),(float)(settings.Geometry.P.Width*.5*ScaledSpace.InverseScaleFactor),0,0));
            // Squares and material longitude use the same phase in both flight charts.
            var flight=RingworldFlight.Instance;
            double epoch=flight!=null&&flight.Active?flight.FrameEpoch:Planetarium.GetUniversalTime();
            root.transform.rotation=Quaternion.Euler(0,(float)(settings.Geometry.P.Omega*epoch*180/Math.PI),0);
            squares.transform.localRotation=Quaternion.Euler(0,(float)(RingGeometry.Wrap(Planetarium.GetUniversalTime()/(flight!=null&&flight.Settings!=null?flight.Settings.Geometry.P.DaySeconds:settings.Geometry.P.DaySeconds),20)*18),0);
        }
        private void UpdateDistantSurface(RingworldFlight flight)
        {
            var options=flight!=null&&flight.Settings!=null?flight.Settings:settings;
            bool enabled=options.FullRingDetail||(flight!=null&&flight.visuals!=null&&flight.visuals.PhotoActive);
            if(!farAttempted)
            {
                farAttempted=true;
                try
                {
                    visualBundle=RingVisualAssets.Acquire();
                    var shader=visualBundle!=null?visualBundle.LoadAsset<Shader>("Assets/Shaders/DistantSurface.shader"):null;
                    if(shader!=null&&shader.isSupported){farMaterial=new Material(shader){renderQueue=2000};wallMaterial=new Material(shader){renderQueue=2000};ribbonRenderer.sharedMaterials=new[]{dark,farMaterial};globalClouds=new GlobalClouds(root.transform,options,visualBundle);Debug.Log("[NivenRingworld] Full-ring distant surface and cloud shaders ready.");}
                    else Debug.LogWarning("[NivenRingworld] Distant surface shader unavailable; using plain ribbon.");
                }
                catch(Exception e){Debug.LogWarning("[NivenRingworld] Distant surface: "+e.Message);}
            }
            farActive=enabled&&farMaterial!=null;
            if(farMaterial==null)return;
            farMaterial.SetFloat("_Detail",enabled?1:0);wallMaterial.SetFloat("_Detail",0);
            farMaterial.SetFloat("_CircumferenceKm",(float)(options.Geometry.P.Circumference/1000));
            farMaterial.SetFloat("_WidthKm",(float)(options.Geometry.P.Width/1000));
            uint seed=unchecked((uint)options.Geometry.P.Seed);
            farMaterial.SetFloat("_SeedLow",seed&65535);farMaterial.SetFloat("_SeedHigh",seed>>16);farMaterial.SetFloat("_Generation",options.GenerationVersion);
            double time=Planetarium.GetUniversalTime();
            if(globalClouds!=null)globalClouds.Update(options,flight,star,time);
            float phase=(float)RingGeometry.Wrap(time/options.Geometry.P.DaySeconds,1);
            farMaterial.SetFloat("_DayPhase",phase);wallMaterial.SetFloat("_DayPhase",phase);Shader.SetGlobalFloat("_RingNightPhase",phase);
            farMaterial.SetFloat("_CloudAmount",(float)options.CloudAmount);
            farMaterial.SetFloat("_CloudDrift",options.DynamicWeather?(float)RingGeometry.Wrap(time*8/options.Geometry.P.Circumference,1):0);
        }
        public void OnDestroy()
        {
            if(globalClouds!=null)globalClouds.Dispose();globalClouds=null;
            if(root!=null)Destroy(root);if(ring!=null)Destroy(ring);if(material!=null)Destroy(material);if(dark!=null)Destroy(dark);
            if(wallMaterial!=null)Destroy(wallMaterial);if(farMaterial!=null)Destroy(farMaterial);if(visualBundle!=null)RingVisualAssets.Release();
            root=null;ring=null;material=null;dark=null;farMaterial=null;wallMaterial=null;visualBundle=null;ribbonRenderer=null;farAttempted=false;farActive=false;
        }
    }
}
