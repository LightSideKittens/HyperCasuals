using System;
using AdjustSdk;
using FunGames.Analytics;
using FunGames.Core.Modules;

namespace FunGames.MMP.AdjustMMP
{
    public class FGAdjust : FGModuleAbstract<FGAdjust, FGMMPCallbacks, FGAdjustSettings>
    {
        public override FGAdjustSettings Settings => FGAdjustSettings.settings;
        protected override FGModule Parent => FGMMPManager.Instance;
        protected override string EventName => "Adjust";
        protected override string RemoteConfigKey => "FGAdjust";

        private static bool _subscribed = false;

        protected override void InitializeCallbacks()
        {
            FGMMPManager.Instance.Callbacks.Initialization += Initialize;
        }
        
        protected override void OnAwake()
        {
            //
        }

        protected override void OnStart()
        {
           //
        }

        protected override void InitializeModule()
        {
            AdjustEnvironment environment = GetEnvironment();
            Log("Environment : " + environment);

            AdjustConfig adjustConfig = new AdjustConfig(FGAdjustSettings.settings.AppToken.Trim(), environment);
            adjustConfig.LogLevel = FGAdjustSettings.settings.logLevel;
            adjustConfig.AttributionChangedDelegate = attributionChangedDelegate;
            adjustConfig.IsPreinstallTrackingEnabled = true;
            adjustConfig.IsSendingInBackgroundEnabled = FGAdjustSettings.settings.sendInBackground;
            adjustConfig.DeferredDeeplinkDelegate = OnDeferredDeepLink;
            Adjust.InitSdk(adjustConfig);
            
            InitializationComplete(!String.IsNullOrEmpty(FGAdjustSettings.settings.AppToken));
        }

        
        public void attributionChangedDelegate(AdjustAttribution attribution)
        {
            Log("Attribution changed");
            FGAnalytics.NewDesignEvent("NetworkAttribution:" + attribution.Network);
        }

        public static AdjustEnvironment GetEnvironment()
        {
            return AdjustEnvironment.Production;
        }
        
        private void OnDeferredDeepLink(string obj)
        {
            Callbacks._onDeferredDeepLink?.Invoke(obj);
            FGMMPManager.Instance.Callbacks._onDeferredDeepLink?.Invoke(obj);
        }

        protected override void ClearInitialization()
        {
            FGMMPManager.Instance.Callbacks.Initialization -= Initialize;
        }
    }
}