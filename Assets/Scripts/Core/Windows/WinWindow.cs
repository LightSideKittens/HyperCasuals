using System;
using Core;
using LSCore;
public class WinWindow : BaseWindow<WinWindow>
{
    public LaLa.PlayClip sound;
    
    protected override void OnShowing()
    {
        FieldSave.Delete();
        if (LoseWindow.IsVisible)
        {
            LoseWindow.Hide();
        }
        sound.Do(); 
        CoreWorld.StopIdleMusic();
        base.OnShowing();
        Manager.canvas.sortingOrder = 120;
        Analytic.LogEvent("win_level", "level", GameSave.currentLevel);
    }

    [Serializable]
    public class Level : ILocalizationArgument
    {
        public int offset;
        public override string ToString() => (GameSave.Level + offset).ToString();
    }
    
    [Serializable]
    public class BestScore : ILocalizationArgument
    {
        public override string ToString() => (GameSave.BestScore).ToString();
    }
    
    [Serializable]
    public class TutorialLevelUp : DoIt
    {
        public override void Do() => GameSave.TutorialLevel++;
    }
    
    [Serializable]
    public class LevelUp : DoIt
    {
        public override void Do() => GameSave.Level++;
    }
}
