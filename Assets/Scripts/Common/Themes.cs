using DG.Tweening;
using LSCore;
using LSCore.Attributes;
using Newtonsoft.Json.Linq;
using SourceGenerators;

[InstanceProxy]
public partial class Themes : SingleScriptableObject<Themes>
{
    [SceneSelector] public string[] _list;
    public LaLa.Play[] idleMusics;
    private LaLa.Play lastIdleMusic;
    private Tween currentTween;
    
    public static void PlayMusic()
    {
        GameSave.Config.ListenAndCall("theme", Instance.Internal_PlayMusic);
    }

    private void Internal_PlayMusic(JToken token)
    {
        lastIdleMusic?.FadeOut();
        lastIdleMusic = idleMusics[GameSave.Theme];
        lastIdleMusic.FadeIn();
    }
}