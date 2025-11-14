using System;
using Core;
using LSCore;
using UnityEngine;

public class WinWindow : BaseWindow<WinWindow>
{
    public ParticleSystem confetti;
    public LaLa.PlayClip sound;
    public LSButton claimButton;
    public FundText reward;
    public FundText rewardX2;
    public UIView main;
    public UIView chest;
    
    protected override void OnShowing()
    {
        Instantiate(confetti);
        if (Ads.IsRewardedReady)
        {
            Analytic.LogEvent("ads_button_seen");
            claimButton.gameObject.SetActive(true);
            claimButton.uiControl.doIter.Unsubscribe();
            IUIControl uiControl = claimButton.uiControl;
            uiControl.Activated += OnClaim;
        }
        else
        {
            claimButton.gameObject.SetActive(false);
        }

        FieldSave.IsEnabled = false;
        FieldSave.gridDirtied = false;
        FieldSave.Delete();
        if (LoseWindow.IsVisible)
        {
            LoseWindow.Hide();
        }
        sound.Do(); 
        CoreWorld.StopIdleMusic();
        Analytic.LogEvent("win_level", GameSave.CurrentLevelParam, GoalWindow.LevelTimeParam);
        base.OnShowing();

        using (UIViewBoss.UseId("WinWindowViews"))
        {
            if (GameSave.IsChestGot)
            {
                chest.Show();
            }
            else
            {
                main.Show();
            }
        }
        
        GameSave.IsChestGot = false;
    }

    private void OnClaim()
    {
        Analytic.LogEvent("double_reward");
        Ads.ShowRewarded(OnRewarded, OnClosed);

        void OnRewarded()
        {
            claimButton.uiControl.doIter.Do();
            claimButton.gameObject.SetActive(false);
            reward.Number = rewardX2.Number;
        }

        void OnClosed()
        {
            if (!Ads.IsRewardedReady)
            {
                claimButton.gameObject.SetActive(false);
            }
        }
    }

    [Serializable]
    public class Level : ILocalizationArgument
    {
        public int offset;
        public override string ToString() => (GameSave.Level + offset).ToString();
    }
    
    [Serializable]
    public class BestScore : ILocalizationArgument
    {
        public override string ToString() => (GameSave.BestScore).ToString("N0");
    }
    
    [Serializable]
    public class TutorialLevelUp : DoIt
    {
        public override void Do() => GameSave.TutorialLevel++;
    }
    
    [Serializable]
    public class LevelUp : DoIt
    {
        public override void Do() => GameSave.Level++;
    }
}
