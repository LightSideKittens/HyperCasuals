using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public static class AndroidLibraryFinder
    {
        public const string AndroidLibExt = ".androidlib";
        public const string AarExt = ".aar";
        public const string JarExt = ".jar";

        /// <summary>
        /// Finds all libraries (.jar,.aar,*.androidlib) in the project.
        /// Also searches in packages (embedded and Library/PackageCache).
        /// </summary>
        /// <returns></returns>
        public static List<AndroidLibrary> FindLibraries()
        {
            List<AndroidLibrary> libs = new List<AndroidLibrary>();

            AddFromAssets(libs);
            AddFromPackages(libs);
            return libs;
        }

        private static void AddFromAssets(List<AndroidLibrary> libs)
        {
            // find all directories which might have Android libraries within them
            List<string> libDirs = new List<string>();
            string[] directories = Directory.GetDirectories(Application.dataPath);
            foreach (string dir in directories)
            {
                RecurseIntoDir(dir, libDirs);
            }

            // find all libraries
            foreach (var libDir in libDirs)
            {
                if (libDir.EndsWith(AndroidLibExt, StringComparison.InvariantCultureIgnoreCase))
                {
                    var lib = new AndroidLibrary(libDir, null, null, null, null, Path.GetFileName(libDir));
                    libs.Add(lib);
                }
                else
                {
                    var files = Directory.GetFiles(libDir);
                    foreach (var file in files)
                    {
                        if (file.EndsWith(JarExt, StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(AarExt, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var lib = FileToLib(file);
                            libs.Add(lib);
                        }
                    }
                }
            }
        }

        private static AndroidLibrary FileToLib(string path)
        {
            string fileName = Path.GetFileName(path);
            string version = null;
            
            // example filename to version -> localbroadcastmanager7-1.0.0 => 1.0.0
            string versionPattern = @"([0-9.]+)";
            Regex versionRegex = new Regex(versionPattern, RegexOptions.IgnoreCase);
            Match m = versionRegex.Match(fileName);
            if (m.Success && m.Groups.Count > 0)
            {
                while (m.Success)
                {
                    if(m.Groups[0].Value[0] != '.')
                        version = m.Groups[0].Value;
                    m = m.NextMatch();
                }
            }

            var lib = new AndroidLibrary(path, fileName, version, package: null, packageVersion: null);
            return lib;
        }

        private static void AddFromPackages(List<AndroidLibrary> libs)
        {
            // package cache dir (where the installed package files live)
            string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");
            string packageCacheDir = projectDir + "Library/PackageCache";
            List<string> libDirs = new List<string>();

            // find all cached package directories which might have libs in them
            if (Directory.Exists(packageCacheDir))
            {
                string[] directories = Directory.GetDirectories(packageCacheDir);
                foreach (string dir in directories)
                {
                    RecurseIntoDir(dir, libDirs);
                }
            }

            // find all embedded package directories which might have libs in them
            string packagesDir = projectDir + "Packages";
            if (Directory.Exists(packagesDir))
            {
                string[] directories = Directory.GetDirectories(packagesDir);
                foreach (string dir in directories)
                {
                    RecurseIntoDir(dir, libDirs);
                }

                // find all libraries
                foreach (var libDir in libDirs)
                {
                    string package;
                    // from cache or embedded?
                    if (libDir.Contains("PackageCache"))
                        package = libDir.Substring(packageCacheDir.Length + 1);
                    else
                        package = libDir.Substring(packagesDir.Length + 1);

                    // fetch first part of relative path
                    int index;
                    int indexA = package.IndexOf('/');
                    int indexB = package.IndexOf('\\');
                    if (indexA >= 0 && indexB >= 0)
                        index = System.Math.Min(indexA, indexB);
                    else
                        index = System.Math.Max(indexA, indexB);
                    package = package.Substring(0, index);

                    if (libDir.EndsWith(AndroidLibExt, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var lib = new AndroidLibrary(libDir, null, null, null, null, Path.GetFileName(libDir));
                        libs.Add(lib);
                    }
                    else
                    {
                        var files = Directory.GetFiles(libDir);
                        foreach (var file in files)
                        {
                            if (file.EndsWith(JarExt, StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(AarExt, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var lib = PackageToLib(file, package);
                                libs.Add(lib);
                            }
                        }
                    }
                }
            }
        }

        private static AndroidLibrary PackageToLib(string path, string package)
        {
            string fileName = Path.GetFileName(path);

            string fileVersion = null;
            string pat = @"([0-9]+\.?[0-9]?\.?[0-9]?)"; // localbroadcastmanager-1.0.0 => 1.0.0
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Match m = r.Match(fileName);
            if (m.Success && m.Groups.Count > 0)
                fileVersion = m.Groups[0].Value;
            if (fileVersion != null && fileVersion.EndsWith("."))
                fileVersion = fileVersion.Substring(-1);
            if (string.IsNullOrEmpty(fileVersion))
                fileVersion = null;

            string packageVersion = null;
            if (!string.IsNullOrEmpty(package))
            {
                string patPack = @"([0-9]+\.?[0-9]?\.?[0-9]?)"; // com.unity.ads@3.7.5 => 3.7.5
                r = new Regex(patPack, RegexOptions.IgnoreCase);
                m = r.Match(package);
                if (m.Success && m.Groups.Count > 0)
                    packageVersion = m.Groups[0].Value;
                if (packageVersion != null && packageVersion.EndsWith("."))
                    packageVersion = packageVersion.Substring(-1);
                if (string.IsNullOrEmpty(packageVersion))
                    packageVersion = null;
            }

            var lib = new AndroidLibrary(path, fileName, fileVersion, package, packageVersion);
            return lib;
        }

        private static void RecurseIntoDir(string dir, List<string> libDirs)
        {
            string[] directories = Directory.GetDirectories(dir);
            foreach (string subDir in directories)
            {
                RecurseIntoDir(subDir, libDirs);
            }

            bool isLibrary = dir.EndsWith(AndroidLibExt, StringComparison.InvariantCultureIgnoreCase);
            bool containsLibs = false;
            string[] files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                if (file.ToLower().EndsWith(JarExt) || file.ToLower().EndsWith(AarExt))
                {
                    containsLibs = true;
                    break;
                }
            }

            if(containsLibs || isLibrary)
            {
                libDirs.Add(dir);
            }
        }

    }
}
