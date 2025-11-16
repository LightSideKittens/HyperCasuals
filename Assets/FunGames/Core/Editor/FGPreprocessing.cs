#if UNITY_EDITOR
using System.Collections.Generic;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace FunGames.Core.Editor
{
    public class FGPreProcessing : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            SetUpAndroidPermissions(report.summary.platform, report.summary.outputPath);
            UpdateAllSettings();
        }

        private static void SetUpAndroidPermissions(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android)
            {
#if UNITY_ANDROID
            AndroidManifestParser.Instance.AddAllPermissions();
#endif
            }
        }

        private void UpdateAllSettings()
        {
            List<FGPackage> packages = ProjectUtils.GetEnumerableOfType<FGPackage>();
            foreach (FGPackage package in packages)
            {
                package.UpdateSettings();
            }
        }
    }
}
#endif