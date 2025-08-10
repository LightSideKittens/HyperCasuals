using System;
using Firebase.Analytics;
using LSCore;
using LSCore.Attributes;
using LSCore.ConditionModule;
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
            Analytic.LogEvent("start_tutorial", "level", GameSave.TutorialLevel.ToString());
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
            Analytic.LogEvent("start_level", "level", GameSave.currentLevel);
            base.Do();
        }
    }

    [SceneSelector] public string[] tutorialLevels;
    [SceneSelector] public string[] levels;
    public static string[] List => Instance.levels;
    public static string[] TutorialList => Instance.tutorialLevels;
}