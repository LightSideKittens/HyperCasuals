using System;
using System.Collections.Generic;
using System.Globalization;
// using FunGames.Analytics;
using FunGames.Core.Modules;
// using FunGames.RemoteConfig;
using FunGames.Tools.Utils;
using UnityEngine;

namespace FunGames.Core
{
    [DefaultExecutionOrder(-43)]
    public class FGCore : FGModuleAbstract<FGCore, FGCoreCallbacks, FGMainSettings>
    {
        public override FGMainSettings Settings => FGMainSettings.settings;
        protected override FGModule Parent { get; }
        protected override string EventName => "SDK";
        protected override string RemoteConfigKey => "FGCore";

        public FGDebugConsoleBuilder DebugConsoleBuilder;
        public FGNoInternetPopup NoInternetPopup;
        public FGUpdatePopup UpdateVersionPopup;

        public const string RC_LOG_LEVEL = "FGLogLevel";
        public const string RC_NO_ADS = "FGNoAds";

        private const string PP_NO_ADS = "no_ads";
        private const string PP_DATE_FIRST_CO = "dateFirstCo";
        private const string PP_DATE_LAST_CO = "dateLastCo";
        private const string PP_CURRENT_SESSION_NUMBER = "CurrentSessionNumber";
        private List<FGAdType> _noAds = new List<FGAdType>();
        private bool _isFirstConnection = false;
        private int _daysSinceFirstConnection = 0;
        private int _daysSinceLastConnection = 0;
        private int _currentSessionNumber = 0;

        protected override void InitializeCallbacks()
        {
            // Keep even if not used, mandatory to instantiate game objects 
            FGPlaytimeMeasurement playtimeMeasurement = FGPlaytimeMeasurement.Instance;
            InitWithoutTimer();
            InitializeConsole();
            InitializeNoInternetPopup();
            InitializeUpdatePopup();
            Callbacks.OnInitialized += SendAnalytics;
            // FGRemoteConfig.Callbacks.OnInitialized += OnRemoteConfigInitialized;
        }

        protected override void OnAwake()
        {
            // FGRemoteConfig.AddDefaultValue(RC_LOG_LEVEL, 0);
            // FGRemoteConfig.AddDefaultValue(RC_NO_ADS, 0);

            CheckPlayerPref();

            if (!PlayerPrefs.HasKey(PP_CURRENT_SESSION_NUMBER))
            {
                PlayerPrefs.SetInt(PP_CURRENT_SESSION_NUMBER, 1);
            }
            else
            {
                int currentSession = PlayerPrefs.GetInt(PP_CURRENT_SESSION_NUMBER) + 1;
                PlayerPrefs.SetInt(PP_CURRENT_SESSION_NUMBER, currentSession);
            }

            _currentSessionNumber = PlayerPrefs.GetInt(PP_CURRENT_SESSION_NUMBER);
        }

        protected override void OnStart()
        {
            Initialize();
        }

        protected override void InitializeModule()
        {
            DateTime today = DateTime.Now;
            DateTime dateFirstCo;
            DateTime dateLastCo;

            _isFirstConnection = !PlayerPrefs.HasKey(PP_DATE_FIRST_CO);
            if (_isFirstConnection)
            {
                string dateAsString = today.ToString(CultureInfo.InvariantCulture);
                PlayerPrefs.SetString(PP_DATE_FIRST_CO, dateAsString);
                Log("Save Date First Connection : " + dateAsString);
                dateFirstCo = today;
                dateLastCo = today;
            }
            else
            {
                dateFirstCo = DateUtils.ConvertInvariant(PlayerPrefs.GetString(PP_DATE_FIRST_CO));
                dateLastCo = DateUtils.ConvertInvariant(PlayerPrefs.GetString(PP_DATE_LAST_CO));
            }

            _daysSinceFirstConnection = (int)today.Subtract(dateFirstCo).TotalDays;
            _daysSinceLastConnection = (int)today.Subtract(dateLastCo).TotalDays;

            PlayerPrefs.SetString(PP_DATE_LAST_CO, today.ToString(CultureInfo.InvariantCulture));
            Log("Day since first connection : " + _daysSinceFirstConnection);
            Log("Day since last connection : " + _daysSinceLastConnection);
            Log("Session #: " + _currentSessionNumber);
            TrackDeviceInfo();
            InitializationComplete(true);
        }

        public void RemoveAds(params FGAdType[] ads)
        {
            if (ads.Length == 0)
            {
                RemoveAllAds();
                return;
            }

            foreach (FGAdType ad in ads)
            {
                PlayerPrefs.SetInt(NoAdPlayerPref(ad), 1);
                _noAds.Add(ad);
                Log("Ad removed : " + ad);
            }

            Callbacks._onAdsRemoved?.Invoke();
        }

        public void RemoveAdsForSession(params FGAdType[] ads)
        {
            if (ads.Length == 0)
            {
                Log("All ads removed for this session !");
                foreach (var adType in Enum.GetValues(typeof(FGAdType))) _noAds.Add((FGAdType)adType);
            }

            foreach (FGAdType ad in ads)
            {
                _noAds.Add(ad);
                Log("Ad removed for this session: " + ad);
            }

            Callbacks._onAdsRemoved?.Invoke();
        }

        private void RemoveAllAds()
        {
            List<FGAdType> adsToRemove = new List<FGAdType>();
            foreach (FGAdType adType in Enum.GetValues(typeof(FGAdType)))
            {
                adsToRemove.Add(adType);
            }

            RemoveAds(adsToRemove.ToArray());
        }


        public bool IsNoAd(FGAdType ad)
        {
            CheckPlayerPref();
            return _noAds.Contains(ad);
        }

        public void RestoreAds()
        {
            foreach (FGAdType adType in Enum.GetValues(typeof(FGAdType)))
            {
                PlayerPrefs.DeleteKey(NoAdPlayerPref(adType));
            }
        }

        private void CheckPlayerPref()
        {
            foreach (FGAdType adType in Enum.GetValues(typeof(FGAdType)))
            {
                if (PlayerPrefs.HasKey(NoAdPlayerPref(adType))) _noAds.Add(adType);
            }
        }

        private string NoAdPlayerPref(FGAdType adType) => PP_NO_ADS + adType;


        public bool IsFirstConnection()
        {
            return _isFirstConnection;
        }

        public int DaysSinceFirstConnection()
        {
            return _daysSinceFirstConnection;
        }

        public int DaysSinceLastConnection()
        {
            return _daysSinceLastConnection;
        }

        public int GetCurrentSessionNumber()
        {
            return _currentSessionNumber;
        }

        public bool HasInternetConnection()
        {
            return NoInternetPopup.HasInternetConnection;
        }

        private void OnRemoteConfigInitialized(bool obj)
        {
            // if (FGRemoteConfig.GetBooleanValue(RC_NO_ADS)) RemoveAdsForSession();
        }

        protected override void ClearInitialization()
        {
            // Nothing to do
        }

        private void InitializeConsole()
        {
            if (DebugConsoleBuilder == null)
            {
                LogWarning("No DebugConsoleBuilder found.");
                return;
            }

            FGDebugConsoleBuilder consoleInstance = FindObjectOfType<FGDebugConsoleBuilder>();
            if (!DebugConsoleBuilder.Equals(consoleInstance))
            {
                Instantiate(DebugConsoleBuilder, transform);
            }
        }

        private void InitializeNoInternetPopup()
        {
            if (NoInternetPopup == null) return;

            FGNoInternetPopup noInternetPopup = FindObjectOfType<FGNoInternetPopup>();
            if (!NoInternetPopup.Equals(noInternetPopup))
            {
                NoInternetPopup = Instantiate(NoInternetPopup, transform);
            }
        }

        private void InitializeUpdatePopup()
        {
            if (UpdateVersionPopup == null) return;

            FGUpdatePopup updatePopup = FindObjectOfType<FGUpdatePopup>();
            if (!UpdateVersionPopup.Equals(updatePopup))
            {
                Instantiate(UpdateVersionPopup, transform);
            }
        }

        private void TrackDeviceInfo()
        {
            DeviceInfo deviceInfo = new DeviceInfo();
            // FGAnalytics.NewDesignEvent("HardwareInfo", new Dictionary<string, object>()
            // {
            //     { "Type", deviceInfo.DeviceType },
            //     { "Model", deviceInfo.DeviceModel },
            //     { "Name", deviceInfo.DeviceName },
            //     { "SystemMemory", deviceInfo.SystemMemorySize },
            //     { "GraphicsMemory", deviceInfo.GraphicsMemorySize },
            //     { "ProcessorCount", deviceInfo.ProcessorCount },
            //     { "ProcessorFrequency", deviceInfo.ProcessorFrequency },
            // });
        }

        private void SendAnalytics(bool success)
        {
            Dictionary<string, object> modules = new Dictionary<string, object>();
            foreach (FGModule module in GetAllModulesRecursively(this))
            {
                if (modules.ContainsKey(module.ModuleInfo.Id)) continue;
                modules.Add(module.ModuleInfo.Id, module.ModuleInfo.Version + ":" + module.IsInitialized);
            }

            // FGAnalytics.NewDesignEvent("SDKLoadComplete", modules);
        }

        private List<FGModule> GetAllModulesRecursively(FGModule module)
        {
            List<FGModule> children = new List<FGModule>();
            children.Add(module);
            foreach (FGModule childModule in module.SubModules)
            {
                if (!children.Contains(childModule)) children.Add(childModule);
                children.AddRange(GetAllModulesRecursively(childModule));
            }

            return children;
        }
    }
}