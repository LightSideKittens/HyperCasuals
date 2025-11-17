using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FunGames.Core.Editor.IntegrationManager;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public class GradleBuildErrorParser : IPreprocessBuildWithReport
    {
        private const string BuildReportDir = "Assets/BuildReports";
        private const string UnityBuildReportPath = "Library/LastBuild.buildreport";

        public int callbackOrder => (int)PreprocessCallbackOrder.Minus100;
        private static readonly List<SuspiciousMessage> LastSuspiciousMessages = new();

        public static bool HasMessages => LastSuspiciousMessages.Count > 0;
        public static bool BuildMade => BuildReportFound && GradleDependencyFinder.GradleBuildFileExists();
        private static bool BuildReportFound => File.Exists(UnityBuildReportPath);
        private static AnalyzerSettings Settings => AnalyzerSettings.GetOrCreateSettings();

        public static Action BuildAnalysisAndMessagesCleared;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            Settings.LastBuildAnalysis = null;
            LastSuspiciousMessages.Clear();
            BuildAnalysisAndMessagesCleared?.Invoke();

            // We have to do it this way because IPostprocessBuildWithReport is not fired if the build fails:
            // see: https://forum.unity.com/threads/ipostprocessbuildwithreport-and-qa-embarrasing-answer-about-a-serious-bug.891055/
            WaitForBuildCompletion(report);
        }

        private async void WaitForBuildCompletion(BuildReport report)
        {
            while (BuildPipeline.isBuildingPlayer || report.summary.result == BuildResult.Unknown)
            {
                await Task.Delay(1000);
            }

            OnPostprocessBuild(report);
        }

        private void OnPostprocessBuild(BuildReport report)
        {
            if (!AnalyzerSettings.GetOrCreateSettings().OfferToAnalyzeAfterAndroidBuild)
                return;
            
            ExtractSuspiciousMessages(report);

            if (LastSuspiciousMessages.Count > 0)
            {
                bool analyze = EditorUtility.DisplayDialog(
                    "FunGames Android Build Error Analyzer",
                    "Some analyzable errors have been found.\nDo you want to analyze them now?",
                    "Yes (analyze now)", "Skip");
                if (analyze)
                {
                    var window = IntegrationManagerWindow.OpenWindow();
                    window.Focus();
                    // window.StartAnalysis();
                }
            }
        }

        public static void AnalyzeLastBuild(LogCallback logCallback)
        {
            // If there are no messages then try to load from the last known log.
            if (!HasMessages)
            {
                (BuildReport report, string assetPath, bool dirExisted) = GetLastBuildReportFromDisk();
                if (report != null)
                {
                    ExtractSuspiciousMessages(report);
                    Settings.LastBuildAnalysis = Analyze(LastSuspiciousMessages, logCallback);
                    CleanUpTmpBuildReportFiles(assetPath, !dirExisted);
                }
                else
                {
                    logCallback?.Invoke(
                        $"No analyzable errors found in the log (no {UnityBuildReportPath} file found). " +
                        $"Please make a build first.",
                        LogLevel.Warning);
                    Settings.LastBuildAnalysis = null;
                }
            }
            else
            {
                Settings.LastBuildAnalysis = Analyze(LastSuspiciousMessages, logCallback);
            }
        }

        private static BuildAnalysis Analyze(List<SuspiciousMessage> messages,
            LogCallback logCallback)
        {
            var duplicateClassErrors = AnalyzeDuplicateClassMessages(messages, logCallback);
            var dexLimitErrorMsg = DexLimitErrorAnalyzer.AnalyzeMessages(messages, logCallback);

            return new BuildAnalysis(
                gradleBuildFileFound: GradleDependencyFinder.GradleBuildFileExists(),
                buildReportFound: BuildReportFound,
                buildFailed: DidLastBuildFail(),
                duplicateClassResults: duplicateClassErrors,
                dexLimitResult: dexLimitErrorMsg
            );
        }

        private static (BuildReport report, string assetPath, bool dirExisted) GetLastBuildReportFromDisk()
        {
            bool dirExisted = true;
            if (!Directory.Exists(BuildReportDir))
            {
                Directory.CreateDirectory(BuildReportDir);
                dirExisted = false;
            }

            if (!File.Exists(UnityBuildReportPath))
            {
                return (null, null, dirExisted);
            }

            var date = File.GetLastWriteTime(UnityBuildReportPath);
            var assetPath = BuildReportDir + "/Build_" + date.ToString("yyyy-dd-MMM-HH-mm-ss") + ".buildreport";
            File.Copy(UnityBuildReportPath, assetPath, true);
            AssetDatabase.ImportAsset(assetPath);
            var buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);

            return (buildReport, assetPath, dirExisted);
        }

        private static void CleanUpTmpBuildReportFiles(string assetPath, bool removeDir)
        {
            AssetDatabase.DeleteAsset(assetPath);
            if (removeDir)
                AssetDatabase.DeleteAsset(BuildReportDir);
        }

        private static void ExtractSuspiciousMessages(BuildReport report)
        {
            LastSuspiciousMessages.Clear();

            if (report.summary.result != BuildResult.Failed)
                return;

            if (report.steps == null || report.steps.Length == 0)
                return;

            // search in messages for clues to why it failed
            for (int i = 0; i < report.steps.Length; i++)
            {
                var step = report.steps[i];

                bool isLast = i == report.steps.Length - 1;
                bool isGradle = step.name.Contains("Gradle");

                if (isLast || isGradle)
                {
                    FindAndAddSuspiciousMessages(report, step, LastSuspiciousMessages);
                }
            }
        }

        private static void FindAndAddSuspiciousMessages(BuildReport report, BuildStep step,
            List<SuspiciousMessage> suspiciousStepMessages)
        {
            foreach (var message in step.messages)
            {
                if (message.type != LogType.Exception && message.type != LogType.Error)
                    continue;

                if (IsSuspicious(message.content) && !ContainsStepMessage(suspiciousStepMessages, message))
                {
                    var newMessage = new SuspiciousMessage(report, step, message);
                    suspiciousStepMessages.Add(newMessage);
                }
            }
        }

        private static bool IsSuspicious(string text)
        {
            // If new error classes should be detected then add them here.
            return DuplicateClassErrorAnalyzer.HasKnownError(text)
                   || DexLimitErrorAnalyzer.HasKnownError(text);
        }

        public static bool ContainsStepMessage(List<SuspiciousMessage> susMsgs, BuildStepMessage newMsg)
        {
            foreach (var existingMsg in susMsgs)
            {
                // existingMsg is contained in newMsg or the other way round
                if (AreContainedInEachOther(existingMsg.StepMessage.content, newMsg.content, 512))
                    return true;
            }

            return false;
        }

        private static bool AreContainedInEachOther(string a, string b, int maxCharsToCompare = int.MaxValue)
        {
            string aShort = a.Substring(0, Mathf.Min(a.Length, maxCharsToCompare));
            if (b.Contains(aShort))
                return true;

            string bShort = b.Substring(0, Mathf.Min(b.Length, maxCharsToCompare));
            if (a.Contains(bShort))
                return true;

            return false;
        }

        private static List<ErrorGroup> AnalyzeDuplicateClassMessages(
            List<SuspiciousMessage> messages, LogCallback logCallback)
        {
            var gradleDependencies = GatherGradleDependencies(logCallback);
            var libFiles = GatherLibFiles();
            var gradleFiles = GradleDependencyFinder.FindGradleFiles();

            var result = DuplicateClassErrorAnalyzer.AnalyzeMessages(messages, libFiles, gradleDependencies,
                gradleFiles, logCallback);
            return result;
        }

        private static List<Dependency> GatherGradleDependencies(LogCallback logCallback)
        {
            return GradleDependencyFinder.GetDependencies(logCallback);
        }

        private static List<AndroidLibrary> GatherLibFiles()
        {
            return AndroidLibraryFinder.FindLibraries();
        }

        private static bool DidLastBuildFail()
        {
            // If there are no messages then try to load from the last know log.
            if (!HasMessages)
            {
                (BuildReport report, string assetPath, bool dirExisted) = GetLastBuildReportFromDisk();
                if (report != null)
                {
                    ExtractSuspiciousMessages(report);
                    CleanUpTmpBuildReportFiles(assetPath, !dirExisted);
                }
            }

            if (HasMessages)
            {
                foreach (var msg in LastSuspiciousMessages)
                {
                    if (msg.Report != null && msg.Report.summary.result == BuildResult.Failed)
                        return true;
                    else if (msg.StepMessage.content.Contains("uild failed"))
                        return true;
                }
            }

            return false;
        }
    }
}
