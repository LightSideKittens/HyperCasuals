#if UNITY_ANDROID

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FunGames.Core.Editor.IntegrationManager;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public class FGAndroidAnalyzer : FGAnalyzer
    {
        private const AndroidSdkVersions AllowedMinSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        private const AndroidSdkVersions MinTargetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;

        private bool CustomManifestExists => File.Exists(
            Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml"));

        protected override List<FGIssue> OwnIssues()
        {
            return new List<FGIssue>
            {
                CheckMinApiLevel(),
                CheckTargetApiLevel(),
                CheckCustomManifestExists(),
                CheckManifestPermissions(),
                CheckDuplicateLibraries()
            }.RemovingNull();
        }

        private FGIssue CheckMinApiLevel()
        {
            AndroidSdkVersions projectMinSdkVersion = PlayerSettings.Android.minSdkVersion;

            if (projectMinSdkVersion >= AllowedMinSdkVersion) return null;

            return new FGIssue
            {
                title = "Android Minimum API Level",
                issueDescription = $"The Minimum API Level {projectMinSdkVersion} set in Project Settings " +
                                   $"is lower than the minimum supported: {AllowedMinSdkVersion}",
                howToFix = $"Set Android Minimum API Level to {AllowedMinSdkVersion}",
                severity = FGSDKIssueSeverity.Warning,
                fix = FixMinimumApiLevel,
                platform = FGSDKIssuePlatform.Android
            };
        }
        
        private FGIssue CheckTargetApiLevel()
        {
            AndroidSdkVersions projectTargetSdkVersion = PlayerSettings.Android.targetSdkVersion;

            if (projectTargetSdkVersion >= MinTargetSdkVersion) return null;

            return new FGIssue
            {
                title = "Android Target API Level",
                issueDescription = $"The Target API Level {projectTargetSdkVersion} set in Project Settings " +
                                   $"is lower than the minimum supported: {MinTargetSdkVersion}",
                howToFix = $"Set Android Target API Level to {MinTargetSdkVersion}",
                severity = FGSDKIssueSeverity.Error,
                platform = FGSDKIssuePlatform.Android,
                fix = FixTargetApiLevel
            };
        }

        private FGIssue CheckCustomManifestExists()
        {
            if (CustomManifestExists) return null;

            return new FGIssue
            {
                title = "Custom Main Manifest for Android",
                issueDescription = "We recommend using a custom main Android manifest.",
                howToFix = "1. Go to Player Settings for Android. Scroll to Publishing Settings section." +
                           "\n2. Enable <color=cyan>Custom Main Main Manifest</color>.",
                severity = FGSDKIssueSeverity.Warning,
                platform = FGSDKIssuePlatform.Android
            };
        }

        private FGIssue CheckManifestPermissions()
        {
            if (!CustomManifestExists) return null;

            AndroidManifestParser manifestParser = AndroidManifestParser.Instance;
            List<string> missingPermissions =
                AndroidManifestParser.RequiredPermissions.Where(
                    permission => !manifestParser.DoesPermissionExist(permission)).ToList();
            string missingPermissionsText = string.Join(",\n", missingPermissions);

            if (missingPermissions.Count == 0) return null;

            return new FGIssue
            {
                title = "Required Permissions missing from Custom Main Manifest",
                issueDescription = "The following required permissions are missing in " +
                                   "Assets/Plugins/Android/AndroidManifest.xml:\n\n<color=cyan>" +
                                   missingPermissionsText + "</color>",
                howToFix = "Click the Fix button to add these permissions. However, they are automatically added " +
                           "by FunGames SDK when you make an Android build.",
                fix = () => { FixMissingPermissions(missingPermissions); },
                severity = FGSDKIssueSeverity.Warning,
                platform = FGSDKIssuePlatform.Android
            };
        }

        private FGIssue CheckDuplicateLibraries()
        {
            var duplicateLibs = DuplicateAndroidLibraryFinder.FindSimilar();

            if (duplicateLibs.Count == 0) return null;

            return new FGIssue
            {
                title = "Duplicate libraries",
                issueDescription = $"The following duplicate libraries were found:\n\n" +
                                   $"{DuplicateLibrariesToString(duplicateLibs)}",
                severity = FGSDKIssueSeverity.Error,
                platform = FGSDKIssuePlatform.Android
            };
        }

        private string DuplicateLibrariesToString(List<List<AndroidLibrary>> duplicates)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < duplicates.Count; i++)
            {
                stringBuilder.Append($"{i + 1}:\n-");
                IEnumerable<string> libPaths = duplicates[i].Select(
                    a => IntegrationUtils.WrapInRichTextColor(a.Path, IntegrationUtils.AmberErrorColor));
                stringBuilder.Append(string.Join("\n\n-", libPaths));
                stringBuilder.Append("\n\n");
            }

            return stringBuilder.ToString();
        }

        private void FixMinimumApiLevel()
        {
            PlayerSettings.Android.minSdkVersion = AllowedMinSdkVersion;
        }
        private void FixTargetApiLevel()
        {
            PlayerSettings.Android.targetSdkVersion = MinTargetSdkVersion;
        }

        private void FixMissingPermissions(List<string> permissions)
        {
            foreach (var permission in permissions)
            {
                AndroidManifestParser.Instance.AddPermission(permission);
            }
            
            AndroidManifestParser.Instance.Save();
        }
    }
}
#endif
