using UnityEditor;

namespace FunGames.Core.Editor
{
    public class FGCorePackage : FGPackageAbstract<FGCorePackage>
    {
        public override string ParentId => "";
        public override string JsonName => "fg_core.json";
        public override string PackageName => "FGCore";
        public override string ModuleFolder => "Core";
        public override string SettingsAssetName => FGMainSettings.NAME;
        public override string DeploymentFolder => "Core/fg_core";

        public override ExportPackageOptions ExportOptions => ExportPackageOptions.Recurse;
    }
}