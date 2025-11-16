using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class IntegrationTheme
    {
        public float Spacing10 = 10;
        public float Spacing20 = 20;
        public float WindowMargin = 20;
        public float w1 = 155;
        public readonly Color _errorColor = new Color(1, 96f / 255, 96f / 255);
        public GUIStyle _titleLabelStyle;
        public GUIStyle _middleCenterLabelStyle;
        public GUIStyle _middleCenterItalicLabelStyle;
        public GUIStyle _richToggleStyle;

        private bool _isInit;

        /// <summary>
        /// Only call this from inside an OnGUI method
        /// </summary>
        public void InitializeOnGUI()
        {
            if(_isInit) return;
            
            _titleLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = EditorStyles.largeLabel.fontSize,
                fontStyle = FontStyle.Bold
            };
            _middleCenterLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = EditorStyles.label.fontSize,
            };
            _middleCenterItalicLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = EditorStyles.miniLabel.fontSize,
                fontStyle = FontStyle.Italic
            };
            _richToggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                richText = true,
                wordWrap = true
            };

            _isInit = true;
        }
    }
}