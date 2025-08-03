using System;
using LSCore;
using LSCore.Attributes;
using SourceGenerators;
using UnityEngine.SceneManagement;

[Serializable]
public class LoadClassicLevel : DoIt
{
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
        SceneManager.LoadScene("ClassicLevel", LoadSceneMode.Additive);
    }
}


[InstanceProxy]
public partial class Levels : SingleScriptableObject<Levels>
{
    [Serializable]
    public class LoadCurrent : DoIt
    {
        public override void Do() => LoadCurrentLevel();
    }
    
    [SceneSelector] public string[] levels;
    
    private void _LoadCurrentLevel()
    {
        SceneManager.sceneLoaded += OnThemeLoaded;
        var themeScene = Themes.List[CoreWorld.Theme];
        SceneManager.LoadScene(themeScene, LoadSceneMode.Single);
    }

    private void OnThemeLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnThemeLoaded;
        var lvlScene = levels.GetWrapped(CoreWorld.Level - 1, 10);
        SceneManager.LoadScene(lvlScene, LoadSceneMode.Additive);
    }
}