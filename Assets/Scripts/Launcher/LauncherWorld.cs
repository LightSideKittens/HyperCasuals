using LSCore;

namespace Launcher
{
    public class LauncherWorld : ServiceManager<LauncherWorld>
    {
        protected override void Awake()
        {
            base.Awake();
            BaseInitializer.Initialize();
            if (!Levels.IsTutorialCompleted.Is)
            {
                new Levels.LoadCurrentTutorial().Do();
                return;
            }
            Init();
        }

        private void Init()
        {
            MainWindow.AsHome();
            MainWindow.Show();
        }
    } 
}