using System;
using Core;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.Extensions;
using UnityEngine;

public class LoseWindow : BaseWindow<LoseWindow>
{
    [Serializable]
    public class Revive : DoIt
    {
        public override void Do() => DoIt();

        public static void DoIt()
        {
            UIViewBoss.GoBack();
            onReviveClicked?.Invoke();
            if (GameSave.currentLevel == "classic")
            {
                Analytic.LogEvent("revive_classic");
            }
            else
            { 
                Analytic.LogEvent("revive", GameSave.CurrentLevelParam);
            }
        }
    }
    
    [SerializeField] private GameObject counter;
    [SerializeField] private LocalizationText reasonText;
    [SerializeField] private LSButton watchButton;
    [SerializeField] private LSButton replayButton;
    [SerializeField] private UIControlRect noThanksButton;
    [SerializeReference] private AnimSequencer timerAnim;
    public RectTransform questPlaceholder;
    private LostQuestView lostQuestView;
    [SerializeField] private LSButton reviveButton;
    public FundText keysFundText;
    
    public static Action onReviveClicked;
    private bool watched;

    protected override void Init()
    {
        base.Init();
        DoEventListener.Listen("exchange_showed", OnExchangeShowed);
        watchButton.Did += Reload;
        noThanksButton.Did += () => SetActiveWatchButton(false);
        onReviveClicked += () => keysFundText.Number *= 2;
        
        var questViewPrefab = Quests.CurrentQuestHandler.ViewState;
        questViewPrefab.gameObject.SetActive(false);
        var questView = Instantiate(questViewPrefab, questPlaceholder);
        questViewPrefab.gameObject.SetActive(true);
        ((QuestView)questView).SetupIcon();
        Destroy(questView);
        lostQuestView = questView.GetComponent<LostQuestView>();
    }

    protected override void OnShowing()
    {
        lostQuestView.gameObject.SetActive(true);
        lostQuestView.slider.value = Quests.CurrentQuest["collectedCount"].ToInt() + Quests.CollectBlocksQuest.collectedCount;
        
        reasonText.Localize(GameSave.loseReason);
        base.OnShowing();

        if (Ads.IsRewardedReady && !watched)
        {
            Analytic.LogEvent("ads_button_seen");
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
            Analytic.LogEvent("lost_level", GameSave.CurrentLevelParam, ("reason", GameSave.loseReason), GoalWindow.LevelTimeParam);
        }
    }

    private void OnExchangeShowed()
    {
        var exchanger = DataBuffer.Get<FundsExchanger>();
        exchanger.SetAmountForTo((int)keysFundText.Number);
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
            Revive.DoIt();
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
        DoEventListener.UnListen("exchange_showed", OnExchangeShowed);
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