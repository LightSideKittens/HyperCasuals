// using FunGames.Analytics;
using FunGames.Core;
// using FunGames.RemoteConfig;
using Proyecto26;
using UnityEngine;
using UnityEngine.UI;

public class FGNoInternetPopup : MonoBehaviour
{
    public GameObject popUp;
    public Button TryConnectionButton;

    public bool HasInternetConnection => _hasInternetConnection;
    private bool _hasInternetConnection = true;
    private float _timeCounter = 0f;
    private bool _noInternetPopupActivated = false;
    private double _checkInternetDelayWhenConnected = 60;
    private double _checkInternetDelayWhenNotConnected = 30;
    private bool _buttonClicked = false;

    private const string RC_NO_INTERNET_POPUP_ACTIVATED = "FGNoInternetPopup";
    private const string RC_CHECK_INTERNET_DELAY_WHEN_CONNECTED = "CheckInternetDelayWhenConnected";
    private const string RC_CHECK_INTERNET_DELAY_WHEN_NOT_CONNECTED = "CheckInternetDelayWhenNotConnected";

    private void Awake()
    {
        TryConnectionButton.onClick.AddListener(CheckConnectionManually);
        // FGRemoteConfig.AddDefaultValue(RC_NO_INTERNET_POPUP_ACTIVATED, _noInternetPopupActivated);
        // FGRemoteConfig.AddDefaultValue(RC_CHECK_INTERNET_DELAY_WHEN_CONNECTED, _checkInternetDelayWhenConnected);
        // FGRemoteConfig.AddDefaultValue(RC_CHECK_INTERNET_DELAY_WHEN_NOT_CONNECTED, _checkInternetDelayWhenNotConnected);
        // FGRemoteConfig.Callbacks.OnInitialized += UpdateRemoteConfigValues;
        _timeCounter = 0;
        CheckConnection();
        popUp.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!_noInternetPopupActivated) return;
        _timeCounter += Time.deltaTime;
        double checkDelay = HasInternetConnection
            ? _checkInternetDelayWhenConnected
            : _checkInternetDelayWhenNotConnected;
        if (_timeCounter > checkDelay)
        {
            FGCore.Instance.Log("Checking internet connection after " + _timeCounter + " secs...");
            _timeCounter = 0;
            CheckConnection();
        }
    }

    public void Show()
    {
        if (!_noInternetPopupActivated) return;

        // if (!popUp.activeSelf) FGAnalytics.NewDesignEvent("NoInternetPopupDisplayed");
        popUp.SetActive(true);
    }

    private void Hide()
    {
        if (!_noInternetPopupActivated) return;
        if (popUp.activeSelf)
        {
            // FGAnalytics.NewDesignEvent("NoInternetPopupDisabledManually: " + _buttonClicked);
            _buttonClicked = false;
        }

        popUp.SetActive(false);
    }

    private void CheckConnectionManually()
    {
        if (!_buttonClicked) _buttonClicked = true;
        CheckConnection();
    }

    private void CheckConnection()
    {
        RestClient.Get("https://www.google.com").Then(response =>
            {
                Hide();
                _hasInternetConnection = true;
                FGCore.Instance.Log("...Internet connection OK !");
            })
            .Catch(error =>
            {
                Show();
                _hasInternetConnection = false;
                FGCore.Instance.Log("...No internet connection !");
            });
    }

    private void UpdateRemoteConfigValues(bool obj)
    {
        // _checkInternetDelayWhenConnected = FGRemoteConfig.GetDoubleValue(RC_CHECK_INTERNET_DELAY_WHEN_CONNECTED);
        // _checkInternetDelayWhenNotConnected = FGRemoteConfig.GetDoubleValue(RC_CHECK_INTERNET_DELAY_WHEN_NOT_CONNECTED);
        // _noInternetPopupActivated = FGRemoteConfig.GetBooleanValue(RC_NO_INTERNET_POPUP_ACTIVATED);
        CheckConnection();
    }
}