#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using HarmonyLib;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class LandmarkSmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            var savedOptions=f.Settings.Save();
            for(int preset=0;preset<RingQualityPresets.Names.Length;preset++)
            {
                var options=f.Settings.Save();RingQualityPresets.Apply(options,preset);
                var check=Settings.Load();check.Apply(options);
                if(RingQualityPresets.Match(check)!=RingQualityPresets.Names[preset]||check.Geometry.P.Seed!=f.Settings.Geometry.P.Seed||check.ForestDensity!=f.Settings.ForestDensity)
                {fail("Quality preset roundtrip/world preservation: "+RingQualityPresets.Names[preset]);yield break;}
                if(preset>=6&&(check.ForestQuality!=0||check.LodRange!=160000000)){fail("Low preset forest/horizon regression");yield break;}
            }
            Debug.Log("[RingworldSmoke] QUALITY all 11 presets roundtrip; seed/density preserved; low tiers Economy/160000km");
            f.arrivalHeight=600;f.Visit();while(!f.Ready)yield return null;
            for(int i=0;i<45;i++)yield return null;
            int count=0;GameObject palace=null;
            foreach(var obj in UnityEngine.Object.FindObjectsOfType<LODGroup>())
                if(obj.name.StartsWith("Ringworld landmark:")){count++;if(obj.name.EndsWith("palace_lotus"))palace=obj.gameObject;}
            Debug.Log("[RingworldSmoke] LANDMARKS streamed="+count);
            if(count<10||palace==null){fail("Architectural landmarks did not stream");yield break;}
            Capture(palace.transform.position,new Vector3(3800,2200,-3800),"landmark-palace.png",palace.transform.up);
            foreach(var group in UnityEngine.Object.FindObjectsOfType<LODGroup>())
                if(group.name.StartsWith("Ringworld landmark:")&&group.transform.localScale.y>=8000){fail("Colossus crowded the landmark region");yield break;}
            bool rareFound=false;ColossusCandidate rare=new ColossusCandidate();
            for(int cell=0;cell<1000&&!rareFound;cell++)
                foreach(var candidate in ColossusDistribution.Nearby(f.Settings.Terrain,cell*2000000,0))
                    if(!f.Settings.Terrain.Sample(candidate.Along,candidate.Across).Wet){rare=candidate;rareFound=true;break;}
            if(!rareFound){fail("No rare dry colossus candidate");yield break;}
            AccessTools.Method(typeof(RingworldFlight),"VisitCoordinates").Invoke(f,new object[]{rare.Along+200000,rare.Across});while(!f.Ready)yield return null;
            for(int i=0;i<45;i++)yield return null;
            GameObject colossus=null;int colossalCount=0;
            foreach(var group in UnityEngine.Object.FindObjectsOfType<LODGroup>())
                if(group.name.StartsWith("Ringworld landmark:")&&group.transform.localScale.y>=8000){colossalCount++;colossus=group.gameObject;}
            if(colossus==null||colossalCount>1){fail("Rare isolated colossus streaming failed: "+colossalCount);yield break;}
            Debug.Log("[RingworldSmoke] COLOSSI rare seed cell="+rare.Key+" streamed="+colossalCount+" sizeMetres="+colossus.transform.localScale);
            f.Visit();while(!f.Ready)yield return null;for(int i=0;i<35;i++)yield return null;
            var surface=(SurfaceStreamer)AccessTools.Field(typeof(RingworldFlight),"surface").GetValue(f);
            // Find a dense, gently sloping forest without altering the save's seed.
            var at=f.Settings.Geometry.Coordinates(f.Position(FlightGlobals.ActiveVessel));double fa=0,fb=0;bool found=false;
            for(int i=0;i<2000&&!found;i++)
            {
                double a=at.Along+(i%50-25)*1600,b=at.Across+(i/50-20)*1600;var t=f.Settings.Terrain.Sample(a,b);
                if(!t.Wet&&Ecology.Sample(f.Settings.Terrain,a,b).TreeCover>.65&&Math.Abs(f.Settings.Terrain.Sample(a+28,b+28).Height-t.Height)<2){fa=a;fb=b;found=true;}
            }
            if(!found){fail("Forest fixture not found");yield break;}
            // Paused canopy work must keep its patch frame if a relocation changes
            // the live orientation before the following row is sampled.
            double originalPhase=f.Settings.Geometry.OrientationRadians;
            var probeBlock=new LodBlock{X=Math.Floor(fa/1024)*1024,Y=Math.Floor(fb/1024)*1024,Size=1024};
            var probe=ForestCanopy.DistantMesh(f.Settings,probeBlock,f.Settings.Geometry.Position(probeBlock.X,probeBlock.Y,0),originalPhase);
            Mesh probeMesh=null;
            try
            {
                probe.MoveNext();f.Settings.Geometry.OrientationRadians=originalPhase+.1;
                while(probe.MoveNext())if(probe.Current!=null)probeMesh=probe.Current;
            }
            finally{f.Settings.Geometry.OrientationRadians=originalPhase;probe.Dispose();}
            if(probeMesh==null||probeMesh.bounds.size.magnitude>5000||probeMesh.bounds.center.magnitude>5000){fail("Queued biome geometry lost its coordinate frame");yield break;}
            UnityEngine.Object.Destroy(probeMesh);Debug.Log("[RingworldSmoke] BIOME queued-frame isolation passed");
            f.arrivalHeight=100;AccessTools.Method(typeof(RingworldFlight),"VisitCoordinates").Invoke(f,new object[]{fa,fb});while(!f.Ready)yield return null;
            for(int i=0;i<35;i++)yield return null;
            int crowns=0,contacts=0,patches=0;
            foreach(var canopy in UnityEngine.Object.FindObjectsOfType<ForestCanopy>())
            {
                crowns+=canopy.TreeCount;contacts+=canopy.ContactCount;
                foreach(var group in canopy.GetComponentsInChildren<LODGroup>())if(group.name=="Continuous forest patch")patches++;
            }
            Debug.Log("[RingworldSmoke] FOREST crowns="+crowns+" patches="+patches+" trunkContacts="+contacts+" tiles="+surface.TileCount);
            int patchesPerSide=(int)Math.Ceiling(f.Settings.TileSize/256);
            if(crowns<10000||contacts>600||patches>surface.TileCount*patchesPerSide*patchesPerSide){fail("Continuous forest density/contact budget regression");yield break;}
            var v=FlightGlobals.ActiveVessel;var pos=f.Position(v);var up=ConvertVector.Unity(f.Settings.Geometry.Up(pos));
            Capture((Vector3)(f.Star.position+ConvertVector.Ksp(pos)),new Vector3(180,180,-220),"forest-groves.png",up);
            // Complete enough adjacent blocks to inspect beyond the former square.
            var streamWatch=System.Diagnostics.Stopwatch.StartNew();int streamFrames=0;
            while((surface.LodPending>0||surface.CanopyPending>0)&&streamWatch.Elapsed.TotalSeconds<150){streamFrames++;yield return null;}
            Debug.Log("[RingworldSmoke] BIOME streamingFPS="+(streamFrames/streamWatch.Elapsed.TotalSeconds).ToString("F1")+" seconds="+streamWatch.Elapsed.TotalSeconds.ToString("F1")+" terrainPending="+surface.LodPending+" canopyPending="+surface.CanopyPending);
            if(surface.LodPending>0||surface.CanopyPending>0){fail("Biome LOD generation did not complete within fixture budget");yield break;}
            int crownsLod=0;foreach(var mesh in UnityEngine.Object.FindObjectsOfType<MeshFilter>())if(mesh.name=="Ring biome LOD canopy")crownsLod++;
            if(crownsLod<10){fail("Intermediate canopy LOD missing");yield break;}
            Debug.Log("[RingworldSmoke] BIOME crown LOD patches="+crownsLod);
            int canopyBlocks=0;foreach(var mesh in UnityEngine.Object.FindObjectsOfType<MeshFilter>())
                if(mesh.sharedMesh!=null&&mesh.sharedMesh.name=="Adaptive ring terrain block")canopyBlocks++;
            Debug.Log("[RingworldSmoke] BIOME LOD blocks="+canopyBlocks);
            if(canopyBlocks<100){fail("Distant biome blocks did not stream");yield break;}
            Capture((Vector3)(f.Star.position+ConvertVector.Ksp(pos)),new Vector3(7000,11000,-14000),"forest-horizon.png",up);
            var watch=System.Diagnostics.Stopwatch.StartNew();int frames=0;while(watch.Elapsed.TotalSeconds<5){frames++;yield return null;}
            Debug.Log("[RingworldSmoke] FOREST observedFPS="+(frames/watch.Elapsed.TotalSeconds).ToString("F1")+" managedMB="+(GC.GetTotalMemory(false)/1048576));
            long before=ForestVertices();var economy=f.Settings.Save();economy.SetValue("forestQuality",0,true);f.ApplyOptions(economy,false);
            for(int i=0;i<65;i++)yield return null;
            var economyWatch=System.Diagnostics.Stopwatch.StartNew();while((surface.LodPending>0||surface.SceneryPending>0)&&economyWatch.Elapsed.TotalSeconds<150)yield return null;
            long after=ForestVertices();int remaining=0,physicalTrees=0;
            foreach(var mesh in UnityEngine.Object.FindObjectsOfType<MeshFilter>())if(mesh.name=="Ring biome LOD canopy")remaining++;
            foreach(var canopy in UnityEngine.Object.FindObjectsOfType<ForestCanopy>())physicalTrees+=canopy.TreeCount;
            if(after>=before/2||remaining!=0||Math.Abs(physicalTrees-crowns)>4||surface.SceneryPending>0||surface.LodPending>0||surface.CanopyPending>0)
            {fail("Economy forest reduction/contact distribution regression: "+before+" -> "+after+" trees="+physicalTrees+" distant="+remaining);yield break;}
            Debug.Log("[RingworldSmoke] QUALITY Economy near vertices="+before+" -> "+after+"; physical candidates="+physicalTrees+" (before "+crowns+", craft-clearance tolerance 4); no distant crown meshes; queues complete");
            Capture((Vector3)(f.Star.position+ConvertVector.Ksp(pos)),new Vector3(7000,11000,-14000),"forest-economy.png",up);
            f.ApplyOptions(savedOptions,false);

        }
        private static long ForestVertices()
        {
            long count=0;foreach(var canopy in UnityEngine.Object.FindObjectsOfType<ForestCanopy>())
                foreach(var mesh in canopy.GetComponentsInChildren<MeshFilter>())if(mesh.sharedMesh!=null&&mesh.sharedMesh.name=="Merged continuous forest crowns")count+=mesh.sharedMesh.vertexCount;
            return count;
        }
        private static void Capture(Vector3 centre,Vector3 offset,string name,Vector3 up)
        {
            var obj=new GameObject("Landmark review camera");var camera=obj.AddComponent<Camera>();camera.enabled=false;
            var rotation=Quaternion.FromToRotation(Vector3.up,up);camera.transform.position=centre+rotation*offset;camera.transform.rotation=Quaternion.LookRotation(centre-camera.transform.position,up);
            camera.nearClipPlane=1;camera.farClipPlane=300000;camera.cullingMask=1<<15;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.40f,.55f,.66f);
            var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();
            var path=Path.GetFullPath(Path.Combine(KSPUtil.ApplicationRootPath,name.StartsWith("colossus")?"../art/colossus-kit/validation":"../art/landmark-kit/validation",name));File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(obj);
        }
    }
}
#endif
