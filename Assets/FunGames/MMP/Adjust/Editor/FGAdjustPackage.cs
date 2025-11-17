using System.Collections.Generic;
using FunGames.Core.Editor;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.MMP.AdjustMMP
{
    public class FGAdjustPackage : FGPackageAbstract<FGAdjustPackage>
    {
        public override string ParentId => "fg_mmp";
        public override string JsonName => "fg_adjust.json";
        public override string PackageName => "FGAdjust";
        public override string ModuleFolder => "MMP/Adjust";
        public override string SettingsAssetName => FGAdjustSettings.NAME;
        public override string DeploymentFolder => "MMP/fg_adjust";
        public override ExportPackageOptions ExportOptions => ExportPackageOptions.Recurse;

        protected override List<GameObject> prefabAssets()
        {
            List<GameObject> prefabs = base.prefabAssets();
            string[] adjustFolder = { "Assets/Adjust" };
            prefabs.AddRange(AssetsUtils.GetAssets<GameObject>(adjustFolder, "Adjust t:prefab"));
            return prefabs;
        }
        
        protected override string[] externalAssets()
        {
            return AssetsUtils.GetAssetsPath(FUNGAMES_EXTERNALS_PATH, "Adjust").ToArray();
        }
        
        public override void AddPrefabs()
        {
            FGMMPPackage.Package.AddPrefabs();
            base.AddPrefabs();
        }
    }
}