using System.Collections.Generic;

namespace FunGames.Core.DatabaseModels
{
    public class FGDBGameConfig
    {
        public static string ModulesKey = nameof(modules);
        
        public string id = "";
        public string name = "";
        public List<FGDBModuleSettings> modules = new();

        private const string LastModuleId = "fg_core";

        public void SortModules()
        {
            foreach (FGDBModuleSettings moduleSettings in modules)
            {
                moduleSettings.submodules.Sort(CompareModules);
            }
            
            modules.Sort(CompareModules);
        }

        private int CompareModules(FGDBModuleSettings a, FGDBModuleSettings b)
        {
            if (a.id == LastModuleId) return 1;
            if (b.id == LastModuleId) return -1;
            return b.is_mandatory.CompareTo(a.is_mandatory);
            
        }
    }
}