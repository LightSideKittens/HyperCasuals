using System;
using System.Collections.Generic;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Modules;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace FunGames.Core.Settings
{
    public abstract class FGModuleSettings : ScriptableObject, IFGModuleSettings
    {
        [HideInInspector] public FGModuleInfo moduleInfo;

        public static Action OnValidateCalled;

        public Color LogColor = Color.white;
        public Color Color => LogColor;

        public bool logEnabled = true;
        public bool LogEnabled => logEnabled;

        protected const string AllCharsNoNewlineRegex = ".*";


        public FGModuleInfo ModuleInfo
        {
            get => moduleInfo;
            set
            {
                moduleInfo = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }

#if UNITY_EDITOR
        protected virtual void FillRemoteParameter(FGDBModuleParameter parameter)
        {
        }

        protected virtual string GetRemoteParameterValue(string parameterId) => null;

        public void FillModuleSettings(FGDBModuleSettings settings)
        {
            foreach (var parameter in settings.parameters)
            {
                FillRemoteParameter(parameter);
            }

            EditorUtility.SetDirty(this);
        }

        public bool IsModuleUpToDate(FGDBModuleSettings remoteSettings, FGModuleInfo localModuleInfo)
        {
            if (remoteSettings.module_version != localModuleInfo.Version) return false;

            foreach (var parameter in remoteSettings.parameters)
            {
                string localValue = GetRemoteParameterValue(parameter.id);
                if (localValue != null && localValue != parameter.value) return false;
            }

            return true;
        }

        public virtual List<FGDBModuleParameter> ExportParameters() => new();

        protected virtual void OnValidate()
        {
            OnValidateCalled?.Invoke();
        }

        public void OnAssetCreated()
        {
            FieldInfo[] fields = GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType != typeof(string)) continue;
                string value = field.GetValue(this) as string;
                if (value == null) field.SetValue(this, "");
            }
        }
#endif

    }
}