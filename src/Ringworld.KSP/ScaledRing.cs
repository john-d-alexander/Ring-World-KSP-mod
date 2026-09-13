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
        private Material material,dark;
        private CelestialBody star;
        private Settings settings;
        public void Start()
        {
            try
            {
                settings=Settings.Load();star=FlightGlobals.Bodies.Find(b=>b.name=="Sun");if(star==null||ScaledSpace.Instance==null)return;
                Shader shader=Shader.Find("Unlit/Color")??Shader.Find("KSP/Unlit");if(shader==null)return;
                material=new Material(shader){color=new Color(.34f,.47f,.32f)};dark=new Material(shader){color=new Color(.012f,.015f,.019f)};
                root=new GameObject("Niven Ringworld scaled habitat");root.layer=10;
                int n=8192;var verts=new Vector3[(n+1)*4];var triangles=new List<int>();double s=ScaledSpace.InverseScaleFactor;
                for(int i=0;i<=n;i++)
                {
                    double along=i*settings.Geometry.P.Circumference/n;
                    for(int j=0;j<4;j++)
                    {
                        double across=(j<2?-1:1)*settings.Geometry.P.Width/2;
                        double altitude=(j==0||j==3)?settings.Geometry.P.WallHeight:0;
                        verts[i*4+j]=ConvertVector.Unity(settings.Geometry.Position(along,across,altitude)*s);
                    }
                    if(i==n)continue;
                    for(int j=0;j<3;j++)
                    {
                        int a=i*4+j,b=a+1,c=a+4,d=c+1;
                        triangles.AddRange(new[]{a,b,c,b,d,c,c,b,a,c,d,b});
                    }
                }
                ring=new Mesh{name="Ringworld scaled ribbon"};ring.vertices=verts;ring.SetTriangles(triangles,0);ring.RecalculateBounds();
                root.AddComponent<MeshFilter>().sharedMesh=ring;root.AddComponent<MeshRenderer>().sharedMaterial=material;
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
                Debug.Log("[NivenRingworld] Scaled ribbon and 20 shadow squares created.");
            }
            catch(Exception e){Debug.LogException(e);}
        }
        public void LateUpdate()
        {
            if(root==null||star==null)return;
            root.transform.position=(Vector3)ScaledSpace.LocalToScaledSpace(star.position);
            // Squares and material longitude use the same phase in both flight charts.
            var flight=RingworldFlight.Instance;
            double epoch=flight!=null&&flight.Active?flight.FrameEpoch:Planetarium.GetUniversalTime();
            root.transform.rotation=Quaternion.Euler(0,(float)(settings.Geometry.P.Omega*epoch*180/Math.PI),0);
            squares.transform.localRotation=Quaternion.Euler(0,(float)(RingGeometry.Wrap(Planetarium.GetUniversalTime()/settings.Geometry.P.DaySeconds,20)*18),0);
        }
        public void OnDestroy(){if(root!=null)Destroy(root);if(ring!=null)Destroy(ring);if(material!=null)Destroy(material);if(dark!=null)Destroy(dark);}
    }
}
