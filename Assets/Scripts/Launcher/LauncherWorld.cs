using LSCore;
using LSCore.Extensions;
using UnityEngine;

namespace Launcher
{
    public class LauncherWorld : ServiceManager<LauncherWorld>
    {
        [SerializeReference] public DoIt[] onInit;
        
        protected override void Awake()
        {
            base.Awake();
            BaseInitializer.Initialize(Init);
            if (!GameSave.currentLevel.IsNullOrEmpty())
            {
                Analytic.LogEvent("level_quit", GameSave.CurrentLevelParam);
            }
        }

        private void Init()
        {
            MainWindow.AsHome();
            MainWindow.Show();
            
            onInit.Do();
        }
    } 
}