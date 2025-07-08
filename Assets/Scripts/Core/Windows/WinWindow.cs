using System;
using LSCore;
public class WinWindow : BaseWindow<WinWindow>
{
    protected override void OnShowing()
    {
        CoreWorld.StopIdleMusic();
        base.OnShowing();
        Manager.canvas.sortingOrder = 120;
    }

    [Serializable]
    public class Level : ILocalizationArgument
    {
        public int offset;
        public override string ToString() => (CoreWorld.Level + offset).ToString();
    }
    
    [Serializable]
    public class LevelUp : DoIt
    {
        public override void Do() => CoreWorld.Level++;
    }
}
