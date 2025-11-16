using System.Collections.Generic;
using FunGames.Core.Editor;
using FunGames.Tools.Utils;
using UnityEditor;

namespace FunGames.Tools.Editor
{
    public class FGToolsPackage : FGPackageAbstract<FGToolsPackage>
    {
        public override string ParentId => "";
        public override string JsonName => "fg_tools.json";
        public override string PackageName => "FGTools";
        public override string ModuleFolder => "Tools";
        public override string SettingsAssetName => FGToolsSettings.NAME;
        public override string DeploymentFolder => "Tools/fg_tools";
        public override ExportPackageOptions ExportOptions => ExportPackageOptions.Recurse;

        protected override string[] assetFolders()
        {
            List<string> folders = new List<string>();
            folders.AddRange(base.assetFolders());
            folders.Add("Assets/RestClient");
            folders.Add( "Assets/Resources/FunGames/Debug");
            return folders.ToArray();
        }
        
        protected override string[] externalAssets()
        {
            string[] pluginFolder = {"Assets/Plugins"};
            return AssetsUtils.GetAssetsPath(pluginFolder, "NativeAppstore").ToArray();
        }
    }
}