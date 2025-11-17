using System.Collections.Generic;
using FunGames.Core;
using FunGames.Core.Modules;

namespace FunGames.Analytics
{
    public class FGAnalyticsManager : FGModuleAbstract<FGAnalyticsManager, FGAnalyticsCallbacks, FGAnalyticsSettings>
    {
        public override FGAnalyticsSettings Settings => FGAnalyticsSettings.settings;
        protected override FGModule Parent => FGCore.Instance;
        protected override string EventName => "Analytics";

        protected override string RemoteConfigKey => "FGAnalytics";
        
        protected override void InitializeCallbacks()
        {
            FunGamesSDK.Callbacks.Initialization += Initialize;
        }

        protected override void OnAwake()
        {}

        protected override void OnStart()
        {}

        protected override void InitializeModule()
        {
           InitializationComplete(true);
        }

        public void SendProgressionEvent(LevelStatus levelStatus, string prog01, int score)
        {
            Callbacks._ProgressionEvent01?.Invoke(levelStatus, prog01, score);
        }

        public void SendProgressionEvent(LevelStatus levelStatus, string prog01, string prog02, int score)
        {
            Callbacks._ProgressionEvent02?.Invoke(levelStatus, prog01, prog02, score);
        }

        public void SendProgressionEvent(LevelStatus levelStatus, string prog01, string prog02, string prog03,
            int score)
        {
            Callbacks._ProgressionEvent03?.Invoke(levelStatus, prog01, prog02, prog03, score);
        }

        public void SendDesignEventSimple(string eventId, float eventValue)
        {
            Callbacks._DesignEventSimple?.Invoke(eventId, eventValue);
        }

        public void SendDesignEventDictio(string eventId, Dictionary<string, object> customFields, float eventValue)
        {
            Callbacks._DesignEventDictio?.Invoke(eventId, customFields, eventValue);
        }

        public void SendAdEvent(AdAction adAction, AdType adType, string adSdkName, string adPlacement)
        {
            Callbacks._AdEvent?.Invoke(adAction, adType, adSdkName, adPlacement);
        }

        protected override void ClearInitialization()
        {}
    }
}