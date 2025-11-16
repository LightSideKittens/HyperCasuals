using System;
using System.Collections;
// using FunGames.Analytics;
using FunGames.Core;
// using FunGames.Mediation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunGames.Tools
{
    public class FGLoadingScreen : MonoBehaviour
    {
        public Image ProgressBar;

        public LoadedAction ActionWhenLoaded = LoadedAction.LoadNextScene;

        public float MaxLoadingTime = 10;

        public ProgressMode ProgressMode = ProgressMode.LoadWithInit;

        private IEnumerator _checkInitCoroutine;
        private bool _useCheckInit = true;
        private float _initCounter = 0;
        private Action<bool> _onSdkInitialized;
        // private Action<FGAdInfo> _enableCheckInitDelegate;
        private Action _appOpenFailedToLoad;

        private float _timer = 0;
        private float _totalTime = 0;

        private void Start()
        {
            _timer = Time.time;
            // FGMediation.Callbacks.OnAppOpenDisplayed += Close;

            // if (FGMediationManager.Instance.MatchAppOpenCondition())
            // {
            //     _useCheckInit = false;
            //     _enableCheckInitDelegate = delegate { _useCheckInit = true; };
            //     _appOpenFailedToLoad = delegate { _useCheckInit = true; };
            //
            //     FGMediation.Callbacks.OnAppOpenAdLoaded += _enableCheckInitDelegate;
            //     FGMediation.Callbacks.OnAppOpenClosed += _enableCheckInitDelegate;
            //     FGMediation.Callbacks.OnAppOpenAdFailedToLoad += _appOpenFailedToLoad;
            //     return;
            // }

            if (ProgressMode.Equals(ProgressMode.LoadWithInit))
            {
                _onSdkInitialized = delegate { CloseLoadingScreen(); };
                FunGamesSDK.Callbacks.OnInitialized += _onSdkInitialized;
            }
        }

        private void Update()
        {
            if (_useCheckInit) CheckInitialization();

            if (ProgressBar == null) return;
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            switch (ProgressMode)
            {
                case ProgressMode.LoadWithInit:
                    ProgressBar.fillAmount = FGCore.Instance.TotalSubModulesCompleted / FGCore.Instance.TotalSubModules;
                    break;
                case ProgressMode.LoadWithTime:
                    ProgressBar.fillAmount += Time.deltaTime / MaxLoadingTime;
                    break;
            }
        }

        private void CloseLoadingScreen()
        {
            // FGMediation.Callbacks.OnAppOpenAdLoaded -= _enableCheckInitDelegate;
            // FGMediation.Callbacks.OnAppOpenClosed -= _enableCheckInitDelegate;
            // FGMediation.Callbacks.OnAppOpenAdFailedToLoad -= _appOpenFailedToLoad;
            FunGamesSDK.Callbacks.OnInitialized -= _onSdkInitialized;
            switch (ActionWhenLoaded)
            {
                case LoadedAction.ClosePanel:
                {
                    gameObject.SetActive(false);
                    break;
                }
                case LoadedAction.LoadNextScene:
                {
                    SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
                    break;
                }
            }

            _totalTime = Time.time - _timer;
            // FGAnalytics.NewDesignEvent("LoadingScreenCompleted", _totalTime);
            Debug.Log("[FGLoadingScreen] Loading screen completed in " + _totalTime + " seconds.");
        }

        private void CheckInitialization()
        {
            _initCounter += Time.deltaTime;
            if (_initCounter >= MaxLoadingTime || FunGamesSDK.IsInitialized)
            {
                _useCheckInit = false;
                CloseLoadingScreen();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnDisable()
        {
            Dispose();
        }

        private void Dispose()
        {
            _useCheckInit = false;
            FunGamesSDK.Callbacks.OnInitialized -= _onSdkInitialized;
        }
    }

    public enum LoadedAction
    {
        LoadNextScene,
        ClosePanel
    }

    public enum ProgressMode
    {
        LoadWithInit,
        LoadWithTime
    }
}