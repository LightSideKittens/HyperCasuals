#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using FunGames.Core.Modules;
using FunGames.Core.Utils;
using FunGames.Core.Settings;

namespace FunGames.Core.Editor
{
    public abstract class FGPackageAbstract<T> : FGPackage where T : FGPackageAbstract<T>, new()
    {
        public override FGModuleInfo ModuleInfo => _moduleInfo;
        
        public override string[] AssetFolders => _assetFolders;
        public override string SettingsAsset => _settingsAsset;

        public override string[] ExternalAssets => _externalAssets;
        public override string DestinationPath => _destinationPath;
        public override List<GameObject> Prefabs => _prefabs;
        public override string JsonFile => _jsonFile;

        protected string[] FUNGAMES_EXTERNALS_PATH = { "Assets/FunGames_Externals" };

        protected FGModuleInfo _moduleInfo;
        protected string[] _assetFolders;
        protected string _settingsAsset;
        protected string[] _externalAssets;
        protected string _destinationPath;
        protected List<GameObject> _prefabs;
        protected string _jsonFile;

        private static T instance = null;

        public static T Package
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }

                return instance;
            }
        }

        protected FGPackageAbstract()
        {
            _jsonFile = Application.dataPath + "/FunGames/" + ModuleFolder + "/" + JsonName;
            _moduleInfo = parseModuleInfo();
            _assetFolders = assetFolders();
            _settingsAsset = settingsAsset();
            _externalAssets = externalAssets();
            _destinationPath = destinationFolder();
            _prefabs = prefabAssets();

        }

        protected virtual string[] externalAssets()
        {
            return new string[] { };
        }

        protected FGModuleInfo parseModuleInfo()
        {
            if (String.IsNullOrEmpty(JsonName)) return new FGModuleInfo();

            if (!File.Exists(_jsonFile))
            {
                Debug.LogWarning("Module info JSON file not found for this module.");
                return new FGModuleInfo();
            }

            return JsonUtility.FromJson<FGModuleInfo>(File.ReadAllText(_jsonFile));
        }

        protected virtual string[] assetFolders()
        {
            string[] folders =
            {
                "Assets/FunGames/" + ModuleFolder,
                "Assets/FunGames/" + ModuleFolder + "/Editor",
                "Assets/FunGames/" + ModuleFolder + "/_Debug",
                "Assets/FunGames/" + ModuleFolder + "/_Package"
            };
            return folders;
        }

        protected virtual string settingsAsset()
        {
            return FGPath.ASSETS_RESOURCES_FUNGAMES + SettingsAssetName + ".asset";
        }


        protected virtual List<GameObject> prefabAssets()
        {
            List<GameObject> prefabs = new List<GameObject>();
#if UNITY_EDITOR
            string prefabFile = "Assets/FunGames/" + ModuleFolder + "/" + PackageName +
                                ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);
            prefabs.Add(prefab);
#endif
            return prefabs;
        }

        protected string destinationFolder()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("/");
            stringBuilder.Append(ModuleFolder);
            return stringBuilder.ToString();
        }

        public override void AddPrefabs()
        {
            _prefabs = prefabAssets();
            if (_prefabs.Count == 0)
            {
                Debug.LogWarning("No prefabs found for " + PackageName);
                return;
            }

            foreach (var prefab in _prefabs)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("No prefabs found for " + PackageName);
                    return;
                }
            }

            if (!PackageName.Equals(FGCorePackage.Package.PackageName)) FGCorePackage.Package.AddPrefabs();
            GameObject fgObject = GameObject.Find("FunGames");
            if (fgObject == null)
            {
                fgObject = new GameObject("FunGames");
                fgObject.AddComponent<DontDestroyOnLoad>();
            }

            foreach (GameObject prefab in _prefabs)
            {
                if (GameObject.Find(prefab.name) != null)
                {
                    Debug.Log(prefab.name + " already exists in scene.");
                    continue;
                }

                GameObject instantiatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instantiatedPrefab == null) continue;
                if (instantiatedPrefab.name.Contains("FG"))
                    instantiatedPrefab.transform.SetParent(fgObject.transform, false);
                instantiatedPrefab.transform.localPosition = Vector3.zero;
                instantiatedPrefab.transform.localRotation = Quaternion.identity;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public override void UpdateSettings()
        {
            // string settingsFileName = Path.GetFileNameWithoutExtension(_settingsAsset);
            if (File.Exists(_settingsAsset))
            {
                var settingAsset = AssetDatabase.LoadAssetAtPath<FGModuleSettings>(_settingsAsset);
                FGModuleInfo moduleInfo = parseModuleInfo();
                settingAsset.ModuleInfo = moduleInfo;
                settingAsset.LogColor = ColorUtils.ToColor(moduleInfo.LogColor);
            }
        }
        
        public override void CreateSettingsAsset()
        {
            string settingsFileName = Path.GetFileNameWithoutExtension(_settingsAsset);
            if (File.Exists(_settingsAsset))
            {
                Debug.Log(settingsFileName + " already exists.");
                UpdateSettings();
                return;
            }

            try
            {
                FGModuleSettings settings = (FGModuleSettings)ScriptableObject.CreateInstance(settingsFileName);
                settings.ModuleInfo = parseModuleInfo();
                AssetDatabase.CreateAsset(settings, _settingsAsset);
                UpdateSettings();
                settings.OnAssetCreated();
                Debug.Log(settingsFileName + " created.");
            }
            catch (Exception e)
            {
                Debug.LogError("An exception occured while attempting to create " + settingsFileName + " : \n" + e);
            }
        }

        public sealed override S GetModuleSettings<S>()
        {
            if (!File.Exists(_settingsAsset))
            {
                CreateSettingsAsset();
            }

            return AssetDatabase.LoadAssetAtPath<S>(_settingsAsset);
        }
    }
}

#endif