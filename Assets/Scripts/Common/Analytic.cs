using Firebase.Analytics;
using LSCore;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class Analytic
{
    private static bool isInited;

#if UNITY_EDITOR
    static Analytic()
    {
        World.Destroyed += () => isInited = false;
    }
#endif
    
    public static void Init()
    {
        if(isInited) return;
        isInited = true;
        InitMixerMuters();
        InitGameSave();
    }

    private static void InitMixerMuters()
    {
        foreach (var property in LaLa.Unmutes.Properties())
        {
            SetUserProperty(property.Name, property.Value.ToString());
        }
        
        SetUserProperty("haptic_enabled", BzBz.Unmuted.ToString());
        BzBz.Muter.Changed += value => SetUserProperty("haptic_enabled", value.ToString());
        LaLa.MixerMuter.Changed += (parameter, value) => SetUserProperty(parameter, value.ToString());
    }

    private static void InitGameSave()
    {
        ListenProperty("level");
        ListenProperty("theme");
        ListenProperty("bestScore");
    }
    

    private static string log = "[Analytic]".ToBold().ToColor(new Color(1f, 0.83f, 0.16f));

    private static void ListenProperty(string name)
    {
        GameSave.Config.ListenAndCall(name, value =>
        {
            if(value == null) return;
            var v = value.Parent as JProperty;
            SetUserProperty(v.Name, v.Value.ToString());
        });
    }
    
    private static void SetUserProperty(string name, string value)
    {
        Burger.Log($"{log} SetUserProperty {name} = {value}");
        FirebaseAnalytics.SetUserProperty(name, value);
    }

    public static void LogEvent(string name)
    {
        Burger.Log($"{log} LogEvent: {name}");
        FirebaseAnalytics.LogEvent(name);
    }
    
    public static void LogEvent(string name, Param param)
    {
        Burger.Log($"{log} LogEvent {name}: {param}");
        FirebaseAnalytics.LogEvent(name, param.parameter);
    }

    public static void LogEvent(string name, params Param[] parameters)
    {
        Burger.Log($"{log} LogEvent {name}: {string.Join(" ",  parameters)}");
        FirebaseAnalytics.LogEvent(name, parameters.ToParameters());
    }

    public static Parameter[] ToParameters(this Param[] param)
    {
        var parameters = new Parameter[param.Length];

        for (int i = 0; i < param.Length; i++)
        {
            parameters[i] = param[i].parameter;
        }

        return parameters;
    }

    public struct Param
    {
        public static implicit operator Param((string, string) param) => new(param.Item1, param.Item2);
        public static implicit operator Param((string, long) param) => new(param.Item1, param.Item2);
        public static implicit operator Param((string, double) param) => new(param.Item1, param.Item2);

        private string log;
        public Parameter parameter;

        public override string ToString()
        {
            return log;
        }

        public Param(string key, string val) : this()
        {
            log = $"\n\r{key}: {val}";
            parameter = new Parameter(key, val);
        }
        
        public Param(string key, long val) : this()
        {
            log = $"\n\r{key}: {val}";
            parameter = new Parameter(key, val);
        }
        
        public Param(string key, double val) : this()
        {
            log = $"\n\r{key}: {val}";
            parameter = new Parameter(key, val);
        }
    }
}