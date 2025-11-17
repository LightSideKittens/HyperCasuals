using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Modules;
using FunGames.Core.Settings;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class RemoteSettingsController
    {
        public Action<List<string>> RequestConfirmPluginUpdates;
        public Action UpdateSdkCompleted;
        
        private const double CacheTTLMinutes = 5;
        private readonly Dictionary<string, FGDBModuleSettings> _moduleConfigMap = new();
        private readonly IntegrationManagerController _imc;
        private readonly FGMainSettings _fgMainSettingsAsset;
        private readonly Dictionary<string, List<string>> _pluginToFgModules = new();
        private readonly FGSDKInstallationManifest _ongoingInstallManifest = new();
        private readonly FGUnityPackageInstaller _unityPackageInstaller;

        private DateTime _lastConfigFetchDate;
        private FGDBGameConfig _gameConfig;
        private Dictionary<string, FGModuleInfo> _installedModuleInfos = new();

        public bool ReadyToRunUpdates => _imc.Data != null;
        public bool IsDownloadingConfig { get; private set; }
        public bool IsUpdatingSdk { get; private set; }
        public bool IsVirginProject { get; private set; } = true;
        public string OutdatedModules { get; private set; }
        public string RequestError { get; private set; } = string.Empty;
        public string SdkInstallProgressStatus { get; private set; } = string.Empty;
        private string SdkConfigUrl =>
            "https://api.tnapps.xyz/v1/studios/games/config/" + PlayerSettings.applicationIdentifier;
        private string SdkConfigFilePath => Application.persistentDataPath + "/fg_sdk_remote_settings.json";
        private string OngoingInstallManifestPath => Application.persistentDataPath + "/fg_sdk_install_manifest.lock";
        private string CompilationBlockerFilePath => Application.dataPath + "/CompilationBlocker.cs";

        public string ApiKey
        {
            get => _fgMainSettingsAsset.ApiKey;
            private set => _fgMainSettingsAsset.ApiKey = value;
        }

        public RemoteSettingsController(IntegrationManagerController imc)
        {
            _imc = imc;
            _fgMainSettingsAsset = FGCorePackage.Package.GetModuleSettings<FGMainSettings>();
            _unityPackageInstaller = new FGUnityPackageInstaller(_ongoingInstallManifest);

            _unityPackageInstaller.StatusUpdated += v => { SdkInstallProgressStatus = v; };
            _unityPackageInstaller.ManifestUpdated += ()=> { WriteInstallManifest(false); };
            FGModuleSettings.OnValidateCalled += OnValidateFGModuleSettings;
            UpdateIsVirginProject();
        }
        
        public void Initialize()
        {
            if (File.Exists(OngoingInstallManifestPath)) ResumeInstallFromConfig();
            else if (!IsCacheUpToDate()) FetchSdkConfig(false);
            else LoadSdkConfig(false);
        }

        private void ReadInstallManifest()
        {
            FGSDKInstallationManifest savedManifest =
                JsonUtility.FromJson<FGSDKInstallationManifest>(File.ReadAllText(OngoingInstallManifestPath));
            _ongoingInstallManifest.CopyFrom(savedManifest);
        }

        private void WriteInstallManifest(bool createFileIfNotExist = true)
        {
            if(!createFileIfNotExist && !File.Exists(OngoingInstallManifestPath)) return;
            File.WriteAllText(
                OngoingInstallManifestPath, JsonUtility.ToJson(_ongoingInstallManifest, true));
        }

        private void DeleteInstallManifest()
        {
            File.Delete(OngoingInstallManifestPath);
        }

        private void WriteCompilationBlocker()
        {
            File.WriteAllText(
                CompilationBlockerFilePath, 
                "// The purpose is to block domain reloads during sdk installation.\n" +
                "public class CompilationBlocker {");
        }

        private void DeleteCompilationBlocker()
        {
            string projectFolder = new DirectoryInfo(Application.dataPath).Parent!.FullName;
            string assetPath = Path.GetRelativePath(projectFolder, CompilationBlockerFilePath);
            AssetDatabase.DeleteAsset(assetPath);
        }

        private void FetchSdkConfig(bool installPackagesAfter, bool showPluginsPromptForInstall = false)
        {
            if(IsDownloadingConfig) return;
            
            if (string.IsNullOrEmpty(ApiKey))
            {
                LoadSdkConfig(false);
                return;
            }

            IsDownloadingConfig = true;
            WebUtils.DownloadFileWAuthorization(SdkConfigUrl, SdkConfigFilePath, (result, responseCode)=>
            {
                IsDownloadingConfig = false;
                RequestError = GetRequestError(responseCode);
                if(!result) return;
                _lastConfigFetchDate = DateTime.Now;
                LoadSdkConfig(installPackagesAfter, showPluginsPromptForInstall);
            });
        }

        private string GetRequestError(long httpResponseCode)
        {
            return httpResponseCode switch
            {
                200 => "",
                204 => $"No content in \"{Application.identifier}\" sdk settings",
                401 => "The API token is invalid",
                404 => $"Sdk settings for \"{Application.identifier}\" not found",
                _ => $"Server response: {httpResponseCode}"
            };
        }

        private void LoadSdkConfig(bool installPackagesAfter, bool showPluginsPromptForInstall = false)
        {
            string configJson = File.Exists(SdkConfigFilePath) ? File.ReadAllText(SdkConfigFilePath) : "";
            _gameConfig = JsonUtility.FromJson<FGDBGameConfig>(configJson) ?? new FGDBGameConfig();
            _moduleConfigMap.Clear();

            foreach (var module in _gameConfig.modules)
            {
                AddModuleToMap(module);
            }
            
            FlagOutdatedModules();

            if (installPackagesAfter)
            {
                InitiateInstallFromConfig(showPluginsPromptForInstall);
            }
        }

        private void AddModuleToMap(FGDBModuleSettings moduleConfig)
        {
            _moduleConfigMap.Add(moduleConfig.id, moduleConfig);
            
            foreach (var module in moduleConfig.submodules)
            {
                AddModuleToMap(module);
            }
        }

        private void FlagOutdatedModules()
        {
            if (IsVirginProject)
            {
                OutdatedModules = "";
                return;
            }
            
            List<string> outdatedModules = new List<string>();
            Dictionary<string, FGPackage> packageMap = IntegrationUtils.GetPackageMap();
            
            foreach (FGPackage package in packageMap.Values)
            {
                if (!_moduleConfigMap.ContainsKey(package.ModuleInfo.Id)) continue;

                var moduleConfig = _moduleConfigMap[package.ModuleInfo.Id];
                if(!moduleConfig.is_active) continue;
                
                FGModuleSettings settingsAsset = package.GetModuleSettings<FGModuleSettings>();

                FGModuleInfo installedInfo = JsonUtility.FromJson<FGModuleInfo>(File.ReadAllText(package.JsonFile));
                if (settingsAsset && !settingsAsset.IsModuleUpToDate(moduleConfig, installedInfo))
                {
                    outdatedModules.Add(package.ModuleInfo.Id);
                }
            }

            foreach (string moduleId in _moduleConfigMap.Keys)
            {
                if (!packageMap.ContainsKey(moduleId) && _moduleConfigMap[moduleId].is_active)
                {
                    outdatedModules.Add(moduleId);
                }
            }

            OutdatedModules = string.Join(", ", outdatedModules);
        }

        public void OnClickUpdateSdk(string apiKey)
        {
            if (IsUpdatingSdk) return;

            bool apiKeyChanged = ApiKey != apiKey;
            ApiKey = apiKey;
            RequestError = "";
            _ongoingInstallManifest.Clear();

            if (_moduleConfigMap.Count == 0 || !IsCacheUpToDate() || apiKeyChanged)
            {
                FetchSdkConfig(true, true);
            }
            else
            {
                InitiateInstallFromConfig(true);
            }
        }

        private void InitiateInstallFromConfig(bool showPluginsPrompt)
        {
            _installedModuleInfos = IntegrationUtils.GetFGModuleInfos();
            
            if (showPluginsPrompt)
            {
                ShowPluginsPrompt();
                return;
            }

            InstallFromConfig();
        }
        
        private void ShowPluginsPrompt()
        {
            HashSet<string> pluginsToPrompt = new();
            _pluginToFgModules.Clear();

            foreach (var (id,moduleConfig) in _moduleConfigMap)
            {
                if(!moduleConfig.is_active || IsInstalledVersionEqual(moduleConfig)) continue;

                FGModuleInfo moduleInfo = _imc.Data.GetModuleInfo(id, moduleConfig.module_version);
                if(moduleInfo == null) continue;
                
                pluginsToPrompt.UnionWith(moduleInfo.Plugins);
                MapPluginsToFGModule(moduleInfo);
            }
            
            if(pluginsToPrompt.Count == 0) InstallFromConfig();
            else RequestConfirmPluginUpdates?.Invoke(pluginsToPrompt.ToList());
        }

        public void OnUserDecidedPluginsToInstall(List<string> pluginsToInstall)
        {
            _ongoingInstallManifest.enabledPluginNames = pluginsToInstall;
            InstallFromConfig();
        }
        
        private async void InstallFromConfig()
        {
            IsUpdatingSdk = true;
            WriteCompilationBlocker();
            WriteInstallManifest();
            SdkInstallProgressStatus = string.Empty;
            
            foreach (var module in _gameConfig.modules)
            {
                await FetchActiveModules(module);
            }
            
            await _unityPackageInstaller.ImportExternalPackages(_imc.Data);
            FillRemoteSettings();
            UpdateIsVirginProject();
            FlagOutdatedModules();
            IsUpdatingSdk = false;
            SdkInstallProgressStatus = string.Empty;
            _ongoingInstallManifest.Clear();
            DeleteInstallManifest();
            DeleteCompilationBlocker();
            Debug.Log("Sdk update completed");
            IntegrationPostInstallProcessor.OnInstallComplete(_moduleConfigMap);
            UpdateSdkCompleted?.Invoke();
        }

        private void ResumeInstallFromConfig()
        {
            Debug.Log("Resuming install...");
            ReadInstallManifest();
            LoadSdkConfig(true, false);
        }
        
        private void MapPluginsToFGModule(FGModuleInfo moduleInfo)
        {
            foreach (var plugin in moduleInfo.Plugins)
            {
                _pluginToFgModules.TryGetValue(plugin, out List<string> modules);
                modules ??= new List<string>();
                modules.Add(moduleInfo.Name);
                _pluginToFgModules[plugin] = modules;
            }
        }

        public List<string> GetModulesForPlugin(string pluginFileName)
        {
            _pluginToFgModules.TryGetValue(pluginFileName, out List<string> modules);
            modules ??= new List<string>();
            return modules;
        }

        private async Task FetchActiveModules(FGDBModuleSettings moduleConfig)
        {
            if(!File.Exists(OngoingInstallManifestPath)) return;
            if(!moduleConfig.is_active) return;

            if (!IsInstalledVersionEqual(moduleConfig))
            {
                await DownloadAndImport(moduleConfig);
                _imc.MapLocalSetup();
            }

            foreach (var module in moduleConfig.submodules)
            {
                await FetchActiveModules(module);
            }
        }
        
        private async Task DownloadAndImport(FGDBModuleSettings moduleConfig)
        {
            Debug.Log($"DownloadAndImport {moduleConfig.id} ...");
            SdkInstallProgressStatus = $"Download and import {moduleConfig.id}...";
            string packageName = moduleConfig.id + "-" + moduleConfig.module_version + ".unitypackage";
            string filePath = Path.GetTempPath() + packageName;
            string packageUrl = IntegrationUtils.GetPackageUrl(
                moduleConfig.id, moduleConfig.module_version, moduleConfig.deployment_folder);
            var downloadTcs = new TaskCompletionSource<string>();

            WebUtils.DownloadFile(
                packageUrl,
                filePath, 
                () =>
                {
                    downloadTcs.SetResult("");
            });
            
            await downloadTcs.Task;
            await ImportFGUnityPackage(filePath);
        }

        private async Task ImportFGUnityPackage(string filePath)
        {
            if(!File.Exists(filePath)) return;

            await _unityPackageInstaller.ImportUnityPackage(filePath, _imc.Data);
            File.Delete(filePath);
        }

        private bool IsInstalledVersionEqual(FGDBModuleSettings moduleConfig)
        {
            if (!_installedModuleInfos.ContainsKey(moduleConfig.id)) return false;
            return _installedModuleInfos[moduleConfig.id].Version == moduleConfig.module_version;
        }

        private void FillRemoteSettings()
        {
            Debug.Log("FillRemoteSettings");
            if(_moduleConfigMap.Count == 0) return;
            
            List<FGPackage> packages = ProjectUtils.GetEnumerableOfType<FGPackage>();

            foreach (FGPackage package in packages)
            {
                if (!_moduleConfigMap.ContainsKey(package.ModuleInfo.Id)) continue;

                FGDBModuleSettings moduleConfig = _moduleConfigMap[package.ModuleInfo.Id];
                FGModuleSettings settingsAsset = package.GetModuleSettings<FGModuleSettings>();
                
                FGModuleInfo installedInfo = JsonUtility.FromJson<FGModuleInfo>(File.ReadAllText(package.JsonFile));
                if(settingsAsset == null || settingsAsset.IsModuleUpToDate(moduleConfig, installedInfo)) continue;
                
                settingsAsset.FillModuleSettings(moduleConfig);
                
                var parameters = _moduleConfigMap[package.ModuleInfo.Id].parameters;
                Debug.Log($"Updated {package.ModuleInfo.Id} params: {string.Join(",", parameters)}");
            }
            
            AssetDatabase.SaveAssets();
        }
        
        private void OnValidateFGModuleSettings()
        {
            if (IsUpdatingSdk)
            {
                FillRemoteSettings();
            }
        }

        private void UpdateIsVirginProject()
        {
            IsVirginProject = ProjectUtils.GetEnumerableOfType<FGPackage>().Count <= 2;
        }
        
        private bool IsCacheUpToDate()
        {
            if (!File.Exists(SdkConfigFilePath)) return false;
            return Math.Abs(_lastConfigFetchDate.Subtract(DateTime.Now).TotalMinutes) <= CacheTTLMinutes;
        }
    }
}