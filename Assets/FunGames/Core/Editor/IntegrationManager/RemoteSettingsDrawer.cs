using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class RemoteSettingsDrawer
    {
        private readonly RemoteSettingsController _rmc;
        private string _apiKey;
        private IntegrationTheme _theme;

        public RemoteSettingsDrawer(RemoteSettingsController rmc, IntegrationTheme theme)
        {
            _theme = theme;
            _rmc = rmc;
            _apiKey = _rmc.ApiKey;
        }

        public void Draw()
        {
            DrawHeader();
            DrawSettings();
        }

        private void DrawHeader()
        {
            if(_rmc.IsVirginProject) return;
            
            GUILayout.Space(_theme.Spacing10);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        }

        private void DrawSettings()
        {
            GUILayout.Space(_theme.Spacing10);

            if (_rmc.IsDownloadingConfig)
            {
                GUILayout.Label("Loading...", EditorStyles.boldLabel, GUILayout.Width(_theme.w1));
                return;
            }

            if (_rmc.IsVirginProject)
            {
                GUILayout.Label("Welcome to FunGames SDK!", _theme._titleLabelStyle);
                GUILayout.Label("Please add your API Token to install the modules" +
                                " and settings required for your game:", _theme._middleCenterLabelStyle);
            }
            else if (string.IsNullOrEmpty(_rmc.ApiKey))
            {
                GUILayout.Label("Please enter your API token to update the modules and settings" +
                                " required for your game: ", _theme._middleCenterLabelStyle);
            }
            
            GUILayout.Space(_theme.Spacing10);
            DrawApiTokenLayout();
            GUILayout.Space(_theme.Spacing10);

            if (_rmc.IsUpdatingSdk)
            {
                GUILayout.Label(
                    "Import in progress. Please leave this window open.", _theme._middleCenterLabelStyle);
                GUILayout.Label(
                    _rmc.SdkInstallProgressStatus, _theme._middleCenterItalicLabelStyle);
            }

            if (!string.IsNullOrEmpty(_rmc.RequestError))
            {
                GUI.contentColor = _theme._errorColor;
                GUILayout.Label(_rmc.RequestError, _theme._middleCenterLabelStyle);
                GUI.contentColor = Color.white;
            }
            
            GUILayout.Space(_theme.Spacing10);
        }

        private void DrawApiTokenLayout()
        {
            GUILayout.BeginHorizontal(new GUIStyle {alignment = TextAnchor.MiddleLeft});
            GUILayout.FlexibleSpace();
            GUILayout.Label("API Token", EditorStyles.label, GUILayout.Width(70));
            _apiKey = GUILayout.TextField(_apiKey, GUILayout.Width(250)).Trim();
            string buttonText = _rmc.IsVirginProject ? "Install" : "Update";
            
            GUI.enabled = !string.IsNullOrEmpty(_apiKey) && _rmc.ReadyToRunUpdates;
            string tooltip = _rmc.ReadyToRunUpdates
                ? ""
                : "Please wait while data is loading...";
            
            if (GUILayout.Button(new GUIContent(buttonText, tooltip), GUILayout.Width(90)))
            {
                _rmc.OnClickUpdateSdk(_apiKey);
            }

            GUI.enabled = true;
            DrawWarning();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        private void DrawWarning()
        {
            if(string.IsNullOrEmpty(_rmc.OutdatedModules)) return;
            
            var iconPath = IntegrationUtils.ICONS_PATH + "warning_icon.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = $"Updates are available for: {_rmc.OutdatedModules}";
            GUILayoutOption[] options = { GUILayout.Width(32), GUILayout.Height(20) };
            GUILayout.Box(new GUIContent(icon, tooltip), options);
        }

        public void OnDecidePluginsToInstall(List<string> pluginsToInstall)
        {
            _rmc.OnUserDecidedPluginsToInstall(pluginsToInstall);
        }
    }
}