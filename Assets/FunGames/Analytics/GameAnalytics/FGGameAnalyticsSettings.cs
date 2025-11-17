using System.Collections.Generic;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.Analytics.GA
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGGameAnalyticsSettings : FGModuleSettingsAbstract<FGGameAnalyticsSettings>
    {
        public const string NAME = "FGGameAnalyticsSettings";
        const string PATH = FGPath.FUNGAMES + "/" + NAME;
        private const string AndroidGameKeyParam = "AndroidGameKey";
        private const string AndroidSecretKeyParam = "AndroidSecretKey";
        private const string IosGameKeyParam = "IosGameKey";
        private const string IosSecretKeyParam = "IosSecretKey";

        protected override FGGameAnalyticsSettings LoadResources()
        {
            return Resources.Load<FGGameAnalyticsSettings>(PATH);
        }

        [Header("GameAnalytics")] [Tooltip("GameAnalytics Ios Game Key")]
        public string gameAnalyticsIosGameKey;

        [Tooltip("GameAnalytics Ios Secret Key")]
        public string gameAnalyticsIosSecretKey;

        [Tooltip("GameAnalytics Android Game Key")]
        public string gameAnalyticsAndroidGameKey;

        [Tooltip("GameAnalytics Android Secret Key")]
        public string gameAnalyticsAndroidSecretKey;
        
#if UNITY_EDITOR

        protected override void FillRemoteParameter(FGDBModuleParameter parameter)
        {
            switch (parameter.id)
            {
                case AndroidGameKeyParam:
                    gameAnalyticsAndroidGameKey = parameter.value;
                    break;
                case AndroidSecretKeyParam:
                    gameAnalyticsAndroidSecretKey = parameter.value;
                    break;
                case IosGameKeyParam:
                    gameAnalyticsIosGameKey = parameter.value;
                    break;
                case IosSecretKeyParam:
                    gameAnalyticsIosSecretKey = parameter.value;
                    break;
            }
        }

        protected override string GetRemoteParameterValue(string parameterId)
        {
            return parameterId switch
            {
                AndroidGameKeyParam => gameAnalyticsAndroidGameKey,
                AndroidSecretKeyParam => gameAnalyticsAndroidSecretKey,
                IosGameKeyParam => gameAnalyticsIosGameKey,
                IosSecretKeyParam => gameAnalyticsIosSecretKey,
                _ => null
            };
        }
        
        public override List<FGDBModuleParameter> ExportParameters()
        {
            return new List<FGDBModuleParameter>
            {
                new()
                {
                    id = AndroidGameKeyParam,
                    name = "Android Game Key",
                    value = GetRemoteParameterValue(AndroidGameKeyParam),
                    platform = FGDBPlatform.Android.ToString(),
                    is_mandatory = true,
                    regex = AllCharsNoNewlineRegex,
                },
                new()
                {
                    id = AndroidSecretKeyParam,
                    name = "Android Secret Key",
                    value = GetRemoteParameterValue(AndroidSecretKeyParam),
                    platform = FGDBPlatform.Android.ToString(),
                    is_mandatory = true,
                    regex = AllCharsNoNewlineRegex,
                },
                new()
                {
                    id = IosGameKeyParam,
                    name = "iOS Game Key",
                    value = GetRemoteParameterValue(IosGameKeyParam),
                    platform = FGDBPlatform.IOS.ToString(),
                    is_mandatory = true,
                    regex = AllCharsNoNewlineRegex,
                },
                new()
                {
                    id = IosSecretKeyParam,
                    name = "iOS Secret Key",
                    value = GetRemoteParameterValue(IosSecretKeyParam),
                    platform = FGDBPlatform.IOS.ToString(),
                    is_mandatory = true,
                    regex = AllCharsNoNewlineRegex,
                },
            };
        }
        
#endif
        
    }
}