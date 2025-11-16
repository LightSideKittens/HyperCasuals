using System.Collections.Generic;
using System.IO;
using FunGames.Core.Editor.IntegrationManager;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.Analyzer
{
    public class DuplicateClassErrorsDrawer
    {
        private readonly Color WarningGroupTitleColor = new Color(1f, 1f, 0.4f);
        private readonly Color ErrorDetailsSubColor = new Color(0.7f, 0.7f, 0.7f);
        private Color ErrorGroupTitleColor => IntegrationUtils.AmberErrorColor;

        public void DrawDuplicateClassErrorResults(List<ErrorGroup> duplicateClasses, bool hadGradleBuildFile)
        {
            if (duplicateClasses is not {Count: > 0}) return;

            BeginHorizontalIndent();
            if (!hadGradleBuildFile)
            {
                DrawLabel(
                    "<b>" + IntegrationUtils.WrapInRichTextColor("WARNING:", WarningGroupTitleColor) +
                    "</b> No gradle build file was found under 'Temp/gradleOut/unityLibrary/build.gradle'. " +
                    "The results will NOT include any detailed information if a gradle dependency is involved.\n\n" +
                    "Unity automatically deletes the contents in 'Temp/gradleOut/' upon closing. " +
                    "Usually that's a good thing but right now this means there is no 'gradle.build' " +
                    "file to analyse.\n\nTo get the full analysis please make a new build and do NOT close Unity " +
                    "afterwards. Then do the analysis again.",
                    richText: true);
                GUILayout.Space(10);
            }

            foreach (var group in duplicateClasses)
            {
                bool groupOpen = false;
                foreach (var line in group.ResultLines)
                {
                    if (line.Type == ResultLine.LineType.Group)
                    {
                        DrawDuplicateClassErrorGroupTitle(@group);

                        if (groupOpen)
                        {
                            GUILayout.EndVertical();
                            EndHorizontalIndent();
                        }

                        BeginHorizontalIndent();
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        groupOpen = true;
                    }
                    else if (line.Type == ResultLine.LineType.ConflictingLibOrDependency)
                    {
                        GUILayout.Space(5);
                        DrawDuplicateClassDetailsTitle(line);
                    }
                    else if (line.Type == ResultLine.LineType.DetailsEntry)
                    {
                        GUILayout.Space(3);
                        BeginHorizontalIndent(15);
                        DrawSelectableLabel(line.Text);
                        EndHorizontalIndent();
                    }
                    else if (line.Type == ResultLine.LineType.DetailsEntrySub)
                    {
                        BeginHorizontalIndent(30);

                        if (line.HasButton)
                        {
                            //GUILayout.BeginHorizontal(ErrorGroupDetailsButtonBoxStyle);
                            GUILayout.BeginHorizontal();
                        }
                        else
                            GUILayout.BeginHorizontal();

                        DrawSelectableLabel(line.Text, color: ErrorDetailsSubColor);
                        if (line.HasButton)
                        {
                            if (GUILayout.Button(line.ButtonLabel, GUILayout.Width(100)))
                            {
                                // TODO
                                if (line.ButtonAction.StartsWith("Assets"))
                                {
                                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(line.ButtonAction);
                                    if (obj != null)
                                        EditorGUIUtility.PingObject(obj);
                                }
                                else
                                {
                                    if (line.ButtonAction.Contains(":"))
                                    {
                                        EditorUtility.RevealInFinder(line.ButtonAction);
                                    }
                                    else
                                    {
                                        string projectDir = Path
                                            .GetFullPath(Path.Combine(Application.dataPath, "../"))
                                            .Replace("\\", "/");
                                        EditorUtility.RevealInFinder(projectDir + line.ButtonAction);
                                    }
                                }
                            }
                        }

                        GUILayout.EndHorizontal();
                        EndHorizontalIndent();
                    }
                    else
                    {
                        DrawLabel(line.Text);
                    }
                }

                if (groupOpen)
                {
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                    EndHorizontalIndent();
                }

                GUILayout.Space(10);
            }

            EndHorizontalIndent();
        }

        private void DrawLabel(
            string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true)
        {
            if (!color.HasValue)
                color = GUI.color;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;

            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;

            GUILayout.Label(text, style);
        }

        private void DrawSelectableLabel(
            string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true)
        {
            if (!color.HasValue)
                color = GUI.color;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;

            var content = new GUIContent(text);
            var position = GUILayoutUtility.GetRect(content, style);
            EditorGUI.SelectableLabel(position, text, style);
        }

        private void BeginHorizontalIndent(int indentAmount = 10, bool beginVerticalInside = true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentAmount);
            if (beginVerticalInside)
                GUILayout.BeginVertical();
        }

        private void EndHorizontalIndent(float indentAmount = 10, bool begunVerticalInside = true)
        {
            if (begunVerticalInside)
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawDuplicateClassErrorGroupTitle(ErrorGroup group)
        {
            string a = IntegrationUtils.WrapInRichTextColor(group.OriginPair.A, ErrorGroupTitleColor);
            string b = IntegrationUtils.WrapInRichTextColor(group.OriginPair.B, ErrorGroupTitleColor);
            DrawLabel("Duplicate classes found in " + a + " and " + b, bold: false);
        }

        private void DrawDuplicateClassDetailsTitle(ResultLine line)
        {
            string name = line.GetLibOrDependencyName();
            DrawLabel(name, bold: true);
        }

        
    }
}