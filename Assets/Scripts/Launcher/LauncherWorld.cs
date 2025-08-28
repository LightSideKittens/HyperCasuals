using LSCore;
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
        }

        private void Init()
        {
            MainWindow.AsHome();
            MainWindow.Show();
            
            onInit.Do();
        }
    } 
}