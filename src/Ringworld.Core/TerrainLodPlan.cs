using System;
using System.Collections.Generic;
using System.Globalization;
namespace Ringworld.Core
{
    public struct LodBlock
    {
        public double X,Y,Size;
        public string Key {get{return X.ToString("R",CultureInfo.InvariantCulture)+":"+Y.ToString("R",CultureInfo.InvariantCulture)+":"+Size.ToString("R",CultureInfo.InvariantCulture);}}
        public double DistanceSquared(double x,double y){double dx=Math.Max(0,Math.Max(X-x,x-X-Size)),dy=Math.Max(0,Math.Max(Y-y,y-Y-Size));return dx*dx+dy*dy;}
    }
    public static class TerrainLodPlan
    {
        public const double Range=2000000;
        public const double MaximumRange=2000000000;
        public static List<LodBlock> Create(double along,double across,double tileSize,int tileRadius,double range=Range)
        {
            var result=new List<LodBlock>();double root=tileSize*1024;
            if(!RingParameters.Finite(range)||range<tileSize||range>MaximumRange)throw new ArgumentOutOfRangeException(nameof(range));
            while(root<range)root*=2;
            double nearX=Math.Floor(along/tileSize)*tileSize,nearY=Math.Floor(across/tileSize)*tileSize;
            for(long y=(long)Math.Floor((across-range)/root);y<=Math.Floor((across+range)/root);y++)
            for(long x=(long)Math.Floor((along-range)/root);x<=Math.Floor((along+range)/root);x++)
                Visit(new LodBlock{X=x*root,Y=y*root,Size=root},along,across,tileSize,tileRadius,nearX,nearY,range,result);
            return result;
        }
        private static void Visit(LodBlock b,double a,double c,double minimum,int radius,double nx,double ny,double range,List<LodBlock> result)
        {
            double d=b.DistanceSquared(a,c);if(d>range*range)return;
            if(b.X>=nx-radius*minimum&&b.Y>=ny-radius*minimum&&b.X+b.Size<=nx+(radius+1)*minimum&&b.Y+b.Size<=ny+(radius+1)*minimum)return;
            if(b.Size>minimum&&d<b.Size*b.Size*4)
            {
                double half=b.Size/2;for(int y=0;y<2;y++)for(int x=0;x<2;x++)Visit(new LodBlock{X=b.X+x*half,Y=b.Y+y*half,Size=half},a,c,minimum,radius,nx,ny,range,result);
            }
            else result.Add(b);
        }
    }
}
