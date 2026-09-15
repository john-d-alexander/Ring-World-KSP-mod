using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace NivenRingworld
{
    // Artist-facing registry. Paths refer to Unity/KSP .mu exports, not .blend
    // files. Multiple entries per kind supply deterministic visual variants.
    internal static class SceneryAssets
    {
        private static Dictionary<string,List<string>> models;
        private static AssetBundle bundle;
        private static bool attempted;
        private static int users;
        // Bundle assets are an optional first-party fallback; .mu registry overrides win.
        private static readonly Dictionary<string,string[]> builtin=new Dictionary<string,string[]>
        {
            {"tree_broadleaf",new[]{"broadleaf_spreading_01","broadleaf_windswept_02"}},
            {"tree_conifer",new[]{"conifer_layered_01","conifer_slender_02"}},
            {"boulder",new[]{"boulder_weathered_01","boulder_sandstone_02"}},
            {"rural_building",new[]{"habitation_thatch_01","habitation_reclaimed_02"}}
        };
        internal static void Acquire(){users++;}
        internal static void Release()
        {
            if(users>0)users--;
            if(users==0)
            {
                // Instances are destroyed at end of frame; retain their assets until Unity collects them.
                if(bundle!=null)bundle.Unload(false);
                bundle=null;attempted=false;models=null;
            }
        }
        internal static GameObject Create(string kind,int variant)
        {
            if(models==null)
            {
                models=new Dictionary<string,List<string>>();
                foreach(var node in GameDatabase.Instance.GetConfigNodes("RINGWORLD_SCENERY_ASSET"))
                {
                    string k=node.GetValue("kind"),path=node.GetValue("model");if(string.IsNullOrEmpty(k)||string.IsNullOrEmpty(path))continue;
                    List<string> list;if(!models.TryGetValue(k,out list)){list=new List<string>();models[k]=list;}list.Add(path);
                }
                var registered=new Dictionary<string,List<string>>();
                foreach(var node in GameDatabase.Instance.GetConfigNodes("RINGWORLD_BUNDLED_SCENERY_ASSET"))
                {
                    string kindName=node.GetValue("kind"),prefab=node.GetValue("prefab");if(string.IsNullOrEmpty(kindName)||string.IsNullOrEmpty(prefab))continue;
                    List<string> list;if(!registered.TryGetValue(kindName,out list)){list=new List<string>();registered[kindName]=list;}list.Add(prefab);
                }
                foreach(var pair in registered)builtin[pair.Key]=pair.Value.ToArray();
            }
            GameObject obj=null;List<string> choices;
            if(models.TryGetValue(kind,out choices)&&choices.Count>0)
                obj=GameDatabase.Instance.GetModel(choices[(variant&int.MaxValue)%choices.Count]);
            string[] names;
            if(obj==null&&builtin.TryGetValue(kind,out names))
            {
                if(!attempted)
                {
                    attempted=true;
                    string path=Path.Combine(KSPUtil.ApplicationRootPath,"GameData","NivenRingworld","Assets","ringworldscenery");
                    if(File.Exists(path))bundle=AssetBundle.LoadFromFile(path);
                }
                if(bundle!=null)
                {
                    var prefab=bundle.LoadAsset<GameObject>("assets/scenery/"+names[(variant&int.MaxValue)%names.Length]+".prefab");
                    if(prefab!=null)obj=Object.Instantiate(prefab);
                }
            }
            if(obj==null)return null;
            foreach(var child in obj.GetComponentsInChildren<Transform>(true))child.gameObject.layer=15;
            obj.SetActive(true);return obj;
        }
        internal static IEnumerable<KeyValuePair<string,string[]>> Catalog {get{Create("",0);return builtin;}}
        internal static GameObject DetailPrefab(string kind)
        {
            // Acquire the bundle using the normal loader once; keep batching data shared.
            if(!attempted){var temporary=Create(kind,0);if(temporary!=null)Object.Destroy(temporary);}
            string[] names;if(bundle==null||!builtin.TryGetValue(kind,out names))return null;
            return bundle.LoadAsset<GameObject>("assets/scenery/"+names[0]+".prefab");
        }
    }
}
