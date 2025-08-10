using System.ComponentModel;
using DG.Tweening;
using LSCore;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using SourceGenerators;
using UnityEngine.Scripting;

[InstanceProxy]
public partial class CoreWorld : ServiceManager<CoreWorld>
{
#if DEBUG
    public class DebugData
    {
        private static DebugData instance = new DebugData();

        public static void Init()
        {
            SRDebug.Instance.AddOptionContainer(instance);
        }

        public static void DeInit()
        {
            SRDebug.Instance?.RemoveOptionContainer(instance);
        }
        
        [Category("Core")]
        [Preserve]
        public void Win()
        {
            WinWindow.Show();
        }

        [Category("Core")]
        [Preserve]
        public void Lose()
        {
            LoseWindow.Show();
        }
    }
#endif
    
    public LaLa.PlayClip idleMusic;
    
    private void _StopIdleMusic()
    {
        var source = idleMusic.obj;
        if (source != null)
        { 
            source.DOFade(0, 0.5f).OnComplete(source.Stop).KillOnDestroy();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        BaseInitializer.Initialize();
        LoseWindow.onReviveClicked += OnReviveClicked;
#if DEBUG
        DebugData.Init();  
#endif
    }

    private void OnReviveClicked()
    {
        DOTween.Kill(idleMusic.obj);
        idleMusic.obj.volume = 1;
        idleMusic.Do();
    }

    private void Start()
    {
        Init();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        LoseWindow.onReviveClicked -= OnReviveClicked;
        _StopIdleMusic();
#if DEBUG
        DebugData.DeInit();
#endif
    }

    private void Init()
    {
        CoreWindow.Show();
        idleMusic.Do();
    }
}