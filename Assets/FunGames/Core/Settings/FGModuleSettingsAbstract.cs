using FunGames.Core.DatabaseModels;
using FunGames.Core.Settings;
using FunGames.Tools.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class FGModuleSettingsAbstract<T> : FGModuleSettings
    where T : FGModuleSettingsAbstract<T>, new()
{
    protected const int ORDER = 10;
    private static T _instance;
    private static object _lock = new object();

    protected abstract T LoadResources();
    
    public static T settings
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var s = CreateInstance<T>();
                        _instance = s.LoadResources();
                    }
                }
            }

            return _instance;
        }
    }
}