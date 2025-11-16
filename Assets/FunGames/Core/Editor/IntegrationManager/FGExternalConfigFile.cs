using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FunGames.Core.Editor.IntegrationManager;
using FunGames.Tools.Utils;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FunGames.Core.Editor.Config
{
    public static class FGExternalConfigFile
    {
        private const string Separator = ",";

        private static Dictionary<string, string> typesMap;

        public static string BuildConfigText()
        {
            typesMap = IntegrationUtils.BuildTypesMap();
            
            string configText =  $"{GetPackageManagerInfos()}" +
                   $"{GetAdjustInfo()}" +
                   $"{GetAdvertySdkInfo()}" +
                   $"{GetAmazonSdkInfo()}" +
                   $"{GetAnzuSdkInfo()}" +
                   $"{GetAppHarbrSdkInfo()}" +
                   $"{GetAudioMobPluginInfo()}" +
                   $"{GetFacebookSdkInfo()}" +
                   $"{GetFirebasePackagesInfo()}" +
                   $"{GetGadsmeSdkInfo()}" +
                   $"{GetGameAnalyticsSdkInfo()}" +
                   $"{GetGoogleMobileAdsInfo()}" +
                   $"{GetGPGPluginInfo()}" +
                   $"{GetGooglePlayReviewInfo()}" +
                   $"{GetIronSourcePluginInfo()}" +
                   $"{GetPlaygapInfo()}" +
                   $"{GetOdeeoSdkInfo()}" +
                   $"{GetOgurySdkInfo()}" +
                   $"{GetRevenueCatDependenciesInfo()}" +
                   $"{GetMaxSdkInfo()}" +
                   $"{GetMaxAdaptersInfo()}";
            
            typesMap.Clear();
            return configText;
        }

        private static string GetPackageManagerInfos()
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> concernedPackages = new HashSet<string>()
            {
                "com.apple.unityplugin.core",
                "com.apple.unityplugin.gamekit",
                "com.monetizr.unityplugin",
                "com.unity.purchasing",
                "com.unity.mobile.notifications",
                "com.metica.unity",
            };
            PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();

            foreach (var package in packages)
            {
                if (concernedPackages.Contains(package.name))
                {
                    sb.Append($"{package.name}{Separator}{package.version}\n");
                }
            }

            return sb.ToString();
        }

        private static string GetAdjustInfo()
        {
            List<string> packageInfoPaths = AssetsUtils.GetAssetsPath(
                    new[] {"Assets"}, "package")
                .Where(a => a.EndsWith("package.json") && a.Contains("/Adjust/"))
                .Select(Path.GetFullPath).ToList();

            StringBuilder sb = new();

            foreach (var packageInfoPath in packageInfoPaths)
            {
                string content = File.ReadAllText(packageInfoPath);
                string version = JsonUtility.FromJson<DetailsModel>(content)?.version ?? "";
                sb.Append($"Adjust,{version}\n");
            }

            return sb.ToString();
        }

        private static string GetAdvertySdkInfo()
        {
            string typeName = "Adverty5.Adverty";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkIntPtrVersionInfo(
                "Adverty", assemblyName, typeName,
                ReflectionMemberType.Method, "GetSDKVersion");
        }

        private static string GetAmazonSdkInfo()
        {
            string typeName = "AmazonConstants";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "Amazon", assemblyName, typeName,
                ReflectionMemberType.Field, "VERSION");
        }
        
        private static string GetAnzuSdkInfo()
        {
            string typeName = "anzu.AnzuNative";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "Anzu", assemblyName, typeName,
                ReflectionMemberType.Method, "GetVersionFloat");
        }
        
        private static string GetAppHarbrSdkInfo()
        {
            string typeName = "AppHarbr";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "AppHarbr", assemblyName, typeName,
                ReflectionMemberType.Property, "Version");
        }
        
        private static string GetAudioMobPluginInfo()
        {
            string typeName = "AudioMob.AudioMobPlugin";
            typesMap.TryGetValue(typeName, out var assemblyName);
            if (assemblyName == null)
            {
                typeName = "Audiomob.AudiomobPlugin";
                typesMap.TryGetValue(typeName, out assemblyName);
            }
            return IntegrationUtils.GetSdkStringVersionInfo(
                "Audiomob", assemblyName, typeName,
                ReflectionMemberType.Property, "PluginVersion");
        }

        private static string GetFacebookSdkInfo()
        {
            string typeName = "Facebook.Unity.FacebookSdkVersion";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "Facebook", assemblyName, typeName,
                ReflectionMemberType.Property, "Build");
        }

        private static string GetFirebasePackagesInfo()
        {
            string versionSubstring = "_version-";
            string FirebasePrefix = "Firebase";
            string manifestSuffix = "_manifest.txt";

            List<string> manifestFiles = AssetsUtils.GetAssetsPath(
                    new[] {"Assets"}, "Firebase")
                .Select(Path.GetFileName)
                .Where(
                    a => a.StartsWith(FirebasePrefix) && a.EndsWith(manifestSuffix)).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var manifestFile in manifestFiles)
            {
                string[] parts = manifestFile.Split(versionSubstring);
                string version = parts[1].Replace(manifestSuffix, "");
                sb.Append($"{parts[0]},{version}\n");
            }

            return sb.ToString();
        }

        private static string GetGadsmeSdkInfo()
        {
            string typeName = "Gadsme.GadsmeSDK";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "Gadsme", assemblyName, typeName,
                ReflectionMemberType.Field, "Version");
        }

        private static string GetGameAnalyticsSdkInfo()
        {
            string className = "GameAnalyticsSDK.Setup.Settings";
            typesMap.TryGetValue(className, out var assemblyName);
            
            return IntegrationUtils.GetSdkStringVersionInfo(
                "GameAnalytics", assemblyName, "GameAnalyticsSDK.Setup.Settings",
                ReflectionMemberType.Field, "VERSION");
        }

        private static string GetGoogleMobileAdsInfo()
        {
            string versionSubstring = "_version-";
            string manifestPrefix = "GoogleMobileAds";
            string manifestSuffix = "_manifest.txt";

            List<string> manifestFiles = AssetsUtils.GetAssetsPath(
                    new[] {"Assets"}, "GoogleMobileAds_version")
                .Select(Path.GetFileName)
                .Where(
                    a => a.StartsWith(manifestPrefix) && a.EndsWith(manifestSuffix)).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var manifestFile in manifestFiles)
            {
                string[] parts = manifestFile.Split(versionSubstring);
                string version = parts[1].Replace(manifestSuffix, "");
                sb.Append($"{parts[0]},{version}\n");
            }

            return sb.ToString();
        }

        private static string GetGPGPluginInfo()
        {
            string typeName = "GooglePlayGames.PluginVersion";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "GooglePlayGames", assemblyName, typeName,
                ReflectionMemberType.Field, "VersionString");
        }

        private static string GetGooglePlayReviewInfo()
        {
            List<string> assets = AssetsUtils.GetAssetsPath(
                new[] {"Assets"}, "package")
                .Where(a => a.EndsWith("package.json") && a.Contains("com.google.play.review"))
                .Select(Path.GetFullPath).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var packageFile in assets)
            {
                string content = File.ReadAllText(packageFile);
                string version = JsonUtility.FromJson<DetailsModel>(content)?.version ?? "";
                sb.Append($"GooglePlayReview,{version}\n");
            }

            return sb.ToString();
        }

        private static string GetIronSourcePluginInfo()
        {
            string version = GetIronSourceVersionMethod1();
            version = string.IsNullOrEmpty(version) ? GetIronSourceVersionMethod2() : version;
            return string.IsNullOrEmpty(version) ? "" : $"IronSource,{version}\n";
        }

        private static string GetIronSourceVersionMethod1()
        {
            string typeName = "IronSource";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return "" + IntegrationUtils.GetSdkVersion(
                assemblyName, typeName,
                ReflectionMemberType.Method, "pluginVersion");
        }

        private static string GetIronSourceVersionMethod2()
        {
            string typeName = "IronSource";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return "" + IntegrationUtils.GetSdkVersion(
                assemblyName, typeName,
                ReflectionMemberType.Field, "UNITY_PLUGIN_VERSION");
        }

        private static string GetPlaygapInfo()
        {
            List<string> mdFiles =
                AssetsUtils.GetAssetsPath(new[] {"Assets"}, "Version")
                    .Where(a => a.EndsWith("Version.md") && a.Contains("Playgap"))
                    .Select(Path.GetFullPath).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var file in mdFiles)
            {
                string content = File.ReadAllText(file);
                string[] contentParts = content.Split("Version: **");
                if (contentParts.Length < 2)
                {
                    sb.Append("Playgap,_\n");
                    continue;
                }

                string[] versionParts = contentParts[1].Split("**");
                if (contentParts.Length < 1)
                {
                    sb.Append("Playgap,_\n");
                    continue;
                }

                string version = versionParts[0];
                sb.Append($"Playgap,{version}\n");
            }

            return sb.ToString();
        }

        private static string GetOdeeoSdkInfo()
        {
            string typeName = "Odeeo.OdeeoBuildConfig";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "Odeeo", assemblyName, typeName,
                ReflectionMemberType.Field, "SDK_VERSION");
        }

        private static string GetRevenueCatDependenciesInfo()
        {
            string dependenciesXmlPath =
                AssetsUtils.GetAssetsPath(new[] {"Assets"}, "RevenueCatDependencies")
                    .Where(a => a.EndsWith(".xml"))
                    .Select(Path.GetFullPath).FirstOrDefault();

            if (dependenciesXmlPath == null) return "";
            StringBuilder sb = new();
            var platformVersions = IntegrationUtils.ParseDependenciesXml(
                dependenciesXmlPath, new[] {"revenuecat", "purchases"});
            
            foreach (var item in platformVersions)
            {
                sb.Append($"RevenueCat {item.platform},{item.version}\n");
            }

            return sb.ToString();
        }

        private static string GetOgurySdkInfo()
        {
            string dependenciesXmlPath =
                AssetsUtils.GetAssetsPath(new[] {"Assets"}, "OguryDependencies")
                    .Where(a => a.EndsWith(".xml"))
                    .Select(Path.GetFullPath).FirstOrDefault();

            if (dependenciesXmlPath == null) return "";
            StringBuilder sb = new();
            var platformVersions = IntegrationUtils.ParseDependenciesXml(
                dependenciesXmlPath, new[] {"ogury"});
            
            foreach (var item in platformVersions)
            {
                sb.Append($"Ogury {item.platform},{item.version}\n");
            }

            return sb.ToString();
        }

        private static string GetMaxSdkInfo()
        {
            string typeName = "MaxSdk";
            typesMap.TryGetValue(typeName, out var assemblyName);
            return IntegrationUtils.GetSdkStringVersionInfo(
                "ApplovinMax", assemblyName, typeName,
                ReflectionMemberType.Property, "Version");
        }

        private static string GetMaxAdaptersInfo()
        {
            return $"{GetAssetsMaxAdaptersInfo()}{GetUPMMaxAdaptersInfo()}";
        }

        private static string GetAssetsMaxAdaptersInfo()
        {
            List<string> dependenciesXmlPaths = 
                AssetsUtils.GetAssetsPath(new[] {"Assets"}, "Dependencies")
                .Where(a => a.EndsWith("Dependencies.xml") && a.Contains("MaxSdk/Mediation"))
                .Select(Path.GetFullPath).ToList();
            StringBuilder sb = new();

            foreach (var path in dependenciesXmlPaths)
            {
                string adapterName = new FileInfo(path).Directory?.Parent?.Name ?? "";
                var dependencies = 
                    IntegrationUtils.ParseDependenciesXml(path, onlyFirstPerPlatform: true);

                foreach (var dep in dependencies)
                {
                    sb.Append($"Applovin {adapterName} Adapter {dep.platform},{dep.version}\n");
                }
            }

            return sb.ToString();
        }
        
        private static string GetUPMMaxAdaptersInfo()
        {
            StringBuilder sb = new StringBuilder();
            PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();

            foreach (var package in packages)
            {
                if (package.name.StartsWith("com.applovin.mediation.adapters"))
                {
                    sb.Append(GetUPMMaxAdapterInfo(package));
                }
            }

            return sb.ToString();
        }

        private static string GetUPMMaxAdapterInfo(PackageInfo package)
        {
            string[] descriptionLines = package.description.Split("\n");
            string firstLine = descriptionLines.Length > 0 ? descriptionLines[0] : "";
            string[] firstLineParts = firstLine.Split(" ");
            string version = firstLineParts.Length > 0 ? firstLineParts[^1] : package.version;
            version = version.Trim('.');
            string displayName = package.displayName.Replace(" Mediation", "");
            return $"{displayName}{Separator}{version}\n";
        }
        
        private class DetailsModel
        {
            public string version;
        }
    }
}
