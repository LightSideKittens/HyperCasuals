using System;
using LSCore;
using LSCore.Attributes;
using LSCore.ConditionModule;
using LSCore.ConfigModule;
using UnityEngine.SceneManagement;

[Serializable]
public abstract class BaseLevelLoader : DoIt
{
    protected abstract string Level { get; }
    
    public override void Do() => Load();

    private void Load()
    {
        SceneManager.sceneLoaded += OnThemeLoaded;
        var themeScene = Themes.List[GameSave.Theme];
        SceneManager.LoadScene(themeScene, LoadSceneMode.Single);
    }

    private void OnThemeLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnThemeLoaded;
        SceneManager.LoadScene(Level, LoadSceneMode.Additive);
    }
}

[Serializable]
public class IsTimeForThemeTutorial : If
{
    public class Pass : DoIt
    {
        public override void Do()
        {
            FirstTime.Pass(key);
        }
    }
    
    private static string key = "Theme tutorial";
    public static bool Is => GameSave.Level > 3 && FirstTime.IsNot(key);
    protected override bool Check() => Is;
}

public class Levels : SingleScriptableObject<Levels>
{
    [Serializable]
    public class IsTutorialCompleted : If
    {
        public static bool Is => GameSave.TutorialLevel >= TutorialList.Length;
        protected override bool Check() => Is;
    }
    
    [Serializable]
    public class LoadCurrentTutorial : BaseLevelLoader
    {
        protected override string Level => TutorialList[GameSave.TutorialLevel];
        public override void Do()
        {
            base.Do();
            Analytic.LogEvent("start_tutorial", ("level", GameSave.TutorialLevel));
        }
    }
    
    [Serializable]
    public class LoadClassic : BaseLevelLoader
    {
        protected override string Level => "ClassicLevel";
        public override void Do()
        {
            base.Do();
            GameSave.currentLevel = "classic";
            Analytic.LogEvent("start_classic_level");
        }
    }
    
    [Serializable]
    public class LoadCurrent : BaseLevelLoader
    {
        protected override string Level => List.GetWrapped(GameSave.Level - 1, 10);
        public override void Do()
        {
            GameSave.currentLevel = $"level_{GameSave.Level}";
            Analytic.LogEvent("start_level", ("level", GameSave.Level));
            base.Do();
        }
    }

    [SceneSelector] public string[] tutorialLevels;
    [SceneSelector] public string[] levels;
    public static string[] List => Instance.levels;
    public static string[] TutorialList => Instance.tutorialLevels;
}