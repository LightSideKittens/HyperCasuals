using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.Analytics
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGAnalyticsSettings : FGModuleSettingsAbstract<FGAnalyticsSettings>
    {
        public const string NAME = "FGAnalyticsSettings";
        const string PATH = FGPath.FUNGAMES + "/" + NAME;

        protected override FGAnalyticsSettings LoadResources()
        {
            return Resources.Load<FGAnalyticsSettings>(PATH);
        }
    }
}