using LSCore;
using LSCore.Extensions;
using SourceGenerators;
using UnityEngine;

namespace Launcher
{
    [InstanceProxy]
    public partial class LauncherWorld : ServiceManager<LauncherWorld>
    {
        [SerializeReference] public DoIt[] onInit;
        
        protected override void Awake()
        {
            base.Awake();
            BaseInitializer.Initialize(Init);
            if (!GameSave.currentLevel.IsNullOrEmpty())
            {
                Analytic.LogEvent("level_quit", GameSave.CurrentLevelParam);
                GameSave.currentLevel = null;
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