using System;
using DG.Tweening;
using Firebase.Analytics;
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
    private static bool rvLoading;
    private static bool isLoading;

    public static bool IsInitialized      => initialized;
    public static bool IsRewardedReady    => rewarded != null && rewarded.IsAdReady();
    public static bool IsInterstitialReady=> interstitial != null && interstitial.IsAdReady();

    private static readonly string logTag = "[Ads]".ToTag(new Color(0.48f, 0.79f, 0f));

    private static float rvRetry = 2f, isRetry = 2f;
    private const float MaxRetry = 30f;

#if UNITY_EDITOR
    static Ads()
    {
        World.Destroyed += () =>
        {
            rewarded = null;
            interstitial = null;
            initialized = false;
            isShowing = false;
            rvLoading = isLoading = false;
            rvRetry = isRetry = 2f;
        };
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

            rewarded    = new LevelPlayRewardedAd(rvUnitId);
            interstitial= new LevelPlayInterstitialAd(isUnitId);

            rewarded.OnAdLoaded += _ =>
            {
                rvLoading = false;
                rvRetry = 2f;
                Burger.Log($"{logTag} RV loaded");
            };
            rewarded.OnAdLoadFailed += e =>
            {
                rvLoading = false;
                Burger.Warning($"{logTag} RV load failed: {e}");
                RetryRewarded();
            };

            interstitial.OnAdLoaded += _ =>
            {
                isLoading = false;
                isRetry = 2f;
                Burger.Log($"{logTag} IS loaded");
            };
            interstitial.OnAdLoadFailed += e =>
            {
                isLoading = false;
                Burger.Warning($"{logTag} IS load failed: {e}");
                RetryInterstitial();
            };

            LoadRewarded();
            LoadInterstitial();

            Burger.Log($"{logTag} LevelPlay initialized");
        };
        
        LevelPlay.OnImpressionDataReady += OnImpressionDataReady;
        LevelPlay.OnInitFailed += e =>
        {
            Burger.Error($"{logTag} LevelPlay init failed: {e}");
        };
        
        LevelPlay.Init(appKey, userId, new[] { REWARDED, INTERSTITIAL });
    }

    private static void OnImpressionDataReady(LevelPlayImpressionData data)
    {
        if (data == null) return;

        var adNetwork = data.AdNetwork ?? "unknown";
        var adUnit = data.MediationAdUnitName ?? "unknown";
        var instance = data.InstanceName ?? "unknown";
        var revenue = data.Revenue ?? 0.0;

        var parameters = new Parameter[]
        {
            new("ad_platform", "ironSource"),
            new("ad_source", adNetwork),
            new("ad_unit_name", instance),
            new("ad_format", adUnit),
            new("value", revenue),
            new("currency", "USD")
        };

        FirebaseAnalytics.LogEvent("ad_impression", parameters);
    }

    public static void LoadRewarded()
    {
        if (rewarded == null)
        {
            Burger.Warning($"{logTag} Rewarded not set");
            return;
        }
        if (rvLoading || rewarded.IsAdReady()) return;

        rvLoading = true;
        rewarded.LoadAd();
    }
    
    public static void LoadInterstitial()
    {
        if (interstitial == null)
        {
            Burger.Warning($"{logTag} Interstitial not set");
            return;
        }
        if (isLoading || interstitial.IsAdReady()) return;

        isLoading = true;
        interstitial.LoadAd();
    }
    
    public static bool ShowRewarded(Action onReward, Action onClosed = null, string placement = null)
    {
        if (rewarded == null || !rewarded.IsAdReady() || isShowing) return false;

        var isRewardedFlag = false;
#if UNITY_EDITOR
        var editorWatchTimer = Wait.Delay(4.9f);
#endif
        Tween delay = null;
        isShowing = true;

        rewarded.OnAdRewarded       += OnRewarded;
        rewarded.OnAdClosed         += OnClosed;
        rewarded.OnAdDisplayFailed  += OnShowFailed;

        Analytic.LogEvent("show_reward_ad");
        rewarded.ShowAd(placement);
        return true;

        void OnRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
#if UNITY_EDITOR
            if (editorWatchTimer.IsActive()) return;
#endif
            isRewardedFlag = true;
            delay ??= Wait.Frames(1, Finish);
        }

        void OnClosed(LevelPlayAdInfo info)
        {
            delay ??= Wait.Frames(1, Finish);
        }

        void OnShowFailed(LevelPlayAdDisplayInfoError error)
        {
            Burger.Warning($"{logTag} Rewarded show failed: {error}");
            delay ??= Wait.Frames(1, Finish);
        }

        void Finish()
        {
            rvRetry = 2f;
            LoadRewarded();

            isShowing = false;
            rewarded.OnAdRewarded      -= OnRewarded;
            rewarded.OnAdClosed        -= OnClosed;
            rewarded.OnAdDisplayFailed -= OnShowFailed;

            if (isRewardedFlag) onReward.SafeInvoke();
            else                onClosed.SafeInvoke();
        }
    }
    
    public static bool ShowInterstitial(Action onClosed = null, string placement = null)
    {
        if (interstitial == null || !interstitial.IsAdReady() || isShowing) return false;

        isShowing = true;

        interstitial.OnAdClosed        += OnClosed;
        interstitial.OnAdDisplayFailed += OnShowFailed;

        interstitial.ShowAd(placement);
        return true;

        void OnClosed(LevelPlayAdInfo info)
        {
            interstitial.OnAdClosed        -= OnClosed;
            interstitial.OnAdDisplayFailed -= OnShowFailed;

            isShowing = false;

            isRetry = 2f;
            LoadInterstitial();

            onClosed.SafeInvoke();
        }

        void OnShowFailed(LevelPlayAdDisplayInfoError error)
        {
            Burger.Warning($"{logTag} Interstitial show failed: {error}");

            interstitial.OnAdClosed        -= OnClosed;
            interstitial.OnAdDisplayFailed -= OnShowFailed;

            isShowing = false;

            RetryInterstitial();

            onClosed.SafeInvoke();
        }
    }

    private static void RetryRewarded()
    {
        if (rewarded == null || rewarded.IsAdReady() || rvLoading) return;

        DOVirtual.DelayedCall(rvRetry, () =>
        {
            if (rewarded == null || rewarded.IsAdReady()) return;
            LoadRewarded();
        });
        rvRetry = Mathf.Min(rvRetry * 1.8f, MaxRetry);
    }

    private static void RetryInterstitial()
    {
        if (interstitial == null || interstitial.IsAdReady() || isLoading) return;

        DOVirtual.DelayedCall(isRetry, () =>
        {
            if (interstitial == null || interstitial.IsAdReady()) return;
            LoadInterstitial();
        });
        isRetry = Mathf.Min(isRetry * 1.8f, MaxRetry);
    }
}
