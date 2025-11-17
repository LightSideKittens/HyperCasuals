using System.Collections.Generic;
using System.IO;
using FunGames.Analytics;
using FunGames.Analytics.GA;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Editor;
using FunGames.MMP;
using FunGames.MMP.AdjustMMP;
using FunGames.Tools.Editor;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.Editor
{
    public static class FGDBConfigExporter
    {
        private static string _exportFolderPath = "";
        private static Dictionary<string, FGPackage> _packagesMap;
        private static Dictionary<string, FGDBModuleSettings> _dbModuleSettingsMap;
        private static List<FGDBModuleSettings> _dbModuleSettingsTree;
        private const string CpiTestVersion = "cpi_test";

        private static readonly HashSet<string> CpiTestModules = new()
        {
            FGCorePackage.Package.ModuleInfo.Id,
            FGAnalyticsPackage.Package.ModuleInfo.Id,
            FGGameAnalyticsPackage.Package.ModuleInfo.Id,
            FGMMPPackage.Package.ModuleInfo.Id,
            FGAdjustPackage.Package.ModuleInfo.Id,
            FGToolsPackage.Package.ModuleInfo.Id
        };

        public static void ExportDatabaseConfig(FGDBConfigType configType, bool includeParameterValues)
        {
            string gameConfigId = GetConfigId(configType);
            string configJson = GetDatabaseConfig(configType, includeParameterValues);
            _exportFolderPath = EditorUtility.OpenFolderPanel("Select Directory", _exportFolderPath, "");
            string exportPath = _exportFolderPath + $"/{gameConfigId}-config.json";
            File.WriteAllText(exportPath, configJson);
            Debug.Log($"Exported to {exportPath}");
        }
        
        private static string GetDatabaseConfig(FGDBConfigType configType, bool includeParameterValues)
        {
            _packagesMap = GetPackagesMap();
            _dbModuleSettingsTree = new List<FGDBModuleSettings>();
            _dbModuleSettingsMap = new Dictionary<string, FGDBModuleSettings>();
            IEnumerable<FGPackage> packages = _packagesMap.Values;

            foreach (FGPackage package in packages)
            {
                if (!IncludeInConfig(configType, package)) continue;
                AddToSettingsTree(package, configType);
            }

            string gameConfigId = GetConfigId(configType);
            FGDBGameConfig gameConfig = new FGDBGameConfig
            {
                id = gameConfigId,
                name = configType.ToString(),
                modules = _dbModuleSettingsTree
            };
            
            if(!includeParameterValues) gameConfig.RemoveParameterValues();
            gameConfig.SortModules();

            return JsonUtility.ToJson(gameConfig, true);
        }

        private static FGDBModuleSettings AddToSettingsTree(FGPackage package, FGDBConfigType configType)
        {
            if (_dbModuleSettingsMap.ContainsKey(package.ModuleInfo.Id))
            {
                return _dbModuleSettingsMap[package.ModuleInfo.Id];
            }

            FGDBModuleSettings parentModuleSettings = null;
            
            if (!string.IsNullOrEmpty(package.ParentId))
            {
                parentModuleSettings = AddToSettingsTree(_packagesMap[package.ParentId], configType);
            }

            string moduleVersion = configType == FGDBConfigType.CpiTest ? CpiTestVersion : package.ModuleInfo.Version;
            
            FGDBModuleSettings moduleModel = new FGDBModuleSettings
            {
                id = package.ModuleInfo.Id,
                name = package.ModuleInfo.Name,
                module_version = moduleVersion,
                is_mandatory = IsMandatoryInConfig(configType, package),
                deployment_folder = package.DeploymentFolder.ToLower(),
                parameters = package.ExportModuleParameters()
            };
            
            _dbModuleSettingsMap.Add(package.ModuleInfo.Id, moduleModel);

            if (parentModuleSettings == null)
            {
                _dbModuleSettingsTree.Add(moduleModel);
            }
            else
            {
                parentModuleSettings.submodules.Add(moduleModel);
            }
            
            return moduleModel;
        }

        private static Dictionary<string, FGPackage> GetPackagesMap()
        {
            List<FGPackage> packages = ProjectUtils.GetEnumerableOfType<FGPackage>();
            Dictionary<string, FGPackage> packagesMap = new Dictionary<string, FGPackage>();

            foreach (FGPackage package in packages)
            {
                packagesMap[package.ModuleInfo.Id] = package;
            }

            return packagesMap;
        }

        private static string GetConfigId(FGDBConfigType gameConfigType)
        {
            switch (gameConfigType)
            {
                case FGDBConfigType.CpiTest:
                    return "cpi_test";
                default:
                    return "";
            }
        }

        private static bool IncludeInConfig(FGDBConfigType gameConfigType, FGPackage package)
        {
            switch (gameConfigType)
            {
                case FGDBConfigType.CpiTest:
                    return CpiTestModules.Contains(package.ModuleInfo.Id);
                default:
                    return false;
            }
        }
        
        private static bool IsMandatoryInConfig(FGDBConfigType gameConfigType, FGPackage package)
        {
            switch (gameConfigType)
            {
                case FGDBConfigType.CpiTest:
                    return CpiTestModules.Contains(package.ModuleInfo.Id);
                default:
                    return false;
            }
        }
        
        
        // [MenuItem("FunGames/Export modules settings",false, 3)]
        // public static void ExportDbConfigWithValues()
        // {
        //     ExportDatabaseConfig(FGDBConfigType.CpiTest, true);
        // }
    }
}
