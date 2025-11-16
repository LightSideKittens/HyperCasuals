using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public static class GradleDependencyFinder
    {
        // We are parsing the output of "gradle dependencies" directly. It might be more desirable (and stable)
        // to add a dedicated task to the gradle file and use the output of that (nice2have, for now this will do).
        private const string TreeNewEntry0 = "+--- ";
        private const string TreeNewEntry1 = "\\--- ";
        private const string TreeSpace     = "|    ";
        private const string EmbeddedGradlePathComponent = "PlaybackEngines/AndroidPlayer/Tools/gradle";
        private const string EmbeddedJDKPathComponent = "PlaybackEngines/AndroidPlayer/OpenJDK";

        /// <summary>
        /// Gets the path to the gradle install dir. To get to the binaries append "/bin" to it.
        /// <br />Notice: In Unity 2020.+ the bin dir does no longer exist!
        /// </summary>
        /// <returns></returns>
        public static string GetGradleInstallPath()
        {
            string gradlePath = EditorPrefs.GetString("GradlePath");

            // gradlePath will be empty if GradleUseEmbedded is checked, thus we need to use the default path.
            bool gradleUseEmbedded = EditorPrefs.GetBool("GradleUseEmbedded", true);
            if (gradleUseEmbedded)
            {
                int index = System.Math.Max(EditorApplication.applicationPath.LastIndexOf('/'), EditorApplication.applicationPath.LastIndexOf('\\'));
                string appPath = EditorApplication.applicationPath.Substring(0, index);
#if UNITY_EDITOR_WIN
                gradlePath = appPath + "/Data/" + EmbeddedGradlePathComponent;
#else
                gradlePath = appPath + "/" + EmbeddedGradlePathComponent;
#endif
            }

            return gradlePath;
        }

        /// <summary>
        /// Gets the path to the JDK install dir. To get to the binaries append "/bin" to it.
        /// </summary>
        /// <returns></returns>
        private static string GetJdkInstallPath()
        {
            string jdkPath = EditorPrefs.GetString("JdkPath");

            // gradlePath will be empty if GradleUseEmbedded is checked, thus we need to use the default path.
            bool gradleUseEmbedded = EditorPrefs.GetBool("JdkUseEmbedded", true);
            if (gradleUseEmbedded)
            {
                int index = System.Math.Max(EditorApplication.applicationPath.LastIndexOf('/'), EditorApplication.applicationPath.LastIndexOf('\\'));
                string appPath = EditorApplication.applicationPath.Substring(0, index);
#if UNITY_EDITOR_WIN
                jdkPath = appPath + "/Data/" + EmbeddedJDKPathComponent;
#else
                jdkPath = appPath + "/" + EmbeddedJDKPathComponent;
#endif
            }

            return jdkPath;
        }

        private static string GetGradleBuildFilePath()
        {
            string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");
            string gradleFile = projectDir + "Temp/gradleOut/unityLibrary/build.gradle";

            // try alternative (unity 2021+)
            if (!File.Exists(gradleFile))
            {
                gradleFile = projectDir + "/Library/Bee/Android/Prj/Mono2x/Gradle/unityLibrary/build.gradle";
            }

            // okay, we gotta search for it
            if (!File.Exists(gradleFile))
            {
                RecurseIntoDirAndFindBuildGradle(projectDir + "/Library", ref gradleFile);
            }

            return gradleFile.Replace("\\", "/");
        }

        private static void RecurseIntoDirAndFindBuildGradle(string dir, ref string buildGradlePath)
        {
            string[] directories = Directory.GetDirectories(dir);
            foreach (string subDir in directories)
            {
                if (subDir.EndsWith("unityLibrary"))
                {
                    var files = Directory.GetFiles(subDir);
                    foreach (var file in files)
                    {
                        if (file.EndsWith("build.gradle"))
                        {
                            buildGradlePath = file;
                            return;
                        }
                    }
                }
                RecurseIntoDirAndFindBuildGradle(subDir, ref buildGradlePath);
            }
        }

        public static bool GradleBuildFileExists()
        {
            return File.Exists(GetGradleBuildFilePath());
        }

        /// <summary>
        /// Uses the 'gradle dependecies' command to fetch the resolved dependency tree. Then parses the tree into a List structure and returns it.
        /// </summary>
        /// <param name="logCallback"></param>
        /// <returns></returns>
        public static List<Dependency> GetDependencies(LogCallback logCallback)
        {
            List<Dependency> dependencies = new List<Dependency>();

            string rawTree = GetRawDependencies(logCallback);

            if (rawTree == null || rawTree.IndexOf('+') < 0)
            {
                logCallback?.Invoke("GradleDependencyAnalyzer: No dependencies found in 'build.gradle' file. Skipping gradle dependency check.", LogLevel.Warning);
                return dependencies;
            }

            // parse tree
            StringReader strReader = new StringReader(rawTree);
            string line;
            Stack<Dependency> current = new Stack<Dependency>();
            while (true)
            {
                line = strReader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    if (line == null)
                        break;
                    else
                        continue;

                if (   !CompareAtDepth(0, line, TreeNewEntry0)
                    && !CompareAtDepth(0, line, TreeNewEntry1)
                    && !CompareAtDepth(0, line, TreeSpace)
                    )
                    continue;

                // Ignore lines ening with "(c)" constraint.
                if (line.EndsWith("(c)"))
                    continue;

                int depth = GetLineDepth(line);

                // parse recursive via stack('current' is the current leaf we are at).
                while (current.Count > depth)
                {
                    current.Pop();
                }
                Dependency parent = null;
                if (depth > 0)
                {
                    if (current.Count > 0)
                    {
                        parent = current.Peek();
                        if (depth == current.Count)
                            parent = parent.Parent;
                    }
                    else if (dependencies.Count > 0)
                    {
                        parent = dependencies[dependencies.Count - 1];
                    }
                }

                // Extract package info (this is where it will most likely break in the future if the format changes).
                // Examples:
                // +--- androidx.lifecycle:lifecycle-runtime:2.0.0 -> 2.1.0 => androidx.lifecycle:lifecycle-runtime:2.0.0 and 2.1.0
                // +--- com.ironsource.sdk:mediationsdk: 7.2.1.1
                // +--- :vungle
                // +--- androidx.core:core:{strictly 1.3.0} -> 1.3.0 (c) => androidx.core:core 1.3.0 and 1.3.0
                // +--- project :unityLibrary:GoogleMobileAdsPlugin.androidlib

                line = line.Replace(TreeNewEntry0, "");
                line = line.Replace(TreeNewEntry1, "");
                line = line.Replace(TreeSpace, "");

                string name = null;
                Dependency.GradleType type = Dependency.GradleType.Package;
                if (line.StartsWith("project "))
                {
                    type = Dependency.GradleType.Project;
                    line = line.Substring(8);
                }

                // Things starting with ':' are either most likely .aar files.
                // Examples:
                //   :vungle
                //   androidx.lifecycle:lifecycle-runtime:2.0.0
                string packagePattern;
                if (line[0] == ':')
                {
                    packagePattern = @"(:[^ ]+)";
                    type = Dependency.GradleType.LibFile;
                }
                else
                {
                    packagePattern = @"([^ ]+:)";
                    type = Dependency.GradleType.Package;
                }

                // Extract package name
                Regex r = new Regex(packagePattern, RegexOptions.IgnoreCase);
                Match m = r.Match(line);
                if (m.Success && m.Groups.Count >= 2)
                {
                    string tmp = m.Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(tmp))
                    {
                        logCallback?.Invoke("Package could not be extracted from gradle dependency tree at line '" + line + "'.", LogLevel.Warning);
                        continue;
                    }
                    name = m.Groups[1].Value.Trim(':');
                }

                // extract version info
                // +--- androidx.lifecycle:lifecycle-runtime:2.0.0 -> 2.1.0 => androidx.lifecycle:lifecycle-runtime:2.0.0 and 2.1.0
                string originalVersion = null;
                string version = null;
                string versionPattern = @"([0-9.]{2,})";
                Regex vr = new Regex(versionPattern, RegexOptions.IgnoreCase);
                Match vm = vr.Match(line);
                if (vm.Success && vm.Groups.Count >= 2)
                {
                    version = vm.Groups[1].Value;

                    vm = vm.NextMatch();
                    if (vm.Groups.Count >= 2)
                        originalVersion = vm.Groups[1].Value;
                }

                // create new dependency
                var dependency = new Dependency(name, type, originalVersion, version, parent);

                // more recursion
                if (parent != null)
                    parent.Children.Add(dependency);

                if (depth > current.Count)
                    current.Push(dependency);
                else if (depth == current.Count)
                {
                    if (current.Count > 0)
                    {
                        current.Pop();
                        current.Push(dependency);
                    }
                }

                if (depth == 0)
                    dependencies.Add(dependency);
            }

            /* For debugging purposes
            foreach (var dep in dependencies)
            {
                Debug.Log(dep.Name);
            }
            //*/

            return dependencies;
        }

        /// <summary>
        /// Uses the 'gradle dependecies' command to fetch the resolved dependency tree. Then parses the tree into a List structure and returns it.
        /// </summary>
        /// <param name="logCallback"></param>
        /// <returns></returns>
        private static string GetRawDependencies(LogCallback logCallback)
        {
            List<Dependency> dependencies = new List<Dependency>();

            // is there a final gradle file to analyze?
            string gradleFile = GetGradleBuildFilePath();
            if (!File.Exists(gradleFile))
            {
                logCallback?.Invoke("GradleDependencyAnalyzer: No temp 'build.gradle' file found! Skipping gradle dependency check. Searched under '" + gradleFile + "'.", LogLevel.Warning);
                return "";
            }

            // jdk path
            string jdkPath = GetJdkInstallPath();
            string gradlePath = GetGradleInstallPath();

            // Resolve dependency tree
            // Choose debugCompileClasspath if export settings is debug
            string configuration = Debug.isDebugBuild ? "debugCompileClasspath" : "releaseCompileClasspath";
            string gradleParams = "dependencies --build-file \"" + gradleFile + "\" --configuration " + configuration;

#if UNITY_2020_1_OR_NEWER
#if UNITY_EDITOR_WIN
            string jdkBin = "java.exe ";
#else
            string jdkBin = "./java ";
#endif
            // find gradle-launcher-x.y.z.jar (Unity 2020+ does not longer ship with the bin/gradle files.
            string launcherJar = null;
            if (Directory.Exists(gradlePath + "/lib"))
                launcherJar = Directory.GetFiles(gradlePath + "/lib").FirstOrDefault(f => (f.Contains("/gradle-launcher") || f.Contains("\\gradle-launcher")) && f.EndsWith(".jar"));
            if (string.IsNullOrEmpty(launcherJar))
            {
                logCallback?.Invoke("Error: Could not find gradle-launcher jar in libs. Searched under '" + gradlePath + "/lib'.");
                return "";
            }

            string command = jdkBin + "-classpath \"" + launcherJar + "\" org.gradle.launcher.GradleMain " + gradleParams;

            string workingDir = jdkPath + "/bin";
#else
#if UNITY_EDITOR_WIN
            string gradleBin = "gradle.bat ";
#else
            string gradleBin = "./gradle ";
#endif
            string command = gradleBin + gradleParams;
            string workingDir = gradlePath + "/bin";
#endif

            string rawTree = Exec(
                command: command,
                workingDir: workingDir,
                timeoutInSec: 20,
                logCallback: logCallback
                );

            if (rawTree == null || rawTree.IndexOf('+') < 0)
            {
                logCallback?.Invoke("GradleDependencyAnalyzer: No dependencies found in 'build.gradle' file. Skipping gradle dependency check. File path '" + gradleFile + "'.", LogLevel.Warning);
                return "";
            }

            return rawTree;
        }

        private static int GetLineDepth(string line)
        {
            int depth = 0;
            while (CompareAtDepth(depth, line, TreeSpace))
            {
                depth++;
            }
            return depth;
        }

        private static bool CompareAtDepth(int depth, string hackstack, string needle)
        {
            int n = needle.Length;
            int index = depth * n;
            for (int i = 0; i < n; i++)
            {
                if (hackstack[index + i] != needle[i])
                    return false;
            }
            return true;
        }

        private static string Exec(string command, string workingDir = null, int timeoutInSec = 10, LogCallback logCallback = null)
        {
            try
            {
#if UNITY_EDITOR_WIN
                string shellCmd = "cmd.exe";
                string shellCmdArg = "/s /c"; // We use /s to avoid escaping problems.
                string cmdArguments = shellCmdArg + " \"" + command + "\"";
#else
                string shellCmd = "bash";
                string shellCmdArg = "-c";
                string cmdArguments = shellCmdArg + " '" + command + "'";
#endif


                System.Diagnostics.Stopwatch stopWatch = null;
                if (logCallback != null)
                {
                    logCallback?.Invoke("GradleDependencyAnalyzer: Executing " + (workingDir != null ? "in " + workingDir + "/: " : " ") + shellCmd + " " + cmdArguments, LogLevel.Log);
                    stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();
                }

                var procStartInfo = new System.Diagnostics.ProcessStartInfo(shellCmd, cmdArguments);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                if (workingDir != null)
                    procStartInfo.WorkingDirectory = workingDir;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                string result = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(timeoutInSec * 1000);

                if (logCallback != null)
                {
                    stopWatch.Stop();
                    logCallback?.Invoke("GradleDependencyAnalyzer: Exec was done after " + Mathf.RoundToInt(stopWatch.ElapsedMilliseconds / 1000f) + " seconds.", LogLevel.Log);
                }

                return result;
            }
            catch (System.Exception e)
            {
                logCallback?.Invoke("GradleDependencyAnalyzer Error: " + e, LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Looks for .gradle files in Assets/ and Library/PackageCache/.
        /// </summary>
        /// <returns></returns>
        public static List<string> FindGradleFiles()
        {
            List<string> gradleFiles = new List<string>();

            RecurseIntoDir(Application.dataPath, gradleFiles);

            string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");
            string packageCacheDir = projectDir + "Library/PackageCache";
            RecurseIntoDir(packageCacheDir, gradleFiles);

            return gradleFiles;
        }

        private static void RecurseIntoDir(string dir, List<string> gradleFiles)
        {
            if (dir.EndsWith("Plugins/Android") || dir.EndsWith("Plugins\\Android"))
            {
                var files = Directory.GetFiles(dir);
                foreach (var file in files)
                {
                    if (file.EndsWith(".gradle"))
                    {
                        gradleFiles.Add(file.Replace("\\", "/"));
                    }
                }
            }

            string[] directories = Directory.GetDirectories(dir);
            foreach (string subDir in directories)
            {
                RecurseIntoDir(subDir, gradleFiles);
            }
        }

    }
}