using System;
using Common;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class Initializer : BaseInitializer
{
    public BuyTheme defaultTheme;
    
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
        
        defaultTheme.Do();
        Analytic.Init();
        onInitialized?.Invoke();
    }
}