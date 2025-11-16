using System.Collections.Generic;
using System.IO;
using FunGames.Core.Editor.IntegrationManager;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public delegate void LogCallback(string message, LogLevel logLevel = LogLevel.Log);

    public class FGGradleAnalyzer : FGAnalyzer
    {
        private static AnalyzerSettings Settings => AnalyzerSettings.GetOrCreateSettings();
        private readonly DuplicateClassErrorsDrawer _duplicateClassErrorsDrawer = new();

        protected override List<FGIssue> OwnIssues()
        {
            return new List<FGIssue>
            {
                CheckGradleVersion(),
                CheckCustomGradleTemplates(),
                CheckBuildIssues()
            }.RemovingNull();
        }

        private FGIssue CheckGradleVersion()
        {
            return new FGIssue
            {
                title = "Gradle info",
                issueDescription =
                    $"Using Gradle Installed with Unity: {(IsGradleEmbeddedWithUnity() ? "Yes" : "No")}" +
                    $"\nGradle version: {GetGradleVersion()}",
                severity = FGSDKIssueSeverity.Info,
                platform = FGSDKIssuePlatform.Android
            };
        }

        private FGIssue CheckCustomGradleTemplates()
        {
            bool mainTemplateExists = File.Exists(
                Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle"));
            bool gradlePropertiesTemplateExists = File.Exists(
                Path.Combine(Application.dataPath, "Plugins/Android/gradleTemplate.properties"));
            bool settingsTemplateExists = File.Exists(
                Path.Combine(Application.dataPath, "Plugins/Android/settingsTemplate.gradle"));

            if (mainTemplateExists && gradlePropertiesTemplateExists && settingsTemplateExists) return null;

            return new FGIssue
            {
                title = "Recommended custom Gradle templates",
                issueDescription = "The following custom Gradle templates are not enabled:\n" +
                                   $"{(mainTemplateExists ? "" : "\n- Custom Main Gradle Template")}" +
                                   $"{(gradlePropertiesTemplateExists ? "" : "\n- Custom Gradle Properties Template")}" +
                                   $"{(settingsTemplateExists ? "" : "\n- Custom Gradle Settings Template")}",
                howToFix = "1. Go to Player Settings for Android. Scroll to Publishing Settings section." +
                           "\n2. Enable <color=cyan>Custom Main Gradle Template</color>, " +
                           "<color=cyan>Custom Gradle Properties Template</color> " +
                           "and <color=cyan>Custom Gradle Settings Template</color> if not enabled.",
                severity = FGSDKIssueSeverity.Warning,
                platform = FGSDKIssuePlatform.Android
            };
        }

        private FGIssue CheckBuildIssues()
        {
            if (Settings.LastBuildAnalysis is {IsValid: true})
                return ReportLastBuildAnalysis();
            if (GradleBuildErrorParser.HasMessages)
                return ReportSuspiciousMessages();
            return GradleBuildErrorParser.BuildMade ? ReportLastBuildReport() : ReportNoBuildInfo();
        }

        private FGIssue ReportLastBuildAnalysis()
        {
            if (!Settings.LastBuildAnalysis.HasResults) return null;

            return new FGIssue
            {
                title = "Duplicate classes",
                severity = FGSDKIssueSeverity.Error,
                platform = FGSDKIssuePlatform.Android,
                customDescriptionDrawer = DrawDuplicateClassErrorResults
            };
        }

        private FGIssue ReportSuspiciousMessages()
        {
            return new FGIssue
            {
                title = "Gradle errors",
                issueDescription = "There were Gradle errors found in your last build.",
                howToFix = "Click the Analyze button to get more information on how to fix the errors.",
                severity = FGSDKIssueSeverity.Warning,
                platform = FGSDKIssuePlatform.Android,
                customAction = () => GradleBuildErrorParser.AnalyzeLastBuild(LogMessage),
                customActionText = "Analyze"
            };
        }

        /// <summary>
        /// Analyzing the last build report is an expensive operation. Hence we start it only by user action.
        /// This returns an FGIssue that displays a custom action button to start the analysis.
        /// </summary>
        /// <returns>FGIssue with a custom action</returns>
        private FGIssue ReportLastBuildReport()
        {
            return new FGIssue
            {
                title = "Analyze last build report",
                issueDescription = "Click the Analyze button to analyze the last build report for errors",
                severity = FGSDKIssueSeverity.Info,
                platform = FGSDKIssuePlatform.Android,
                customAction = () => GradleBuildErrorParser.AnalyzeLastBuild(LogMessage),
                customActionText = "Analyze"
            };
        }

        private FGIssue ReportNoBuildInfo()
        {
            return new FGIssue
            {
                title = "No build report found",
                issueDescription = "GradleAnalyzer could not determine if there are errors. " +
                                   "Please make an Android build first to get more information.",
                severity = FGSDKIssueSeverity.Info,
                platform = FGSDKIssuePlatform.Android
            };
        }

        private void DrawDuplicateClassErrorResults()
        {
            if (Settings == null || Settings.LastBuildAnalysis == null) return;
                    
            _duplicateClassErrorsDrawer.DrawDuplicateClassErrorResults(
                Settings.LastBuildAnalysis.DuplicateClassResults,
                Settings.LastBuildAnalysis.GradleBuildFileFound);
        }

        private bool IsGradleEmbeddedWithUnity()
        {
            return EditorPrefs.GetBool("GradleUseEmbedded", true);
        }

        private string GetGradleVersion()
        {
            string gradleCoreFile = GetGradleCoreJarFile();
            gradleCoreFile = gradleCoreFile.Replace(".jar", "");
            string[] parts = gradleCoreFile.Split("-");

            if (parts.Length == 0) return "";

            return parts[^1];
        }

        private string GetGradleCoreJarFile()
        {
            string installPath = GradleDependencyFinder.GetGradleInstallPath();
            string libPath = Path.Combine(installPath, "lib");
            if (!Directory.Exists(libPath)) return "";

            var files = Directory.GetFiles(libPath, "gradle-core*", SearchOption.TopDirectoryOnly);
            return files.Length > 0 ? files[0] : "";
        }

        private void LogMessage(string message, LogLevel logLevel = LogLevel.Log)
        {
            if (logLevel < AnalyzerSettings.GetOrCreateSettings().LogLevel)
                return;

            switch (logLevel)
            {
                case LogLevel.Log:
                    Debug.Log("FGGradleAnalyzer: " + message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning("FGGradleAnalyzer: " + message);
                    break;
                case LogLevel.Error:
                    Debug.LogError("FGGradleAnalyzer: " + message);
                    break;
            }
        }

    }
}