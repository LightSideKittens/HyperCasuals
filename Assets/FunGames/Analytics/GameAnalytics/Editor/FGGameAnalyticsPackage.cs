using System.Collections.Generic;
using FunGames.Core.Editor;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.Analytics.GA
{
    public class FGGameAnalyticsPackage : FGPackageAbstract<FGGameAnalyticsPackage>
    {
        public override string ParentId => "fg_analytics";
        public override string JsonName => "fg_game_analytics.json";

        public override string PackageName => "FGGameAnalytics";

        public override string ModuleFolder => "Analytics/GameAnalytics";

        public override string SettingsAssetName => FGGameAnalyticsSettings.NAME;
        public override string DeploymentFolder => "Analytics/fg_game_analytics";
        public override ExportPackageOptions ExportOptions => ExportPackageOptions.Recurse;

        protected override List<GameObject> prefabAssets()
        {
            List<GameObject> prefabs = base.prefabAssets();
            string[] gameAnalyticsFolder = { "Assets/GameAnalytics" };
            prefabs.AddRange(AssetsUtils.GetAssets<GameObject>(gameAnalyticsFolder, "GameAnalytics t:prefab"));
            return prefabs;
        }
        
        protected override string[] externalAssets()
        {
            return AssetsUtils.GetAssetsPath(FUNGAMES_EXTERNALS_PATH, "GA_SDK").ToArray();
        }
        
        public override void AddPrefabs()
        {
            FGAnalyticsPackage.Package.AddPrefabs();
            base.AddPrefabs();
        }
    }
}