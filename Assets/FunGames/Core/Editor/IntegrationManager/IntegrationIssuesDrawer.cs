using System;
using System.Collections.Generic;
using System.Linq;
using FunGames.Core.Editor.Analyzer;
using FunGames.Tools.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class IntegrationIssuesDrawer: IPreprocessBuildWithReport
    {
        private readonly Color enabledToggleColor = new Color(.9f, .9f, .9f);
        private readonly Color disabledToggleColor = new Color(.4f, .4f, .4f);
        
        private readonly IntegrationIssuesController _controller;
        private readonly Dictionary<string, bool> _topCheckersFoldoutStates = new();
        private readonly Dictionary<FGSDKIssueSeverity, bool> severityToggles = new();
        private Vector2 _scrollPosition;
        private Rect _windowPosition;
        private GUIStyle _rightAlignedLabelStyle;
        private GUIStyle _descriptionLabelStyle;
        private List<FGAnalyzer> TopLevelAnalyzers => _controller.TopLevelAnalyzers;
        public int callbackOrder => 1000;
        
        public IntegrationIssuesDrawer()
        {
            _controller = new IntegrationIssuesController();
            _controller.Initialize();

            foreach (var severity in GetSeverityLevels())
            {
                severityToggles[severity] = true;
            }
            
            UpdateFilteredAnalyzers();
        }
        
        public void OnPreprocessBuild(BuildReport report)
        {
            CheckCriticalErrors();
        }

        private void RefreshIssues()
        {
            _controller.RefreshIssues();
            UpdateFilteredAnalyzers();
        }

        private bool GetPackageFoldoutState(string id)
        {
            if(!_topCheckersFoldoutStates.ContainsKey(id)) return true;

            return _topCheckersFoldoutStates[id];
        }

        private FGSDKIssueSeverity[] GetSeverityLevels()
        {
            return Enum.GetValues(typeof(FGSDKIssueSeverity)).Cast<FGSDKIssueSeverity>().ToArray();
        }

        private void UpdateFilteredAnalyzers()
        {
            var enabledSeverityLevels = 
                severityToggles.Keys.Where(a => severityToggles[a]).ToHashSet();
            
            foreach (var analyzer in TopLevelAnalyzers)
            {
                analyzer.FilteredIssues = 
                    analyzer.Issues.Where(a => enabledSeverityLevels.Contains(a.severity)).ToList();
            }
        }
        
        public void Draw(Rect windowPosition)
        {
            _rightAlignedLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = EditorStyles.miniBoldLabel.fontSize,
                fontStyle = EditorStyles.miniBoldLabel.fontStyle
            };

            _descriptionLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = EditorStyles.label.fontSize,
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                richText = true
            };

            _windowPosition = windowPosition;
            _scrollPosition = EditorGUILayout.BeginScrollView(
                _scrollPosition, GUILayout.Width(_windowPosition.width));

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                RefreshIssues();
            }
            GUILayout.FlexibleSpace();
            DrawSeverityFilters();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            foreach (var checker in TopLevelAnalyzers)
            {
                DrawTopLevelChecker(checker);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawSeverityFilters()
        {
            Color defaultBgColor = GUI.backgroundColor;

            foreach (var severity in GetSeverityLevels())
            {
                severityToggles.TryGetValue(severity, out bool isEnabled);
                GUI.backgroundColor = isEnabled ? enabledToggleColor : disabledToggleColor;
                Texture icon = GetSeverityIconTexture(severity);
                string tooltip = $"Filter {severity} issues";

                if (GUILayout.Button(new GUIContent(icon, tooltip), GUILayout.Width(32), GUILayout.Height(20)))
                {
                    severityToggles[severity] = !isEnabled;
                    UpdateFilteredAnalyzers();
                }
            }

            GUI.backgroundColor = defaultBgColor;
        }

        private void DrawTopLevelChecker(FGAnalyzer analyzer)
        {
            List<FGIssue> issues = analyzer.FilteredIssues;
                
            if(issues.Count == 0) return;

            bool foldout = GetPackageFoldoutState(analyzer.Id);
            _topCheckersFoldoutStates[analyzer.Id] = EditorGUILayout.Foldout(foldout, analyzer.Name);

            if (!foldout) return;
            

            foreach (var issue in issues)
            {
                DrawIssue(issue);
            }

            EditorGUILayout.Space(20);
        }

        private void DrawIssue(FGIssue issue)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            DrawTitleSeverityIcon(issue);
            EditorGUILayout.LabelField(issue.title, EditorStyles.miniLabel);
            EditorGUILayout.Space();
            
            GUILayout.Label(issue.affectedSdk, _rightAlignedLabelStyle);
            EditorGUILayout.EndHorizontal();

            if (issue.customDescriptionDrawer == null) 
                EditorGUILayout.LabelField($"{issue.issueDescription}", _descriptionLabelStyle);
            else
                issue.customDescriptionDrawer?.Invoke();

            if (!string.IsNullOrEmpty(issue.howToFix))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"How to fix:", EditorStyles.miniBoldLabel);
                float labelHeight = _descriptionLabelStyle.CalcHeight(
                    new GUIContent(issue.howToFix), 
                    EditorGUIUtility.currentViewWidth - 20);
                EditorGUILayout.SelectableLabel(
                    issue.howToFix, _descriptionLabelStyle, GUILayout.Height(labelHeight));
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            
            if (issue.fix != null && GUILayout.Button("Fix", GUILayout.Width(50)))
            {
                issue.fix?.Invoke();
                RefreshIssues();
            }
            
            if(issue.customAction != null && GUILayout.Button(issue.customActionText, GUILayout.Width(100)))
            {
                issue.customAction?.Invoke();
                RefreshIssues();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(20);
        }

        private void DrawTitleSeverityIcon(FGIssue issue)
        {
            Texture icon = GetSeverityIconTexture(issue.severity);
            GUILayoutOption[] options = { GUILayout.Width(16), GUILayout.Height(16) };
            EditorGUILayout.LabelField(new GUIContent(icon), options);
        }

        private Texture GetSeverityIconTexture(FGSDKIssueSeverity severity)
        {
            string filename = severity switch
            {
                FGSDKIssueSeverity.Info => "info-icon.png",
                FGSDKIssueSeverity.Warning => "warning_icon.png",
                FGSDKIssueSeverity.Error => "alert_icon.png",
                _ => ""
            };
            
            string iconPath = IntegrationUtils.ICONS_PATH + filename;
            return AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
        }

        private void CheckCriticalErrors()
        {
            if(!AnalyzerSettings.GetOrCreateSettings().ShowPrebuildWarning) return;

            if (TopLevelAnalyzers.Any(a => a.Issues.Any(b => b.severity == FGSDKIssueSeverity.Error)))
            {
                ShowPrebuildWarning();
            }
        }

        private void ShowPrebuildWarning()
        {
            bool abortBuild = EditorUtility.DisplayDialog(
                "Critical issues were found",
                "Some critical issues exist in your project which will affect the proper functioning of " +
                "your game.",
                "Show me", "Ignore");
            if (abortBuild)
            {
                var window = IntegrationManagerWindow.OpenWindow();
                window.Focus();
                window.SelectIssuesTab();
                throw new BuildFailedException("Build canceled");
            }
        }
    }
}