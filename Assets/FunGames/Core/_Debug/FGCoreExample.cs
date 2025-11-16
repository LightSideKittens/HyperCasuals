using FunGames.Core;
using UnityEngine;

public class FGCoreExample : MonoBehaviour
{
    void Awake()
    {
        FunGamesSDK.Callbacks.Initialization += OnInitialization; // Triggered when module initialization start
        FunGamesSDK.Callbacks.OnInitialized += OnInitialized; // Triggered when module initialization is completed
        FunGamesSDK.Callbacks.OnAdsRemoved += OnAdsRemoved; // Triggered after ads were removed by RemoveAds()
    }

    private void OnInitialization()
    {
        // Do something when module start initializing.
    }

    private void OnInitialized(bool success)
    {
        // Do something when module initialization completed.
    }

    private void OnAdsRemoved()
    {
        // Do something when ads are removed.
    }

    private void OnDestroy()
    {
        FunGamesSDK.Callbacks.Initialization -= OnInitialization;
        FunGamesSDK.Callbacks.OnInitialized -= OnInitialized;
        FunGamesSDK.Callbacks.OnAdsRemoved -= OnAdsRemoved;
    }
}