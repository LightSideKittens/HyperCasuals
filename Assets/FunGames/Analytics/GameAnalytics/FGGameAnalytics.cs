using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using FunGames.Core.Modules;
using UnityEngine;

namespace FunGames.Analytics.GA
{
    public class FGGameAnalytics : FGAnalyticsAbstract<FGGameAnalytics, FGAnalyticsCallbacks, FGGameAnalyticsSettings>
    {
        public override FGGameAnalyticsSettings Settings => FGGameAnalyticsSettings.settings;
        protected override FGModule Parent => FGAnalyticsManager.Instance;
        protected override string EventName => "GameAnalytics";

        protected override string RemoteConfigKey => "FGGameAnalytics";

        protected override void OnAwake()
        {
            // throw new NotImplementedException();
        }

        protected override void OnStart()
        {
            // throw new NotImplementedException();
        }

        private bool isInit = false;

        /// <summary>
        /// Private function that initializes all GA elements
        /// </summary>
        protected override void Init()
        {
            if (isInit)
            {
                LogWarning("GameAnalytics is already initialized");
                return;
            }

            isInit = true;

            var gameAnalytics = FindObjectOfType<GameAnalytics>();

            if (gameAnalytics == null)
            {
                throw new Exception("It seems like you haven't instantiated GameAnalytics GameObject");
            }

#if UNITY_IOS
            string gameKey = FGGameAnalyticsSettings.settings.gameAnalyticsIosGameKey.Trim();
            string gameSecretKey = FGGameAnalyticsSettings.settings.gameAnalyticsIosSecretKey.Trim();
            AddOrUpdatePlatform(RuntimePlatform.IPhonePlayer, gameKey, gameSecretKey);
#else
            string gameKey = FGGameAnalyticsSettings.settings.gameAnalyticsAndroidGameKey.Trim();
            string gameSecretKey = FGGameAnalyticsSettings.settings.gameAnalyticsAndroidSecretKey.Trim();
            AddOrUpdatePlatform(RuntimePlatform.Android, gameKey, gameSecretKey);
#endif
            GameAnalytics.SettingsGA.InfoLogBuild = false;
            GameAnalytics.SettingsGA.InfoLogEditor = false;
            GameAnalytics.SettingsGA.SubmitFpsAverage = true;
            GameAnalytics.SettingsGA.SubmitFpsCritical = true;
            GameAnalyticsILRD.SubscribeMaxImpressions();
            GameAnalytics.Initialize();

            InitializationComplete(!String.IsNullOrEmpty(gameKey) &&
                                   !String.IsNullOrEmpty(gameSecretKey));

            if (String.IsNullOrEmpty(gameKey) || String.IsNullOrEmpty(gameSecretKey))
                LogError("Some Key is missing in FG GameAnalytics Settings");
        }

        /// <summary>
        /// Init the Game Analytic Settings for the game on each plateform is on
        /// </summary>
        /// <param name="platform">Android or iOS</param>
        /// <param name="gameKey">GA Gamekey (public key)</param>
        /// <param name="secretKey">GA Secret Key</param>
        private static void AddOrUpdatePlatform(RuntimePlatform platform, string gameKey, string secretKey)
        {
            if (!GameAnalytics.SettingsGA.Platforms.Contains(platform))
            {
                GameAnalytics.SettingsGA.AddPlatform(platform);
            }

            var index = GameAnalytics.SettingsGA.Platforms.IndexOf(platform);

            GameAnalytics.SettingsGA.UpdateGameKey(index, gameKey);
            GameAnalytics.SettingsGA.UpdateSecretKey(index, secretKey);
            GameAnalytics.SettingsGA.Build[index] = Application.version;
        }

        /// <summary>
        /// Remove the Settings of GA for a plateform
        /// </summary>
        /// <param name="platform">Android or iOS</param>
        private static void RemovePlatform(RuntimePlatform platform)
        {
            if (!GameAnalytics.SettingsGA.Platforms.Contains(platform)) return;

            var index = GameAnalytics.SettingsGA.Platforms.IndexOf(platform);
            GameAnalytics.SettingsGA.RemovePlatformAtIndex(index);
        }

        protected override void ProgressionEvent(LevelStatus levelStatus, string progression01,
            int score = FGAnalytics.NO_SCORE)
        {
            if (score == FGAnalytics.NO_SCORE)
            {
                GameAnalytics.NewProgressionEvent(GetGAStatus(levelStatus), progression01);
            }
            else
            {
                GameAnalytics.NewProgressionEvent(GetGAStatus(levelStatus), progression01, score);
            }
        }

        protected override void ProgressionEvent(LevelStatus levelStatus, string progression01, string progression02,
            int score = FGAnalytics.NO_SCORE)
        {
            if (score == FGAnalytics.NO_SCORE)
            {
                GameAnalytics.NewProgressionEvent(GetGAStatus(levelStatus), progression01, progression02);
            }
            else
            {
                GameAnalytics.NewProgressionEvent(GetGAStatus(levelStatus), progression01, progression02, score);
            }
        }

        protected override void ProgressionEvent(LevelStatus levelStatus, string progression01, string progression02,
            string progression03,
            int score = FGAnalytics.NO_SCORE)
        {
            if (score == FGAnalytics.NO_SCORE)
            {
                GameAnalytics.NewProgressionEvent(GetGAStatus(levelStatus), progression01, progression02,
                    progression03);
            }
            else
            {
                GameAnalytics.NewProgressionEvent(GetGAStatus(levelStatus), progression01, progression02, progression03,
                    score);
            }
        }

        /// <summary>
        /// Private function that sends a simple Design Event from GA with FGA data
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventValue"></param>
        protected override void DesignEventSimple(string eventId, float eventValue)
        {
            GameAnalytics.NewDesignEvent(ValidString(eventId), eventValue);
        }

        /// <summary>
        /// Private function that sends a Desisgn Event with dictionnary data store in it from GA with FGA data
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="customFields"></param>
        /// <param name="eventValue"></param>
        protected override void DesignEventDictio(string eventId, Dictionary<string, object> customFields)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            foreach (var keyValue in customFields)
            {
                fields.Add(ValidString(keyValue.Key), ValidString(keyValue.Value.ToString()));
            }

            GameAnalytics.NewDesignEvent(ValidString(eventId), fields);
        }

        /// <summary>
        /// Private function that sends a Ad Event from GA with FGA data
        /// </summary>
        /// <param name="adAction"></param>
        /// <param name="adType"></param>
        /// <param name="adSdkName"></param>
        /// <param name="adPlacement"></param>
        protected override void AdEvent(AdAction adAction, AdType adType, string adSdkName, string adPlacement)
        {
            GAAdAction action;
            switch (adAction)
            {
                case AdAction.Clicked:
                    action = GAAdAction.Clicked;
                    break;
                case AdAction.FailedShow:
                    action = GAAdAction.FailedShow;
                    break;
                case AdAction.Loaded:
                    action = GAAdAction.Loaded;
                    break;
                case AdAction.Request:
                    action = GAAdAction.Request;
                    break;
                case AdAction.RewardReceived:
                    action = GAAdAction.RewardReceived;
                    break;
                case AdAction.Show:
                    action = GAAdAction.Show;
                    break;
                default:
                    string eventId = FGAnalytics.CreateEventId("Ad" + adAction, adType.ToString(), adPlacement);
                    DesignEventDictio(eventId, new Dictionary<string, object>() { { "sdkNetwork", adSdkName } });
                    return;
            }

            GAAdType type;
            switch (adType)
            {
                case AdType.Banner:
                    type = GAAdType.Banner;
                    break;
                case AdType.Interstitial:
                    type = GAAdType.Interstitial;
                    break;
                case AdType.OfferWall:
                    type = GAAdType.OfferWall;
                    break;
                case AdType.Playable:
                    type = GAAdType.Playable;
                    break;
                case AdType.RewardedVideo:
                    type = GAAdType.RewardedVideo;
                    break;
                case AdType.Video:
                    type = GAAdType.Video;
                    break;
                default:
                    string eventId = FGAnalytics.CreateEventId("Ad" + adAction, adType.ToString(), adPlacement);
                    DesignEventDictio(eventId, new Dictionary<string, object>() { { "sdkNetwork", adSdkName } });
                    return;
            }

            GameAnalytics.NewAdEvent(action, type, ValidString(adSdkName), ValidString(adPlacement));
        }

        protected override void ClearInitialization()
        {
            base.ClearInitialization();
            RemovePlatform(Application.platform);
        }

        private string ValidString(string str)
        {
            if (String.IsNullOrEmpty(str)) return "ND";
            return str.Replace(" ", "_");
        }

        private GAProgressionStatus GetGAStatus(LevelStatus levelStatus)
        {
            GAProgressionStatus status;
            switch (levelStatus)
            {
                case LevelStatus.Complete:
                    status = GAProgressionStatus.Complete;
                    break;
                case LevelStatus.Fail:
                    status = GAProgressionStatus.Fail;
                    break;
                default:
                    status = GAProgressionStatus.Start;
                    break;
            }

            return status;
        }
    }
}