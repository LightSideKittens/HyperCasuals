using System;
using Core;
using LSCore;
public class WinWindow : BaseWindow<WinWindow>
{
    public LaLa.PlayClip sound;
    public LSButton claimButton;
    public FundText reward;
    public FundText rewardX2;
    
    protected override void OnShowing()
    {
        if (Ads.IsRewardedReady)
        {
            claimButton.gameObject.SetActive(true);
            claimButton.submittable.doIter.Unsubscribe();
            ISubmittable submittable = claimButton.submittable;
            submittable.Submitted += OnClaim;
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
        base.OnShowing();
        Manager.canvas.sortingOrder = 120;
        Analytic.LogEvent("win_level", ("level", GameSave.currentLevel));
    }

    private void OnClaim()
    {
        Analytic.LogEvent("double_reward");
        Ads.ShowRewarded(OnRewarded, OnClosed);

        void OnRewarded()
        {
            claimButton.submittable.doIter.Do();
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
        public override string ToString() => (GameSave.BestScore).ToString();
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
