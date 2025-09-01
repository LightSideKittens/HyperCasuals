using System;
using Common;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class Initializer : BaseInitializer
{
    public BuyTheme defaultTheme;
    
#if UNITY_ANDROID
    private string appKey = "235a541e5";
    private string rewardAdUnit = "jib1u7rkjccrrvfd";
    private string interAdUnit = "nm1m0f54txlyc37x";
#elif UNITY_IOS
    private string appKey = "YOUR_IOS_APP_KEY";
    private string rewardAdUnit = "jib1u7rkjccrrvfd";
    private string interAdUnit = "nm1m0f54txlyc37x";
#endif
    
    protected override async void OnInitialize(Action onInitialized)
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        }
        else
        {
            Debug.LogError($"[Firebase] Dependencies not available: {status}");
        }
        
        Ads.Init(appKey, rewardAdUnit, interAdUnit, true);
        Analytic.Init();
        
        defaultTheme.Do();
        NotificationHandlers.AddHandler("test", token =>
        {
            Debug.Log($"Test notification: {token}");
        });
        
        NotificationHandlers.Init();
        
        if (!Levels.IsTutorialCompleted.Is)
        {
            new Levels.LoadCurrentTutorial().Do();
            return;
        }
        
        onInitialized?.Invoke();
    }
}