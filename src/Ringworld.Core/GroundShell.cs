using System;
using System.Collections.Generic;

namespace Ringworld.Core
{
    public static class GroundShell
    {
        // Two matching (n+1)^2 vertex grids: top first, underside second.
        // Every edge has two oppositely directed neighbours, including tile walls.
        public static int[] Triangles(int n)
        {
            if(n<1||n>128)throw new ArgumentOutOfRangeException(nameof(n));
            int count=(n+1)*(n+1);var triangles=new List<int>();
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {int a=y*(n+1)+x,b=a+1,c=a+n+1,d=c+1;triangles.AddRange(new[]{a,c,b,b,c,d});}
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {int a=count+y*(n+1)+x,b=a+1,c=a+n+1,d=c+1;triangles.AddRange(new[]{a,b,c,b,d,c});}
            var boundary=new List<int>();
            for(int y=0;y<n;y++)boundary.Add(y*(n+1));
            for(int x=0;x<n;x++)boundary.Add(n*(n+1)+x);
            for(int y=n;y>0;y--)boundary.Add(y*(n+1)+n);
            for(int x=n;x>0;x--)boundary.Add(x);
            for(int i=0;i<boundary.Count;i++)
            {int a=boundary[i],b=boundary[(i+1)%boundary.Count];triangles.AddRange(new[]{a,a+count,b,b,a+count,b+count});}
            return triangles.ToArray();
        }
    }
}
