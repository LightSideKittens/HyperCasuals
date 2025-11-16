using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FunGames.Tools.Utils;
using UnityEngine;
using FunGames.Core.Modules;

namespace FunGames.Core.Editor.IntegrationManager
{
    public enum ReflectionMemberType
    {
        Field,
        Property,
        Method
    }

    public static class IntegrationUtils
    {
        public const string ICONS_PATH = "Assets/FunGames/Core/Editor/IntegrationManager/Icons/";
        public static string[] AssetsFolderAsArray = new[] {"Assets"};
        public static Color AmberErrorColor = new Color(1f, 0.6f, 0.4f);

        public static Dictionary<string, FGPackage> GetPackageMap()
        {
            List<FGPackage> packages = ProjectUtils.GetEnumerableOfType<FGPackage>();
            Dictionary<string, FGPackage> map = new Dictionary<string, FGPackage>();

            foreach (FGPackage package in packages)
            {
                map.Add(package.ModuleInfo.Id, package);
            }

            return map;
        }

        public static List<T> RemovingNull<T>(this List<T> items)
        {
            return items.Where(a => a != null).ToList();
        }
    
        public static string GetPackageUrl(string moduleId, string moduleVersion, string deploymentFolder)
        {
            string packageUrl = "https://gitlab.com/fungames-sdk/";
            string packageName = moduleId + "-" + moduleVersion + ".unitypackage";
            packageUrl += deploymentFolder;
            packageUrl += "/-/raw/" + moduleVersion + "/";
            packageUrl += packageName;
            return packageUrl;
        }

        public static string WrapInRichTextColor(string text, Color color)
        {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);
            return "<color=#" + hexColor + ">" + text + "</color>";
        }

        /// <summary>
        /// Parses the xml at filePath for EDM4U dependencies.
        /// </summary>
        /// <param name="filePath">The dependencies xml filePath</param>
        /// <param name="filter">Specifies only the dependencies to extract if containing any of the strings.
        /// Pass null or empty list if you want to get all dependencies</param>
        /// <param name="onlyFirstPerPlatform"></param>
        /// <returns>The platform and version of the dependencies in android_x.x.x_iOS_x.x.x format.
        /// The filter is applied first, then <paramref name="onlyFirstPerPlatform"/>
        /// is applied to the filtered list</returns>
        public static List<(string platform, string name, string version)> ParseDependenciesXml(
            string filePath, string[] filter = null, bool onlyFirstPerPlatform = false)
        {
            List<(string platform, string name, string version)> deps = new();
            
            try
            {
                XDocument xmlDocument = XDocument.Load(filePath);

                var androidPackages = xmlDocument
                    .Descendants("androidPackage")
                    .Select(p => p.Attribute("spec")?.Value ?? "")
                    .ToList();

                var iosPods = xmlDocument
                    .Descendants("iosPod")
                    .Select(pod => new
                    {
                        Name = pod.Attribute("name")?.Value,
                        Version = pod.Attribute("version")?.Value
                    })
                    .ToList();
                
                if (filter is {Length: > 0})
                {
                    androidPackages = androidPackages
                        .Where(pkg => filter.Any(f => pkg.ToLower().Contains(f.ToLower())))
                        .ToList();
                    
                    iosPods = iosPods
                        .Where(pod => filter.Any(f => pod.Name.ToLower().Contains(f.ToLower())))
                        .ToList();
                }

                if (onlyFirstPerPlatform)
                {
                    androidPackages = androidPackages.Count > 0
                        ? androidPackages.GetRange(0, 1)
                        : androidPackages;
                    iosPods = iosPods.Count > 0
                        ? iosPods.GetRange(0, 1)
                        : iosPods;
                }

                foreach (var spec in androidPackages)
                {
                    string[] specParts = spec.Split(":");
                    string name = specParts.Length > 0 ? specParts[0] : "";
                    string version = specParts.Length > 1 ? specParts[^1] : "";
                    version = version.Replace("[", "").Replace("]", "");
                    deps.Add(($"Android",name,version));
                }

                foreach (var pod in iosPods)
                {
                    deps.Add(("iOS",pod.Name,pod.Version));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading {filePath}: {ex.Message}");
            }

            return deps;
        }
        
        public static string GetSdkStringVersionInfo(
            string sdkDisplayName, string assemblyName, string typeName,
            ReflectionMemberType memberType, string memberName)
        {
            object versionObject = GetSdkVersion(assemblyName, typeName, memberType, memberName);
            string versionString = $"{versionObject}".Replace(",", ".");
            return !string.IsNullOrEmpty(versionString) ? $"{sdkDisplayName},{versionString}\n" : "";
        }
        
        public static string GetSdkIntPtrVersionInfo(
            string sdkDisplayName, string assemblyName, string typeName,
            ReflectionMemberType memberType, string memberName)
        {
            object versionObject = GetSdkVersion(assemblyName, typeName, memberType, memberName);
            if (versionObject == null) return string.Empty;
            
            try
            {
                IntPtr ptr = (IntPtr) versionObject;
                string version = Marshal.PtrToStringAnsi(ptr);
                return $"{sdkDisplayName},{version}\n";
            }
            catch (Exception)
            {
                return $"{sdkDisplayName},\n";
            }
        }

        public static object GetSdkVersion(
            string assemblyName, string typeName,
            ReflectionMemberType versionMemberType, string versionMemberName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null) return null;

                var type = assembly.GetType(typeName);
                if (type == null) return null;

                switch (versionMemberType)
                {
                    case ReflectionMemberType.Field:
                        FieldInfo fieldInfo = type.GetField(versionMemberName);
                        if (fieldInfo != null) return fieldInfo.GetValue(null);
                        break;
                    case ReflectionMemberType.Property:
                        PropertyInfo propertyInfo = type.GetProperty(versionMemberName);
                        if (propertyInfo != null) return propertyInfo.GetValue(null);
                        break;
                    case ReflectionMemberType.Method:
                        MethodInfo methodInfo = type.GetMethod(versionMemberName);
                        if (methodInfo != null) return methodInfo.Invoke(null, Array.Empty<object>());
                        break;
                }

                return "";
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Builds a Dictionary of Type name to Assembly name
        /// </summary>
        /// <returns>A Dictionary</returns>
        public static Dictionary<string, string> BuildTypesMap()
        {
            Dictionary<string, string> typesMap = new();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var classes = assembly.GetTypes().Where(t => t.IsClass);

                    foreach (var classRef in classes)
                    {
                        typesMap.TryAdd(classRef.FullName, assembly.FullName);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return typesMap;
        }

        public static bool IsFGModuleInfoFile(string path)
        {
            string relativePath = Path.GetRelativePath(Application.dataPath, Path.GetFullPath(path));
            return path.EndsWith(".json") 
                   && Path.GetFileName(path).StartsWith("fg_") && relativePath.StartsWith("FunGames");
        }

        public static Dictionary<string, FGModuleInfo> GetFGModuleInfos()
        {
            Dictionary<string, FGModuleInfo> moduleInfos = new();
            List<string> matches = AssetsUtils.GetAssetsPath(
                new[] {"Assets/FunGames"}, "t:TextAsset fg_");

            foreach (var path in matches)
            {
                if (IsFGModuleInfoFile(path))
                {
                    string content = File.ReadAllText(path);
                    FGModuleInfo moduleInfo = JsonUtility.FromJson<FGModuleInfo>(content);
                    if (moduleInfo != null) moduleInfos.TryAdd(moduleInfo.Id, moduleInfo);
                }
            }

            return moduleInfos;
        }
    }
}
