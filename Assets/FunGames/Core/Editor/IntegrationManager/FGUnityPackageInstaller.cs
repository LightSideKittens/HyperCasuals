using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FunGames.Core.Modules;
using FunGames.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class FGUnityPackageInstaller
    {
        public Action<string> StatusUpdated;
        public Action ManifestUpdated;
        
        private const string UnityPackageExt = ".unitypackage";
        private const string TarballExt1 = ".tgz";
        private const string TarballExt2 = ".tar.gz";
        private readonly FGSDKInstallationManifest _ongoingInstallManifest;

        public FGUnityPackageInstaller(FGSDKInstallationManifest manifest)
        {
            _ongoingInstallManifest = manifest;
        }

        private static bool IsUnityPackage(string path) => path.EndsWith(UnityPackageExt);
        private static bool IsTarball(string path) => path.EndsWith(TarballExt1) || path.EndsWith(TarballExt2);
        public static bool IsFileBasedPackage(string path) => IsUnityPackage(path) || IsTarball(path);

        public async Task ImportExternalPackages(FGMainJsonImport mainJsonImport)
        {
            List<string> packagesClone = _ongoingInstallManifest.externalPackagesToImport.ToList();
            
            foreach (var item in packagesClone)
            {
                if (IsUnityPackage(item)) await ImportUnityPackage(item, mainJsonImport);
                else if (IsTarball(item)) await ImportTarball(item);
                else await ImportUPMPackage(item);

                _ongoingInstallManifest.externalPackagesToImport.Remove(item);
                ManifestUpdated?.Invoke();
            }

            CleanupFunGamesExternals();
        }

        public async Task ImportUnityPackage(string assetPath, FGMainJsonImport mainJsonImport)
        {
            RemoveExternalUnityPackageFiles(assetPath, mainJsonImport);
            string filename = Path.GetFileName(assetPath);
            StatusUpdated?.Invoke($"Importing {filename}");
            Debug.Log($"Importing {assetPath}");
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            AssetDatabase.ImportPackageCallback finishedCallback = (_) => { OnPackageImportEnded(); };
            AssetDatabase.ImportPackageFailedCallback failedCallback = (s, e) =>
            {
                Debug.LogError($"Failed to import: {s}\n\n{e}");
                OnPackageImportEnded();
            };

            AssetDatabase.importPackageCancelled += finishedCallback;
            AssetDatabase.importPackageFailed += failedCallback;
            AssetDatabase.onImportPackageItemsCompleted += OnImportPackageItemsCompleted;

            AssetDatabase.ImportPackage(assetPath, false);
            await tcs.Task;
            
            AssetDatabase.importPackageCancelled -= finishedCallback;
            AssetDatabase.importPackageFailed -= failedCallback;
            AssetDatabase.onImportPackageItemsCompleted -= OnImportPackageItemsCompleted;
            
            void OnPackageImportEnded() => tcs.TrySetResult(true);
            void OnImportPackageItemsCompleted(string[] items)
            {
                foreach (var path in items)
                {
                    if (IsFileBasedPackage(path) && path.Contains("FunGames_Externals"))
                        OnReceiveExternalPackageAsset(path);
                    else if(IntegrationUtils.IsFGModuleInfoFile(path)) OnReceiveFGModuleInfo(path);
                }
                
                OnPackageImportEnded();
            }
        }

        private async Task ImportTarball(string assetPath)
        {
            string packagePath = "file:../" + assetPath;
            await ImportUPMPackage(packagePath);
        }

        private async Task ImportUPMPackage(string id)
        {
            StatusUpdated?.Invoke($"Importing {id}");
            Debug.Log($"Importing {id}");
            
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            AddRequest request = Client.Add(id);
            EditorApplication.update += OnImportUpdate;
            await tcs.Task;

            void OnImportUpdate()
            {
                if (!request.IsCompleted) return;
                
                if (request.Status != StatusCode.Success) 
                    Debug.LogError("Failed to import package: " + request.Error.message);

                EditorApplication.update -= OnImportUpdate;
                tcs.SetResult(true);
            }
        }

        private void OnReceiveFGModuleInfo(string assetPath)
        {
            string json = File.ReadAllText(Path.GetFullPath(assetPath));
            FGModuleInfo moduleInfo = JsonUtility.FromJson<FGModuleInfo>(json);
            
            foreach (var item in moduleInfo.Plugins)
            {
                if(!IsFileBasedPackage(item)) _ongoingInstallManifest.externalPackagesToImport.Add(item);
            }
            
            ManifestUpdated?.Invoke();
        }

        private void OnReceiveExternalPackageAsset(string path)
        {
            string fileName = Path.GetFileName(path);

            if (!_ongoingInstallManifest.enabledPluginNames.Contains(fileName))
            {
                Debug.Log($"Skipping {fileName}\n\n{JsonUtility.ToJson(_ongoingInstallManifest)}");
                return;
            }

            if (_ongoingInstallManifest.externalPackagesToImport.Any(a => a.Contains(fileName))) return;

            _ongoingInstallManifest.externalPackagesToImport.Add(path);
            ManifestUpdated?.Invoke();
        }

        private void RemoveExternalUnityPackageFiles(string packagePath, FGMainJsonImport mainJsonImport)
        {
            if (!packagePath.Contains("FunGames_Externals")) return;
            List<string> pathsToDelete = new();
            string packagePathLower = packagePath.ToLowerInvariant();

            foreach (var (packageNamePart, pathsInPackage) in mainJsonImport.PackageToAssetPaths)
            {
                if (packagePathLower.Contains(packageNamePart.ToLowerInvariant()))
                {
                    pathsToDelete = pathsInPackage;
                    break;
                }
            }

            foreach (string pathToDelete in pathsToDelete)
            {
                string fullPath = $"{Application.dataPath}/{pathToDelete}";

                if (!pathToDelete.Contains("*"))
                {
                    DeleteFileSystemInfo(new DirectoryInfo(fullPath));
                    DeleteFileSystemInfo(new FileInfo(fullPath));
                    continue;
                }
                
                DirectoryInfo parentFolder = new DirectoryInfo(fullPath).Parent;
                if (parentFolder is not {Exists: true}) continue;

                string pattern = fullPath;
                pattern = pattern.Replace("/", "\\/").Replace("*", ".*");
                pattern = pattern + "$";
                List<FileSystemInfo> filesAndFolders = parentFolder
                    .GetFileSystemInfos("*", SearchOption.AllDirectories)
                    .Where(a => new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(a.FullName))
                    .ToList();
                filesAndFolders.ForEach(DeleteFileSystemInfo);
            }
        }

        private void DeleteFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            try
            {
                if (fileSystemInfo is DirectoryInfo directoryInfo) directoryInfo.Delete(true);
                else if (fileSystemInfo is FileInfo fileInfo) fileInfo.Delete();
            }
            catch
            {
                // ignored
            }
        }

        private void CleanupFunGamesExternals()
        {
            DirectoryInfo folder = new DirectoryInfo(Path.Combine(Application.dataPath, "FunGames_Externals"));
            if(!folder.Exists) return;

            var packageFiles = folder.GetFiles("*", SearchOption.AllDirectories)
                .Where(a => IsFileBasedPackage(a.FullName))
                .ToList();
            packageFiles.ForEach(fileInfo =>
            {
                AssetDatabase.DeleteAsset(GetProjectRelativePath(fileInfo.FullName));
            });

            List<DirectoryInfo> emptySubfolders = folder
                .GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Where(IsEmptyUnityDirectory)
                .ToList();
            emptySubfolders.ForEach(dirInfo =>
            {
                AssetDatabase.DeleteAsset(GetProjectRelativePath(dirInfo.FullName));
            });
        }

        private bool IsEmptyUnityDirectory(DirectoryInfo dirInfo)
        {
            string relativePath = GetProjectRelativePath(dirInfo.FullName);
            string[] assets = AssetDatabase.FindAssets("", new[] {relativePath});
            return assets.Length == 0;
        }

        private string GetProjectRelativePath(string fullPath)
        {
            string pathWithoutDataPath = fullPath.Replace(Application.dataPath, "").Trim('/');
            return Path.Combine("Assets", pathWithoutDataPath);
        } 
    }
}