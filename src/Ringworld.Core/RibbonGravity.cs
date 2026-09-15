using System;
namespace Ringworld.Core
{
    // Uniform cylindrical ribbon: integrate width analytically and azimuth with
    // Gaussian quadrature on logarithmic panels. Softening represents unresolved
    // structural thickness, not a point-mass SOI or centrifugal attraction.
    public static class RibbonGravity
    {
        public const double G=6.67430e-11;
        private static readonly double[] Nodes={-.8611363115940526,-.3399810435848563,.3399810435848563,.8611363115940526};
        private static readonly double[] Weights={.3478548451374538,.6521451548625461,.6521451548625461,.3478548451374538};
        public static DVec Acceleration(DVec p,RingParameters ring,double surfaceDensity,double softening=50,int refinement=1)
        {
            if(!RingParameters.Finite(surfaceDensity)||surfaceDensity<0||!RingParameters.Finite(softening)||softening<=0||refinement<1||refinement>8)throw new ArgumentOutOfRangeException("ribbon gravity parameters");
            if(surfaceDensity==0)return new DVec();
            double r=Math.Sqrt(p.X*p.X+p.Z*p.Z),radius=ring.Radius;
            double low=-ring.Width/2-p.Y,high=ring.Width/2-p.Y;
            double nearestY=Math.Max(0,Math.Abs(p.Y)-ring.Width/2);
            double step=Math.Min(.125,Math.Max(1e-12,Math.Sqrt((r-radius)*(r-radius)+nearestY*nearestY+softening*softening)/radius));
            double radial=0,vertical=0,left=0;
            while(left<Math.PI)
            {
                double right=Math.Min(Math.PI,left+step);
                for(int panel=0;panel<refinement;panel++)
                {
                    double half=(right-left)/(2*refinement),mid=left+(2*panel+1)*half;
                    for(int i=0;i<4;i++)
                    {
                        double angle=mid+half*Nodes[i],sin=Math.Sin(angle/2),oneMinusCos=2*sin*sin;
                        double d=(r-radius)*(r-radius)+2*r*radius*oneMinusCos+softening*softening;
                        double l=Math.Sqrt(d+low*low),h=Math.Sqrt(d+high*high);
                        radial+=half*Weights[i]*(radius-r-radius*oneMinusCos)*(high/h-low/l)/d;
                        // Rationalized difference avoids cancellation on the midplane.
                        vertical+=half*Weights[i]*(high*high-low*low)/(l*h*(l+h));
                    }
                }
                left=right;step=Math.Min(.25,step*1.7);
            }
            double factor=2*G*surfaceDensity*radius;
            return new DVec(r>0?radial*p.X/r*factor:0,vertical*factor,r>0?radial*p.Z/r*factor:0);
        }
    }
    public struct FlightState
    {
        public DVec Position,Velocity;
        public FlightState(DVec p,DVec v){Position=p;Velocity=v;}
    }
    public static class NumericalFlight
    {
        public static FlightState Step(FlightState s,double dt,Func<DVec,DVec,DVec> acceleration)
        {
            var a=acceleration(s.Position,s.Velocity);
            var v2=s.Velocity+a*(dt/2);var a2=acceleration(s.Position+s.Velocity*(dt/2),v2);
            var v3=s.Velocity+a2*(dt/2);var a3=acceleration(s.Position+v2*(dt/2),v3);
            var v4=s.Velocity+a3*dt;var a4=acceleration(s.Position+v3*dt,v4);
            return new FlightState(s.Position+(s.Velocity+v2*2+v3*2+v4)*(dt/6),s.Velocity+(a+a2*2+a3*2+a4)*(dt/6));
        }
    }
}
