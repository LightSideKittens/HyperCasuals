using DG.Tweening;
using LSCore;
using LSCore.Attributes;
using LSCore.Extensions;
using SourceGenerators;
using UnityEngine;

[InstanceProxy]
public partial class Themes : SingleScriptableObject<Themes>
{
    [SceneSelector] public string[] _list;
    public LaLa.Play[] idleMusics;
    public Sprite[] backgrounds;
    
    private LaLa.Play lastIdleMusic;
    private Tween currentTween;

    public static Sprite CurrentBackground => Instance.backgrounds[GameSave.Theme];
    
    public static void PlayMusic()
    {
        GameSave.Config.ListenAndCall("theme", Instance.Internal_PlayMusic);
    }

    private void Internal_PlayMusic()
    {
        lastIdleMusic?.FadeOut();
        lastIdleMusic = idleMusics[GameSave.Theme];
        lastIdleMusic.FadeIn();
    }
}