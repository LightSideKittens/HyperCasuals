using System;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using LSCore.Extensions;
using UnityEngine;
using Unity.Services.LevelPlay;
using static com.unity3d.mediation.LevelPlayAdFormat;

public static class Ads
{
    private static bool initialized;
    private static LevelPlayRewardedAd rewarded;
    private static LevelPlayInterstitialAd interstitial;

    private static string rvUnitId;
    private static string isUnitId;
    
    private static bool isShowing;

    public static bool IsInitialized => initialized;
    public static bool IsRewardedReady => rewarded != null && rewarded.IsAdReady();
    public static bool IsInterstitialReady => interstitial != null && interstitial.IsAdReady();
    private static readonly string logTag = "[Ads]".ToTag(new Color(0.48f, 0.79f, 0f));

#if UNITY_EDITOR
    static Ads()
    {
        World.Destroyed += Dispose;
    }
#endif

    public static void Init(
        string appKey,
        string rewardedUnitId,
        string interstitialUnitId,
        bool consentGiven,
        bool doNotSell = false,
        bool childDirected = false,
        string userId = null)
    {
        if (initialized) return;

        rvUnitId = rewardedUnitId;
        isUnitId = interstitialUnitId;

        LevelPlay.SetConsent(consentGiven);
        LevelPlay.SetMetaData("do_not_sell", doNotSell ? "true" : "false");
        LevelPlay.SetMetaData("is_child_directed", childDirected ? "true" : "false");

        LevelPlay.OnInitSuccess += _ =>
        {
            initialized = true;

            rewarded = new LevelPlayRewardedAd(rvUnitId);
            interstitial = new LevelPlayInterstitialAd(isUnitId);

            rewarded.OnAdLoadFailed += e => Burger.Warning($"{logTag} RV load failed: {e}");
            interstitial.OnAdLoadFailed += e => Burger.Warning($"{logTag} IS load failed: {e}");

            LoadRewarded();
            LoadInterstitial();

            Burger.Log($"{logTag} LevelPlay initialized");
        };

        LevelPlay.OnInitFailed += e => { Burger.Error($"{logTag} LevelPlay init failed: {e}"); };

        LevelPlay.Init(appKey, userId, new[] { REWARDED, INTERSTITIAL });
    }

    public static void LoadRewarded()
    {
        if (rewarded == null)
        {
            Burger.Warning($"{logTag} Rewarded not set");
            return;
        }

        rewarded.LoadAd();
    }

    public static void LoadInterstitial()
    {
        if (interstitial == null)
        {
            Burger.Warning($"{logTag} Interstitial not set");
            return;
        }

        interstitial.LoadAd();
    }


    public static bool ShowRewarded(Action onReward, Action onClosed = null, string placement = null)
    {
        if (rewarded == null || !rewarded.IsAdReady() || isShowing) return false;
        
        var isRewarded = false;
#if UNITY_EDITOR
        var timer = Wait.Delay(4.9f);
#endif
        Tween delay = null;
        isShowing = true;

        rewarded.OnAdRewarded += OnRewarded;
        rewarded.OnAdClosed += OnClosed;
        rewarded.OnAdDisplayFailed += OnShowFailed;

        rewarded.ShowAd(placement);
        return true;

        void OnRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
#if UNITY_EDITOR
            if (timer.IsActive())
            {
                return;
            }
#endif
            isRewarded = true;
            delay ??= Wait.Frames(1, OnDelay);
        }

        void OnClosed(LevelPlayAdInfo info)
        {
            delay ??= Wait.Frames(1, OnDelay);
        }

        void OnShowFailed(LevelPlayAdDisplayInfoError error)
        {
            Burger.Warning($"{logTag} Rewarded show failed: {error}");
            delay ??= Wait.Frames(1, OnDelay);
        }

        void OnDelay()
        {
            LoadRewarded();
            isShowing = false;
            rewarded.OnAdRewarded -= OnRewarded;
            rewarded.OnAdClosed -= OnClosed;
            rewarded.OnAdDisplayFailed -= OnShowFailed;

            if (isRewarded)
            {
                onReward.SafeInvoke();
            }
            else
            {
                onClosed.SafeInvoke();
            }
        }
    }

    public static bool ShowInterstitial(Action onClosed = null, string placement = null)
    {
        if (interstitial == null || !interstitial.IsAdReady() || isShowing)
            return false;

        isShowing = true;

        void OnClosed(LevelPlayAdInfo info)
        {
            interstitial.OnAdClosed -= OnClosed;
            interstitial.OnAdDisplayFailed -= OnShowFailed;

            isShowing = false;
            onClosed.SafeInvoke();

            LoadInterstitial();
        }

        void OnShowFailed(LevelPlayAdDisplayInfoError error)
        {
            Burger.Warning($"{logTag} Interstitial show failed: {error}");

            interstitial.OnAdClosed -= OnClosed;
            interstitial.OnAdDisplayFailed -= OnShowFailed;

            isShowing = false;
            onClosed.SafeInvoke();

            LoadInterstitial();
        }

        interstitial.OnAdClosed += OnClosed;
        interstitial.OnAdDisplayFailed += OnShowFailed;

        interstitial.ShowAd(placement);
        return true;
    }

    public static void Dispose()
    {
        rewarded = null;
        interstitial = null;
        initialized = isShowing = false;
    }
}
