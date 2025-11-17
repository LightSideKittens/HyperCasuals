using FunGames.Core.Editor;
using UnityEditor;

namespace FunGames.Analytics
{
    public class FGAnalyticsPackage : FGPackageAbstract<FGAnalyticsPackage>
    {
        public override string ParentId => "";
        public override string JsonName => "fg_analytics.json";
        public override string PackageName => "FGAnalytics";
        public override string ModuleFolder => "Analytics";
        public override string SettingsAssetName => FGAnalyticsSettings.NAME;
        public override string DeploymentFolder => "Analytics/fg_analytics";
        public override ExportPackageOptions ExportOptions => ExportPackageOptions.Default;
        
        
    }
}