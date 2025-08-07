using System;
using LSCore;
using LSCore.Attributes;
using LSCore.ConditionModule;
using SourceGenerators;
using UnityEngine.SceneManagement;

[Serializable]
public abstract class BaseLevelLoader : DoIt
{
    protected abstract string Level { get; }
    
    public override void Do() => Load();

    private void Load()
    {
        SceneManager.sceneLoaded += OnThemeLoaded;
        var themeScene = Themes.List[CoreWorld.Theme];
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
        public static bool Is => CoreWorld.TutorialLevel >= TutorialList.Length;
        protected override bool Check() => Is;
    }
    
    [Serializable]
    public class LoadCurrentTutorial : BaseLevelLoader
    {
        protected override string Level => TutorialList[CoreWorld.TutorialLevel];
    }
    
    [Serializable]
    public class LoadClassic : BaseLevelLoader
    {
        protected override string Level => "ClassicLevel";
    }
    
    [Serializable]
    public class LoadCurrent : BaseLevelLoader
    {
        protected override string Level => List.GetWrapped(CoreWorld.Level - 1, 10);
    }

    [SceneSelector] public string[] tutorialLevels;
    [SceneSelector] public string[] levels;
    public static string[] List => Instance.levels;
    public static string[] TutorialList => Instance.tutorialLevels;
}