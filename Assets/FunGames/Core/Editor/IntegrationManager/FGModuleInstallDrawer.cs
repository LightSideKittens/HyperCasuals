using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FunGames.Core.Modules;
using FunGames.Core.Utils;
using FunGames.Editor;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class FGModuleInstallDrawer
    {
        private readonly IntegrationManagerController _imc;
        private readonly IntegrationManagerWindow _imw;
        private static Dictionary<string, int> _versionSelected = new Dictionary<string, int>();
        private float w1;
        private float w2;
        private float w4;
        public const string ICONS_PATH = "Assets/FunGames/Core/Editor/IntegrationManager/Icons/";
        private const string WARNING_ICON = "warning_icon.png";
        private const string ALERT_ICON = "alert_icon.png";
        private const string BETA_ICON = "Beta-Icon-Small.png";

        public FGModuleInstallDrawer(IntegrationManagerController controller, IntegrationManagerWindow window)
        {
            _imc = controller;
            _imw = window;
            w1 = 155; //180; //position.width * 0.2f;
            w2 = 80; //120; //position.width * 0.2f;
            w4 = 100; //position.width * 0.4f / 3;

            foreach (var id in _imc.Data.GetAllIds())
            {
                int selectedIndex = _imc.Data.GetAllValidVersions(id).Count - 1;
                if (!_versionSelected.ContainsKey(id)) _versionSelected.Add(id, selectedIndex);
                else _versionSelected[id] = selectedIndex;
            }
        }

        public void Draw(FGModuleVersion moduleVersion, int index)
        {
            if (moduleVersion.Versions.Count == 0)
            {
                foreach (var module in moduleVersion.SubModules) Draw(module, index);
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(moduleVersion.Name, index == 0 ? EditorStyles.boldLabel : EditorStyles.label,
                GUILayout.Width(w1+w2));
            string id = _imc.Data.GetIdFromName(moduleVersion.Name);
            string selectedVersion = _imc.Data.GetAllValidVersions(id)[_versionSelected[id]];

            FGModuleInfo currentInstalledModuleInfo = _imc.Data.GetModuleInfo(id, _imc.GetCurrentVersion(id));
            FGModuleInfo selectedModuleInfo = _imc.Data.GetModuleInfo(id, selectedVersion);

            DrawWarning(currentInstalledModuleInfo);

            EditorGUILayout.LabelField(_imc.GetCurrentVersion(id), GUILayout.Width(w2));
            // EditorGUILayout.LabelField(_imc.Data.GetLatestVersion(id), GUILayout.Width(w3));

            DrawSelectedVersionInfo(selectedModuleInfo);
            // DrawChoicePopup(selectedModuleInfo);
            // DrawInstallButton(selectedModuleInfo);

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!_imc.IsModuleInstalled(id));
            DrawPrefabButton(id);
            DrawSettingsButton(id);
            DrawDocumentationButton(id);
            EditorGUI.BeginDisabledGroup(index == 0);
            DrawRemovePackageButton(id);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            foreach (var module in moduleVersion.SubModules)
            {
                EditorGUI.indentLevel++;
                index++;
                Draw(module, index);
                EditorGUI.indentLevel--;
            }


            EditorGUILayout.EndVertical();
        }

        private void DrawInstallButton(FGModuleInfo selectedModuleInfo)
        {
            bool upToDate = _imc.IsModuleUpToDateComparedToVersion(selectedModuleInfo.Id, selectedModuleInfo.Version);
            EditorGUI.BeginDisabledGroup(upToDate && _imc.IsModuleInstalled(selectedModuleInfo.Id));
            if (GUILayout.Button(GetInstallButtonLabel(selectedModuleInfo.Id, selectedModuleInfo.Version),
                GUILayout.Height(20), GUILayout.Width(w4)))
            {
                _imc.DownloadAndInstall(selectedModuleInfo);
            }

            EditorGUI.EndDisabledGroup();
        }

        //
        private string GetInstallButtonLabel(string id, string versionOfSelectedModule)
        {
            string label;
            if (!_imc.IsModuleInstalled(id)) label = "Install";
            else if (!_imc.IsModuleUpToDateComparedToVersion(id, versionOfSelectedModule))
            {
                switch (VersionUtils.CompareVersions(_imc.GetCurrentVersion(id), versionOfSelectedModule))
                {
                    case CompareVersionResult.FirstIsGreater:
                        label = "Revert";
                        break;
                    default:
                        label = "Update";
                        break;
                }
            }
            else label = "Installed";

            return label;
        }

        private void DrawChoicePopup(FGModuleInfo selectedModuleInfo)
        {
            _versionSelected[selectedModuleInfo.Id] = EditorGUILayout.Popup(_versionSelected[selectedModuleInfo.Id],
                _imc.Data.GetAllValidVersions(selectedModuleInfo.Id).ToArray(), GUILayout.Height(20),
                GUILayout.Width(w4));
        }

        private void DrawPrefabButton(string id)
        {
            EditorGUI.BeginDisabledGroup(!_imc.IsModuleInstalled(id));
            var iconPath = ICONS_PATH + "Box-Icon.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = "Add Prefab(s) to Scene";
            tooltip += "\n- " + _imc.GetPackage(id)?.ModuleInfo.Name;
            GUILayoutOption[] options = { GUILayout.Width(32), GUILayout.Height(20) };
            if (GUILayout.Button(new GUIContent(icon, tooltip), options))
            {
                _imc.GetPackage(id)?.AddPrefabs();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawSettingsButton(string id)
        {
            EditorGUI.BeginDisabledGroup(!_imc.IsModuleInstalled(id));
            var iconPath = ICONS_PATH + "Settings-Icon-Small.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = "Select Settings Asset";
            tooltip += "\n- " + _imc.GetPackage(id)?.ModuleInfo.Name;
            if (GUILayout.Button(new GUIContent(icon, tooltip), GUILayout.Width(32), GUILayout.Height(20)))
            {
                var obj = Resources.Load("FunGames/" + _imc.GetPackage(id)?.SettingsAssetName);
                if(obj == null) _imc.GetPackage(id)?.CreateSettingsAsset();
                Selection.activeObject = obj;
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawDocumentationButton(string id)
        {
            var iconPath = ICONS_PATH + "Book-Opened-Icon.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = "Link to Guide";
            tooltip += "\n- " + _imc.GetPackage(id)?.ModuleInfo.Name;
            if (GUILayout.Button(new GUIContent(icon, tooltip), GUILayout.Width(32), GUILayout.Height(20)))
            {
                string docUrl = "https://gitlab.com/fungames-sdk/";
                docUrl += _imc.GetPackage(id)?.DeploymentFolder;
                docUrl += "/-/blob/";
                docUrl += _imc.GetPackage(id)?.ModuleInfo.Version;
                // docUrl += "/Documentation.md";
                Application.OpenURL(docUrl);
            }
        }

        private void DrawRemovePackageButton(string id)
        {
            EditorGUI.BeginDisabledGroup(!_imc.IsModuleInstalled(id));
            var iconPath = ICONS_PATH + "Trash-Icon-Small.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = "Remove package";
            tooltip += "\n- " + _imc.GetPackage(id)?.ModuleInfo.Name;
            if (GUILayout.Button(new GUIContent(icon, tooltip), GUILayout.Width(32), GUILayout.Height(20)))
            {
                string destinationPath = _imc.GetPackage(id)?.DestinationPath;
                if (!String.IsNullOrEmpty(destinationPath))
                {
                    string packageFolder = Application.dataPath + "/" + FGPath.FUNGAMES + destinationPath;
                    Directory.Delete(packageFolder, true);
                }

                Thread.Sleep(2000);
                _imc.MapLocalSetup();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawWarning(FGModuleInfo currentInstalledModuleInfo)
        {
            var iconPath = ICONS_PATH;
            List<FGModuleInfo> dependenciesToUpdate = CheckDependencies(currentInstalledModuleInfo);
            TooltipBuilder tooltipBuilder = new TooltipBuilder();
            bool showWarning = false;

            if (currentInstalledModuleInfo != null && currentInstalledModuleInfo.IsDeprecated)
            {
                iconPath += ALERT_ICON;
                tooltipBuilder.Add(
                    "Current installed version is Deprecated. Please update or revert to a working version !");
                showWarning = true;
            }
            else if (dependenciesToUpdate.Count != 0)
            {
                iconPath += ALERT_ICON;
                string tooltip = "Some dependencies need to be updated :\n";
                foreach (var dependencyToUpdate in dependenciesToUpdate)
                {
                    tooltip += "- " + dependencyToUpdate.Name + " (" + dependencyToUpdate.Version + "+)\n";
                }

                tooltipBuilder.Add(tooltip);
                showWarning = true;
            }
            else if (currentInstalledModuleInfo != null && currentInstalledModuleInfo.HasIssue)
            {
                iconPath += WARNING_ICON;
                tooltipBuilder.Add(
                    "Some issues have been detected with the current installed module. Please update or revert to a working version !");
                showWarning = true;
            }
            else if (currentInstalledModuleInfo != null && currentInstalledModuleInfo.IsBeta)
            {
                iconPath += BETA_ICON;
                tooltipBuilder.Add("Current installed version is a Beta version.");
                showWarning = true;
            }

            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            if (showWarning)
            {
                GUILayout.Box(new GUIContent(icon, tooltipBuilder.ToString()), GUILayout.Width(20),
                    GUILayout.Height(20));
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(20), GUILayout.Height(20));
            }
        }

        private void DrawSelectedVersionInfo(FGModuleInfo selectedModuleInfo)
        {
            var iconPath = ICONS_PATH + BETA_ICON;
            TooltipBuilder tooltipBuilder = new TooltipBuilder();
            bool showWarning = false;

            if (selectedModuleInfo != null && selectedModuleInfo.IsBeta)
            {
                tooltipBuilder.Add("Selected version is a Beta version.");
                showWarning = true;
            }

            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            if (showWarning)
            {
                GUILayout.Box(new GUIContent(icon, tooltipBuilder.ToString()), GUILayout.Width(20),
                    GUILayout.Height(20));
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(20), GUILayout.Height(20));
            }
        }

        private List<FGModuleInfo> CheckDependencies(FGModuleInfo moduleInfo)
        {
            List<FGModuleInfo> dependenciesToUpdate = new List<FGModuleInfo>();
            if (moduleInfo is null) return dependenciesToUpdate;
            List<FGModuleInfo> allDependencies = _imc.Data.GetAllDependencies(moduleInfo);
            foreach (var dependency in allDependencies)
            {
                if (IntegrationManagerController.NOT_INSTALLED.Equals(_imc.GetCurrentVersion(dependency.Id)))
                {
                    dependenciesToUpdate.Add(dependency);
                    continue;
                }

                CompareVersionResult result =
                    VersionUtils.CompareVersions(_imc.GetCurrentVersion(dependency.Id), dependency.Version);
                if (CompareVersionResult.SecondIsGreater == result) dependenciesToUpdate.Add(dependency);
            }

            return dependenciesToUpdate;
        }
    }
}