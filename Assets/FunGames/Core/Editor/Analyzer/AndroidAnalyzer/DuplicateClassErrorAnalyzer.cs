using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public static class DuplicateClassErrorAnalyzer
    {
        public static bool HasKnownError(string message)
        {
            return message.IndexOf("duplicate class", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static List<ErrorGroup> AnalyzeMessages(
            List<SuspiciousMessage> messages,
            List<AndroidLibrary> libFiles,
            List<Dependency> gradleDependencies,
            List<string> gradleFiles,
            LogCallback logCallback)
        {
            var groupList = new List<ErrorGroup>();

            foreach (var msg in messages)
            {
                if (!HasKnownError(msg.StepMessage.content))
                    continue;

                // A line might look like this:
                // Duplicate class android.support.v4.app.RemoteActionCompatParcelizer found in modules core-1.3.0-runtime.jar (androidx.core:core:1.3.0) and jetified-core-1.1.0.jar (core-1.1.0.jar)
                // Duplicate class com.google.gson.DefaultDateTypeAdapter found in the following modules: jetified-gson-2.8.6.jar(com.google.code.gson:gson:2.8.6), jetified-gson-2.8.6.jar(gson -2.8.0.jar) and jetified-gson-2.8.6.jar(gson-2.8.6.jar)
                string patternClass = @"Duplicate class ([^ ]+)";
                Regex regExClass = new Regex(patternClass, RegexOptions.IgnoreCase);

                // jetified-gson-2.8.6.jar(com.google.code.gson:gson:2.8.6), jetified-gson-2.8.6.jar(gson -2.8.0.jar), jetified-gson-2.8.6.jar(gson-2.8.6.jar)
                // to jetified-gson-2.8.6.jar, com.google.code.gson:gson:2.8.6 | jetified-gson-2.8.6.jar,gson -2.8.0.jar | jetified-gson-2.8.6.jar,gson-2.8.6.jar
                string patternModule = @"(?:,? ?([^)]+)\(([^)]+))";
                Regex regExModules = new Regex(patternModule, RegexOptions.IgnoreCase);

                // analyze each line in the message
                StringReader strReader = new StringReader(msg.StepMessage.content);
                string line;
                List<string> matchedModules = new List<string>();

                while (true)
                {
                    line = strReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        if (line == null)
                            break;
                        else
                            continue;

                    // class
                    Match m = regExClass.Match(line);
                    if (!m.Success || m.Groups.Count < 2)
                        continue;

                    string duplicateClass = m.Groups[1].Value;

                    // modules
                    matchedModules.Clear();
                    int modulesIndex = line.IndexOf(" modules");
                    string modulesText = line.Substring(modulesIndex);
                    if (modulesText[0] == ':')
                        modulesText = modulesText.Substring(1);
                    modulesText = modulesText.Replace(") and ", "), ");

                    m = regExModules.Match(modulesText);
                    while (m.Success)
                    {
                        // group errors with similar origin
                        if (m.Groups.Count == 3)
                        {
                            // Gradle error origin formats (they are a little confusing):
                            // origin for .aar files example: it is ":name:" instead of the expected "name.aar"
                            // origin for .gradle package example: it is "androidx.core:core:1.3.0"
                            // origin for .jar libs example: it is "core-1.1.0.jar" just as you would expect
                            matchedModules.Add(m.Groups[1].Value.Trim());
                            matchedModules.Add(m.Groups[2].Value.Trim());
                        }
                        m = m.NextMatch();
                    }
                    if (matchedModules.Count >= 4)
                    {
                        // TODO: Add more than one pair to the DuplicateClassError (cases if 3 or more libs are in conflict for one class are not covered at the moment).
                        // Trim the ':' because the GradleDependencyFinder also trims them and we need them to match exactly.
                        string firstLib = matchedModules[0].Trim(':');
                        string firstOrigin = matchedModules[1].Trim(':');
                        string secondLib = matchedModules[2].Trim(':');
                        string secondOrigin = matchedModules[3].Trim(':');
                        var error = new DuplicateClassError(msg, duplicateClass,
                            firstLib, firstOrigin,
                            secondLib, secondOrigin
                            );

                        var group = GetGroup(groupList, firstOrigin, secondOrigin);
                        if (group == null)
                        {
                            var libPair = new StringPair(firstOrigin, secondOrigin);
                            group = new ErrorGroup(libPair);
                            groupList.Add(group);
                        }

                        group.Errors.Add(error);
                    }
                    else
                    {
                        logCallback?.Invoke("Unknown format encountered in: '" + line + "'.", LogLevel.Warning);
                    }
                }
            }

            // Add some context to the found groups to help the user.
            ////////////////////////////////////////////////////////////

            // get .gradle file contents to search for dendencies
            Dictionary<string, string> gradleFileContents = new Dictionary<string, string>();
            foreach (var file in gradleFiles)
            {
                var text = File.ReadAllText(file);
                if (!string.IsNullOrWhiteSpace(text))
                    gradleFileContents.Add(file, text);
            }

            foreach (var group in groupList)
            {
                group.ResultLines.Add(new ResultLine(ResultLine.LineType.Group, null));

                // Find out where the conflicting library files have come from (each of these lists may have multiple entries or be empty).
                var libsA = libFiles.Where(l => l.FileName == group.OriginPair.A).ToList();
                var libsB = libFiles.Where(l => l.FileName == group.OriginPair.B).ToList();
                var gradlesA = gradleDependencies.SelectMany(d => d.Find(group.OriginPair.A)).ToList();
                var gradlesB = gradleDependencies.SelectMany(d => d.Find(group.OriginPair.B)).ToList();

                List<string> handledPairs = new List<string>();
                string pair;

                // Check libFilesA with libFilesB and gradlesB
                foreach (var libA in libsA)
                {
                    // libFilesB
                    foreach (var libB in libsB)
                    {
                        pair = libA.FileName + libB.FileName;
                        if (!handledPairs.Contains(pair))
                        {
                            handledPairs.Add(pair);
                            AnalyzeLibDetails(group, libsA);
                            AnalyzeLibDetails(group, libsB);
                        }
                    }

                    // gradlesB
                    foreach (var gradleB in gradlesB)
                    {
                        pair = libA.FileName + gradleB.Name;
                        if (!handledPairs.Contains(pair))
                        {
                            handledPairs.Add(pair);
                            AnalyzeLibDetails(group, libsA);
                            AnalyzeGradleDetails(group, gradlesB, gradleFileContents, libFiles);
                        }
                    }
                }

                // Check gradlesA and libFilesB and gradlesB
                foreach (var gradleA in gradlesA)
                {
                    // libFilesB
                    foreach (var libB in libsB)
                    {
                        pair = gradleA.Name + libB.FileName;
                        if (!handledPairs.Contains(pair))
                        {
                            handledPairs.Add(pair);
                            AnalyzeGradleDetails(group, gradlesA, gradleFileContents, libFiles);
                            AnalyzeLibDetails(group, libsB);
                        }
                    }

                    // gradlesB
                    foreach (var gradleB in gradlesB)
                    {
                        pair = gradleA.Name + gradleB.Name;
                        if (!handledPairs.Contains(pair))
                        {
                            handledPairs.Add(pair);
                            AnalyzeGradleDetails(group, gradlesA, gradleFileContents, libFiles);
                            AnalyzeGradleDetails(group, gradlesB, gradleFileContents, libFiles);
                        }
                    }
                }
            }

            return groupList;
        }

        private static void AnalyzeLibDetails(
            ErrorGroup group, List<AndroidLibrary> libraries)
        {
            AndroidLibrary library = libraries[0];
            string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");

            group.ResultLines.Add(new ResultLine(ResultLine.LineType.ConflictingLibOrDependency, null, null, library, null, null));

            // list all origins for this gradle dependency
            foreach (var lib in libraries)
            {
                string relativePath = lib.Path;
                relativePath = relativePath.Replace(projectDir, "");

                // was included because it was added the dependencies in the gradle.build file
                if (lib.IsFromPackage)
                {
                    group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntry,
                        "Added because '" + lib.FileName + "' is located in the <b>" + lib.Package + " package</b> in a Plugins/Android dir."));
                    group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntrySub,
                        "Found in '" + relativePath + "'."
                        , null, library, "go to file", relativePath));
                }
                else
                {
                    group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntry,
                        "Added because '" + lib.FileName + "' is located in a Plugins/Android dir."));
                    group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntrySub,
                        "Found in '" + relativePath + "'."
                        , null, library, "go to file", relativePath));
                }
            }
        }

        private static void AnalyzeGradleDetails(
            ErrorGroup group, List<Dependency> dependencies, Dictionary<string, string> gradleFileContents, 
            List<AndroidLibrary> libraries)
        {
            Dependency dependency = dependencies[0];
            string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");

            group.ResultLines.Add(new ResultLine(ResultLine.LineType.ConflictingLibOrDependency, null, dependency, null, null, null));

            // list all origins for this gradle depedency
            List<string> uniqueOrigins = new List<string>();
            foreach (var dep in dependencies)
            {
                string msg;
                string rootName = dep.GetRoot().Name;
                if (!uniqueOrigins.Contains(rootName))
                {
                    if (rootName == dep.Name)
                    {
                        if (dep.Type == Dependency.GradleType.Project)
                        {
                            msg = "Added because '" + rootName + "' was added as a gradle project.";
                            group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntry, msg, dependency, null, null, null));
                        }
                        else if (dep.Type == Dependency.GradleType.LibFile)
                        {
                            msg = "Added because '" + rootName + "' was added as a gradle lib file (probably '" + rootName + ".aar').";
                            group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntry, msg, dependency, null, null, null));

                            string searchForLibNameAar = (rootName + ".aar").ToLower();
                            string searchForLibNameJar = (rootName + ".jar").ToLower();
                            foreach (var libFile in libraries)
                            {
                                if (libFile.FileName != null && (libFile.FileName.ToLower() == searchForLibNameAar || libFile.FileName.ToLower() == searchForLibNameJar))
                                {
                                    string relativePath = libFile.Path.Replace(projectDir, "");
                                    msg = "Found in '" + relativePath + "'";
                                    group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntrySub, msg, dependency, null, "go to file", relativePath));
                                }
                            }
                        }
                        else
                        {
                            // was included because it was added to the dependencies in the gradle.build file
                            msg = "Added because '" + rootName + "' was added as a gradle dependency.";
                            group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntry, msg, dependency, null, null, null));
                        }
                    }
                    else
                    {
                        // was included because some other package depends on it.
                        msg = "Added because '" + rootName + "' depends on it.";
                        group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntry, msg, dependency, null, null, null));
                    }
                    uniqueOrigins.Add(rootName);

                    // Find gradle template containing rootName and list it.
                    bool found = false;
                    foreach (var kv in gradleFileContents)
                    {
                        StringReader strReader = new StringReader(kv.Value);
                        string line;
                        int lineNr = 0;
                        while (true)
                        {
                            lineNr++;

                            line = strReader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line))
                                if (line == null)
                                    break;
                                else
                                    continue;

                            // Look for special syntax in gradle file. Examples:
                            //   implementation(name: 'vunglePlugin', ext:'aar')
                            //   implementation project('GoogleMobileAdsPlugin.androidlib')
                            string strToFind = rootName;
                            if(dep.Type == Dependency.GradleType.LibFile)
                            {
                                strToFind = "'" + rootName + "', ext:";
                            }
                            else if (dep.Type == Dependency.GradleType.LibFile)
                            {
                                strToFind = "project('" + rootName + "')";
                            }
                            if (line.Contains(strToFind))
                            {
                                found = true;
                                string relativePath = kv.Key;
                                relativePath = relativePath.Replace(projectDir, "");
                                msg = "Found in gradle file '" + relativePath + "' at line " + lineNr + ".";
                                group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntrySub, msg, dependency, null, "go to file", relativePath));
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        msg = "No gradle file containing this dependency found. Probably added via PreProcessBuild hooks by Unity or a third party.";
                        group.ResultLines.Add(new ResultLine(ResultLine.LineType.DetailsEntrySub, msg, dependency, null, null, null));
                    }
                }
            }
        }

        private static ErrorGroup GetGroup(List<ErrorGroup> list, string libA, string libB)
        {
            foreach (var group in list)
                if (group.OriginPair.Equals(libA, libB))
                    return group;

            return null;
        }
    }
}