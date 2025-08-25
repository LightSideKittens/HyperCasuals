using LSCore;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

public class GameSave
{
    public static RJObject Config => config ?? JTokenGameConfig.Get("GameCoreData");
    private static RJObject config;
    public static string currentLevel;
    public static string loseReason;

    public static int Level
    {
        get => Config.As("level", 1);
        set => Config["level"] = value;
    }
    
    public static int TutorialLevel
    {
        get => Config.As("tutorialLevel", 0);
        set => Config["tutorialLevel"] = value;
    }
    
    public static int Theme
    {
        get => Config.As("theme", 0);
        set => Config["theme"] = value;
    }

    public static int BestScore
    {
        get => Config.As("bestScore", 0);
        set => Config["bestScore"] = value;
    }
    
    private static JHashSet<int> themesSet;
    private static int id;

    private static void InitThemesSet(JArray themes)
    {
#if UNITY_EDITOR
        if (World.IsDiff(ref id))
        {
            themesSet = new JHashSet<int>(themes);
        }
#else
        themesSet ??= new JHashSet<int>(themes);
#endif
    }
    
    public static bool BuyTheme(int theme)
    {
        var themes = Config.AsJ<JArray>("themes");
        InitThemesSet(themes);
        return themesSet.Add(theme);
    }

    public static bool HasTheme(int theme)
    {
        var themes = Config.AsJ<JArray>("themes");
        InitThemesSet(themes);
        return themesSet.Contains(theme) || Theme == theme;
    }

}