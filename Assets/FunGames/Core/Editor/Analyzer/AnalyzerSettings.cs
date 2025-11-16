using System.IO;
using FunGames.Core.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public class AnalyzerSettings: ScriptableObject
    {
        private const string SettingsFilePath = 
            FGPath.ASSETS_RESOURCES_FUNGAMES + "Analyzer/AnalyzerSettings.asset";
        private static AnalyzerSettings cachedSettings;
        
        [Tooltip("Should a dialog warn me of critical errors before builds?")]
        public bool ShowPrebuildWarning = true;
        
        [Tooltip("Should a dialog offer to analyse the failed Android build once it has finished?")]
        public bool OfferToAnalyzeAfterAndroidBuild = true;
        
        [HideInInspector] public BuildAnalysis LastBuildAnalysis;
        public LogLevel LogLevel = LogLevel.Error;

        public static AnalyzerSettings GetOrCreateSettings()
        {
            if (cachedSettings != null) return cachedSettings;
            
            cachedSettings = AssetDatabase.LoadAssetAtPath<AnalyzerSettings>(SettingsFilePath);

            // Still not found? Then search for it.
            if (cachedSettings == null)
            {
                string typeName = nameof(AnalyzerSettings);
                string[] results = AssetDatabase.FindAssets("t:" + typeName);
                if (results.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(results[0]);
                    cachedSettings = AssetDatabase.LoadAssetAtPath<AnalyzerSettings>(path);
                }
            }

            // Still not found? Then create settings.
            if (cachedSettings == null)
            {
                cachedSettings = CreateInstance<AnalyzerSettings>();
                cachedSettings.LogLevel = LogLevel.Warning;
                cachedSettings.OfferToAnalyzeAfterAndroidBuild = true;

                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }
                    
                AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                AssetDatabase.SaveAssets();
            }

            return cachedSettings;
        }
    }
}