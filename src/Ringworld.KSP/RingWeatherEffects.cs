using System;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    // Bounded local precipitation. Streaks are replaced by a quiet veil during high warp.
    internal sealed class RingWeatherEffects : IDisposable
    {
        private readonly GameObject root,boltObject;private readonly Mesh mesh;
        private readonly Material material;private readonly LineRenderer bolt;
        private readonly Vector3[] vertices=new Vector3[384*4];private readonly Color[] colours=new Color[384*4];
        private readonly int[] triangles=new int[384*6];
        internal WeatherSample Current;internal int Drops;internal float Flash;internal bool Veil;
        internal RingWeatherEffects()
        {
            root=new GameObject("Ringworld precipitation");root.layer=15;mesh=new Mesh{name="Bounded ring rain"};mesh.MarkDynamic();
            root.AddComponent<MeshFilter>().sharedMesh=mesh;material=new Material(Shader.Find("Sprites/Default"));root.AddComponent<MeshRenderer>().sharedMaterial=material;
            for(int i=0;i<384;i++){int v=i*4,t=i*6;triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v;triangles[t+4]=v+2;triangles[t+5]=v+3;}
            boltObject=new GameObject("Ringworld lightning");boltObject.layer=15;bolt=boltObject.AddComponent<LineRenderer>();bolt.sharedMaterial=material;bolt.useWorldSpace=false;bolt.positionCount=9;bolt.widthMultiplier=3;bolt.startColor=bolt.endColor=new Color(.8f,.88f,1,1);
            root.SetActive(false);boltObject.SetActive(false);
        }
        internal void Update(bool allowed,Settings s,Vector3d star)
        {
            var camera=FlightCamera.fetch!=null?FlightCamera.fetch.mainCamera:null;Drops=0;Flash=0;Veil=false;
            if(!allowed||camera==null){root.SetActive(false);boltObject.SetActive(false);return;}
            var observer=ConvertVector.Core((Vector3d)camera.transform.position-star);var c=s.Geometry.Coordinates(observer);double time=Planetarium.GetUniversalTime();
            bool inside=s.Atmosphere&&c.Altitude>=0&&c.Altitude<9000&&Math.Abs(c.Across)<s.Geometry.P.Width/2;
            Current=s.Weather(c.Along,c.Across,time);var photo=RingworldFlight.Instance.visuals;int tier=s.VisualQuality;
            Veil=inside&&s.RainEnabled&&Current.Rain>.001&&TimeWarp.CurrentRate>10;
            bool rain=inside&&s.RainEnabled&&Current.Rain>.001&&!Veil;root.SetActive(rain);
            var up=ConvertVector.Unity(s.Geometry.Up(observer));var right=camera.transform.right;
            if(rain)
            {
                Drops=(int)((tier==0?48:tier==1?144:384)*s.RainDensity*Current.Rain);root.transform.position=camera.transform.position;
                var forward=Vector3.Cross(right,up).normalized;
                for(int i=0;i<384;i++)
                {
                    float x=(float)(s.Terrain.Scatter(i,0,1259)*40-20),z=(float)(s.Terrain.Scatter(i,1,1259)*40+5);
                    float y=(float)(RingGeometry.Wrap(s.Terrain.Scatter(i,2,1259)*30-time*24,30)-10);
                    var centre=right*x+forward*z+up*y;float alpha=i<Drops?.25f:0;
                    int v=i*4;vertices[v]=centre-right*.015f;vertices[v+1]=centre+right*.015f;vertices[v+2]=centre+right*.015f+up*1.3f;vertices[v+3]=centre-right*.015f+up*1.3f;
                    for(int j=0;j<4;j++)colours[v+j]=new Color(.58f,.70f,.8f,alpha);
                }
                mesh.vertices=vertices;mesh.colors=colours;mesh.triangles=triangles;mesh.RecalculateBounds();
            }
            Flash=inside&&s.LightningEnabled&&TimeWarp.CurrentRate<=10?(float)RingWeather.Lightning(s.Terrain,time,Current.Storm):0;
            boltObject.SetActive(Flash>.001);
            if(Flash>.001)
            {
                boltObject.transform.position=camera.transform.position;var forward=Vector3.ProjectOnPlane(camera.transform.forward,up).normalized;
                for(int i=0;i<9;i++)bolt.SetPosition(i,forward*1800+right*(float)((s.Terrain.Scatter((long)Math.Floor(time/17),i,1277)-.5)*260)+up*(i*350));
                bolt.startColor=bolt.endColor=new Color(.8f,.88f,1,Flash);
            }
        }
        internal void DrawVeil()
        {
            if(!Veil||Event.current.type!=EventType.Repaint)return;
            var old=GUI.color;GUI.color=new Color(.32f,.39f,.46f,(float)(Current.Rain*.08));GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=old;
        }
        public void Dispose(){UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(boltObject);UnityEngine.Object.Destroy(mesh);UnityEngine.Object.Destroy(material);}
    }
}
