using System;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using UnityEngine;

public class LoseWindow : BaseWindow<LoseWindow>
{
    public LaLa.PlayClip sound;
    [SerializeField] private LSButton watchButton;
    [SerializeReference] private AnimSequencer timerAnim;
    public static Action onReviveClicked;
    
    protected override void OnShowing()
    {
        sound.Do();
        CoreWorld.StopIdleMusic();
        base.OnShowing();
        watchButton.Submitted += Reload; 
        timerAnim.Animate();
        Analytic.LogEvent("lost_level", "level", GameSave.currentLevel);
    }

    private void Reload()
    {
        UIViewBoss.GoBack();
        onReviveClicked?.Invoke();
        Analytic.LogEvent("revive", "level", GameSave.currentLevel);
    }

    protected override void OnHiding()
    {
        base.OnHiding();
        watchButton.Submitted -= Reload;
        timerAnim.Kill();
    }

    protected override void DeInit()
    {
        base.DeInit();
        onReviveClicked = null;
    }

    public static void Show(Action onRevive)
    {
        if(WinWindow.IsVisible) return;
        onReviveClicked += OnRevive;
        Show();

        void OnRevive()
        {
            onReviveClicked -= OnRevive;
            onRevive();
        }
    }

    public static void Hide() => Instance.Manager.OnlyHide();
}