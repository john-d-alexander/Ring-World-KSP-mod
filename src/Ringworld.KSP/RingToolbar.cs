using System;
using KSP.UI.Screens;
using UnityEngine;

namespace NivenRingworld
{
    // Let the stock launcher own ordering and placement alongside other mods.
    internal sealed class RingToolbar : IDisposable
    {
        private readonly Action<bool> changed;
        private ApplicationLauncherButton button;
        private Texture2D icon;
        private bool selected;
        internal bool UiVisible { get; private set; } = true;
        internal RingToolbar(Action<bool> changed)
        {
            this.changed=changed;
            GameEvents.onGUIApplicationLauncherReady.Add(Ready);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(Remove);
            GameEvents.onHideUI.Add(HideUi);GameEvents.onShowUI.Add(ShowUi);
            if(ApplicationLauncher.Ready)Ready();
        }
        private void HideUi(){UiVisible=false;}
        private void ShowUi(){UiVisible=true;}
        private void Ready()
        {
            if(button!=null||ApplicationLauncher.Instance==null)return;
            if(icon==null)
            {
                icon=new Texture2D(38,38,TextureFormat.RGBA32,false);
                var pixels=new Color[38*38];
                for(int y=0;y<38;y++)for(int x=0;x<38;x++)
                {
                    float u=(x-18.5f)/15,v=(y-18.5f)/15;
                    float radius=Mathf.Sqrt(u*u+v*v*3);
                    pixels[y*38+x]=Mathf.Abs(radius-1)<.13f?new Color(.65f,.9f,.75f,1):Color.clear;
                    if(u*u+v*v<.055f)pixels[y*38+x]=new Color(1,.86f,.35f,1);
                }
                icon.SetPixels(pixels);icon.Apply(false,true);
            }
            button=ApplicationLauncher.Instance.AddModApplication(()=>Select(true),()=>Select(false),null,null,null,null,
                ApplicationLauncher.AppScenes.FLIGHT|ApplicationLauncher.AppScenes.MAPVIEW,icon);
            SetSelected(selected);
        }
        private void Select(bool value){selected=value;changed(value);}
        internal void SetSelected(bool value)
        {
            selected=value;
            if(button!=null){if(value)button.SetTrue(false);else button.SetFalse(false);}
        }
        private void Remove()
        {
            if(button!=null&&ApplicationLauncher.Instance!=null)ApplicationLauncher.Instance.RemoveModApplication(button);
            button=null;
        }
        public void Dispose()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(Ready);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(Remove);
            GameEvents.onHideUI.Remove(HideUi);GameEvents.onShowUI.Remove(ShowUi);
            Remove();if(icon!=null)UnityEngine.Object.Destroy(icon);
        }
    }
}
