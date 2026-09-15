using System.IO;
using UnityEngine;
namespace NivenRingworld
{
    internal static class RingVisualAssets
    {
        private static AssetBundle bundle;
        private static int users;
        internal static AssetBundle Acquire()
        {
            if(bundle==null)bundle=AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath,"GameData","NivenRingworld","Assets","ringworldvisuals"));
            if(bundle!=null)users++;return bundle;
        }
        internal static void Release(){if(users>0)users--;if(users==0&&bundle!=null){bundle.Unload(true);bundle=null;}}
    }
}
