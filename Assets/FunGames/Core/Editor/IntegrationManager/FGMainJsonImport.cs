using System;
using System.Collections.Generic;
using FunGames.Core.Modules;
using FunGames.Tools.Utils;

namespace FunGames.Editor
{
    public class FGMainJsonImport
    {
        public const string URL = "https://gitlab.com/fungames-sdk/fg_main/-/raw/3.4.0.0/fg_main.json";
        public const string ID_VERSION_SEPARATOR = "-";
        public FGMainJson MainJson => _mainJson;
        public readonly Dictionary<string, List<string>> PackageToAssetPaths = new ();
        private readonly Dictionary<string, List<string>> _idToVersions = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> _parentToChildrenIds = new Dictionary<string, List<string>>();
        readonly Dictionary<string, string> _nameToId = new Dictionary<string, string>();
        private readonly FGMainJson _mainJson;

        public FGMainJsonImport(FGMainJson mainJson)
        {
            _mainJson = mainJson;
            MapIdToVersions(_mainJson.Versions);
            MapParentToChild(_mainJson.Versions);
            MapPackageToPaths(_mainJson.PackagePaths);
        }

        private List<string> GetAllVersions(string id)
        {
            if (_idToVersions.ContainsKey(id))
            {
                return _idToVersions[id];
            }

            return new List<string>();
        }

        public List<string> GetAllValidVersions(string id)
        {
            List<string> validVersions = new List<string>();
            foreach (var version in GetAllVersions(id))
            {
                FGModuleInfo moduleInfo = GetModuleInfo(id, version);
                if (moduleInfo.HasIssue || moduleInfo.IsDeprecated) continue;
                validVersions.Add(version);
            }

            return validVersions;
        }

        public List<string> GetAllIds()
        {
            List<string> ids = new List<string>();
            foreach (var id in _idToVersions.Keys)
            {
                if (!ids.Contains(id)) ids.Add(id);
            }

            return ids;
        }

        public FGModuleInfo GetModuleInfo(string id, string version)
        {
            foreach (var versionDetail in _mainJson.VersionsDetails)
            {
                if (versionDetail.Id != id) continue;
                foreach (var moduleInfo in versionDetail.ModuleInfos)
                {
                    if (moduleInfo.Version == version)
                    {
                        return moduleInfo;
                    }
                }
            }

            return null;
        }

        public List<string> GetAllParents()
        {
            List<string> parents = new List<string>();
            foreach (var key in _parentToChildrenIds.Keys) parents.Add(key);
            return parents;
        }

        public List<string> GetAllChildren()
        {
            List<string> child = new List<string>();
            foreach (var val in _parentToChildrenIds.Values) child.AddRange(val);
            return child;
        }

        public List<string> GetAllChildrenIds(string id)
        {
            if (_parentToChildrenIds.ContainsKey(id))
            {
                return _parentToChildrenIds[id];
            }

            return new List<string>();
        }

        public List<FGModuleInfo> GetAllDependencies(FGModuleInfo moduleInfo)
        {
            List<FGModuleInfo> dependencies = new List<FGModuleInfo>();
            if (moduleInfo == null) return dependencies;
                foreach (var dependency in moduleInfo.Dependencies)
            {
                FGModuleInfo moduleInfoDependency = GetModuleInfo(GetId(dependency), GetVersion(dependency));
                if (moduleInfoDependency != null) dependencies.Add(moduleInfoDependency);
            }

            return dependencies;
        }

        public string GetIdFromName(string name)
        {
            if (_nameToId.ContainsKey(name)) return _nameToId[name];
            return String.Empty;
        }

        public string GetLatestVersion(string id)
        {
            return VersionUtils.GetLatest(GetAllValidVersions(id));
        }

        private void MapIdToVersions(List<FGModuleVersion> moduleVersions)
        {
            foreach (var moduleVersion in moduleVersions)
            {
                string id = String.Empty;
                List<string> versions = new List<string>();
                foreach (var versionRef in moduleVersion.Versions)
                {
                    id = GetId(versionRef);
                    string version = GetVersion(versionRef);
                    versions.Add(version);
                }

                if (!String.IsNullOrEmpty(id) && versions.Count != 0)
                {
                    _idToVersions.Add(id, versions);
                    _nameToId.Add(moduleVersion.Name, id);
                }

                MapIdToVersions(moduleVersion.SubModules);
            }
        }

        private void MapParentToChild(List<FGModuleVersion> moduleVersions)
        {
            foreach (var moduleVersion in moduleVersions)
            {
                if (moduleVersion.Versions.Count != 0 && moduleVersion.SubModules.Count != 0)
                {
                    List<string> subModules = new List<string>();
                    string id = GetId(moduleVersion.Versions[0]);
                    foreach (var versionRef in moduleVersion.SubModules)
                    {
                        subModules.Add(GetId(versionRef.Versions[0]));
                    }


                    _parentToChildrenIds.Add(id, subModules);
                }

                MapParentToChild(moduleVersion.SubModules);
            }
        }

        private string GetId(string versionRef)
        {
            return versionRef.Split(ID_VERSION_SEPARATOR)[0];
        }

        private string GetVersion(string versionRef)
        {
            return versionRef.Split(ID_VERSION_SEPARATOR)[1];
        }

        private void MapPackageToPaths(List<FGPathsInPackage> paths)
        {
            foreach (var item in paths)
            {
                PackageToAssetPaths.TryAdd(item.Key, item.Paths);
            }
        }
    }
}