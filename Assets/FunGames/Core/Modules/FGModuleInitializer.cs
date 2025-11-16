using System.Collections.Generic;

namespace FunGames.Core.Modules
{
    public class FGModuleInitializer
    {
        private List<FGModule> _fgModules = new List<FGModule>();

        public void AddModule(FGModule fgModule)
        {
            _fgModules.Add(fgModule); 
            fgModule.SetManualInit();
        }

        public void ForceInit()
        {
            foreach (var fgModule in _fgModules)
            {
                fgModule.UnlockManualInit();
                fgModule.Awake();
            }

            foreach (var fgModule in _fgModules) fgModule.Start();
            foreach (var fgModule in _fgModules) fgModule.Initialize();
        }
        
        public void Clear()
        {
            _fgModules.Clear();
        }
    }
}