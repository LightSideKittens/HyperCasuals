using System.Collections.Generic;
using AdjustSdk;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.MMP.AdjustMMP
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGAdjustSettings : FGModuleSettingsAbstract<FGAdjustSettings>
    {
        public const string NAME = "FGAdjustSettings";
        const string PATH = FGPath.FUNGAMES + "/" + NAME;
        private const string AppTokenParam = "AppToken";
        private const string AdjustEventInterIdParam = "AdjustEventInterID";
        private const string AdjustEventRewardedIdParam = "AdjustEventRewardedID";

        protected override FGAdjustSettings LoadResources()
        {
            return Resources.Load<FGAdjustSettings>(PATH);
        }

        [Tooltip("Adjust App Token")] public string AppToken;

        [Tooltip("Adjust Log Level")] public AdjustLogLevel logLevel = AdjustLogLevel.Info;

        // [Tooltip("Adjust Environment")] public AdjustEnvironment environment = AdjustEnvironment.Sandbox;
        public bool sendInBackground = true;
        public bool setLaunchDeferredDeeplink = true;

#if UNITY_EDITOR
        
        protected override void FillRemoteParameter(FGDBModuleParameter parameter)
        {
            switch (parameter.id)
            {
                case AppTokenParam:
                    AppToken = parameter.value;
                    break;
            }
        }

        protected override string GetRemoteParameterValue(string parameterId)
        {
            return parameterId switch
            {
                AppTokenParam => AppToken,
                _ => null
            };
        }

        public override List<FGDBModuleParameter> ExportParameters()
        {
            return new List<FGDBModuleParameter>
            {
                new()
                {
                    id = AppTokenParam,
                    name = "App Token",
                    value = GetRemoteParameterValue(AppTokenParam),
                    platform = FGDBPlatform.ALL.ToString(),
                    is_mandatory = true,
                    regex = AllCharsNoNewlineRegex,
                }
            };
        }
        
#endif

    }
}