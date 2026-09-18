using System.Collections.Generic;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    // A flexible attached part can chatter forever while its pose is bounded.
    // Observe poses, not a low-pass velocity that could conceal steady motion.
    internal sealed class RingRestPose
    {
        private sealed class Pose {internal Vector3 Position;internal Quaternion Rotation;}
        private readonly Dictionary<Part,Pose> parts=new Dictionary<Part,Pose>();
        private DVec position;private Quaternion rotation;private double epoch;
        private float since,last=-1;private bool initialized;
        internal double PositionError,AngleError;
        internal bool Observe(RingworldFlight f,Vessel v)
        {
            float now=Time.fixedTime;
            if(now==last)return initialized&&now-since>=1;
            last=now;
            var root=v.rootPart;var p=ConvertVector.Core((Vector3d)v.transform.position-f.Center);var q=root.transform.rotation;
            bool reset=!initialized||epoch!=f.FrameEpoch||parts.Count!=v.parts.Count;
            PositionError=initialized?(p-position).Length:0;
            AngleError=initialized?Quaternion.Angle(q,rotation):0;
            var inverse=Quaternion.Inverse(q);
            foreach(var part in v.parts)
            {
                if(part==null){reset=true;continue;}
                Pose old;
                if(!parts.TryGetValue(part,out old)){reset=true;continue;}
                var local=inverse*(part.transform.position-root.transform.position);
                var localRotation=inverse*part.transform.rotation;
                PositionError=System.Math.Max(PositionError,(local-old.Position).magnitude);
                AngleError=System.Math.Max(AngleError,Quaternion.Angle(localRotation,old.Rotation));
            }
            if(!RingParameters.Finite(PositionError)||!RingParameters.Finite(AngleError)||PositionError>.03||AngleError>.5)reset=true;
            if(reset)
            {
                initialized=true;since=now;position=p;rotation=q;epoch=f.FrameEpoch;parts.Clear();
                foreach(var part in v.parts)if(part!=null)parts[part]=new Pose{Position=inverse*(part.transform.position-root.transform.position),Rotation=inverse*part.transform.rotation};
                return false;
            }
            return now-since>=1;
        }
    }
}
