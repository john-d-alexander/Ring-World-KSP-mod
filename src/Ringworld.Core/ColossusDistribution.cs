using System;
using System.Collections.Generic;
namespace Ringworld.Core
{
    public struct ColossusCandidate {public string Key;public double Along,Across,Variant;}
    public static class ColossusDistribution
    {
        public const double LandmarkClearance=600000;
        public static IEnumerable<ColossusCandidate> Nearby(TerrainGenerator t,double along,double across,double cellSize=2000000,double chance=.15,double reach=500000)
        {
            var g=t.Geometry;long columns=Math.Max(1,(long)Math.Ceiling(g.P.Circumference/cellSize));double width=g.P.Circumference/columns;
            long cx=(long)Math.Floor(along/width),cy=(long)Math.Floor(across/cellSize);
            int radius=(int)Math.Ceiling(reach/Math.Min(width,cellSize));var seen=new HashSet<string>();
            for(long y=cy-radius;y<=cy+radius;y++)for(long x=cx-radius;x<=cx+radius;x++)
            {
                long column=(x%columns+columns)%columns;string key=column+":"+y;if(!seen.Add(key))continue;
                if(t.Scatter(column,y,2601)>=chance)continue;
                double a=(column+.2+.6*t.Scatter(column,y,2603))*width,b=(y+.2+.6*t.Scatter(column,y,2609))*cellSize;
                if(Math.Abs(b)>g.P.Width/2-100000)continue;
                bool near=false;foreach(var site in t.Landmarks)
                {
                    double da=Math.Abs(RingGeometry.Wrap(a-site.Along+g.P.Circumference/2,g.P.Circumference)-g.P.Circumference/2);
                    double db=b-site.Across;if(da*da+db*db<LandmarkClearance*LandmarkClearance){near=true;break;}
                }
                if(!near)yield return new ColossusCandidate{Key=key,Along=a,Across=b,Variant=t.Scatter(column,y,2617)};
            }
        }
    }
}
