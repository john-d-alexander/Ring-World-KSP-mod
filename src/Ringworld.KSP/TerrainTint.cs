using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class TerrainTint
    {
        private static readonly Color[] colors={new Color(.02f,.18f,.29f),new Color(.07f,.30f,.40f),new Color(.10f,.36f,.43f),new Color(.28f,.37f,.19f),
            new Color(.32f,.40f,.20f),new Color(.16f,.31f,.16f),new Color(.60f,.50f,.33f),new Color(.41f,.39f,.36f),new Color(.76f,.81f,.81f),
            new Color(.23f,.27f,.30f),new Color(.47f,.45f,.36f),new Color(.28f,.32f,.36f),new Color(.31f,.30f,.26f)};
        internal static Color Color(TerrainSample s){return s.BlendedColor?new Color((float)s.GroundColor.X,(float)s.GroundColor.Y,(float)s.GroundColor.Z):colors[(int)s.Biome];}
        internal static Texture2D Texture(int size,Color[] pixels)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,true){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};
            texture.SetPixels(pixels);texture.Apply(true,true);return texture;
        }
    }
}
