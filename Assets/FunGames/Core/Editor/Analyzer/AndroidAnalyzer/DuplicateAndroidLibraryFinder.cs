using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public static class DuplicateAndroidLibraryFinder
    {
        /// <summary>
        /// Finds similarly named libraries (.jar,.aar,.androidlib).
        /// </summary>
        /// <returns></returns>
        public static List<List<AndroidLibrary>> FindSimilar()
        {
            var libs = AndroidLibraryFinder.FindLibraries();

            var results = new List<List<AndroidLibrary>>();
            foreach (var lib in libs)
            {
                bool added = false;
                foreach (var list in results)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (AreLibsSimilar(lib, list[i]))
                        {
                            list.Add(lib);
                            added = true;
                            break;
                        }
                    }
                    if (added)
                        break;
                }
                if (!added)
                    results.Add(new List<AndroidLibrary>() { lib });
            }

            for (int i = results.Count - 1; i >= 0; i--)
            {
                if (results[i].Count < 2)
                    results.RemoveAt(i);
            }

            return results;
        }

        private static bool AreLibsSimilar(AndroidLibrary libA, AndroidLibrary libB)
        {
            string libACanonical = "";
            if (!string.IsNullOrEmpty(libA.FileName))
                libACanonical = MakeNameCanonical(libA.FileName);
            else if (!string.IsNullOrEmpty(libA.AndroidLibDirName))
                libACanonical = MakeNameCanonical(libA.AndroidLibDirName);

            string libBCanonical = "";
            if (!string.IsNullOrEmpty(libB.FileName)) libBCanonical = MakeNameCanonical(libB.FileName);
            else if (!string.IsNullOrEmpty(libB.AndroidLibDirName))
                libBCanonical = MakeNameCanonical(libB.AndroidLibDirName);
            
            if (libACanonical == libBCanonical)
                return true;

            int distance = CalcLevenshteinDistance(libACanonical, libBCanonical);
            return distance < 3;
        }

        private static string MakeNameCanonical(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // remove all the fluff around the actual lib names
            name = name.Replace(AndroidLibraryFinder.AarExt,"");
            name = name.Replace(AndroidLibraryFinder.JarExt,"");
            name = name.Replace(AndroidLibraryFinder.AndroidLibExt, "");
            name = name.Replace("jetified-", "");
            name = name.Replace("-runtime", "");
            name = Regex.Replace(name, @"[^a-zA-Z]", "");

            return name.ToLowerInvariant();
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings.
        /// Thanks to:
        /// https://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int CalcLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (string.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Mathf.Min
                        (
                        Mathf.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }
    }
}
