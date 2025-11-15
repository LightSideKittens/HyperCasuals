using System;
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
    public static Analytic.Param CurrentLevelParam => ("level", currentLevel);
    
    public static long RenewalDateTime
    {
        get => Config.As("renewalTime", DateTime.UtcNow.Ticks);
        set => Config["renewalTime"] = value;
    }

    public static int CollectedChests
    {
        get => Config.As("collectedChests", 0);
        set => Config["collectedChests"] = value;
    }

    public static bool IsChestGot
    {
        get => Config.As("isChestGot", false);
        set => Config["isChestGot"] = value;
    }
    
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
    
    public static JArray BoughtThemes => Config.AsJ<JArray>("themes");
    private static JHashSet<int> themesSet;
    protected static JHashSet<int> ThemesSet => themesSet = W.RS(ref themesSet) ?? new JHashSet<int>(BoughtThemes);
    private static int id;

    public static bool BuyTheme(int theme) => ThemesSet.Add(theme);
    public static bool HasTheme(int theme) => ThemesSet.Contains(theme) || Theme == theme;
}