#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Core
{
    [InitializeOnLoad]
    public static partial class FieldSave
    {
        static FieldSave()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnGUI);
        }

        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool("Field Save Enabled", false);
            set => EditorPrefs.SetBool("Field Save Enabled", value);
        }
        
        private static void OnGUI()
        {
            var enabled = IsEnabled;
            var text = enabled ? "✅Field Save Enabled✅" : "❌Field Save Disabled❌";
            if (GUILayout.Button(text, GUILayout.MaxWidth(200)))
            {
                IsEnabled = !enabled;
            }
        }
    }
}
#endif
