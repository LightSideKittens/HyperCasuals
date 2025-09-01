using System;
using Core;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using UnityEngine;

public class LoseWindow : BaseWindow<LoseWindow>
{
    public LaLa.PlayClip sound;
    [SerializeField] private GameObject counter;
    [SerializeField] private LocalizationText reasonText;
    [SerializeField] private LSButton watchButton;
    [SerializeField] private LSButton replayButton;
    [SerializeField] private SubmittableRect noThanksButton;
    [SerializeReference] private AnimSequencer timerAnim;
    public static Action onReviveClicked;
    private bool watched;

    protected override void Init()
    {
        base.Init();
        watchButton.Submitted += Reload;
        noThanksButton.Submitted += () => SetActiveWatchButton(false);
    }

    protected override void OnShowing()
    {
        reasonText.Localize(GameSave.loseReason);
        sound.Do();
        CoreWorld.StopIdleMusic();
        base.OnShowing();

        if (Ads.IsRewardedReady && !watched)
        { 
            SetActiveWatchButton(true);
            timerAnim.Animate().OnComplete(() => SetActiveWatchButton(false));
        }
        else
        {
            SetActiveWatchButton(false);
        }
        
        if (GameSave.currentLevel == "classic")
        {
            Analytic.LogEvent("lost_classic");
        }
        else
        { 
            Analytic.LogEvent("lost_level", ("level", GameSave.Level), ("reason", GameSave.loseReason));
        }
    }
    
    private void Reload()
    {
        Ads.ShowRewarded(OnRewarded, OnClosed);
        
        void OnClosed()
        {
            SetActiveWatchButton(false);
        }
        
        void OnRewarded()
        {
            watched = true; 
            UIViewBoss.GoBack();
            onReviveClicked?.Invoke();
            if (GameSave.currentLevel == "classic")
            {
                Analytic.LogEvent("revive_classic");
            }
            else
            { 
                Analytic.LogEvent("revive", ("level", GameSave.Level));
            }
        }
    }

    private void SetActiveWatchButton(bool active)
    {
        watchButton.gameObject.SetActive(active);
        noThanksButton.gameObject.SetActive(active);
        counter.SetActive(active);
        replayButton.gameObject.SetActive(!active);
    }

    protected override void OnHiding()
    {
        base.OnHiding();
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