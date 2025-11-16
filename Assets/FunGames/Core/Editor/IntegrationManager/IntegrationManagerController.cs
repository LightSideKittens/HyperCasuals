using System;
using System.Collections.Generic;
using System.IO;
using FunGames.Core.Modules;
using FunGames.Editor;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class IntegrationManagerController
    {
        public static string NOT_INSTALLED = " - ";
        public FGMainJsonImport Data => _mainJsonImport;

        private FGMainJson _mainJson;
        private FGMainJsonImport _mainJsonImport;
        private DateTime _lastUpdateDate;

        private const string REMOTE_DATA_FILE = "/fg_main.json";

        private Action _onDataLoaded;
        private AssetDatabase.ImportPackageCallback _packageImported;

        public event Action OnDataLoaded
        {
            add => _onDataLoaded += value;
            remove => _onDataLoaded -= value;
        }

        private Dictionary<string, FGPackage> _packages = new Dictionary<string, FGPackage>();

        private string RemoteDataFile => Application.persistentDataPath + REMOTE_DATA_FILE;

        public void Initialize()
        {
            Debug.Log(RemoteDataFile);
            if (File.Exists(RemoteDataFile) && IsCacheUpToDate()) LoadData();
            else WebUtils.DownloadFile(FGMainJsonImport.URL, RemoteDataFile,()=>
            {
                _lastUpdateDate = DateTime.Now;
                LoadData();
            });
        }

        private void LoadData()
        {
            _mainJson = JsonUtility.FromJson<FGMainJson>(File.ReadAllText(RemoteDataFile));
            _mainJsonImport = new FGMainJsonImport(_mainJson);
            MapLocalSetup();
            _onDataLoaded?.Invoke();
        }

        public string GetCurrentVersion(string id)
        {
            if (_packages.ContainsKey(id)) return _packages[id].ModuleInfo.Version;
            return NOT_INSTALLED;
        }

        public bool IsModuleInstalled(string id)
        {
            return GetPackage(id) != null;
        }

        public bool IsModuleUpToDateComparedToVersion(string id, string comparedVersion)
        {
            if (NOT_INSTALLED.Equals(GetCurrentVersion(id))) return false;
            var result = VersionUtils.CompareVersions(GetCurrentVersion(id), comparedVersion);
            return CompareVersionResult.Equal == result;
        }

        public void DownloadAndInstall(FGModuleInfo module)
        {
            string packageName = module.Id + "-" + module.Version + ".unitypackage";
            string tempPath = Path.GetTempPath() + packageName;
            WebUtils.DownloadFile(module.PackageUrl, tempPath, () => PackageDownloaded(tempPath, module));
        }


        public FGPackage GetPackage(string id)
        {
            if (_packages.ContainsKey(id)) return _packages[id];
            return null;
        }

        public void MapLocalSetup()
        {
            _packages.Clear();
            List<FGPackage> packages = ProjectUtils.GetEnumerableOfType<FGPackage>();
            foreach (FGPackage package in packages)
            {
                _packages.Add(package.ModuleInfo.Id, package);
            }
        }

        private void PackageDownloaded(string directory, FGModuleInfo module)
        {
            _packageImported = (s) => OnImportPackageCompleted(directory, module);
            AssetDatabase.importPackageCompleted += _packageImported;
            AssetDatabase.ImportPackage(directory, true);
        }

        private void OnImportPackageCompleted(string directory, FGModuleInfo module)
        {

            // Delete the temporary file
            File.Delete(directory);
            MapLocalSetup();
            GetPackage(module.Id)?.CreateSettingsAsset();
            AssetDatabase.importPackageCompleted -= _packageImported;
        }

        private void MainDownloaded(UnityWebRequest uwr)
        {
            Debug.Log("File downloaded at: " + RemoteDataFile);
            File.Delete(RemoteDataFile);
            File.WriteAllBytes(RemoteDataFile, uwr.downloadHandler.data);
            LoadData();
        }

        private bool IsCacheUpToDate()
        {
            if (!File.Exists(RemoteDataFile)) return false;
            return Math.Abs(_lastUpdateDate.Subtract(DateTime.Now).TotalMinutes) <= 5;
        }
    }
}