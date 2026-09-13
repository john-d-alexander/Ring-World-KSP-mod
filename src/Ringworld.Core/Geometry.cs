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
            if (!Finite(Radius) || Radius < 1000000 || !Finite(Width) || Width < 10000 || Width > Radius ||
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
        public RingGeometry(RingParameters p) { p.Validate(); P=p; }
        public static double Wrap(double x, double period) { return x-period*Math.Floor(x/period); }
        public double AlongDistance(double a, double b) { return Wrap(a-b+P.Circumference/2,P.Circumference)-P.Circumference/2; }
        // Axis is +Y, floor normal is inward. Positive Along follows omega cross radius.
        public DVec Position(double along, double across, double altitude)
        {
            double t=Wrap(along,P.Circumference)/P.Radius, r=P.Radius-altitude;
            return new DVec(r*Math.Cos(t),across,-r*Math.Sin(t));
        }
        public RingPoint Coordinates(DVec p)
        {
            return new RingPoint(Wrap(Math.Atan2(-p.Z,p.X),2*Math.PI)*P.Radius,p.Y,P.Radius-Math.Sqrt(p.X*p.X+p.Z*p.Z));
        }
        public DVec Up(DVec p) { return new DVec(-p.X,0,-p.Z).Unit; }
        public DVec SpinVelocity(DVec p) { return DVec.Cross(new DVec(0,P.Omega,0),p); }
        public DVec InertialVelocity(DVec p, DVec rotatingVelocity) { return rotatingVelocity+SpinVelocity(p); }
        public DVec RotatingVelocity(DVec p, DVec inertialVelocity) { return inertialVelocity-SpinVelocity(p); }
        public DVec Acceleration(DVec p, DVec rotatingVelocity, double starMu)
        {
            DVec omega=new DVec(0,P.Omega,0);
            double r=p.Length;
            DVec stellar=r>1 ? p*(-starMu/(r*r*r)) : new DVec();
            return stellar-DVec.Cross(omega,DVec.Cross(omega,p))-DVec.Cross(omega,rotatingVelocity)*2;
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
