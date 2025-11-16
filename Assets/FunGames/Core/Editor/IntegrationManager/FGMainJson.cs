using System;
using System.Collections.Generic;
using FunGames.Core.Modules;

namespace FunGames.Editor
{
    [Serializable]
    public class FGMainJson
    {
        public List<FGModuleVersion> Versions = new List<FGModuleVersion>();
        public List<FGModuleVersionDetail> VersionsDetails = new List<FGModuleVersionDetail>();
        public List<FGPathsInPackage> PackagePaths = new List<FGPathsInPackage>();
    }
    
    
    [Serializable]
    public class FGModuleVersion
    {
        public string Name;
        public List<string> Versions = new List<string>();
        public List<FGModuleVersion> SubModules = new List<FGModuleVersion>();

        public FGModuleVersion(string name)
        {
            Name = name;
        }
    }
    
    [Serializable]
    public class FGModuleVersionDetail
    {
        public string Id;
        public List<FGModuleInfo> ModuleInfos = new List<FGModuleInfo>();
        public FGModuleVersionDetail(string id)
        {
            Id = id;
        }
    }
    
    
    [Serializable]
    public class FGPathsInPackage
    {
        public string Key;
        public List<string> Paths = new List<string>();
    }
}