using System;
using System.Collections.Generic;
using FunGames.Core.Editor.IntegrationManager;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public class FGProjectSettingsAnalyzer: FGAnalyzer
    {
        protected override List<FGIssue> OwnIssues()
        {
            return new List<FGIssue>{CheckUnityVersion()}.RemovingNull();
        }

        private FGIssue CheckUnityVersion()
        {
            try
            {
                Version minVersionAllowed = Version.Parse("2022.3.39");
                Version installedVersion = Version.Parse(InstalledUnityVersion());

                if (installedVersion >= minVersionAllowed) return null;

                return new FGIssue
                {
                    title = "Recommended Unity version",
                    issueDescription =
                        $"Your Unity editor version {installedVersion} is lower than the " +
                        $"recommended minimum version {minVersionAllowed}",
                    howToFix = $"Update Unity editor to {minVersionAllowed} or higher.",
                    severity = FGSDKIssueSeverity.Info
                };
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private string InstalledUnityVersion()
        {
            string installedVersion = Application.unityVersion;
            installedVersion = RemoveUnityReleaseTypeComponent(installedVersion, "a");
            installedVersion = RemoveUnityReleaseTypeComponent(installedVersion, "b");
            installedVersion = RemoveUnityReleaseTypeComponent(installedVersion, "rc");
            installedVersion = RemoveUnityReleaseTypeComponent(installedVersion, "f");
            return installedVersion;
        }

        private string RemoveUnityReleaseTypeComponent(string unityVersion, string releaseType)
        {
            return unityVersion.Split(releaseType)[0];
        }
    }
}