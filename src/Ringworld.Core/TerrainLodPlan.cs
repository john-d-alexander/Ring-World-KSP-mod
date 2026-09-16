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
        // Compatibility constant: the setting is limited only by finite double representation.
        public const double MaximumRange=double.MaxValue;
        public static List<LodBlock> Create(double along,double across,double tileSize,int tileRadius,double range=Range,double halfWidth=double.PositiveInfinity,double halfCircumference=double.PositiveInfinity,double maximumBlockSize=double.PositiveInfinity)
        {
            var result=new List<LodBlock>();double root=tileSize*1024;
            if(!RingParameters.Finite(range)||range<tileSize||range>MaximumRange)throw new ArgumentOutOfRangeException(nameof(range));
            // Coordinates beyond this cannot retain a tile-sized difference in a double.
            // Actual ring streaming supplies the tighter finite physical extent.
            range=Math.Min(range,Math.Min(tileSize*1099511627776.0,halfWidth+halfCircumference+Math.Abs(across)));
            while(root<range)root*=2;
            double nearX=Math.Floor(along/tileSize)*tileSize,nearY=Math.Floor(across/tileSize)*tileSize;
            for(long y=(long)Math.Floor((across-range)/root);y<=Math.Floor((across+range)/root);y++)
            for(long x=(long)Math.Floor((along-range)/root);x<=Math.Floor((along+range)/root);x++)
                Visit(new LodBlock{X=x*root,Y=y*root,Size=root},along,across,tileSize,tileRadius,nearX,nearY,range,result,halfWidth,halfCircumference,maximumBlockSize);
            return result;
        }
        private static void Visit(LodBlock b,double a,double c,double minimum,int radius,double nx,double ny,double range,List<LodBlock> result,double halfWidth,double halfCircumference,double maximumBlockSize)
        {
            if(b.Y>=halfWidth||b.Y+b.Size<=-halfWidth||b.X>=a+halfCircumference||b.X+b.Size<=a-halfCircumference)return;
            double d=b.DistanceSquared(a,c);if(d>range*range)return;
            if(b.X>=nx-radius*minimum&&b.Y>=ny-radius*minimum&&b.X+b.Size<=nx+(radius+1)*minimum&&b.Y+b.Size<=ny+(radius+1)*minimum)return;
            if(b.Size>minimum&&(d<b.Size*b.Size*4||b.Size>maximumBlockSize))
            {
                double half=b.Size/2;for(int y=0;y<2;y++)for(int x=0;x<2;x++)Visit(new LodBlock{X=b.X+x*half,Y=b.Y+y*half,Size=half},a,c,minimum,radius,nx,ny,range,result,halfWidth,halfCircumference,maximumBlockSize);
            }
            else result.Add(b);
        }
    }
}
