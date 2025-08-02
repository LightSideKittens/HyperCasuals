using System;
using LSCore;
using LSCore.Attributes;
using SourceGenerators;
using UnityEngine.SceneManagement;

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
        var levels = SingleAssets.Get<Levels>("Levels").levels;
        var themes = Themes.List[CoreWorld.Theme];
        SceneManager.LoadScene(themes, LoadSceneMode.Single);
        SceneManager.LoadScene(levels.GetWrapped(CoreWorld.Level-1, 10), LoadSceneMode.Additive);
    }
}