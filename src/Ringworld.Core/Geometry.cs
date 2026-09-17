using System;

namespace Ringworld.Core
{
    public struct DVec
    {
        public readonly double X, Y, Z;
        public DVec(double x, double y, double z) { X = x; Y = y; Z = z; }
        public double Length { get { return Math.Sqrt(X * X + Y * Y + Z * Z); } }
        public DVec Unit { get { double m = Length; return m > 0 ? this / m : new DVec(); } }
        public static DVec operator +(DVec a, DVec b) { return new DVec(a.X+b.X,a.Y+b.Y,a.Z+b.Z); }
        public static DVec operator -(DVec a, DVec b) { return new DVec(a.X-b.X,a.Y-b.Y,a.Z-b.Z); }
        public static DVec operator -(DVec a) { return a * -1; }
        public static DVec operator *(DVec a, double b) { return new DVec(a.X*b,a.Y*b,a.Z*b); }
        public static DVec operator /(DVec a, double b) { return a * (1/b); }
        public static DVec Cross(DVec a, DVec b) { return new DVec(a.Y*b.Z-a.Z*b.Y,a.Z*b.X-a.X*b.Z,a.X*b.Y-a.Y*b.X); }
        public static double Dot(DVec a, DVec b) { return a.X*b.X+a.Y*b.Y+a.Z*b.Z; }
    }

    public sealed class RingParameters
    {
        public double SurfaceDensity = 0; // kg/m²; zero preserves legacy massless simulations
        public double Radius = 15300000000;
        public double Width = 160500000;
        public double WallHeight = 160000;
        public double Gravity = 9.72;
        public double DaySeconds = 10800;
        public double AtmosphereHeight = 60000;
        public double ScaleHeight = 8000;
        public int Seed = 1970;
        public double Circumference { get { return 2*Math.PI*Radius; } }
        public double Omega { get { return Math.Sqrt(Gravity/Radius); } }
        public double RotationSeconds { get { return 2*Math.PI/Omega; } }
        public void Validate()
        {
            // Preserve centimetre-scale contact coordinates and representable terrain
            // cell indices. This is a numeric precision constraint, not a world-size preset.
            if (!Finite(Circumference) || Radius + .01 == Radius || Width + .01 == Width || Circumference >= long.MaxValue)
                throw new ArgumentException("Ring dimensions exceed coordinate precision (centimetre offsets must remain representable).");
            if (!Finite(SurfaceDensity) || SurfaceDensity<0 || !Finite(Radius) || Radius < 1000000 || !Finite(Width) || Width < 10000 || Width > Radius ||
                !Finite(WallHeight) || WallHeight <= 0 || WallHeight >= Radius/10 ||
                !Finite(Gravity) || Gravity <= 0 || Gravity > 100 || !Finite(DaySeconds) || DaySeconds < 60 ||
                !Finite(AtmosphereHeight) || AtmosphereHeight < 100 || AtmosphereHeight > WallHeight ||
                !Finite(ScaleHeight) || ScaleHeight <= 0) throw new ArgumentException("Invalid ring dimensions or atmosphere.");
        }
        public static bool Finite(double x) { return !double.IsInfinity(x) && !double.IsNaN(x); }
    }

    public struct RingPoint
    {
        public double Along, Across, Altitude;
        public RingPoint(double along, double across, double altitude) { Along=along; Across=across; Altitude=altitude; }
    }

    public sealed class RingGeometry
    {
        public readonly RingParameters P;
        // Orientation of material longitude zero in the current physics chart.
        public double OrientationRadians;
        public RingGeometry(RingParameters p) { p.Validate(); P=p; }
        public static double Wrap(double x, double period) { return x-period*Math.Floor(x/period); }
        public double AlongDistance(double a, double b) { return Wrap(a-b+P.Circumference/2,P.Circumference)-P.Circumference/2; }
        // Axis is +Y, floor normal is inward. Positive Along follows omega cross radius.
        public DVec Position(double along, double across, double altitude)
        {
            double t=Wrap(along,P.Circumference)/P.Radius+OrientationRadians, r=P.Radius-altitude;
            return new DVec(r*Math.Cos(t),across,-r*Math.Sin(t));
        }
        public RingPoint Coordinates(DVec p)
        {
            return new RingPoint(Wrap(Math.Atan2(-p.Z,p.X)-OrientationRadians,2*Math.PI)*P.Radius,p.Y,P.Radius-Math.Sqrt(p.X*p.X+p.Z*p.Z));
        }
        public static DVec Rotate(DVec v,double radians)
        {
            double c=Math.Cos(radians),s=Math.Sin(radians);
            return new DVec(c*v.X+s*v.Z,v.Y,-s*v.X+c*v.Z);
        }
        public DVec ToInertialPosition(DVec p,double elapsed) { return Rotate(p,P.Omega*elapsed); }
        public DVec ToInertialVelocity(DVec p,DVec v,double elapsed) { return Rotate(InertialVelocity(p,v),P.Omega*elapsed); }
        // Hysteresis encompasses the rim tops as well as the air. No speed threshold:
        // fast arrivals retain their full air-relative kinetic energy.
        public bool InArrivalRegion(DVec p,bool alreadyInside)
        {
            var c=Coordinates(p);double margin=alreadyInside?100000:50000;
            return c.Altitude>=-margin && c.Altitude<=P.WallHeight+margin && Math.Abs(c.Across)<=P.Width/2+margin;
        }
        public double TimeToArrival(DVec p,DVec velocity,double horizon)
        {
            if(InArrivalRegion(p,false))return 0;
            double best=double.PositiveInfinity;
            double a=velocity.X*velocity.X+velocity.Z*velocity.Z,b=p.X*velocity.X+p.Z*velocity.Z;
            double r=Math.Sqrt(p.X*p.X+p.Z*p.Z);
            foreach(double boundary in new[]{P.Radius+50000,P.Radius-P.WallHeight-50000})
            {
                double c=(r-boundary)*(r+boundary),disc=b*b-a*c;
                if(a<1e-20||disc<0)continue;
                double q=-b-(b>=0?1:-1)*Math.Sqrt(disc);
                foreach(double t in new[]{q/a,Math.Abs(q)>1e-20?c/q:-b/a})
                    if(t>=0&&t<=horizon&&InArrivalRegion(p+velocity*(t+1e-6),false))best=Math.Min(best,t);
            }
            if(Math.Abs(velocity.Y)>1e-12)
                foreach(double y in new[]{-P.Width/2-50000,P.Width/2+50000})
                {double t=(y-p.Y)/velocity.Y;if(t>=0&&t<=horizon&&InArrivalRegion(p+velocity*(t+1e-6),false))best=Math.Min(best,t);}
            return best;
        }
        public DVec Up(DVec p) { return new DVec(-p.X,0,-p.Z).Unit; }
        public DVec SpinVelocity(DVec p) { return DVec.Cross(new DVec(0,P.Omega,0),p); }
        public DVec InertialVelocity(DVec p, DVec rotatingVelocity) { return rotatingVelocity+SpinVelocity(p); }
        public DVec RotatingVelocity(DVec p, DVec inertialVelocity) { return inertialVelocity-SpinVelocity(p); }
        public DVec Acceleration(DVec p, DVec rotatingVelocity, double starMu, bool includeRibbon=true)
        {
            DVec omega=new DVec(0,P.Omega,0);
            double r=p.Length;
            DVec stellar=r>1 ? p*(-starMu/(r*r*r)) : new DVec();
            return stellar+(includeRibbon?RibbonGravity.Acceleration(p,P,P.SurfaceDensity):new DVec())-DVec.Cross(omega,DVec.Cross(omega,p))-DVec.Cross(omega,rotatingVelocity)*2;
        }
        public double Density(double altitude)
        {
            if(altitude>=P.AtmosphereHeight) return 0;
            double fade=Math.Min(1, Math.Max(0,(P.AtmosphereHeight-altitude)/5000));
            return 1.225*Math.Exp(-Math.Max(0,altitude)/P.ScaleHeight)*fade*fade;
        }
        public double Daylight(double along, double time)
        {
            // Twenty independently orbiting shadow squares; relative cadence sets the day.
            double phase=Wrap(20*along/P.Circumference-time/P.DaySeconds,1);
            double edge=Math.Min(phase,1-phase);
            // Same 4-million-km tangential panel length and 46-million-km orbital radius
            // ratio as the scaled models. The small penumbra is an artistic approximation.
            double halfShadow=10*Math.Atan(2.0/46.0)/Math.PI;
            return Math.Max(0,Math.Min(1,(edge-halfShadow)/.02));
        }
    }
}
