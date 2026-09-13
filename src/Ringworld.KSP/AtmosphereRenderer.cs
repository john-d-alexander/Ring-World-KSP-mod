using System;
using System.Collections.Generic;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    // CPU scattering on a low-resolution sky mesh avoids a Unity editor/asset-bundle
    // dependency. Depth-tested cloud sheets share the double-precision terrain chart.
    internal sealed class AtmosphereRenderer : IDisposable
    {
        private readonly Settings settings;
        private readonly RingAtmosphere model;
        private readonly GameObject sky,clouds;
        private readonly Mesh skyMesh,cloudMesh;
        private readonly Material skyMaterial,cloudMaterial;
        private readonly Vector3[] directions;
        private readonly Color[] skyColors;
        private double cloudAlong=double.NaN,cloudAcross,cloudPhase;
        private DVec cloudAnchor;
        private float nextSky,nextCloud;
        internal AtmosphereRenderer(Settings settings)
        {
            this.settings=settings;model=new RingAtmosphere(settings.Geometry);
            var shader=Shader.Find("Sprites/Default");
            if(shader==null)throw new InvalidOperationException("Sprites/Default shader unavailable for ring atmosphere.");
            skyMaterial=new Material(shader){renderQueue=1000};cloudMaterial=new Material(shader){renderQueue=3000};
            sky=new GameObject("Ringworld single-scattering sky");sky.layer=15;
            clouds=new GameObject("Ringworld procedural cloud layers");clouds.layer=15;
            const int columns=48,rows=24;
            directions=new Vector3[(columns+1)*(rows+1)];skyColors=new Color[directions.Length];
            var triangles=new List<int>();
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                double a=x*Math.PI*2/columns,b=-Math.PI/2+y*Math.PI/rows;
                directions[y*(columns+1)+x]=new Vector3((float)(Math.Cos(a)*Math.Cos(b)),(float)Math.Sin(b),(float)(Math.Sin(a)*Math.Cos(b)));
                if(x<columns&&y<rows)AddQuad(triangles,y*(columns+1)+x,columns+1);
            }
            skyMesh=new Mesh{name="Integrated Rayleigh-Mie sky"};skyMesh.vertices=directions;skyMesh.SetTriangles(triangles,0);skyMesh.colors=skyColors;skyMesh.RecalculateBounds();
            sky.AddComponent<MeshFilter>().sharedMesh=skyMesh;sky.AddComponent<MeshRenderer>().sharedMaterial=skyMaterial;
            cloudMesh=new Mesh{name="Material-coordinate cloud field"};
            clouds.AddComponent<MeshFilter>().sharedMesh=cloudMesh;clouds.AddComponent<MeshRenderer>().sharedMaterial=cloudMaterial;
            sky.SetActive(false);clouds.SetActive(false);
        }
        internal void Update(bool enabled,Vector3d star)
        {
            var camera=FlightCamera.fetch==null?null:FlightCamera.fetch.mainCamera;
            if(camera==null)enabled=false;
            DVec observer=camera==null?new DVec():ConvertVector.Core((Vector3d)camera.transform.position-star);
            var c=settings.Geometry.Coordinates(observer);
            enabled=enabled&&settings.Atmosphere&&c.Altitude>-1000&&c.Altitude<600000&&Math.Abs(c.Across)<settings.Geometry.P.Width/2+500000;
            sky.SetActive(enabled);clouds.SetActive(enabled);if(!enabled)return;
            sky.transform.position=camera.transform.position;
            sky.transform.localScale=Vector3.one*1000;
            double time=Planetarium.GetUniversalTime();
            if(Time.realtimeSinceStartup>=nextSky)
            {
                nextSky=Time.realtimeSinceStartup+.2f;
                for(int i=0;i<directions.Length;i++)
                {
                    var s=model.Sky(observer,ConvertVector.Core(directions[i]),time);
                    // Store straight alpha; the stock sprite shader premultiplies it.
                    double alpha=Math.Max(.000001,s.Opacity);
                    skyColors[i]=new Color((float)(1-Math.Exp(-s.Radiance.X))/ (float)alpha,(float)(1-Math.Exp(-s.Radiance.Y))/(float)alpha,(float)(1-Math.Exp(-s.Radiance.Z))/(float)alpha,(float)s.Opacity);
                }
                skyMesh.colors=skyColors;
            }
            if(double.IsNaN(cloudAlong)||Math.Abs(settings.Geometry.AlongDistance(c.Along,cloudAlong))>12000||Math.Abs(c.Across-cloudAcross)>12000||Time.realtimeSinceStartup>nextCloud)
            {
                BuildClouds(c,time);nextCloud=Time.realtimeSinceStartup+8;
            }
            double phase=settings.Geometry.OrientationRadians-cloudPhase;
            clouds.transform.rotation=Quaternion.AngleAxis((float)(phase*180/Math.PI),Vector3.up);
            clouds.transform.position=(Vector3)(star+ConvertVector.Ksp(RingGeometry.Rotate(cloudAnchor,phase)));
        }
        private void BuildClouds(RingPoint observer,double time)
        {
            cloudAlong=observer.Along;cloudAcross=observer.Across;cloudPhase=settings.Geometry.OrientationRadians;
            cloudAnchor=settings.Geometry.Position(cloudAlong,cloudAcross,0);
            const int n=96;const double extent=300000;
            var vertices=new Vector3[(n+1)*(n+1)*3];var colors=new Color[vertices.Length];var triangles=new List<int>();
            // Three translucent decks provide depth while flying through the cloud band.
            for(int layer=0;layer<3;layer++)for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
            {
                int i=layer*(n+1)*(n+1)+y*(n+1)+x;
                double da=(x/(double)n*2-1)*extent,db=(y/(double)n*2-1)*extent;
                double along=cloudAlong+da,across=cloudAcross+db,height=4800+layer*650;
                vertices[i]=ConvertVector.Unity(settings.Geometry.Position(along,across,height)-cloudAnchor);
                double cover=model.CloudCoverage(along,across,time);
                // Mountains pierce the cloud deck; fade the finite patch boundary.
                if(Math.Abs(across)>settings.Geometry.P.Width/2||settings.Terrain.Sample(along,across).Height>height)cover=0;
                double fade=Math.Max(0,Math.Min(1,(extent-Math.Max(Math.Abs(da),Math.Abs(db)))/60000));
                float light=(float)(.12+.88*settings.Geometry.Daylight(along,time));
                colors[i]=new Color(light*.92f,light*.96f,light,(float)(cover*fade*.38));
                if(x<n&&y<n)AddQuad(triangles,i,n+1);
            }
            cloudMesh.Clear();cloudMesh.vertices=vertices;cloudMesh.colors=colors;cloudMesh.SetTriangles(triangles,0);cloudMesh.RecalculateBounds();
        }
        private static void AddQuad(List<int> list,int a,int stride)
        {int b=a+1,c=a+stride,d=c+1;list.AddRange(new[]{a,b,c,b,d,c});}
        public void Dispose()
        {
            UnityEngine.Object.Destroy(sky);UnityEngine.Object.Destroy(clouds);
            UnityEngine.Object.Destroy(skyMesh);UnityEngine.Object.Destroy(cloudMesh);
            UnityEngine.Object.Destroy(skyMaterial);UnityEngine.Object.Destroy(cloudMaterial);
        }
    }
}
