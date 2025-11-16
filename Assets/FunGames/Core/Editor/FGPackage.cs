using System.Collections.Generic;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Modules;
using FunGames.Core.Settings;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor
{
    public abstract class FGPackage
    {
        public virtual string ParentId { get; } = "";
        public abstract FGModuleInfo ModuleInfo { get; }
        public abstract string JsonName { get; }
        public abstract string PackageName { get; }
        public abstract string ModuleFolder { get; }
        public abstract string[] AssetFolders { get; }
        public abstract string SettingsAsset { get; }
        public abstract string SettingsAssetName { get; }
        public abstract string[] ExternalAssets { get; }
        public abstract string DestinationPath { get; }
        public abstract string DeploymentFolder { get; }
        public abstract ExportPackageOptions ExportOptions { get; }
        public abstract List<GameObject> Prefabs { get; }
        public abstract string JsonFile { get; }

        public abstract void AddPrefabs();
        
        public abstract void CreateSettingsAsset();

        public abstract S GetModuleSettings<S>() where S : FGModuleSettings;

        public abstract void UpdateSettings();
        
        public List<FGDBModuleParameter> ExportModuleParameters()
        {
            return GetModuleSettings<FGModuleSettings>().ExportParameters();
        }
    }
}