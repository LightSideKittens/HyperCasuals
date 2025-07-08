using System;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        watchButton.Submitted += onReviveClicked;
        watchButton.Submitted += Reload;
        sequence = timerAnim.Animate();
    }

    private void Reload()
    {
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    protected override void OnHiding()
    {
        base.OnHiding();
        watchButton.Submitted -= onReviveClicked;
        watchButton.Submitted -= UIViewBoss.GoBack;

        sequence?.Kill();
    }
}