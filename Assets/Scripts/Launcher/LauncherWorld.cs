using LSCore;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Launcher
{
    public class LauncherWorld : ServiceManager<LauncherWorld>
    {
        [SerializeReference] public DoIt[] onInit;
        public Image background;
        
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
            GameSave.Config["theme"].ListenAndCall(UpdateBackground);
            MainWindow.AsHome();
            MainWindow.Show();
            
            onInit.Do();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameSave.Config["theme"].UnListen(UpdateBackground);
        }

        private void UpdateBackground()
        {
            background.sprite = Themes.CurrentBackground;
        }
    } 
}