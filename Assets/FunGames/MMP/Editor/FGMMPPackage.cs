using FunGames.Analytics;
using FunGames.Core.Editor;
using UnityEditor;

namespace FunGames.MMP
{
    public class FGMMPPackage : FGPackageAbstract<FGMMPPackage>
    {
        public override string ParentId => "";
        public override string JsonName => "fg_mmp.json";
        public override string PackageName => "FGMMP";
        public override string ModuleFolder => "MMP";
        public override string SettingsAssetName => FGMMPSettings.NAME;
        public override string DeploymentFolder => "MMP/fg_mmp";
        public override ExportPackageOptions ExportOptions => ExportPackageOptions.Default;
    }
}