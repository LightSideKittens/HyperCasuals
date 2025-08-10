using Firebase;
using Firebase.Analytics;
using LSCore;

namespace Launcher
{
    public class LauncherWorld : ServiceManager<LauncherWorld>
    {
        protected override void Awake()
        {
            base.Awake();
            InitFirebase();
        }

        private async void InitFirebase()
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status == DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            else
            {
                UnityEngine.Debug.LogError($"[Firebase] Dependencies not available: {status}");
            }
            
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