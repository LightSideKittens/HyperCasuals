using System;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using UnityEngine;

public class LoseWindow : BaseWindow<LoseWindow>
{
    [SerializeField] private LSButton watchButton;
    [SerializeReference] private AnimSequencer timerAnim;
    public static Action onReviveClicked;

    private Sequence sequence;
    protected override void OnShowing()
    {
        CoreWorld.StopIdleMusic();
        base.OnShowing();
        watchButton.Submitted += Reload;
        sequence = timerAnim.Animate();
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
        sequence?.Kill();
    }

    protected override void DeInit()
    {
        base.DeInit();
        onReviveClicked = null;
    }

    public static void Show(Action onRevive)
    {
        onReviveClicked += OnRevive;
        Show();

        void OnRevive()
        {
            onReviveClicked -= OnRevive;
            onRevive();
        }
    }
}