#if UNITY_EDITOR
using FunGames.Core.Editor.Config;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace FunGames.Core.Editor
{
    public class FGPostProcessing: IPostprocessBuildWithReport
    {
        public int callbackOrder { get; }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            ExportSdkConfig(report);
        }
        
        private void ExportSdkConfig(BuildReport report)
        {
            FGConfigFile.ExportToRoot();
        }
    }
}
#endif