#if UNITY_EDITOR
using LSCore;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Core
{
    [InitializeOnLoad]
    public static partial class FieldSave
    {
        private static bool wasEnabledOnStart;
        static FieldSave()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnGUI);
            World.Creating += () => wasEnabledOnStart = IsEnabled;
        }

        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool("Field Save Enabled", false);
            set
            {
                if (wasEnabledOnStart || World.IsEditMode)
                {
                    EditorPrefs.SetBool("Field Save Enabled", value);
                }
            }
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
