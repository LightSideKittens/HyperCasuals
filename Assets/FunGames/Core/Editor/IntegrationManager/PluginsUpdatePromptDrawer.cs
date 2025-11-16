using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class PluginsUpdatePromptDrawer
    {
        public Action PromptCancelled;
        public Action<List<string>> PluginsConfirmed;

        private const int MaxWordLength = 45;
        private readonly IntegrationTheme _theme;
        private readonly Dictionary<string, bool> _toggleStates = new();
        private readonly RemoteSettingsController _rmc;
        private List<string> _pluginsToPrompt = new();
        private Vector2 scrollPos = Vector2.zero;

        public PluginsUpdatePromptDrawer(RemoteSettingsController rmc, IntegrationTheme theme)
        {
            _rmc = rmc;
            _theme = theme;
        }
        
        public void Draw(Rect windowPosition)
        {
            GUILayout.Space(_theme.Spacing10);
            GUILayout.Label("The following external SDKs will be imported:", _theme._titleLabelStyle);
            GUILayout.Space(_theme.Spacing10);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(windowPosition.width));
            EditorGUILayout.BeginVertical();

            foreach (var plugin in _pluginsToPrompt)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(_theme.WindowMargin);
                string modules = string.Join(", ", _rmc.GetModulesForPlugin(plugin));
                string lineBreak = plugin.Length >= MaxWordLength ? "\n" : "";
                
                bool toggleState = GUILayout.Toggle(
                    GetToggleValue(plugin), 
                    $"{plugin} {lineBreak}<color=grey>(Used by {modules})</color>",
                    _theme._richToggleStyle);
                _toggleStates[plugin] = toggleState;
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(_theme.Spacing20);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(_theme.Spacing20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(_theme.WindowMargin);
            
            if (GUILayout.Button("Cancel"))
            {
                PromptCancelled?.Invoke();
            }

            if (GUILayout.Button("Continue"))
            {
                OnConfirmClicked();
            }
            
            GUILayout.Space(_theme.WindowMargin);
            GUILayout.EndHorizontal();
            GUILayout.Space(_theme.Spacing20);
        }

        private void OnConfirmClicked()
        {
            List<string> pluginsToInstall = _pluginsToPrompt.Where(GetToggleValue).ToList();
            PluginsConfirmed?.Invoke(pluginsToInstall);
        }

        private bool GetToggleValue(string pluginFileName)
        {
            return !_toggleStates.ContainsKey(pluginFileName) || _toggleStates[pluginFileName];
        }

        public void SetPluginsToPrompt(List<string> pluginNames)
        {
            List<String> validPlugins = new List<string>();
            bool isProjectBuiltIn = ProjectUtils.IsProjectBuiltInRenderPipeline;

            foreach (var item in pluginNames)
            {
                if (!FGUnityPackageInstaller.IsFileBasedPackage(item))
                {
                    validPlugins.Add(item);
                    continue;
                }

                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(item);
                bool hasBuiltInPipelineSpec = fileNameWithoutExt.EndsWith("_BUILTIN");
                bool hasURPSpec = fileNameWithoutExt.EndsWith("_URP");
                if(isProjectBuiltIn && hasURPSpec || !isProjectBuiltIn && hasBuiltInPipelineSpec) continue;
                validPlugins.Add(item);
            }

            validPlugins.Sort();
            _pluginsToPrompt = validPlugins;
        }
    }
}