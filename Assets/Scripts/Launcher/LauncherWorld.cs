using LSCore;

namespace Launcher
{
    public class LauncherWorld : ServiceManager<LauncherWorld>
    {
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            MainWindow.AsHome();
            MainWindow.Show();
        }
    } 
}