using System;
using System.Threading.Tasks;
using Common;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class Initializer : BaseInitializer
{
    public BuyTheme defaultTheme;
    
#if UNITY_ANDROID
    private const string appKey = "235a541e5";
    private const string afDevKey  = "dh7B2YXTcmzJGikoZ7XEdH";
    private const string rewardAdUnit = "jib1u7rkjccrrvfd";
    private const string interAdUnit = "nm1m0f54txlyc37x";
#elif UNITY_IOS
    private string appKey = "YOUR_IOS_APP_KEY";
    private string rewardAdUnit = "jib1u7rkjccrrvfd";
    private string interAdUnit = "nm1m0f54txlyc37x";
#endif
    
    protected override void OnInitialize(Action onInitialized)
    {
        Analytic.Init(InitFirebase());
        
        defaultTheme.Do();
        Themes.PlayMusic();
        
        if (!Levels.IsTutorialCompleted.Is)
        {
            new Levels.LoadCurrentTutorial().Do();
            return;
        }
        
        onInitialized?.Invoke();
    }

    private static Task<bool> InitFirebase()
    {
        var task = new TaskCompletionSource<bool>();
        Init();
        return task.Task;

        async void Init()
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            var success = status == DependencyStatus.Available;
            task.SetResult(success);
        
            if (success)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            else
            {
                Debug.LogError($"[Firebase] Dependencies not available: {status}");
            }
        
            NotificationHandlers.AddHandler("test", token =>
            {
                Debug.Log($"Test notification: {token}");
            });
        
            NotificationHandlers.Init();
        }
    }
}