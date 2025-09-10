// EditTimeDurationDirect.cs
using System;
using LSCore;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Форматирует TimeSpan через Unity Localization SmartFormatter по строковому паттерну,
/// без String Table ключей. Работает в Edit Mode.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class EditTimeDurationDirect : MonoBehaviour
{
    [Header("Output Target (assign одно из полей)")]
    public TMP_Text tmpText;
    public Text uText;

    [Header("Preview (Edit Time)")]
    [Min(0)]
    public double previewSeconds = 185; // 3m 05s по умолчанию

    public Timely.Preset preset;

    
    void OnEnable()
    {
        RefreshNow();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            RefreshNow();
        }
    }

    public void Set(TimeSpan value)
    {
        RefreshNow();
    }

    void OnLocaleChanged(Locale _)
    {
        RefreshNow();
    }

    void RefreshNow()
    {
        string result = TimeSpan.FromSeconds(previewSeconds).Timelyze(preset);
        
        if (tmpText) tmpText.SetText(result);
        if (uText) uText.text = result;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        if (tmpText) EditorUtility.SetDirty(tmpText);
        if (uText) EditorUtility.SetDirty(uText);
#endif
    }
}
