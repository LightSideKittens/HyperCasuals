using System;
using System.Collections.Generic;
using UnityEngine;

namespace FunGames.Tools.Utils
{
    public class VersionUtils
    {
        public static CompareVersionResult CompareVersions(string v1, string v2)
        {
            if (v1.Equals(v2)) return CompareVersionResult.Equal;

            try
            {
                string[] v1Split = v1.Split('.');
                string[] v2Split = v2.Split('.');

                string[] smallestVersionString = v1Split.Length < v2Split.Length ? v1Split : v2Split;
                for (int i = 0; i < smallestVersionString.Length; i++)
                {
                    if (Convert.ToInt32(v1Split[i]) > Convert.ToInt32(v2Split[i]))
                        return CompareVersionResult.FirstIsGreater;
                    if (Convert.ToInt32(v1Split[i]) < Convert.ToInt32(v2Split[i]))
                        return CompareVersionResult.SecondIsGreater;
                }
            }
            catch (Exception)
            {
                // Debug.LogWarning("Error while comparing versions: " + e.Message);
                return default;
            }

            return default;
        }

        public static string GetLatest(List<string> versions)
        {
            string latest = versions[0];
            for (int i = 0; i < versions.Count; i++)
            {
                CompareVersionResult result = CompareVersions(latest, versions[i]);
                if (CompareVersionResult.SecondIsGreater == result) latest = versions[i];
            }

            return latest;
        }
    }

    public enum CompareVersionResult
    {
        Equal,
        FirstIsGreater,
        SecondIsGreater
    }
}